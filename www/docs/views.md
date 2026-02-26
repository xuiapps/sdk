---
title: View System
description: The View base class, layout protocol, lifecycle, and built-in views.
---

# View System

All UI in Xui is a tree of `View` objects. Each view participates in layout (measure, arrange, render), handles input, and reports its own hit region.

**Source:** `Xui/Core/Core/UI/View.cs` and the `View.*.cs` partials alongside it.

## Core properties

```csharp
public partial class View
{
    public string? Id { get; set; }           // lookup via FindViewById
    public View?   Parent { get; }            // set by the container

    public Rect    Frame  { get; }            // final position in window coords
    public Frame   Margin { get; set; }       // external spacing

    public HorizontalAlignment HorizontalAlignment { get; set; }  // default: Stretch
    public VerticalAlignment   VerticalAlignment   { get; set; }  // default: Stretch

    public nfloat MinimumWidth  { get; set; }
    public nfloat MinimumHeight { get; set; }
    public nfloat MaximumWidth  { get; set; }  // default: PositiveInfinity
    public nfloat MaximumHeight { get; set; }  // default: PositiveInfinity
}
```

`Frame` is in window coordinates (top-left origin). Use it inside `RenderCore` to know where to draw.

## Layout protocol

Layout runs in three passes, each driven by `View.Update(LayoutGuide)`.

### Measure

```csharp
protected virtual Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
```

Return the minimum size your content needs. `availableBorderEdgeSize` is already margin-subtracted — the engine handles margins. The base implementation measures children and returns their max.

### Arrange

```csharp
protected virtual void ArrangeCore(Rect rect, IMeasureContext context)
```

`rect` is the final border-edge rectangle. Position your children here:

```csharp
protected override void ArrangeCore(Rect rect, IMeasureContext context)
{
    var childRect = rect.Inset(padding);
    myChild.Arrange(childRect, context);
}
```

### Render

```csharp
protected virtual void RenderCore(IContext context)
```

Draw into `context`. `Frame` is set by the time `RenderCore` is called. See [Canvas API](canvas.md) for drawing primitives.

### Combining passes

Containers can combine all three passes in one call:

```csharp
child.Update(new LayoutGuide
{
    Pass          = LayoutGuide.LayoutPass.Measure
                  | LayoutGuide.LayoutPass.Arrange
                  | LayoutGuide.LayoutPass.Render,
    AvailableSize  = availableSize,
    Anchor         = origin,
    XSize          = LayoutGuide.SizeTo.Exact,
    YSize          = LayoutGuide.SizeTo.Exact,
    XAlign         = LayoutGuide.Align.Start,
    YAlign         = LayoutGuide.Align.Start,
    MeasureContext = context,
    RenderContext  = context,
});
```

### LayoutPass flags

```csharp
[Flags]
public enum LayoutPass : byte
{
    Animate = 1 << 0,   // advance per-frame state
    Measure = 1 << 1,   // compute desired size
    Arrange = 1 << 2,   // set Frame
    Render  = 1 << 3,   // draw
}
```

### SizeTo constraint

| Value | Meaning |
|---|---|
| `Exact` | View must match the given size exactly |
| `AtMost` | View may shrink to content, but not exceed the constraint |

## Animation

```csharp
protected virtual void AnimateCore(TimeSpan previousTime, TimeSpan currentTime) { }
```

Override `AnimateCore` to advance per-frame state. Call `InvalidateRender()` when visuals change and `RequestAnimationFrame()` to continue next frame:

```csharp
protected override void AnimateCore(TimeSpan prev, TimeSpan current)
{
    _angle += (current - prev).TotalSeconds * Math.PI;
    RequestAnimationFrame();
    InvalidateRender();
}
```

## Lifecycle

Views have four lifecycle hooks called by the framework when they enter or leave the tree.

```csharp
protected virtual void OnAttach(ref AttachEventRef e)   { }  // platform contexts available
protected virtual void OnDetach(ref DetachEventRef e)   { }  // release resources

protected virtual void OnActivate()                     { }  // start animations / subscriptions
protected virtual void OnDeactivate()                   { }  // stop animations / subscriptions
```

- **Attach / Detach** fire when the subtree joins or leaves the window's visual tree. Platform contexts (text measure, image pipeline) are reachable via `GetService<T>()` in `OnAttach`.
- **Activate / Deactivate** fire when the view becomes visible (e.g., a tab page becomes current). A view in a recycled panel stays attached but may be deactivated.
- **Subtree order:** Attach and Activate propagate parent-first (top-down). Detach and Deactivate propagate children-first (bottom-up).

Example — acquiring an image on attach:

```csharp
public class ThumbnailView : View
{
    private IImage? _image;

    protected override void OnAttach(ref AttachEventRef e)
    {
        _image = this.GetService<IImage>();
        _image.Load("Assets/thumbnail.png");
    }

    protected override void OnDetach(ref DetachEventRef e)
    {
        _image = null;
    }

    protected override void RenderCore(IContext context)
    {
        if (_image != null)
            context.DrawImage(_image, Frame);
    }
}
```

## Hit testing

```csharp
public virtual bool HitTest(Point point)
```

Default: checks children depth-first (reverse order), then the view's own `Frame`. Override to exclude transparent areas or expand the hit region:

```csharp
public override bool HitTest(Point point)
{
    // circular hit region
    var center = Frame.Center;
    var radius = Frame.Width / 2;
    return Point.Distance(center, point) <= radius;
}
```

## Children

`View` inherits a built-in child collection. Access children with `this[i]` and `this.Count`. Containers initialise their children declaratively:

```csharp
public class MyPanel : View
{
    private readonly Label _label = new Label { Text = "Hello" };
    private readonly ImageView _icon = new ImageView();

    public MyPanel()
    {
        Add(_label);
        Add(_icon);
    }
}
```

## Built-in views

| Type | Purpose |
|---|---|
| `Label` | Single-line or multiline text |
| `ImageView` | Renders an `IImage` |
| `Border` | Background fill, border stroke, and a single child |
| `ScrollView` | Scrollable container for a single child |
| `HorizontalStack` | Lays out children left to right |
| `VerticalStack` | Lays out children top to bottom |
| `HorizontalUniformStack` | Equal-width horizontal layout |
| `VerticalUniformStack` | Equal-height vertical layout |
| `TextBox` | Single-line editable text input |

## Custom view example

```csharp
public class ProgressBar : View
{
    public double Value { get; set; } = 0.5;

    protected override Size MeasureCore(Size available, IMeasureContext context)
        => (available.Width, 8);  // always 8 px tall, full width

    protected override void RenderCore(IContext context)
    {
        // Track
        context.SetFill(new Color(0xE0, 0xE0, 0xE0, 0xFF));
        context.FillRect(Frame);

        // Fill
        context.SetFill(new Color(0x00, 0x78, 0xD4, 0xFF));
        context.FillRect(new Rect(Frame.X, Frame.Y,
            (nfloat)(Frame.Width * Value), Frame.Height));
    }
}
```
