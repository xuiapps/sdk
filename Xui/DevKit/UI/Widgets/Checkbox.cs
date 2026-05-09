using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A checkbox with a checkmark drawn via path commands.
/// Consumes design system tokens for colors, shape, and sizing.
/// </summary>
public class Checkbox : View
{
    private bool isChecked;
    private bool hover;
    private bool pressed;

    private Color checkedFillColor;
    private Color uncheckedFillColor;
    private Color checkmarkColor;
    private Color borderColor;
    private Color hoverBorderColor;
    private CornerRadius cornerRadius;
    private NFloat boxSize;

    /// <summary>Gets or sets whether the checkbox is checked.</summary>
    public bool IsChecked
    {
        get => isChecked;
        set { isChecked = value; InvalidateRender(); }
    }

    /// <summary>Invoked when the checked state changes.</summary>
    public Action<bool>? Changed { get; set; }

    /// <summary>Creates a checkbox with content-sized alignment.</summary>
    public Checkbox()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
    }

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    protected override void OnActivate()
    {
        base.OnActivate();
        ApplyDesignSystem();
    }

    private void ApplyDesignSystem()
    {
        var ds = this.GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (ds == null) return;

        checkedFillColor = ds.Colors.Primary.Background;
        uncheckedFillColor = ds.Colors.Surface.Background;
        checkmarkColor = ds.Colors.Primary.Foreground;
        borderColor = ds.Colors.Outline;
        hoverBorderColor = ds.Colors.Primary.Background;
        cornerRadius = ds.Shape.ExtraSmall;
        boxSize = ds.Spacing.Passive.XL;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        return new Size(boxSize, boxSize);
    }

    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();

        var boxRect = new Rect(Frame.X, Frame.Y, boxSize, boxSize);

        // Fill
        context.BeginPath();
        context.RoundRect(boxRect, cornerRadius);
        context.SetFill(isChecked ? checkedFillColor : uncheckedFillColor);
        context.Fill(FillRule.NonZero);

        // Border
        context.BeginPath();
        context.RoundRect(boxRect, cornerRadius);
        context.SetStroke(hover || pressed ? hoverBorderColor : (isChecked ? checkedFillColor : borderColor));
        context.LineWidth = isChecked ? 0 : 1.5f;
        context.Stroke();

        // Checkmark
        if (isChecked)
        {
            var cx = Frame.X;
            var cy = Frame.Y;
            var s = boxSize;

            context.BeginPath();
            context.MoveTo(new Point(cx + s * 0.22f, cy + s * 0.52f));
            context.LineTo(new Point(cx + s * 0.42f, cy + s * 0.72f));
            context.LineTo(new Point(cx + s * 0.78f, cy + s * 0.30f));
            context.SetStroke(checkmarkColor);
            context.LineWidth = s * 0.12f;
            context.Stroke();
        }
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.Type == PointerEventType.Enter) { hover = true; InvalidateRender(); }
        else if (e.Type == PointerEventType.Leave) { hover = false; InvalidateRender(); }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            CapturePointer(e.PointerId);
            pressed = true;
            InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Up)
        {
            ReleasePointer(e.PointerId);
            if (pressed && Frame.Contains(e.State.Position))
            {
                isChecked = !isChecked;
                Changed?.Invoke(isChecked);
            }
            pressed = false;
            InvalidateRender();
        }

        base.OnPointerEvent(ref e, phase);
    }
}
