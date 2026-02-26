# ScrollView Implementation Plan

## Overview

`ScrollView` е контейнер, който клипира съдържанието си и позволява вертикален scroll чрез drag/fling (touch/pointer) и scroll wheel (мишка/trackpad). Показва тънка overlay лента вдясно като индикатор за позиция — без логика за show/hide.

---

## Архитектура

```
ScrollView (View)
 └─ Content (View) — измерва се с ∞ height, наредено с Y offset = -scrollOffsetY
```

Rendering stack:
```
RenderCore:
  Save() → BeginPath/Rect(Frame) → Clip()
    └─ Content.Render()
    └─ DrawScrollbarIndicator()    ← вътре в clip-а, overlay над content
  Restore()
  (НЕ се вика base.RenderCore — иначе content се рендерира два пъти)
```

---

## Нови / Изменени файлове

| Файл | Действие |
|------|----------|
| `Xui/Core/Core/UI/ScrollView.cs` | **НОВ** — основна имплементация |
| `Xui/Core/Core/UI/View.Input.cs` | **MODIFY** — добавя `virtual OnScrollWheel` |
| `Xui/Core/Core/Abstract/Events/ScrollWheelEventRef.cs` | **MODIFY** — добавя `bool Handled` |
| `Xui/Core/Core/Abstract/Window.cs` | **MODIFY** — route `OnScrollWheel` → RootView |
| `Xui/Core/Core/UI/RootView.cs` | **MODIFY** — track `lastMousePosition`; `IContent.OnScrollWheel` → `EventRouter.Dispatch` |
| `Xui/Core/Core/UI/Input/EventRouter.cs` | **MODIFY** — `Dispatch(ScrollWheelEventRef, Point)` с hit-test |
| `Xui/Middleware/Emulator/Devices/DeviceProfile.cs` | **MODIFY** — добавя `nfloat ScreenCornerRadius` |
| `Xui/Middleware/Emulator/Devices/DeviceCatalog.cs` | **MODIFY** — попълва `ScreenCornerRadius` за всеки профил |
| `Xui/Middleware/Emulator/Actual/EmulatorWindow.cs` | **MODIFY** — добавя `CurrentDevice` property; ползва `CurrentDevice.ScreenCornerRadius` |
| `Xui/Core/Core/Abstract/IWindow.cs` | **MODIFY** — добавя `nfloat ScreenCornerRadius { get; set; }` |
| `Xui/Core/Core/Abstract/Window.cs` | **MODIFY** — expose `ScreenCornerRadius` |
| `Xui/Apps/XuiSDK/Pages/ScrollDemoPage.cs` | **НОВ** — demo страница |

---

## Step 1 — `ScrollWheelEventRef.Handled`

Нужен е за да спрем propagation при `DispatchScrollWheel` — иначе листови view-та (Label, Button) блокират ScrollView-а над тях.

```csharp
// ScrollWheelEventRef.cs
public ref struct ScrollWheelEventRef
{
    /// <summary>
    /// The scroll delta. Positive Y = upward scroll (content moves up, offset increases).
    /// </summary>
    public Vector Delta;

    /// <summary>
    /// Set to true by a handler to stop further propagation.
    /// </summary>
    public bool Handled;
}
```

---

## Step 2 — `View.OnScrollWheel` (virtual hook)

За да избегнем type-check в `EventRouter`, добавяме виртуален метод към `View`:

```csharp
// View.Input.cs
public virtual void OnScrollWheel(ref ScrollWheelEventRef e)
{
    // override in subclasses (e.g. ScrollView)
}
```

---

## Step 3 — `ScrollView.cs`

### Полета

```csharp
public class ScrollView : View
{
    private View? content;
    private nfloat scrollOffsetY;
    private nfloat contentHeight;      // от последен MeasureCore
    private nfloat contentWidth;       // от последен MeasureCore
    private nfloat viewportHeight;     // от последен ArrangeCore

    // Drag tracking
    private bool isDragging;
    private bool isScrollGesture;      // true след като delta надмине прага
    private Point dragStartPos;
    private Point lastPointerPos;
    private long lastPointerTick;      // Environment.TickCount64
    private nfloat dragVelocity;       // pts/sec, positive = scroll down (виждаш повече content)

    // Fling animation
    private ExponentialDecayCurve? flingCurve;
    private nfloat? pendingFlingVelocity; // задава се при Up, инициализира се в AnimateCore

    // Scrollbar appearance
    public nfloat ScrollbarWidth { get; set; } = 3f;
    public nfloat ScrollbarEndInset { get; set; } = 4f; // offset от горния/долния край
```

### `Content` property

```csharp
    public View? Content
    {
        get => content;
        set => SetProtectedChild(ref content, value);
    }

    public override int Count => content is not null ? 1 : 0;

    public override View this[int index] => index == 0 && content is not null
        ? content : throw new IndexOutOfRangeException();
```

### `MeasureCore`

Content се мери с `∞` height. ScrollView взима цялото available пространство. Ако available е `∞`, връщаме размера на content вместо безкрайност. И `contentWidth` и `contentHeight` се пазят за ArrangeCore.

```csharp
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        contentWidth = 0;
        contentHeight = 0;

        if (content != null)
        {
            var desired = content.Measure((available.Width, nfloat.PositiveInfinity), context);
            contentWidth = desired.Width;
            contentHeight = desired.Height;
        }

        nfloat w = nfloat.IsFinite(available.Width)  ? available.Width  : contentWidth;
        nfloat h = nfloat.IsFinite(available.Height) ? available.Height : contentHeight;
        return (w, h);
    }
```

### `ArrangeCore`

Подрежда content с `-scrollOffsetY` offset. Подава `(contentWidth, contentHeight)` като desired size — не viewport ширината — за да може content с `HorizontalAlignment != Stretch` да запази естествения си размер.

```csharp
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        viewportHeight = rect.Height;
        ClampScrollOffset();

        if (content != null)
        {
            var contentRect = new Rect(rect.X, rect.Y - scrollOffsetY, rect.Width, contentHeight);
            content.Arrange(contentRect, context, new Size(contentWidth, contentHeight));
        }
    }

    private void ClampScrollOffset()
    {
        scrollOffsetY = nfloat.Clamp(scrollOffsetY, 0, MaxScrollOffset);
    }

    private nfloat MaxScrollOffset => nfloat.Max(0, contentHeight - viewportHeight);
```

### `HitTest`

Клипира hit testing до Frame на ScrollView — content може да е нареден извън видимите граници.

```csharp
    public override bool HitTest(Point point)
    {
        if (!this.Frame.Contains(point)) return false;
        for (int i = this.Count - 1; i >= 0; i--)
            if (this[i].HitTest(point))
                return true;
        return true; // ScrollView винаги улавя input в своите граници
    }
```

### `RenderCore`

Scrollbar се рисува ВЪТРЕ в clip-а (след content, преди Restore) — така е правилно клипиран и е overlay над content. `base.RenderCore` НЕ се вика, за да не се рендерира content повторно.

```csharp
    protected override void RenderCore(IContext context)
    {
        context.Save();

        context.BeginPath();
        context.Rect(this.Frame);
        context.Clip();

        content?.Render(context);
        DrawScrollbarIndicator(context); // overlay вътре в clip-а

        context.Restore();
        // base.RenderCore НЕ се вика — иначе content се рендерира два пъти
    }

    private void DrawScrollbarIndicator(IContext context)
    {
        if (MaxScrollOffset <= 0) return;

        nfloat trackH = viewportHeight - ScrollbarEndInset * 2;
        nfloat ratio = viewportHeight / contentHeight;
        nfloat barH = nfloat.Max(trackH * ratio, 20f);
        nfloat scrollProgress = scrollOffsetY / MaxScrollOffset;
        nfloat barTop = this.Frame.Y + ScrollbarEndInset + (trackH - barH) * scrollProgress;
        nfloat barX = this.Frame.Right - ScrollbarWidth - 2f;

        context.SetFill(new Color(0f, 0f, 0f, 0.35f));
        context.BeginPath();
        context.RoundRect(new Rect(barX, barTop, ScrollbarWidth, barH), ScrollbarWidth / 2);
        context.Fill(FillRule.NonZero);
    }
```

### `OnPointerEvent` — drag + fling с gesture disambiguation

Pointer capture само след надминаване на `ScrollThreshold` — до тогава бутоните вътре работят нормално.

```csharp
    private const nfloat ScrollThreshold = 8f; // pts

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;

        switch (e.Type)
        {
            case PointerEventType.Down:
                isDragging = true;
                isScrollGesture = false;
                dragStartPos = e.State.Position;
                lastPointerPos = e.State.Position;
                lastPointerTick = Environment.TickCount64;
                dragVelocity = 0;
                flingCurve = null;
                pendingFlingVelocity = null;
                // НЕ хващаме pointer тук — чакаме ScrollThreshold
                break;

            case PointerEventType.Move when isDragging:
            {
                var totalDy = e.State.Position.Y - dragStartPos.Y;

                if (!isScrollGesture && nfloat.Abs(totalDy) > ScrollThreshold)
                {
                    isScrollGesture = true;
                    CapturePointer(e.PointerId);
                    lastPointerPos = e.State.Position; // нулираме за точна velocity
                    lastPointerTick = Environment.TickCount64;
                }

                if (!isScrollGesture) break;

                var dy = (nfloat)(e.State.Position.Y - lastPointerPos.Y);
                var dt = (Environment.TickCount64 - lastPointerTick) / 1000.0;

                if (dt > 0)
                {
                    // positive velocity = scroll down (scrollOffset расте)
                    // пръст нагоре (dy < 0) → sample = -dy/dt > 0 → scrollOffset расте → виждаш повече content ✓
                    nfloat sample = (nfloat)(-dy / dt);
                    dragVelocity = dragVelocity * 0.6f + sample * 0.4f;
                }

                scrollOffsetY = nfloat.Clamp(scrollOffsetY - dy, 0, MaxScrollOffset);
                lastPointerPos = e.State.Position;
                lastPointerTick = Environment.TickCount64;

                InvalidateArrange();
                InvalidateRender();
                break;
            }

            case PointerEventType.Up when isDragging:
                isDragging = false;

                if (isScrollGesture)
                {
                    isScrollGesture = false;
                    ReleasePointer(e.PointerId);

                    if (nfloat.Abs(dragVelocity) > 50f)
                    {
                        pendingFlingVelocity = dragVelocity;
                        RequestAnimationFrame();
                    }
                }
                break;

            case PointerEventType.Cancel when isDragging:
                isDragging = false;
                if (isScrollGesture)
                {
                    isScrollGesture = false;
                    ReleasePointer(e.PointerId);
                }
                flingCurve = null;
                pendingFlingVelocity = null;
                break;
        }
    }
```

### `OnScrollWheel` — мишка/trackpad

`ScrollWheelEventRef.Delta.Y` — positive = upward scroll = content moves up = `scrollOffset` расте (виждаш повече надолу). Затова `+= Delta.Y`.

```csharp
    public override void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        if (e.Handled) return;
        scrollOffsetY = nfloat.Clamp(scrollOffsetY + (nfloat)e.Delta.Y * 30f, 0, MaxScrollOffset);
        flingCurve = null;
        pendingFlingVelocity = null;
        e.Handled = true;
        InvalidateArrange();
        InvalidateRender();
    }
```

*(scale factor 30f е примерен — нагласява се по платформа)*

### `AnimateCore` — inertia с ExponentialDecayCurve

`pendingFlingVelocity` се инициализира тук (а не в `OnPointerEvent`) за да получим точен `startTime` от animation frame.

```csharp
    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        if (pendingFlingVelocity.HasValue)
        {
            flingCurve = new ExponentialDecayCurve(
                startTime: current,
                startPosition: scrollOffsetY,
                initialVelocity: pendingFlingVelocity.Value,
                decayPerSecond: ExponentialDecayCurve.Normal);
            pendingFlingVelocity = null;
        }

        if (flingCurve is { } curve && !isDragging)
        {
            nfloat newOffset = curve[current];
            nfloat clamped = nfloat.Clamp(newOffset, 0, MaxScrollOffset);
            scrollOffsetY = clamped;

            bool atBoundary = clamped != newOffset;
            bool notDone = current < curve.EndTime;

            if (notDone && !atBoundary)
                RequestAnimationFrame();
            else
                flingCurve = null;

            InvalidateArrange();
            InvalidateRender();
        }
    }
```

### `OnActivate` — ScrollbarEndInset от ScreenCornerRadius

```csharp
    protected override void OnActivate()
    {
        if (TryFindParent<RootView>(out var root))
        {
            var r = root.Window.ScreenCornerRadius;
            if (r > 0)
                ScrollbarEndInset = nfloat.Max(ScrollbarEndInset, r / 3f);
        }
    }
```

---

## Step 4 — ScrollWheel routing

### `EventRouter.cs` — `Dispatch(ScrollWheelEventRef, Point)`

Depth-first, bubbles чрез `Handled`. Листови view-та не блокират ScrollView над тях.

```csharp
public void Dispatch(ref ScrollWheelEventRef e, Point position)
{
    DispatchScrollWheel(_rootView, ref e, position);
}

private static void DispatchScrollWheel(View view, ref ScrollWheelEventRef e, Point position)
{
    if (e.Handled) return;
    if (!view.Frame.Contains(position)) return;

    // Depth-first: innermost child first
    for (int i = view.Count - 1; i >= 0; i--)
    {
        DispatchScrollWheel(view[i], ref e, position);
        if (e.Handled) return;
    }

    // Bubble to this view if no child handled it
    view.OnScrollWheel(ref e);
}
```

### `RootView.cs` — tracking на lastMousePosition + routing

```csharp
private Point lastMousePosition;

void IContent.OnMouseMove(ref MouseMoveEventRef e)
{
    lastMousePosition = e.Position;
    this.EventRouter.Dispatch(ref e);
}

void IContent.OnMouseDown(ref MouseDownEventRef e)
{
    lastMousePosition = e.Position;
    this.EventRouter.Dispatch(ref e);
}

void IContent.OnScrollWheel(ref ScrollWheelEventRef e)
{
    this.EventRouter.Dispatch(ref e, lastMousePosition);
}
```

### `Window.cs`

```csharp
public override void OnScrollWheel(ref ScrollWheelEventRef e)
{
    ((IContent)this.RootView).OnScrollWheel(ref e);
}
```

---

## Step 5 — `DeviceProfile.ScreenCornerRadius`

### `DeviceProfile.cs`

```csharp
public readonly struct DeviceProfile
{
    // ... existing fields ...
    public nfloat ScreenCornerRadius { get; init; }
}
```

### `DeviceCatalog.cs`

| Модел | ScreenCornerRadius |
|-------|--------------------|
| iPhone 15 Pro | 44 |
| iPhone 14 | 44 |
| iPhone SE (3rd gen) | 16 |
| iPad Pro 12.9" | 18 |

### `IWindow.cs`

```csharp
public interface IWindow
{
    // ... existing ...
    nfloat ScreenCornerRadius { get; set; }
}
```

На десктоп — 0. Емулаторът и iOS runtime слагат стойността при инициализация.

---

## Step 6 — `EmulatorWindow` — `CurrentDevice` + `ScreenCornerRadius`

`EmulatorWindow` в момента има hardcoded `NFloat screenCornerRadius = 45f` в `Render()`. Трябва да се добави `CurrentDevice` property:

```csharp
// EmulatorWindow.cs
public DeviceProfile CurrentDevice { get; set; } = DeviceCatalog.All[0]; // iPhone 15 Pro
```

И в `Render()` се заменят всички `screenCornerRadius = 45f` с:

```csharp
NFloat screenCornerRadius = (NFloat)CurrentDevice.ScreenCornerRadius;
```

Емулаторът слага стойността в Abstract window при показване:

```csharp
Abstract!.ScreenCornerRadius = CurrentDevice.ScreenCornerRadius;
```

---

## Step 7 — Demo страница

```csharp
// Xui/Apps/XuiSDK/Pages/ScrollDemoPage.cs
new ScrollView
{
    Content = new VerticalStack
    {
        Children = Enumerable.Range(1, 30).Select(i =>
            new Label { Text = $"Item {i}" }
        ).ToList()
    }
}
```

---

## Последователност на имплементация

1. `ScrollWheelEventRef` — добавя `bool Handled`
2. `View.Input.cs` — добавя `virtual OnScrollWheel`
3. `ScrollView.cs` — Measure / Arrange / HitTest / RenderCore / OnPointerEvent / OnScrollWheel / AnimateCore / OnActivate
4. `EventRouter` — `Dispatch(ScrollWheelEventRef, Point)`
5. `RootView` — `lastMousePosition` tracking + `OnScrollWheel` routing
6. `Window.cs` — `OnScrollWheel` routing
7. `DeviceProfile` + `DeviceCatalog` — `ScreenCornerRadius`
8. `IWindow` + `Window` — `ScreenCornerRadius` property
9. `EmulatorWindow` — `CurrentDevice` + ползва `ScreenCornerRadius`
10. Demo страница в XuiSDK
11. Unit тестове — Measure/Arrange behavior

---

## Step 8 — Хоризонтален scroll (`ScrollDirection`)

### `ScrollDirection` enum

```csharp
public enum ScrollDirection { Vertical, Horizontal, Both }
```

### Нови полета в `ScrollView`

```csharp
private nfloat scrollOffsetX;
private nfloat viewportWidth;           // captured in ArrangeCore
private nfloat dragVelocityX;           // pts/sec, positive = scroll right
private ExponentialDecayCurve? flingCurveX;
private nfloat? pendingFlingVelocityX;

public ScrollDirection Direction { get; set; } = ScrollDirection.Vertical;
```

Съществуващите `dragVelocity`, `flingCurve`, `pendingFlingVelocity` се преименуват на `dragVelocityY`, `flingCurveY`, `pendingFlingVelocityY`.

### `MeasureCore` — съобразен с посоката

| Direction | Available за content |
|-----------|----------------------|
| Vertical   | `(available.Width, ∞)` |
| Horizontal | `(∞, available.Height)` |
| Both       | `(∞, ∞)` |

```csharp
var measureSize = Direction switch
{
    ScrollDirection.Horizontal => new Size(nfloat.PositiveInfinity, available.Height),
    ScrollDirection.Both       => new Size(nfloat.PositiveInfinity, nfloat.PositiveInfinity),
    _                          => new Size(available.Width, nfloat.PositiveInfinity)
};
```

### `ArrangeCore`

```csharp
viewportWidth = rect.Width;
// contentRect offsets both axes
var contentRect = new Rect(rect.X - scrollOffsetX, rect.Y - scrollOffsetY, contentWidth, contentHeight);
```

### `ClampScrollOffset`

```csharp
private nfloat MaxScrollOffsetY => nfloat.Max(0, contentHeight - viewportHeight);
private nfloat MaxScrollOffsetX => nfloat.Max(0, contentWidth - viewportWidth);

private void ClampScrollOffset()
{
    scrollOffsetY = nfloat.Clamp(scrollOffsetY, 0, MaxScrollOffsetY);
    scrollOffsetX = nfloat.Clamp(scrollOffsetX, 0, MaxScrollOffsetX);
}
```

### Scrollbars

- Вертикален индикатор (вдясно) — само когато `Direction != Horizontal`
- Хоризонтален индикатор (отдолу) — само когато `Direction != Vertical`

```csharp
private void DrawScrollbarIndicatorH(IContext context)
{
    if (Direction == ScrollDirection.Vertical) return;
    if (MaxScrollOffsetX <= 0) return;

    nfloat trackW = viewportWidth - ScrollbarEndInset * 2;
    nfloat ratio = viewportWidth / contentWidth;
    nfloat barW = nfloat.Max(trackW * ratio, 20f);
    nfloat scrollProgress = scrollOffsetX / MaxScrollOffsetX;
    nfloat barLeft = this.Frame.X + ScrollbarEndInset + (trackW - barW) * scrollProgress;
    nfloat barY = this.Frame.Bottom - ScrollbarWidth - 2f;

    context.SetFill(new Color(0f, 0f, 0f, 0.35f));
    context.BeginPath();
    context.RoundRect(new Rect(barLeft, barY, barW, ScrollbarWidth), ScrollbarWidth / 2);
    context.Fill(FillRule.NonZero);
}
```

### `OnPointerEvent` — и двете оси при `Direction == Both`

При `Direction == Both` се scrollва и по X, и по Y едновременно (map-panning). Gesture se lock-ва към доминиращата ос само при `Vertical` и `Horizontal` режим (не се налага — само една ос е активна).

```csharp
// Down: reset both velocities
dragVelocityY = 0; dragVelocityX = 0;

// Move threshold: Max(|totalDx|, |totalDy|) > ScrollThreshold
if (!isScrollGesture && nfloat.Max(nfloat.Abs(totalDx), nfloat.Abs(totalDy)) > ScrollThreshold)
    isScrollGesture = true;

// Apply per-axis (guarded by Direction)
if (Direction != ScrollDirection.Horizontal)
    scrollOffsetY = nfloat.Clamp(scrollOffsetY - dy, 0, MaxScrollOffsetY);
if (Direction != ScrollDirection.Vertical)
    scrollOffsetX = nfloat.Clamp(scrollOffsetX - dx, 0, MaxScrollOffsetX);
```

### `OnScrollWheel` — trackpad хоризонтален scroll

`Delta.X` носи хоризонтална стойност от trackpad (WM_MOUSEHWHEEL / pan gesture). Само ако нещо е обработено се слага `Handled = true`.

```csharp
public override void OnScrollWheel(ref ScrollWheelEventRef e)
{
    if (e.Handled) return;
    bool changed = false;

    if (Direction != ScrollDirection.Horizontal && e.Delta.Y != 0)
    { scrollOffsetY = nfloat.Clamp(scrollOffsetY - (nfloat)e.Delta.Y / 120f * 80f, 0, MaxScrollOffsetY); changed = true; }
    if (Direction != ScrollDirection.Vertical && e.Delta.X != 0)
    { scrollOffsetX = nfloat.Clamp(scrollOffsetX - (nfloat)e.Delta.X / 120f * 80f, 0, MaxScrollOffsetX); changed = true; }

    if (!changed) return;
    flingCurveY = null; flingCurveX = null;
    pendingFlingVelocityY = null; pendingFlingVelocityX = null;
    e.Handled = true;
    InvalidateArrange(); InvalidateRender();
}
```

### `AnimateCore` — независими fling криви за X и Y

```csharp
// Initialize pending curves
if (pendingFlingVelocityY.HasValue) { flingCurveY = new ExponentialDecayCurve(...); pendingFlingVelocityY = null; }
if (pendingFlingVelocityX.HasValue) { flingCurveX = new ExponentialDecayCurve(...); pendingFlingVelocityX = null; }

// Advance each curve independently; request next frame if either is still running
bool needsFrame = false;
bool changed = false;
// ... advance Y, advance X, check boundaries ...
if (needsFrame) RequestAnimationFrame();
if (changed) { InvalidateArrange(); InvalidateRender(); }
```

---

## Бележки и ограничения

- ~~**Само вертикален scroll**~~ — вече поддържа `Vertical`, `Horizontal`, `Both`
- **Един content child** — не е ViewCollection
- **Scrollbar винаги видим** — няма fade in/out
- **Velocity знак**: positive = scroll down (scrollOffset расте = виждаш повече content надолу)
  - пръст нагоре: `dy < 0` → `sample = -dy/dt > 0` → scrollOffset расте ✓
  - пръст надолу: `dy > 0` → `sample = -dy/dt < 0` → scrollOffset намалява ✓
- **ScrollWheel знак**: `Delta.Y` positive = upward scroll = `scrollOffset += Delta.Y * scale`
  - платформата може да праща обратна посока — нагласява се при тестване
- **Velocity tracking**: `Environment.TickCount64` (wall-clock ms) — достатъчно точно за gesture detection
- **Bounce overscroll**: не е в плана — `ClampScrollOffset()` ограничава до `[0, maxOffset]`
- **Tap vs scroll**: gesture disambiguation чрез `ScrollThreshold = 8pt`
- **ScrollView в ScrollView**: `e.Handled = true` в `OnScrollWheel` гарантира innermost wins
- **`InvalidateArrange` при scroll**: всяка промяна на `scrollOffsetY` изисква и двете — `InvalidateArrange()` за repositioning на content, `InvalidateRender()` за trigger на нов frame

---

## Файлова структура

```
Xui/Core/Core/Abstract/Events/
  ScrollWheelEventRef.cs         ← +bool Handled

Xui/Core/Core/UI/
  View.Input.cs                  ← +virtual OnScrollWheel
  ScrollView.cs                  ← NEW
  RootView.cs                    ← +lastMousePosition, +OnScrollWheel routing
  Input/EventRouter.cs           ← +Dispatch(ScrollWheelEventRef, Point)

Xui/Core/Core/Abstract/
  IWindow.cs                     ← +nfloat ScreenCornerRadius
  Window.cs                      ← +OnScrollWheel routing, +ScreenCornerRadius

Xui/Middleware/Emulator/Devices/
  DeviceProfile.cs               ← +nfloat ScreenCornerRadius
  DeviceCatalog.cs               ← +стойности

Xui/Middleware/Emulator/Actual/
  EmulatorWindow.cs              ← +CurrentDevice property, ScreenCornerRadius вместо 45f

Xui/Apps/XuiSDK/Pages/
  ScrollDemoPage.cs              ← NEW
```
