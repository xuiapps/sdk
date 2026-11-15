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
public class Checkbox : View, IDesignSystemChangeNotifications
{
    private bool isChecked;
    private bool hover;
    private bool pressed;

    private Color checkedFillColor => designSystem.Current?.Colors.Primary.Background ?? default;
    private Color uncheckedFillColor => designSystem.Current?.Colors.Surface.Background ?? default;
    private Color checkmarkColor => designSystem.Current?.Colors.Primary.Foreground ?? default;
    private Color borderColor => designSystem.Current?.Colors.Outline ?? default;
    private Color hoverBorderColor => designSystem.Current?.Colors.Primary.Background ?? default;
    private CornerRadius cornerRadius => designSystem.Current?.Shape.ExtraSmall ?? default;
    private NFloat boxSize => designSystem.Current?.Spacing.Active.XL ?? 0;
    private readonly DesignSystemCache designSystem = new();

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

    /// <summary>Gets the number of child views, which is always zero.</summary>
    public override int Count => 0;

    /// <summary>Gets a child view by index.</summary>
    /// <exception cref="IndexOutOfRangeException">Always thrown because a checkbox has no child views.</exception>
    public override View this[int index] => throw new IndexOutOfRangeException();

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
        designSystem.Deactivate(this);
        base.OnDeactivate();
    }

    /// <summary>Measures the checkbox using its design-system-defined square size.</summary>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        return new Size(boxSize, boxSize);
    }

    /// <summary>Renders the checkbox box, border, and optional checkmark.</summary>
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

    /// <summary>Updates pointer feedback and toggles the checked state on a completed tap.</summary>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.Type == PointerEventType.Enter) { hover = true; InvalidateRender(); }
        else if (e.Type == PointerEventType.Leave) { hover = false; InvalidateRender(); }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            CapturePointer(e.PointerId, PointerGestures.Tap);
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
