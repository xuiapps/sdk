using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A card container that uses design system tokens for background, outline, shape, and padding.
/// By default uses Surface colors. Set <see cref="Role"/> to use a specific color group.
/// </summary>
public class Card : View
{
    private View? content;

    private Color backgroundColor;
    private Color outlineColor;
    private CornerRadius cornerRadius;
    private nfloat padding;

    /// <summary>The content view inside the card.</summary>
    public View? Content
    {
        get => content;
        set => this.SetProtectedChild(ref content, value);
    }

    /// <summary>
    /// When null, the card uses Surface colors. When set, uses the specified color group's
    /// Container/OnContainer as background/foreground.
    /// </summary>
    public ColorRole? Role { get; set; }

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
    }

    private void ApplyDesignSystem()
    {
        var ds = this.GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (ds == null) return;

        if (Role.HasValue)
        {
            var group = Role.Value switch
            {
                ColorRole.Primary => ds.Colors.Primary,
                ColorRole.Secondary => ds.Colors.Secondary,
                ColorRole.Tertiary => ds.Colors.Tertiary,
                ColorRole.Warning => ds.Colors.Warning,
                ColorRole.Error => ds.Colors.Error,
                ColorRole.Neutral => ds.Colors.Neutral,
                _ => ds.Colors.Primary,
            };
            backgroundColor = group.Container;
            outlineColor = group.Background;
        }
        else
        {
            backgroundColor = ds.Colors.Surface.Background;
            outlineColor = ds.Colors.OutlineVariant;
        }

        cornerRadius = ds.Shape.Large;
        padding = ds.Spacing.Passive.L;
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        if (content == null)
            return new Size(padding * 2, padding * 2);

        var inner = new Size(
            available.Width - padding * 2,
            available.Height - padding * 2
        );
        var childSize = content.Measure(inner, context);
        return new Size(
            childSize.Width + padding * 2,
            childSize.Height + padding * 2
        );
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        if (content == null) return;

        var inner = new Rect(
            this.Frame.X + padding,
            this.Frame.Y + padding,
            this.Frame.Width - padding * 2,
            this.Frame.Height - padding * 2
        );
        content.Arrange(inner, context);
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();
        // Fill
        context.BeginPath();
        context.RoundRect(this.Frame, cornerRadius);
        context.SetFill(backgroundColor);
        context.Fill();

        // Outline
        context.BeginPath();
        context.RoundRect(this.Frame, cornerRadius);
        context.SetStroke(outlineColor);
        context.LineWidth = 1;
        context.Stroke();

        if (content != null)
            content.Render(context);
    }
}
