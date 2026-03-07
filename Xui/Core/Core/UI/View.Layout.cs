using Xui.Core.Canvas;
using Xui.Core.Debug;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

public partial class View
{
    /// <summary>
    /// Drives one or more layout passes for this view.
    /// <para>
    /// The default implementation dispatches each requested pass to its shell method in order.
    /// Views that can handle all four passes in a single DFS traversal (LuminarFlow) should
    /// override this method and check <see cref="LayoutGuide.IsLuminarFlow"/> to do so.
    /// A fork — a container that must measure all children before it can position any of them —
    /// cannot do a single-pass walk and should leave this method as-is, calling the public
    /// convenience methods <see cref="Animate"/>, <see cref="Measure"/>, <see cref="Arrange"/>,
    /// <see cref="Render"/> separately from its parent.
    /// </para>
    /// </summary>
    public virtual LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsAnimate) this.AnimateShell(ref guide);
        if (guide.IsMeasure) this.MeasureShell(ref guide);
        if (guide.IsArrange) this.ArrangeShell(ref guide);
        if (guide.IsRender)  this.RenderShell(ref guide);

        return guide;
    }

    // Public convenience entry points

    /// <summary>
    /// Advances animation state for this view and all descendants for the current frame.
    /// </summary>
    public void Animate(TimeSpan previousTime, TimeSpan currentTime)
    {
        var guide = new LayoutGuide
        {
            Pass = LayoutGuide.LayoutPass.Animate,
            PreviousTime = previousTime,
            CurrentTime = currentTime,
        };
        this.AnimateShell(ref guide);
    }

    /// <summary>
    /// Measures the view and returns the desired margin-box size.
    /// </summary>
    public Size Measure(Size availableSize, IMeasureContext context)
    {
        var guide = new LayoutGuide
        {
            Pass = LayoutGuide.LayoutPass.Measure,
            AvailableSize = availableSize,
            XSize = LayoutGuide.SizeTo.AtMost,
            YSize = LayoutGuide.SizeTo.AtMost,
            MeasureContext = context,
        };
        this.MeasureShell(ref guide);
        return guide.DesiredSize;
    }

    /// <summary>
    /// Arranges the view within <paramref name="rect"/>, finalising its position and size.
    /// </summary>
    public Rect Arrange(Rect rect, IMeasureContext context, Size? desiredSize = null)
    {
        var guide = new LayoutGuide
        {
            Pass = LayoutGuide.LayoutPass.Arrange,
            AvailableSize = rect.Size,
            // TODO: cache desired size instead of re-measuring here — delete desiredSize param once caching lands.
            DesiredSize = desiredSize ?? this.Measure(rect.Size, context),
            XSize = LayoutGuide.SizeTo.Exact,
            YSize = LayoutGuide.SizeTo.Exact,
            MeasureContext = context,
            Anchor = rect.TopLeft,
        };
        this.ArrangeShell(ref guide);
        return guide.ArrangedRect;
    }

    /// <summary>
    /// Renders the view. Must be called after layout is complete.
    /// </summary>
    public void Render(IContext context)
    {
        var guide = new LayoutGuide
        {
            Pass = LayoutGuide.LayoutPass.Render,
            MeasureContext = context,
            RenderContext = context,
        };
        this.RenderShell(ref guide);
    }

    // Shell methods - bookkeeping wrappers, call the Core virtuals

    /// <summary>
    /// Resets animation flags then calls <see cref="AnimateCore"/>,
    /// which by default recurses into children.
    /// </summary>
    protected void AnimateShell(ref LayoutGuide guide)
    {
        this.ResetAnimationFlags();
        this.AnimateCore(guide.PreviousTime, guide.CurrentTime);
    }

    /// <summary>
    /// Applies fixed-size short-circuit, min/max clamping, and logging around
    /// <see cref="MeasureCore"/>. Writes the result into <see cref="LayoutGuide.DesiredSize"/>.
    /// </summary>
    protected void MeasureShell(ref LayoutGuide guide)
    {
        Size availableMarginBoxSize = Size.Max((0, 0), guide.AvailableSize);

        bool fixedWidth =
            guide.XSize == LayoutGuide.SizeTo.Exact &&
            nfloat.IsFinite(guide.AvailableSize.Width) &&
            this.HorizontalAlignment == HorizontalAlignment.Stretch;

        bool fixedHeight =
            guide.YSize == LayoutGuide.SizeTo.Exact &&
            nfloat.IsFinite(guide.AvailableSize.Height) &&
            this.VerticalAlignment == VerticalAlignment.Stretch;

        Size desiredBorderEdgeBoxSize;
        if (fixedWidth && fixedHeight)
        {
            desiredBorderEdgeBoxSize = availableMarginBoxSize - this.Margin;
        }
        else
        {
            desiredBorderEdgeBoxSize = this.MeasureCore(availableMarginBoxSize - this.Margin, guide.MeasureContext!);
            if (fixedWidth)
                desiredBorderEdgeBoxSize.Width = guide.AvailableSize.Width;
            if (fixedHeight)
                desiredBorderEdgeBoxSize.Height = guide.AvailableSize.Height;
        }

        desiredBorderEdgeBoxSize = (
            nfloat.Clamp(desiredBorderEdgeBoxSize.Width,  this.MinimumWidth,  this.MaximumWidth),
            nfloat.Clamp(desiredBorderEdgeBoxSize.Height, this.MinimumHeight, this.MaximumHeight)
        );

        guide.DesiredSize = desiredBorderEdgeBoxSize + this.Margin;

        guide.Instruments.Log(Scope.ViewMeasure, LevelOfDetail.Info,
            $"Measure {this.GetType().Name} Available({guide.AvailableSize.Width:F1}, {guide.AvailableSize.Height:F1}) Margin({this.Margin.Left:F1}, {this.Margin.Top:F1}, {this.Margin.Right:F1}, {this.Margin.Bottom:F1}) -> Desired({guide.DesiredSize.Width:F1}, {guide.DesiredSize.Height:F1})");
    }

    /// <summary>
    /// Computes the border-edge rectangle from anchor, alignment and available space, assigns
    /// <see cref="Frame"/>, then calls <see cref="ArrangeCore"/>.
    /// Writes the result into <see cref="LayoutGuide.ArrangedRect"/>.
    /// </summary>
    protected void ArrangeShell(ref LayoutGuide guide)
    {
        nfloat x = guide.Anchor.X - guide.DesiredSize.Width  * (int)guide.XAlign * (nfloat).5;
        nfloat y = guide.Anchor.Y - guide.DesiredSize.Height * (int)guide.YAlign * (nfloat).5;
        nfloat width  = this.HorizontalAlignment == HorizontalAlignment.Stretch ? guide.AvailableSize.Width  : guide.DesiredSize.Width;
        nfloat height = this.VerticalAlignment   == VerticalAlignment.Stretch   ? guide.AvailableSize.Height : guide.DesiredSize.Height;

        if (guide.XSize == LayoutGuide.SizeTo.Exact &&
            this.HorizontalAlignment != HorizontalAlignment.Stretch &&
            nfloat.IsFinite(guide.AvailableSize.Width))
        {
            nfloat xViewAlignment = ((int)this.HorizontalAlignment - 1 - (int)guide.XAlign) * (nfloat).5;
            x += (guide.AvailableSize.Width - guide.DesiredSize.Width) * xViewAlignment;
        }

        if (guide.YSize == LayoutGuide.SizeTo.Exact &&
            this.VerticalAlignment != VerticalAlignment.Stretch &&
            nfloat.IsFinite(guide.AvailableSize.Height))
        {
            nfloat yViewAlignment = ((int)this.VerticalAlignment - 1 - (int)guide.YAlign) * (nfloat).5;
            y += (guide.AvailableSize.Height - guide.DesiredSize.Height) * yViewAlignment;
        }

        guide.ArrangedRect = new Rect(x, y, width, height) - this.Margin;
        this.Frame = guide.ArrangedRect;

        guide.Instruments.Log(Scope.ViewArrange, LevelOfDetail.Info,
            $"Arrange {this.GetType().Name} Anchor({guide.Anchor.X:F1}, {guide.Anchor.Y:F1}) Desired({guide.DesiredSize.Width:F1}, {guide.DesiredSize.Height:F1}) -> Frame({this.Frame.X:F1}, {this.Frame.Y:F1}, {this.Frame.Width:F1}, {this.Frame.Height:F1})");

        this.ValidateArrange();
        this.ArrangeCore(guide.ArrangedRect, guide.MeasureContext!);
    }

    /// <summary>
    /// Validates render state then calls <see cref="RenderCore"/>.
    /// </summary>
    protected void RenderShell(ref LayoutGuide guide)
    {
        this.ValidateRender();
        this.RenderCore(guide.RenderContext!);
    }

    // Core virtuals - override points for subclasses

    /// <summary>
    /// Per-frame animation hook. Mutate time-based state and call
    /// <see cref="RequestAnimationFrame"/> if the animation should continue.
    /// The default implementation recurses into children; override to customise child traversal.
    /// </summary>
    protected virtual void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
    {
        for (int i = 0; i < this.Count; i++)
            this[i].Animate(previousTime, currentTime);
    }

    /// <summary>
    /// Returns the desired border-edge size given the available border-edge space.
    /// Margin, min/max clamping, and fixed-size short-circuit are handled by <see cref="MeasureShell"/>.
    /// </summary>
    protected virtual Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        Size size = (0, 0);
        for (var i = 0; i < this.Count; i++)
            size = Size.Max(size, this[i].Measure(availableBorderEdgeSize, context));
        return size;
    }

    /// <summary>
    /// Positions children within the border-edge rectangle.
    /// Frame assignment and alignment offset are handled by <see cref="ArrangeShell"/>.
    /// </summary>
    protected virtual void ArrangeCore(Rect rect, IMeasureContext context)
    {
        for (var i = 0; i < this.Count; i++)
            this[i].Arrange(rect, context);
    }

    /// <summary>
    /// Draws this view's content and recurses into children.
    /// </summary>
    protected virtual void RenderCore(IContext context)
    {
        for (var i = 0; i < this.Count; i++)
            this[i].Render(context);
    }
}
