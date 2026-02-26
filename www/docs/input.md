---
title: Input Handling
description: Pointer events, keyboard, scroll, focus, and window hit testing.
---

# Input Handling

Input in Xui is event-driven. The platform window receives raw events and routes them through the view hierarchy. All event structs are `ref struct` — no heap allocation on the hot path.

**Source:** `Xui/Core/Core/UI/View.Input.cs`, `Xui/Core/Core/UI/Input/EventRouter.cs`, `Xui/Core/Core/Abstract/Events/`.

## Pointer events

Mouse, touch, and pen all arrive as a unified `PointerEventRef`. Override `OnPointerEvent` on any view:

```csharp
public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
{
    if (e.Type == PointerEventType.Down && phase == EventPhase.Bubble)
    {
        var pos = e.State.Position;
        // handle press at pos
    }
}
```

### Event types

| `PointerEventType` | W3C equivalent | When |
|---|---|---|
| `Over` | `pointerover` | Pointer enters the hit region |
| `Enter` | `pointerenter` | Pointer enters the element or a descendant |
| `Down` | `pointerdown` | Button pressed |
| `Move` | `pointermove` | Pointer moved |
| `Up` | `pointerup` | Button released |
| `Cancel` | `pointercancel` | System cancelled the interaction |
| `Out` | `pointerout` | Pointer left the hit region |
| `Leave` | `pointerleave` | Pointer left the element and all descendants |
| `GotCapture` | `gotpointercapture` | Pointer capture acquired |
| `LostCapture` | `lostpointercapture` | Pointer capture released |

### Event phases

Every pointer event travels two phases through the view hierarchy:

- **Tunnel** (`EventPhase.Tunnel`) — dispatched root → target. Use this to intercept events before children see them.
- **Bubble** (`EventPhase.Bubble`) — dispatched target → root. Most handlers go here.

```csharp
public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
{
    if (phase == EventPhase.Tunnel)
    {
        // parent-first: can intercept before children
    }
    else // EventPhase.Bubble
    {
        // child-first: normal handler
    }
}
```

### Pointer state

`e.State` carries the full physical state of the pointer:

```csharp
Point       position  = e.State.Position;
PointerType type      = e.State.PointerType;  // Mouse, Touch, Pen, Unknown
PointerButton button  = e.State.Button;        // Left, Right, Middle, X1, X2, Eraser
```

For high-frequency input (stylus drawing), use `e.CoalescedStates` to access all intermediate samples since the previous event.

### Pointer capture

Capture keeps events flowing to your view even after the pointer leaves its bounds — useful for drag interactions:

```csharp
public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
{
    if (e.Type == PointerEventType.Down && phase == EventPhase.Bubble)
        CapturePointer(e.PointerId);

    if (e.Type == PointerEventType.Up)
        ReleasePointer(e.PointerId);
}
```

## Scroll

```csharp
public override void OnScrollWheel(ref ScrollWheelEventRef e)
{
    // e.Delta is the scroll vector (positive Y = scroll up)
    _offset += e.Delta;
    e.Handled = true;    // stop propagation to parent
    InvalidateRender();
}
```

`ScrollView` handles this automatically for its content child.

## Keyboard

Keyboard events reach the **focused** view only. Override `Focusable` to opt in:

```csharp
public override bool Focusable => true;

public override void OnKeyDown(ref KeyEventRef e)
{
    if (e.Key == VirtualKey.Return)
    {
        e.Handled = true;
        Submit();
    }
}

public override void OnChar(ref KeyEventRef e)
{
    _text += e.Character;
    InvalidateRender();
}
```

`OnKeyDown` fires for every key press (and auto-repeats if `e.IsRepeat` is true). `OnChar` fires for printable characters after platform key translation.

### Focus API

```csharp
this.Focus();            // request keyboard focus
this.Blur();             // release focus

bool f = this.IsFocused;

protected override void OnFocus() { /* show caret / highlight */ }
protected override void OnBlur()  { /* hide caret / unhighlight */ }
```

Tab cycles focus forward through focusable views; Shift+Tab cycles backward. Focus order is managed by `RootView`.

## Window hit testing

Override `WindowHitTest` in your `Window` subclass to define draggable and resizable regions in a custom-chrome window (no OS title bar):

```csharp
public class MainWindow : Window
{
    public MainWindow(IServiceProvider services) : base(services) { ... }

    public override void WindowHitTest(ref WindowHitTestEventRef e)
    {
        // Top 48 px acts as the drag handle
        if (e.Point.Y < 48)
            e.Area = WindowHitTestEventRef.WindowArea.Title;
    }
}
```

Available `WindowArea` values:

| Value | Effect |
|---|---|
| `Default` | Platform decides (transparent to framework) |
| `Client` | Normal content area |
| `Title` | Draggable title bar |
| `BorderTop` / `BorderBottom` / `BorderLeft` / `BorderRight` | Resize edge |
| `BorderTopLeft` / `BorderTopRight` / `BorderBottomLeft` / `BorderBottomRight` | Resize corner |
| `Transparent` | Click-through (no hit) |
