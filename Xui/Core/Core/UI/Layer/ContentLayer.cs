using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Layer;

/// <summary>
/// A terminal layer that delegates layout and rendering to a child <see cref="View"/>,
/// bridging the layer tree back into the standard view hierarchy.
/// Input routing is left to the <see cref="Input.EventRouter"/> which hit-tests the child
/// view directly, so all input methods here are intentional no-ops.
/// </summary>
public struct ContentLayer : ILayer<View>
{
    /// <summary>The child view to measure, arrange, and render.</summary>
    public View? Child;

    /// <inheritdoc/>
    public void Update(View view, ref LayoutGuide guide)
    {
        if (guide.IsAnimate) Animate(view, guide.PreviousTime, guide.CurrentTime);
        if (guide.IsMeasure) guide.DesiredSize = Measure(view, guide.AvailableSize, guide.MeasureContext!);
        if (guide.IsArrange) Arrange(view, guide.ArrangedRect, guide.MeasureContext!);
        if (guide.IsRender)  Render(view, guide.RenderContext!);
    }

    /// <inheritdoc/>
    public Size Measure(View view, Size availableSize, IMeasureContext context)
        => Child?.Measure(availableSize, context) ?? Size.Empty;

    /// <inheritdoc/>
    public void Arrange(View view, Rect rect, IMeasureContext context)
        => Child?.Arrange(rect, context);

    /// <inheritdoc/>
    public void Render(View view, IContext context)
        => Child?.Render(context);

    /// <inheritdoc/>
    public void Animate(View view, TimeSpan previousTime, TimeSpan currentTime)
        => Child?.Animate(previousTime, currentTime);

    // EventRouter dispatches to the child View directly via hit-testing.
    /// <inheritdoc/>
    public void OnPointerEvent(View view, ref PointerEventRef e, EventPhase phase) { }
    /// <inheritdoc/>
    public void OnKeyDown(View view, ref KeyEventRef e) { }
    /// <inheritdoc/>
    public void OnChar(View view, ref KeyEventRef e) { }
    /// <inheritdoc/>
    public void OnFocus(View view) { }
    /// <inheritdoc/>
    public void OnBlur(View view) { }
}
