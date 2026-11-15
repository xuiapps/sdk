using Xui.Core.Abstract.Events;
using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Shared tap negotiation and moving selection for the demo's segmented controls.</summary>
internal abstract class DesignSystemSegments : View
{
    protected IDesignSystem? Design { get; private set; }
    protected abstract int OptionCount { get; }
    protected abstract int SelectedIndex { get; }
    protected abstract string Name(int index);
    protected abstract void Select(int index);
    protected abstract void DrawIcon(IContext context, Point center, int index, Color color);
    protected virtual bool Compact => false;
    protected virtual bool AnimateSelection => true;
    protected virtual NFloat SelectionPosition => position;
    private TextStyle TextStyle => (Compact ? Design?.Typography.Label.L : Design?.Typography.Label.S) ?? default;
    private NFloat Padding => Design?.Spacing.Active.S ?? 10;
    private NFloat LineHeight => TextStyle.LineHeight;
    private int TextLineCount
    {
        get
        {
            for (var i = 0; i < OptionCount; i++)
                if (Name(i).Contains('\n')) return 2;
            return 1;
        }
    }
    private int selected = -1, pressed = -1, pointer = -1;
    private NFloat position, from;
    private bool animating;
    private TimeSpan? started;

    protected override void OnActivate()
    {
        base.OnActivate();
        Design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (Design is not null) Design.Changed += RefreshSelection;
        RefreshSelection(DesignSystemChange.All);
    }

    protected override void OnDeactivate()
    {
        if (Design is not null) Design.Changed -= RefreshSelection;
        Design = null;
        selected = -1;
        animating = false;
        pressed = pointer = -1;
        base.OnDeactivate();
    }

    internal void RefreshSelection(DesignSystemChange changes = DesignSystemChange.None)
    {
        if ((changes & (DesignSystemChange.Typography | DesignSystemChange.Spacing)) != 0) Invalidate();
        var next = SelectedIndex;
        if (!AnimateSelection)
        {
            selected = next;
            animating = false;
            InvalidateRender();
            return;
        }
        if (next != selected)
        {
            from = position;
            animating = selected >= 0 && Design?.Motion.Standard.DefaultDuration > TimeSpan.Zero;
            selected = next;
            started = null;
            if (animating) RequestAnimationFrame();
            else position = next;
        }
        if (Design?.Motion.ReducedMotion == true || Design?.Motion.Standard.DefaultDuration == TimeSpan.Zero)
        {
            position = next;
            animating = false;
        }
        InvalidateRender();
    }

    protected override Size MeasureCore(Size available, IMeasureContext context) =>
        (available.Width, Compact ? NFloat.Max(44, LineHeight + 2 * Padding + 8) : 24 + 3 * Padding + TextLineCount * LineHeight + 8);

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        if (!animating) return;
        // Advance on the first requested frame as well. Retargeting starts at the
        // currently displayed position, so rapid taps never jump to an old endpoint.
        started ??= previous;
        var duration = Design!.Motion.Standard.DefaultDuration * 0.6; // 180 ms at normal speed.
        var t = duration <= TimeSpan.Zero ? 1 :
            NFloat.Clamp((NFloat)((current - started.Value) / duration), 0, 1);
        position = from + (selected - from) * Easing.EaseInOutCubic(t);
        animating = t < 1;
        InvalidateRender();
        if (animating) RequestAnimationFrame();
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        // Capture changes are delivered directly in the bubble phase.
        if (phase != EventPhase.Tunnel && e.Type != PointerEventType.LostCapture) return;
        if (e.Type == PointerEventType.Down && pointer < 0)
        {
            pressed = HitIndex(e.State.Position);
            if (pressed < 0) return;
            pointer = e.PointerId;
            CapturePointer(pointer, PointerGestures.Tap);
            InvalidateRender();
        }
        else if (e.PointerId == pointer && e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
        {
            var commit = e.Type == PointerEventType.Up && HitIndex(e.State.Position) == pressed;
            var index = pressed;
            pressed = pointer = -1;
            if (e.Type != PointerEventType.LostCapture) ReleasePointer(e.PointerId);
            if (commit) { Select(index); RefreshSelection(); }
            InvalidateRender();
        }
    }

    private int HitIndex(Point point) => Frame.Contains(point) && Frame.Width > 8
        ? Math.Clamp((int)((point.X - Frame.X - 4) / ((Frame.Width - 8) / OptionCount)), 0, OptionCount - 1) : -1;

    protected virtual bool DrawSurface => true;
    protected virtual Color SelectionColor => Design!.Colors.Primary.Background;
    protected virtual Color SelectedTextColor => Design!.Colors.Primary.Foreground;

    protected override void RenderCore(IContext context)
    {
        var ds = Design;
        if (ds is null) return;
        var width = (Frame.Width - 8) / OptionCount;
        // Inset corners share their centers: first clamp the outer geometry,
        // then subtract the physical gap instead of reusing the same token.
        var outer = Compact ? ds.Shape.Full : CornerRadius.Max(0, ds.Shape.Large - new CornerRadius(ds.Spacing.Passive.L));
        if (Compact && GetService(typeof(IDesignSystemEditor)) is IDesignSystemEditor { ShapePreset: ShapePreset.Soft })
            outer = new CornerRadius(9999);
        outer = CornerRadius.Min(outer, new CornerRadius(NFloat.Min(Frame.Width, Frame.Height) / 2));
        var inner = CornerRadius.Max(0, outer - new CornerRadius(4));
        if (DrawSurface) Fill(context, Frame, outer, ds.Colors.Surface.Container);
        var indicator = new Rect(Frame.X + 4 + SelectionPosition * width, Frame.Y + 4, width, Frame.Height - 8);
        Fill(context, indicator, inner, SelectionColor);
        var lines = Compact ? 1 : TextLineCount;
        for (var i = 0; i < OptionCount; i++)
        {
            var cell = new Rect(Frame.X + 4 + i * width, Frame.Y + 4, width, Frame.Height - 8);
            var color = i == selected ? SelectedTextColor : ds.Colors.Surface.Foreground;
            DrawIcon(context, Compact ? (cell.Center.X - 36, cell.Center.Y) : (cell.Center.X, cell.Top + Padding + 12), i, color);
            context.SetFill(color);
            var style = TextStyle;
            context.SetFont(new Font(style.FontSize, style.FontFamily, style.FontWeight, style.FontStyle));
            context.TextAlign = TextAlign.Center;
            context.TextBaseline = TextBaseline.Middle;
            var text = Name(i);
            var split = text.IndexOf('\n');
            if (split < 0)
                context.FillText(text, (cell.Center.X + (Compact ? 12 : 0), Compact ? cell.Center.Y : cell.Top + 24 + 2 * Padding + lines * LineHeight / 2));
            else
            {
                context.FillText(text.AsSpan(0, split), (cell.Center.X, cell.Top + 24 + 2 * Padding + LineHeight / 2));
                context.FillText(text.AsSpan(split + 1), (cell.Center.X, cell.Top + 24 + 2 * Padding + LineHeight * 1.5f));
            }
        }
    }

    private static void Fill(IContext context, Rect rect, CornerRadius radius, Color color)
    {
        context.BeginPath(); context.RoundRect(rect, radius); context.SetFill(color); context.Fill();
    }
}
