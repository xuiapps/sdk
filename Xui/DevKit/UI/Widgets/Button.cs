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
public partial class Button : View, IDesignSystemChangeNotifications
{
    private bool hover;

    /// <summary>Creates a new button with content-sized alignment.</summary>
    public Button()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
    }

    private Color fillColor => group?.Background ?? default;
    private Color outlineColor => group?.Background ?? default;
    private Color textColor => Variant == ButtonVariant.Outline ? group?.Background ?? default : Variant == ButtonVariant.Text ? group?.OnContainer ?? default : group?.Foreground ?? default;
    private Color hoverFillColor => group?.HoverBackground ?? default;
    private Color pressedFillColor => group?.PressedBackground ?? default;
    private Color hoverTextBgColor => group?.Container ?? default;
    private Color pressedTextBgColor => group?.PressedContainer ?? default;
    private CornerRadius cornerRadius => designSystem.Current?.Shape.Full ?? default;
    private nfloat paddingH => designSystem.Current?.Spacing.Active.L ?? 0;
    private nfloat paddingV => designSystem.Current?.Spacing.Active.S ?? 0;
    private TextStyle textStyle => designSystem.Current?.Typography.Label.L ?? default;
    private string text = "";
    private readonly DesignSystemCache designSystem = new();

    /// <summary>The button label text.</summary>
    public string Text
    {
        get => text;
        set
        {
            if (text == value) return;
            text = value;
            this.Invalidate();
        }
    }

    /// <summary>Which color group to use. Default is Primary.</summary>
    public ColorRole Role { get; set; } = ColorRole.Primary;

    /// <summary>Visual weight of the button. Default is Filled.</summary>
    public ButtonVariant Variant { get; set; } = ButtonVariant.Filled;

    /// <summary>Horizontal alignment of the button label. Default is centered.</summary>
    public TextAlign LabelTextAlign { get; set; } = TextAlign.Center;

    /// <summary>Invoked when the button is clicked.</summary>
    public Action? Clicked { get; set; }

    /// <inheritdoc/>
    public override int Count => 0;

    /// <inheritdoc/>
    public override View this[int index] => throw new IndexOutOfRangeException();

    private ColorGroup? group => designSystem.Current is { } ds ? ResolveGroup(ds.Colors) : null;

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
        designSystem.Resolve(this, this);
    }

    /// <inheritdoc/>
    public void OnDesignTokenChange(DesignSystemChange changes)
    {
        if ((changes & (DesignSystemChange.Typography | DesignSystemChange.Spacing)) != 0)
            this.Invalidate();
        else if (changes != DesignSystemChange.None)
            this.InvalidateRender();
    }

    /// <inheritdoc/>
    protected override void OnDeactivate()
    {
        // Navigation may immediately attach this button under another page host.
        // Cancel live input, but let completed taps finish on their original clock.
        var pointer = pressedPointer;
        if (pointer != -1) { EndPress(false); ReleasePointer(pointer); }
        hover = false;
        designSystem.Deactivate(this);
        base.OnDeactivate();
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        context.SetFont(new Font(
            textStyle.FontSize,
            textStyle.FontFamily,
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

        RenderRipples(context);
        RenderLabel(context);
    }

    private void RenderFilled(IContext context)
    {
        var fill = Blend(hover ? hoverFillColor : fillColor, pressedFillColor, FeedbackOpacity);
        context.BeginPath();
        context.RoundRect(this.Frame, cornerRadius);
        context.SetFill(fill);
        context.Fill();
    }

    private void RenderOutline(IContext context)
    {
        RenderStateLayer(context);

        // Always draw outline
        context.BeginPath();
        context.RoundRect(this.Frame, cornerRadius);
        context.SetStroke(outlineColor);
        context.LineWidth = 1;
        context.Stroke();
    }

    private void RenderText(IContext context)
    {
        RenderStateLayer(context);
    }

    private void RenderStateLayer(IContext context)
    {
        var opacity = FeedbackOpacity;
        if (!hover && opacity <= 0) return;
        var color = hover ? Blend(hoverTextBgColor, pressedTextBgColor, opacity) : pressedTextBgColor;
        if (!hover) color.Alpha *= opacity;
        context.BeginPath(); context.RoundRect(Frame, cornerRadius);
        context.SetFill(color); context.Fill();
    }

    private void RenderLabel(IContext context)
    {
        context.SetFont(new Font(
            textStyle.FontSize,
            textStyle.FontFamily,
            textStyle.FontWeight,
            textStyle.FontStyle
        ));
        context.TextBaseline = TextBaseline.Top;
        context.SetFill(textColor);

        var textMeasure = context.MeasureText(Text);
        var tx = LabelTextAlign switch
        {
            TextAlign.Left or TextAlign.Start => this.Frame.X + paddingH,
            TextAlign.Right or TextAlign.End => this.Frame.Right - paddingH - textMeasure.Size.Width,
            _ => this.Frame.X + (this.Frame.Width - textMeasure.Size.Width) / 2,
        };
        var ty = this.Frame.Y + (this.Frame.Height - textMeasure.Size.Height) / 2;
        context.FillText(Text, new Point(tx, ty));
    }

    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.Type == PointerEventType.Enter && e.State.PointerType == PointerType.Mouse)
        {
            hover = true;
            InvalidateRender();
        }
        else if (e.Type == PointerEventType.Leave)
        {
            hover = false;
            InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down && pressedPointer == -1)
        {
            hover = e.State.PointerType == PointerType.Mouse && hover;
            var delay = false;
            if (e.State.PointerType == PointerType.Touch)
                foreach (var candidate in e.PotentialGestures)
                    if (candidate.Handler != this && candidate.Gesture is IDrag) { delay = true; break; }
            BeginPress(e.PointerId, e.State.Position, delay);
            CapturePointer(e.PointerId, PointerGestures.Tap);
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Up && pressedPointer == e.PointerId)
        {
            var tapped = HitTest(e.State.Position);
            EndPress(tapped);
            ReleasePointer(e.PointerId);
            if (tapped) Clicked?.Invoke();
        }
        else if (e.Type is PointerEventType.Cancel or PointerEventType.LostCapture && pressedPointer == e.PointerId)
        {
            EndPress(false);
            hover = false;
        }

        base.OnPointerEvent(ref e, phase);
    }
}
