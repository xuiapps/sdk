using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A button that consumes design system tokens for colors, shape, and typography.
/// Supports <see cref="ButtonVariant"/> (Filled, Outline, Text) and hover/pressed states.
/// </summary>
public class Button : View
{
    private bool hover;
    private bool pressed;

    /// <summary>Creates a new button with content-sized alignment.</summary>
    public Button()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
    }

    private Color fillColor;
    private Color outlineColor;
    private Color textColor;
    private Color hoverFillColor;
    private Color pressedFillColor;
    private Color hoverTextBgColor;
    private Color pressedTextBgColor;
    private CornerRadius cornerRadius;
    private nfloat paddingH;
    private nfloat paddingV;
    private TextStyle textStyle;

    /// <summary>The button label text.</summary>
    public string Text { get; set; } = "";

    /// <summary>Which color group to use. Default is Primary.</summary>
    public ColorRole Role { get; set; } = ColorRole.Primary;

    /// <summary>Visual weight of the button. Default is Filled.</summary>
    public ButtonVariant Variant { get; set; } = ButtonVariant.Filled;

    /// <summary>Invoked when the button is clicked.</summary>
    public Action? Clicked { get; set; }

    /// <inheritdoc/>
    public override int Count => 0;

    /// <inheritdoc/>
    public override View this[int index] => throw new IndexOutOfRangeException();

    /// <inheritdoc/>
    protected override void OnActivate()
    {
        base.OnActivate();
        ApplyDesignSystem();
    }

    private ColorGroup ResolveGroup(IColorSystem colors) => Role switch
    {
        ColorRole.Secondary => colors.Secondary,
        ColorRole.Tertiary => colors.Tertiary,
        ColorRole.Warning => colors.Warning,
        ColorRole.Error => colors.Error,
        ColorRole.Neutral => colors.Neutral,
        _ => colors.Primary,
    };

    private void ApplyDesignSystem()
    {
        var ds = this.GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (ds == null) return;

        var group = ResolveGroup(ds.Colors);
        var isDark = ds.Colors.IsDark;

        // Filled variant colors
        fillColor = group.Background;
        outlineColor = group.Background;
        textColor = group.Foreground;
        // Compute hover/pressed relative to the base lightness
        var baseL = group.BackgroundLightness;
        var hoverOffset = isDark ? 0.08f : -0.06f;
        var pressedOffset = isDark ? 0.15f : -0.12f;
        hoverFillColor = group.Ramp[nfloat.Clamp(baseL + hoverOffset, 0, 1)];
        pressedFillColor = group.Ramp[nfloat.Clamp(baseL + pressedOffset, 0, 1)];
        hoverTextBgColor = group.Container;
        pressedTextBgColor = group.Ramp[nfloat.Clamp(baseL + (isDark ? -0.06f : 0.06f), 0, 1)];

        // Override text/outline colors based on variant
        if (Variant == ButtonVariant.Outline)
        {
            textColor = group.Background; // use the group's strong color for text
        }
        else if (Variant == ButtonVariant.Text)
        {
            textColor = group.Background;
        }

        cornerRadius = ds.Shape.Full;
        paddingH = ds.Spacing.Active.L;
        paddingV = ds.Spacing.Active.S;
        textStyle = ds.Typography.Label.L;
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        context.SetFont(new Font(
            textStyle.FontSize,
            [textStyle.FontFamily],
            textStyle.FontWeight,
            textStyle.FontStyle
        ));
        var textSize = context.MeasureText(Text).Size;
        return new Size(
            textSize.Width + paddingH * 2,
            textSize.Height + paddingV * 2
        );
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();

        switch (Variant)
        {
            case ButtonVariant.Filled:
                RenderFilled(context);
                break;
            case ButtonVariant.Outline:
                RenderOutline(context);
                break;
            case ButtonVariant.Text:
                RenderText(context);
                break;
        }

        RenderLabel(context);
    }

    private void RenderFilled(IContext context)
    {
        var fill = pressed ? pressedFillColor : hover ? hoverFillColor : fillColor;
        context.BeginPath();
        context.RoundRect(this.Frame, cornerRadius);
        context.SetFill(fill);
        context.Fill();
    }

    private void RenderOutline(IContext context)
    {
        if (pressed)
        {
            context.BeginPath();
            context.RoundRect(this.Frame, cornerRadius);
            context.SetFill(pressedTextBgColor);
            context.Fill();
        }
        else if (hover)
        {
            context.BeginPath();
            context.RoundRect(this.Frame, cornerRadius);
            context.SetFill(hoverTextBgColor);
            context.Fill();
        }

        // Always draw outline
        context.BeginPath();
        context.RoundRect(this.Frame, cornerRadius);
        context.SetStroke(outlineColor);
        context.LineWidth = 1;
        context.Stroke();
    }

    private void RenderText(IContext context)
    {
        // Only show background on hover or press
        if (pressed)
        {
            context.BeginPath();
            context.RoundRect(this.Frame, cornerRadius);
            context.SetFill(pressedTextBgColor);
            context.Fill();
        }
        else if (hover)
        {
            context.BeginPath();
            context.RoundRect(this.Frame, cornerRadius);
            context.SetFill(hoverTextBgColor);
            context.Fill();
        }
    }

    private void RenderLabel(IContext context)
    {
        context.SetFont(new Font(
            textStyle.FontSize,
            [textStyle.FontFamily],
            textStyle.FontWeight,
            textStyle.FontStyle
        ));
        context.TextBaseline = TextBaseline.Top;
        context.SetFill(textColor);

        var textMeasure = context.MeasureText(Text);
        var tx = this.Frame.X + (this.Frame.Width - textMeasure.Size.Width) / 2;
        var ty = this.Frame.Y + (this.Frame.Height - textMeasure.Size.Height) / 2;
        context.FillText(Text, new Point(tx, ty));
    }

    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.Type == PointerEventType.Enter)
        {
            hover = true;
            this.InvalidateRender();
        }
        else if (e.Type == PointerEventType.Leave)
        {
            hover = false;
            this.InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            this.CapturePointer(e.PointerId);
            pressed = true;
            this.InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Up)
        {
            this.ReleasePointer(e.PointerId);
            if (pressed && this.Frame.Contains(e.State.Position))
                Clicked?.Invoke();
            pressed = false;
            this.InvalidateRender();
        }
        else if (e.Type == PointerEventType.LostCapture)
        {
            // Another view (e.g. ScrollView) stole the pointer — cancel press.
            pressed = false;
            hover = false;
            this.InvalidateRender();
        }

        base.OnPointerEvent(ref e, phase);
    }
}
