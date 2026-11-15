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
public class Toggle : View, IDesignSystemChangeNotifications
{
    private bool isOn;
    private bool hover;
    private bool pressed;

    private Color trackOnColor => designSystem.Current?.Colors.Primary.Background ?? default;
    private Color trackOffColor => designSystem.Current?.Colors.Surface.Container ?? default;
    private Color thumbColor => designSystem.Current?.Colors.Surface.Background ?? default;
    private Color hoverTrackOnColor => designSystem.Current?.Colors.Primary.ToggleHoverBackground ?? default;
    private Color hoverTrackOffColor => designSystem.Current?.Colors.Outline ?? default;
    private NFloat trackWidth => trackHeight * 2;
    private NFloat trackHeight => designSystem.Current?.Spacing.Active.XL ?? 0;
    private NFloat thumbRadius => (trackHeight - 4) / 2;
    private readonly DesignSystemCache designSystem = new();

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

    /// <summary>Gets the number of child views, which is always zero.</summary>
    public override int Count => 0;

    /// <summary>Gets a child view by index.</summary>
    /// <exception cref="IndexOutOfRangeException">Always thrown because a toggle has no child views.</exception>
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

    /// <summary>Measures the toggle using its design-system-defined track dimensions.</summary>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        return new Size(trackWidth, trackHeight);
    }

    /// <summary>Renders the toggle track and thumb for its current state.</summary>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();

        var trackRect = new Rect(Frame.X, Frame.Y, trackWidth, trackHeight);
        var trackRadius = designSystem.Current?.Shape.Full ?? new CornerRadius(trackHeight / 2);

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
        context.SetStroke(isOn ? trackOnColor : designSystem.Current?.Colors.Outline ?? default);
        context.LineWidth = 1;
        context.Stroke();

        // Thumb
        var thumbX = isOn
            ? Frame.X + trackWidth - thumbRadius - 3
            : Frame.X + thumbRadius + 3;
        var thumbY = Frame.Y + trackHeight / 2;

        context.BeginPath();
        var outerRadius = CornerRadius.Min(trackRadius, new CornerRadius(trackHeight / 2));
        var innerRadius = CornerRadius.Max(0, outerRadius - new CornerRadius(3));
        context.RoundRect(new Rect(thumbX - thumbRadius, thumbY - thumbRadius, thumbRadius * 2, thumbRadius * 2), innerRadius);
        context.SetFill(thumbColor);
        context.Fill(FillRule.NonZero);
    }

    /// <summary>Updates pointer feedback and changes the toggle value on a completed tap.</summary>
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
                isOn = !isOn;
                Changed?.Invoke(isOn);
            }
            pressed = false;
            InvalidateRender();
        }

        base.OnPointerEvent(ref e, phase);
    }
}
