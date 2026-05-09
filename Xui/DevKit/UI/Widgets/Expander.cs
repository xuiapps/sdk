using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A card-like container with a clickable title that expands/collapses its content
/// with an animated transition. Consumes motion tokens from the design system.
/// </summary>
public class Expander : View
{
    private View? content;
    private bool isExpanded;
    private nfloat animProgress = 0; // 0 = collapsed, 1 = expanded
    private bool animating;
    private TimeSpan? animStartTime;
    private nfloat animFrom;
    private nfloat animTo;

    // Cached design tokens
    private Color backgroundColor;
    private Color outlineColor;
    private Color titleColor;
    private Color glyphColor;
    private CornerRadius cornerRadius;
    private nfloat padding;
    private nfloat titleHeight;
    private TextStyle titleStyle;
    private CurveToken curve;

    /// <summary>The title text shown in the header.</summary>
    public string Title { get; set; } = "";

    /// <summary>Gets or sets whether the expander is expanded.</summary>
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value) return;
            isExpanded = value;
            StartAnimation(value ? 1 : 0);
            // Force immediate layout update
            this.InvalidateMeasure();
            this.InvalidateRender();
        }
    }

    /// <summary>The content view shown when expanded.</summary>
    public View? Content
    {
        get => content;
        set => this.SetProtectedChild(ref content, value);
    }

    /// <inheritdoc/>
    public override int Count => content is null ? 0 : 1;

    /// <inheritdoc/>
    public override View this[int index] =>
        index == 0 && content is not null ? content : throw new IndexOutOfRangeException();

    /// <inheritdoc/>
    protected override void OnActivate()
    {
        base.OnActivate();
        ApplyDesignSystem();
        animProgress = isExpanded ? 1 : 0;
    }

    private void ApplyDesignSystem()
    {
        var ds = this.GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (ds == null) return;

        backgroundColor = ds.Colors.Surface.Background;
        outlineColor = ds.Colors.OutlineVariant;
        titleColor = ds.Colors.Surface.Foreground;
        glyphColor = ds.Colors.Outline;
        cornerRadius = ds.Shape.Large;
        padding = ds.Spacing.Passive.L;
        titleHeight = ds.Spacing.Passive.XXL;
        titleStyle = ds.Typography.Label.L;
        curve = ds.Motion.EmphasizedDecelerate; // 400ms, smooth settle
    }

    private void StartAnimation(nfloat target)
    {
        // Snap instantly if duration is zero (ReducedMotion / MotionPreset.None)
        if (curve.DefaultDuration <= TimeSpan.Zero)
        {
            animProgress = target;
            animating = false;
            this.InvalidateMeasure();
            this.InvalidateRender();
            return;
        }

        animFrom = animProgress;
        animTo = target;
        animating = true;
        animStartTime = null;
        this.RequestAnimationFrame();
        this.InvalidateMeasure();
        this.InvalidateRender();
    }

    /// <inheritdoc/>
    protected override void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
    {
        if (animating)
        {
            if (!animStartTime.HasValue)
                animStartTime = currentTime;

            var elapsed = currentTime - animStartTime.Value;
            var duration = curve.DefaultDuration;
            var t = nfloat.Min(1, (nfloat)(elapsed.TotalMilliseconds / duration.TotalMilliseconds));
            var eased = curve.Curve.Evaluate(t);

            animProgress = animFrom + (animTo - animFrom) * eased;

            if (t >= 1)
            {
                animProgress = animTo;
                animating = false;
            }
            else
            {
                this.RequestAnimationFrame();
            }

            this.InvalidateMeasure();
            this.InvalidateRender();
        }

        base.AnimateCore(previousTime, currentTime);
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();

        var headerH = titleHeight + padding;
        var contentH = (nfloat)0;

        if (content != null)
        {
            var innerWidth = available.Width - padding * 2;
            var contentSize = content.Measure(new Size(innerWidth, nfloat.PositiveInfinity), context);
            contentH = contentSize.Height + padding;
        }

        var totalH = headerH + contentH * animProgress + padding;
        return new Size(available.Width, totalH);
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        if (content == null) return;

        var innerX = this.Frame.X + padding;
        var innerY = this.Frame.Y + titleHeight + padding;
        var innerW = this.Frame.Width - padding * 2;

        var contentSize = content.Measure(new Size(innerW, nfloat.PositiveInfinity), context);
        content.Arrange(new Rect(innerX, innerY, innerW, contentSize.Height), context);
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();

        // Card background
        context.BeginPath();
        context.RoundRect(this.Frame, cornerRadius);
        context.SetFill(backgroundColor);
        context.Fill(FillRule.NonZero);

        // Outline
        context.BeginPath();
        context.RoundRect(this.Frame, cornerRadius);
        context.SetStroke(outlineColor);
        context.LineWidth = 1;
        context.Stroke();

        // Chevron glyph (rotates with animation)
        var chevronX = this.Frame.X + padding + 4;
        var chevronY = this.Frame.Y + padding + titleHeight / 2;
        var chevronSize = (nfloat)5;
        var angle = animProgress * (nfloat)(Math.PI / 2); // 0 = right, 90° = down

        context.Save();
        context.Translate(new Vector(chevronX, chevronY));
        context.Rotate(angle);

        context.BeginPath();
        context.MoveTo(new Point(-chevronSize * 0.3f, -chevronSize));
        context.LineTo(new Point(chevronSize * 0.7f, 0));
        context.LineTo(new Point(-chevronSize * 0.3f, chevronSize));
        context.SetStroke(glyphColor);
        context.LineWidth = 1.5f;
        context.Stroke();

        context.Restore();

        // Title text
        context.SetFont(new Font(
            titleStyle.FontSize,
            [titleStyle.FontFamily],
            titleStyle.FontWeight,
            titleStyle.FontStyle
        ));
        context.TextBaseline = TextBaseline.Top;
        context.SetFill(titleColor);
        var titleX = this.Frame.X + padding + 22; // offset past chevron
        var titleY = this.Frame.Y + padding + (titleHeight - titleStyle.FontSize) / 2;
        context.FillText(Title, new Point(titleX, titleY));

        // Content (clipped to visible area)
        if (content != null && animProgress > 0.01f)
        {
            context.Save();
            context.BeginPath();
            var clipY = this.Frame.Y + titleHeight + padding;
            var clipH = this.Frame.Height - titleHeight - padding * 2;
            context.Rect(new Rect(this.Frame.X, clipY, this.Frame.Width, nfloat.Max(0, clipH)));
            context.Clip();

            content.Render(context);

            context.Restore();
        }
    }

    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            // Only toggle if clicking the header area
            var localY = e.State.Position.Y - this.Frame.Y;
            if (localY <= titleHeight + padding)
            {
                IsExpanded = !IsExpanded;
            }
        }

        base.OnPointerEvent(ref e, phase);
    }
}
