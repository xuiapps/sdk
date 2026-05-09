using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A toggle switch that slides a thumb circle between on/off positions.
/// Consumes design system tokens for colors, shape, and sizing.
/// </summary>
public class Toggle : View
{
    private bool isOn;
    private bool hover;
    private bool pressed;

    private Color trackOnColor;
    private Color trackOffColor;
    private Color thumbColor;
    private Color hoverTrackOnColor;
    private Color hoverTrackOffColor;
    private NFloat trackWidth;
    private NFloat trackHeight;
    private NFloat thumbRadius;

    /// <summary>Gets or sets whether the toggle is on.</summary>
    public bool IsOn
    {
        get => isOn;
        set { isOn = value; InvalidateRender(); }
    }

    /// <summary>Invoked when the toggle value changes.</summary>
    public Action<bool>? Changed { get; set; }

    /// <summary>Creates a toggle with content-sized alignment.</summary>
    public Toggle()
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

        trackOnColor = ds.Colors.Primary.Background;
        trackOffColor = ds.Colors.Surface.Container;
        thumbColor = ds.Colors.Surface.Background;
        hoverTrackOnColor = ds.Colors.Primary.Ramp[ds.Colors.IsDark ? 0.85f : 0.48f];
        hoverTrackOffColor = ds.Colors.Outline;

        trackWidth = ds.Spacing.Passive.XXXL;
        trackHeight = ds.Spacing.Passive.XL;
        thumbRadius = (trackHeight - 4) / 2;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        return new Size(trackWidth, trackHeight);
    }

    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();
        var twoPi = (NFloat)(2 * Math.PI);

        var trackRect = new Rect(Frame.X, Frame.Y, trackWidth, trackHeight);
        var trackRadius = trackHeight / 2;

        // Track
        Color trackColor;
        if (hover || pressed)
            trackColor = isOn ? hoverTrackOnColor : hoverTrackOffColor;
        else
            trackColor = isOn ? trackOnColor : trackOffColor;

        context.BeginPath();
        context.RoundRect(trackRect, trackRadius);
        context.SetFill(trackColor);
        context.Fill(FillRule.NonZero);

        // Outline
        context.BeginPath();
        context.RoundRect(trackRect, trackRadius);
        context.SetStroke(isOn ? trackOnColor : new Color(0x99999966));
        context.LineWidth = 1;
        context.Stroke();

        // Thumb
        var thumbX = isOn
            ? Frame.X + trackWidth - thumbRadius - 3
            : Frame.X + thumbRadius + 3;
        var thumbY = Frame.Y + trackHeight / 2;

        context.BeginPath();
        context.Arc(new Point(thumbX, thumbY), thumbRadius, 0, twoPi);
        context.SetFill(thumbColor);
        context.Fill(FillRule.NonZero);
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
                isOn = !isOn;
                Changed?.Invoke(isOn);
            }
            pressed = false;
            InvalidateRender();
        }

        base.OnPointerEvent(ref e, phase);
    }
}
