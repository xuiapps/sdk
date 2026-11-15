using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A radio button (circle with filled dot when selected).
/// Consumes design system tokens for colors and sizing.
/// </summary>
public class RadioButton : View, IDesignSystemChangeNotifications
{
    private bool isSelected;
    private bool hover;
    private bool pressed;

    private Color selectedFillColor => designSystem.Current?.Colors.Primary.Background ?? default;
    private Color unselectedBorderColor => designSystem.Current?.Colors.Outline ?? default;
    private Color dotColor => designSystem.Current?.Colors.Primary.Foreground ?? default;
    private Color hoverBorderColor => designSystem.Current?.Colors.Primary.Background ?? default;
    private NFloat outerRadius => (designSystem.Current?.Spacing.Active.XL ?? 0) / 2;
    private readonly DesignSystemCache designSystem = new();

    /// <summary>Gets or sets whether the radio button is selected.</summary>
    public bool IsSelected
    {
        get => isSelected;
        set { isSelected = value; InvalidateRender(); }
    }

    /// <summary>Invoked when the selection state changes.</summary>
    public Action<bool>? Changed { get; set; }

    /// <summary>Creates a radio button with content-sized alignment.</summary>
    public RadioButton()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
    }

    /// <summary>Gets the number of child views, which is always zero.</summary>
    public override int Count => 0;

    /// <summary>Gets a child view by index.</summary>
    /// <exception cref="IndexOutOfRangeException">Always thrown because a radio button has no child views.</exception>
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

    /// <summary>Measures the radio button using its design-system-defined diameter.</summary>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        var size = outerRadius * 2;
        return new Size(size, size);
    }

    /// <summary>Renders the selected or unselected radio-button indicator.</summary>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();
        var twoPi = (NFloat)(2 * Math.PI);
        var center = new Point(Frame.X + outerRadius, Frame.Y + outerRadius);

        if (isSelected)
        {
            // Filled outer circle
            context.BeginPath();
            context.Arc(center, outerRadius, 0, twoPi);
            context.SetFill(selectedFillColor);
            context.Fill(FillRule.NonZero);

            // Inner dot
            context.BeginPath();
            context.Arc(center, outerRadius * 0.4f, 0, twoPi);
            context.SetFill(dotColor);
            context.Fill(FillRule.NonZero);
        }
        else
        {
            // Empty circle with border
            context.BeginPath();
            context.Arc(center, outerRadius - 1, 0, twoPi);
            context.SetStroke(hover || pressed ? hoverBorderColor : unselectedBorderColor);
            context.LineWidth = 1.5f;
            context.Stroke();
        }
    }

    /// <summary>Updates pointer feedback and selects the button on a completed tap.</summary>
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
                if (!isSelected)
                {
                    isSelected = true;
                    Changed?.Invoke(true);
                }
            }
            pressed = false;
            InvalidateRender();
        }

        base.OnPointerEvent(ref e, phase);
    }
}
