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
public class RadioButton : View
{
    private bool isSelected;
    private bool hover;
    private bool pressed;

    private Color selectedFillColor;
    private Color unselectedBorderColor;
    private Color dotColor;
    private Color hoverBorderColor;
    private NFloat outerRadius;

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

        selectedFillColor = ds.Colors.Primary.Background;
        unselectedBorderColor = ds.Colors.Outline;
        dotColor = ds.Colors.Primary.Foreground;
        hoverBorderColor = ds.Colors.Primary.Background;
        outerRadius = ds.Spacing.Passive.XL / 2;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        var size = outerRadius * 2;
        return new Size(size, size);
    }

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
