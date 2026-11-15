using Xui.Core.Canvas;
using Xui.Core.Debug;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

public partial class View
{
    private Size lastMeasuredAvailableSize;
    private Size lastMeasuredDesiredSize;
    private IMeasureContext? lastMeasureContext;
    private LayoutSizeMode lastMeasuredXSize;
    private LayoutSizeMode lastMeasuredYSize;
    private bool hasLastMeasure;

    private Size lastArrangedAvailableSize;
    private Size lastArrangedDesiredSize;
    private Point lastArrangedAnchor;
    private LayoutSizeMode lastArrangedXSize;
    private LayoutSizeMode lastArrangedYSize;
    private LayoutAlign lastArrangedXAlign;
    private LayoutAlign lastArrangedYAlign;
    private IMeasureContext? lastArrangeContext;
    private bool hasLastArrange;
    private Rect? renderCullingFrame;

    /// <summary>The active render culling rectangle in this view's coordinate space, if any.</summary>
    protected Rect? RenderCullingFrame => this.renderCullingFrame;

    /// <summary>Drives the requested passes using shared frame state and per-view constraints.</summary>
    public virtual void Update(in LayoutFrameContext frame, in LayoutUpdate update, ref LayoutMeasurements measurements)
    {
        if (update.IsAnimate) this.AnimateShell(in frame);
        if (update.IsMeasure)
        {
            var measure = update.Measure;
            this.MeasureShell(in frame, in measure, ref measurements);
        }
        if (update.IsArrange)
        {
            var arrange = update.IsMeasure
                ? new ArrangeConstraints(
                    update.Arrange.AvailableSize,
                    measurements.DesiredSize,
                    update.Arrange.Anchor,
                    update.Arrange.XSize,
                    update.Arrange.YSize,
                    update.Arrange.XAlign,
                    update.Arrange.YAlign)
                : update.Arrange;
            this.ArrangeShell(in frame, in arrange, ref measurements);
        }
        if (update.IsRender) this.RenderShell(in frame);
    }

    /// <summary>Advances animation state for this view and all descendants.</summary>
    public void Animate(TimeSpan previousTime, TimeSpan currentTime)
    {
        var frame = new LayoutFrameContext(previousTime, currentTime, null, null, this.Instruments);
        this.AnimateShell(in frame);
    }

    /// <summary>Measures the view and returns the desired margin-box size.</summary>
    public Size Measure(Size availableSize, IMeasureContext context)
    {
        var frame = new LayoutFrameContext(TimeSpan.Zero, TimeSpan.Zero, context, null, this.Instruments);
        var constraints = new MeasureConstraints(availableSize);
        LayoutMeasurements measurements = default;
        this.MeasureShell(in frame, in constraints, ref measurements);
        return measurements.DesiredSize;
    }

    /// <summary>Arranges the view within <paramref name="rect"/>, finalising its position and size.</summary>
    public Rect Arrange(Rect rect, IMeasureContext context, Size? desiredSize = null)
    {
        var frame = new LayoutFrameContext(TimeSpan.Zero, TimeSpan.Zero, context, null, this.Instruments);
        var desired = desiredSize ?? this.Measure(rect.Size, context);
        var constraints = new ArrangeConstraints(rect.Size, desired, rect.TopLeft);
        LayoutMeasurements measurements = default;
        this.ArrangeShell(in frame, in constraints, ref measurements);
        return measurements.ArrangedRect;
    }

    /// <summary>Renders the view. Must be called after layout is complete.</summary>
    public void Render(IContext context)
    {
        var frame = new LayoutFrameContext(TimeSpan.Zero, TimeSpan.Zero, context, context, this.Instruments);
        this.RenderShell(in frame);
    }

    /// <summary>Renders this view using a culling rectangle expressed in its parent coordinates.</summary>
    internal void Render(IContext context, Rect cullingFrame)
    {
        var frame = new LayoutFrameContext(TimeSpan.Zero, TimeSpan.Zero, context, context, this.Instruments);
        this.RenderShell(in frame, cullingFrame);
    }

    /// <summary>Performs bookkeeping and animation for this view.</summary>
    protected void AnimateShell(in LayoutFrameContext frame)
    {
        this.Instruments.TrackView(Scope.ViewAnimation, this);
        this.ResetAnimationFlags();
        this.AnimateCore(frame.PreviousTime, frame.CurrentTime);
    }

    /// <summary>Measures this view using the supplied immutable constraints.</summary>
    protected void MeasureShell(in LayoutFrameContext frame, in MeasureConstraints constraints, ref LayoutMeasurements measurements)
    {
        if (this.CanReuseMeasure(in frame, in constraints))
        {
            measurements.DesiredSize = this.lastMeasuredDesiredSize;
            return;
        }

        this.Instruments.TrackView(Scope.ViewMeasure, this);
        Size availableMarginBoxSize = Size.Max((0, 0), constraints.AvailableSize);
        bool fixedWidth = constraints.XSize == LayoutSizeMode.Exact && nfloat.IsFinite(constraints.AvailableSize.Width) && this.HorizontalAlignment == HorizontalAlignment.Stretch;
        bool fixedHeight = constraints.YSize == LayoutSizeMode.Exact && nfloat.IsFinite(constraints.AvailableSize.Height) && this.VerticalAlignment == VerticalAlignment.Stretch;

        Size desiredBorderEdgeBoxSize;
        if (fixedWidth && fixedHeight)
        {
            desiredBorderEdgeBoxSize = availableMarginBoxSize - this.Margin;
        }
        else
        {
            desiredBorderEdgeBoxSize = this.MeasureCore(availableMarginBoxSize - this.Margin, frame.MeasureContext!);
            if (fixedWidth) desiredBorderEdgeBoxSize.Width = constraints.AvailableSize.Width;
            if (fixedHeight) desiredBorderEdgeBoxSize.Height = constraints.AvailableSize.Height;
        }

        desiredBorderEdgeBoxSize = (
            nfloat.Clamp(desiredBorderEdgeBoxSize.Width, this.MinimumWidth, this.MaximumWidth),
            nfloat.Clamp(desiredBorderEdgeBoxSize.Height, this.MinimumHeight, this.MaximumHeight));
        measurements.DesiredSize = desiredBorderEdgeBoxSize + this.Margin;

        this.lastMeasuredAvailableSize = constraints.AvailableSize;
        this.lastMeasuredDesiredSize = measurements.DesiredSize;
        this.lastMeasureContext = frame.MeasureContext;
        this.lastMeasuredXSize = constraints.XSize;
        this.lastMeasuredYSize = constraints.YSize;
        this.hasLastMeasure = true;
        this.ValidateMeasure();

        frame.Instruments.Log(Scope.ViewMeasure, LevelOfDetail.Info,
            $"Measure {this.GetType().Name} Available({constraints.AvailableSize.Width:F1}, {constraints.AvailableSize.Height:F1}) Margin({this.Margin.Left:F1}, {this.Margin.Top:F1}, {this.Margin.Right:F1}, {this.Margin.Bottom:F1}) -> Desired({measurements.DesiredSize.Width:F1}, {measurements.DesiredSize.Height:F1})");
    }

    private bool CanReuseMeasure(in LayoutFrameContext frame, in MeasureConstraints constraints)
        => this.hasLastMeasure && (this.Flags & ViewFlags.MeasureChanged) == 0 &&
           this.lastMeasuredAvailableSize == constraints.AvailableSize && this.lastMeasuredXSize == constraints.XSize &&
           this.lastMeasuredYSize == constraints.YSize && ReferenceEquals(this.lastMeasureContext, frame.MeasureContext);

    /// <summary>Arranges this view using the supplied immutable constraints.</summary>
    protected void ArrangeShell(in LayoutFrameContext frame, in ArrangeConstraints constraints, ref LayoutMeasurements measurements)
    {
        if (this.CanReuseArrange(in frame, in constraints))
        {
            measurements.ArrangedRect = this.Frame;
            return;
        }

        this.Instruments.TrackView(Scope.ViewArrange, this);
        nfloat x = constraints.Anchor.X - constraints.DesiredSize.Width * (int)constraints.XAlign * (nfloat).5;
        nfloat y = constraints.Anchor.Y - constraints.DesiredSize.Height * (int)constraints.YAlign * (nfloat).5;
        nfloat width = this.HorizontalAlignment == HorizontalAlignment.Stretch ? constraints.AvailableSize.Width : constraints.DesiredSize.Width;
        nfloat height = this.VerticalAlignment == VerticalAlignment.Stretch ? constraints.AvailableSize.Height : constraints.DesiredSize.Height;

        if (constraints.XSize == LayoutSizeMode.Exact && this.HorizontalAlignment != HorizontalAlignment.Stretch && nfloat.IsFinite(constraints.AvailableSize.Width))
        {
            nfloat alignment = ((int)this.HorizontalAlignment - 1 - (int)constraints.XAlign) * (nfloat).5;
            x += (constraints.AvailableSize.Width - constraints.DesiredSize.Width) * alignment;
        }
        if (constraints.YSize == LayoutSizeMode.Exact && this.VerticalAlignment != VerticalAlignment.Stretch && nfloat.IsFinite(constraints.AvailableSize.Height))
        {
            nfloat alignment = ((int)this.VerticalAlignment - 1 - (int)constraints.YAlign) * (nfloat).5;
            y += (constraints.AvailableSize.Height - constraints.DesiredSize.Height) * alignment;
        }

        measurements.ArrangedRect = new Rect(x, y, width, height) - this.Margin;
        this.Frame = measurements.ArrangedRect;
        frame.Instruments.Log(Scope.ViewArrange, LevelOfDetail.Info,
            $"Arrange {this.GetType().Name} Anchor({constraints.Anchor.X:F1}, {constraints.Anchor.Y:F1}) Desired({constraints.DesiredSize.Width:F1}, {constraints.DesiredSize.Height:F1}) -> Frame({this.Frame.X:F1}, {this.Frame.Y:F1}, {this.Frame.Width:F1}, {this.Frame.Height:F1})");
        this.ValidateArrange();
        this.ArrangeCore(measurements.ArrangedRect, frame.MeasureContext!);

        this.lastArrangedAvailableSize = constraints.AvailableSize;
        this.lastArrangedDesiredSize = constraints.DesiredSize;
        this.lastArrangedAnchor = constraints.Anchor;
        this.lastArrangedXSize = constraints.XSize;
        this.lastArrangedYSize = constraints.YSize;
        this.lastArrangedXAlign = constraints.XAlign;
        this.lastArrangedYAlign = constraints.YAlign;
        this.lastArrangeContext = frame.MeasureContext;
        this.hasLastArrange = true;
    }

    private bool CanReuseArrange(in LayoutFrameContext frame, in ArrangeConstraints constraints)
        => this.hasLastArrange && (this.Flags & (ViewFlags.ArrangeChanged | ViewFlags.DescendantArrangeChanged)) == 0 &&
           this.lastArrangedAvailableSize == constraints.AvailableSize && this.lastArrangedDesiredSize == constraints.DesiredSize &&
           this.lastArrangedAnchor == constraints.Anchor && this.lastArrangedXSize == constraints.XSize &&
           this.lastArrangedYSize == constraints.YSize && this.lastArrangedXAlign == constraints.XAlign &&
           this.lastArrangedYAlign == constraints.YAlign && ReferenceEquals(this.lastArrangeContext, frame.MeasureContext);

    /// <summary>Performs bookkeeping and renders this view.</summary>
    protected void RenderShell(
        in LayoutFrameContext frame,
        Rect? parentCullingFrame = null)
    {
        parentCullingFrame ??= this.Parent is null ? this.Frame : null;
        if (parentCullingFrame is { } cullingFrame && !this.CullingFrame.Intersects(cullingFrame))
            return;

        var previousCullingFrame = this.renderCullingFrame;
        this.renderCullingFrame = parentCullingFrame is { } frameCulling
            ? this.TransformRect(frameCulling)
            : null;

        this.Instruments.TrackView(Scope.ViewRendering, this);
        var context = frame.RenderContext!;
        context.Save();
        try
        {
            this.ValidateRender();
            this.SetDefaultRenderState(context);
            this.RenderCore(context);
        }
        finally
        {
            context.Restore();
            this.renderCullingFrame = previousCullingFrame;
        }
    }

    /// <summary>Override to animate this view and its children.</summary>
    protected virtual void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
    {
        for (int i = 0; i < this.Count; i++) this[i].Animate(previousTime, currentTime);
    }

    /// <summary>Override to compute the desired border-edge size.</summary>
    protected virtual Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        Size size = (0, 0);
        for (var i = 0; i < this.Count; i++) size = Size.Max(size, this[i].Measure(availableBorderEdgeSize, context));
        return size;
    }

    /// <summary>Override to arrange children within this view's final frame.</summary>
    protected virtual void ArrangeCore(Rect rect, IMeasureContext context)
    {
        for (var i = 0; i < this.Count; i++) this[i].Arrange(rect, context);
    }

    /// <summary>Override to render this view and its children.</summary>
    protected virtual void RenderCore(IContext context)
    {
        for (var i = 0; i < this.Count; i++)
            this.RenderChild(context, this[i]);
    }

    /// <summary>
    /// Applies the drawing defaults used at the start of every view render.
    /// The active transform and clip are intentionally inherited from the parent.
    /// </summary>
    protected virtual void SetDefaultRenderState(IContext context)
    {
        context.GlobalAlpha = 1;
        context.LineCap = LineCap.Butt;
        context.LineJoin = LineJoin.Miter;
        context.LineWidth = 1;
        context.MiterLimit = 10;
        context.LineDashOffset = 0;
        context.SetLineDash([]);
        context.SetStroke(Colors.Black);
        context.SetFill(Colors.Gray);
        context.SetFont(new Font(12, "Inter"));
        context.TextAlign = TextAlign.Start;
        context.TextBaseline = TextBaseline.Alphabetic;
        context.BeginPath();
    }

    /// <summary>Renders a child while preserving this view's active culling rectangle.</summary>
    protected internal void RenderChild(IContext context, View child)
    {
        if (this.renderCullingFrame is { } cullingFrame)
            child.Render(context, cullingFrame);
        else
            child.Render(context);
    }

    /// <summary>Renders a child using an explicit culling rectangle in this view's coordinate space.</summary>
    protected internal void RenderChild(IContext context, View child, Rect cullingFrame) =>
        child.Render(context, cullingFrame);
}
