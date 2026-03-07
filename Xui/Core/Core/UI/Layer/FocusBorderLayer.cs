using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Layer;

/// <summary>
/// A <see cref="BorderLayer{TChild}"/> variant that switches border color when the host
/// view has keyboard focus. All layout and rendering is delegated to an inner
/// <see cref="BorderLayer{TChild}"/>; only <see cref="Render"/> swaps the color, and
/// <see cref="OnFocus"/> / <see cref="OnBlur"/> request a repaint.
/// </summary>
public struct FocusBorderLayer<TView, TChild> : ILayer<TView>
    where TView : ILayerHost
    where TChild : struct, ILayer<TView>
{
    /// <summary>The inner border layer that owns all geometry and rendering.</summary>
    public BorderLayer<TView, TChild> Border;

    /// <summary>Border color used when the host view has keyboard focus.</summary>
    public Color FocusedBorderColor;

    // ── Forwarding properties ────────────────────────────────────────────

    /// <inheritdoc cref="BorderLayer{TChild}.BorderThickness"/>
    public Frame BorderThickness
    {
        get => Border.BorderThickness;
        set => Border.BorderThickness = value;
    }

    /// <inheritdoc cref="BorderLayer{TChild}.CornerRadius"/>
    public CornerRadius CornerRadius
    {
        get => Border.CornerRadius;
        set => Border.CornerRadius = value;
    }

    /// <inheritdoc cref="BorderLayer{TChild}.BackgroundColor"/>
    public Color BackgroundColor
    {
        get => Border.BackgroundColor;
        set => Border.BackgroundColor = value;
    }

    /// <inheritdoc cref="BorderLayer{TChild}.BorderColor"/>
    public Color BorderColor
    {
        get => Border.BorderColor;
        set => Border.BorderColor = value;
    }

    /// <inheritdoc cref="BorderLayer{TChild}.Padding"/>
    public Frame Padding
    {
        get => Border.Padding;
        set => Border.Padding = value;
    }

    // ── ILayer<TView> ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Update(TView view, ref LayoutGuide guide)
    {
        if (guide.IsAnimate) Animate(view, guide.PreviousTime, guide.CurrentTime);
        if (guide.IsMeasure) guide.DesiredSize = Measure(view, guide.AvailableSize, guide.MeasureContext!);
        if (guide.IsArrange) Arrange(view, guide.ArrangedRect, guide.MeasureContext!);
        if (guide.IsRender)  Render(view, guide.RenderContext!);
    }

    /// <inheritdoc/>
    public Size Measure(TView view, Size availableSize, IMeasureContext context)
        => Border.Measure(view, availableSize, context);

    /// <inheritdoc/>
    public void Arrange(TView view, Rect rect, IMeasureContext context)
        => Border.Arrange(view, rect, context);

    /// <inheritdoc/>
    public void Render(TView view, IContext context)
    {
        // Temporarily swap border color when focused, then restore.
        var saved = Border.BorderColor;
        if (view.IsFocused) Border.BorderColor = FocusedBorderColor;
        Border.Render(view, context);
        Border.BorderColor = saved;
    }

    /// <inheritdoc/>
    public void Animate(TView view, TimeSpan previousTime, TimeSpan currentTime)
        => Border.Animate(view, previousTime, currentTime);

    /// <inheritdoc/>
    public void OnPointerEvent(TView view, ref PointerEventRef e, EventPhase phase)
        => Border.OnPointerEvent(view, ref e, phase);

    /// <inheritdoc/>
    public void OnKeyDown(TView view, ref KeyEventRef e)
        => Border.OnKeyDown(view, ref e);

    /// <inheritdoc/>
    public void OnChar(TView view, ref KeyEventRef e)
        => Border.OnChar(view, ref e);

    /// <inheritdoc/>
    public void OnFocus(TView view)
    {
        view.InvalidateRender();
        Border.OnFocus(view);
    }

    /// <inheritdoc/>
    public void OnBlur(TView view)
    {
        view.InvalidateRender();
        Border.OnBlur(view);
    }
}
