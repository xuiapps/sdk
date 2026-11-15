using Xui.Core.Abstract.Events;
using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

/// <summary>One immediate-mode view: no furniture views, textures, or retained native groups.</summary>
public sealed class FloorplanView : View
{
    public FloorplanDrawing Drawing { get; }
    private Size viewport;
    private Vector pan;
    private NFloat scale;
    private NFloat zoomMultiplier = 1;
    private int first = -1, second = -1;
    private Point firstPosition, secondPosition;
    private Point lastPointerPosition;
    private bool hasPointerPosition;
    private NFloat markerOpacity = 1, markerFrom = 1, markerTarget = 1;
    private NFloat fadeElapsed;

    public FloorplanView() : this(FloorplanDrawing.Shared) { }
    public FloorplanView(FloorplanDrawing drawing) { Drawing = drawing; Id = "Floorplan.Map"; }
    public NFloat ZoomPercent => viewport.Width > 0 ? scale * 1000 / viewport.Width * 100 : 0;
    private NFloat DetailZoom => ZoomPercent / zoomMultiplier;
    public int DetailLevel => DetailZoom <= 125 ? 0 : DetailZoom < 220 ? 1 : DetailZoom < 400 ? 2 : 3;
    public NFloat MarkerOpacity => markerOpacity;
    public Rect VisibleWorld => new(-pan.X / scale, 120 - pan.Y / scale, viewport.Width / scale, viewport.Height / scale);
    private NFloat MinScale => (NFloat).8 * NFloat.Min(viewport.Width / 1000, viewport.Height / 760);
    private NFloat MaxScale => NFloat.Max(MinScale, 5 * zoomMultiplier * viewport.Width / 1000);

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var old = viewport;
        var multiplier = GetService(typeof(IDeviceInfo)) is IDeviceInfo { FormFactor: DeviceFormFactor.Mobile } ? (NFloat)2 : 1;
        if (old.Width == rect.Width && old.Height == rect.Height && zoomMultiplier == multiplier)
            return;
        zoomMultiplier = multiplier;
        viewport = (rect.Width, rect.Height);
        if (old.Width <= 0 || old.Height <= 0 || scale <= 0)
            Fit();
        else
        {
            var center = (new Vector(old.Width / 2, old.Height / 2) - pan) / scale;
            scale = NFloat.Clamp(scale * viewport.Width / old.Width, MinScale, MaxScale);
            pan = new Vector(viewport.Width / 2, viewport.Height / 2) - center * scale;
            Changed();
        }
    }

    public void Fit()
    {
        if (viewport.Width <= 0 || viewport.Height <= 0)
            return;
        scale = MinScale;
        pan = ((viewport.Width - 1000 * scale) / 2, (viewport.Height - 760 * scale) / 2);
        Changed();
    }

    /// <summary>Zoom about a point in the same parent coordinates as Frame.</summary>
    public void ZoomAt(Point point, NFloat factor)
    {
        if (scale <= 0 || factor <= 0 || !NFloat.IsFinite(factor))
            return;
        var anchor = point - new Point(Frame.X, Frame.Y);
        var next = NFloat.Clamp(scale * factor, MinScale, MaxScale);
        pan = anchor - (anchor - pan) * (next / scale);
        scale = next;
        Changed();
    }

    private void Changed()
    {
        pan = (ClampAxis(pan.X, 1000 * scale, viewport.Width), ClampAxis(pan.Y, 760 * scale, viewport.Height));
        var target = DetailZoom <= 160 ? (NFloat)1 : 0;
        if (target != markerTarget)
        {
            markerFrom = markerOpacity;
            markerTarget = target;
            fadeElapsed = 0;
            RequestAnimationFrame();
        }
        InvalidateRender();
    }

    private static NFloat ClampAxis(NFloat offset, NFloat extent, NFloat available) => extent <= available
        ? (available - extent) / 2
        : NFloat.Clamp(offset, available - extent - available * (NFloat).1, available * (NFloat).1);

    public override void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        // Wheel events retain root coordinates; pointer dispatch already maps into this view.
        var anchor = e.HasPosition ? GlobalToLocal(e.Position) : hasPointerPosition ? lastPointerPosition : Frame.Center;
        ZoomAt(anchor, NFloat.Exp(NFloat.Clamp(e.Delta.Y * (NFloat).002, -20, 20)));
        e.Handled = true;
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble)
            return;
        if (e.Type is PointerEventType.Move or PointerEventType.Enter or PointerEventType.Down)
        { lastPointerPosition = e.State.Position; hasPointerPosition = true; }
        if (e.Type == PointerEventType.Down)
        {
            if (e.PointerId == first)
                firstPosition = e.State.Position;
            else if (e.PointerId == second)
                secondPosition = e.State.Position;
            else if (first == -1)
            { first = e.PointerId; firstPosition = e.State.Position; }
            else if (second == -1)
            { second = e.PointerId; secondPosition = e.State.Position; }
            else
                return;
            CapturePointer(e.PointerId, PointerGestures.Drag);
        }
        else if (e.PointerId == first || e.PointerId == second)
        {
            if (e.Type == PointerEventType.Move)
            {
                if (second != -1)
                {
                    var oldCenter = Midpoint(firstPosition, secondPosition);
                    var distance = Distance(firstPosition, secondPosition);
                    if (e.PointerId == first)
                        firstPosition = e.State.Position;
                    else
                        secondPosition = e.State.Position;
                    var newCenter = Midpoint(firstPosition, secondPosition);
                    var next = distance > 1 ? NFloat.Clamp(scale * Distance(firstPosition, secondPosition) / distance, MinScale, MaxScale) : scale;
                    var origin = new Point(Frame.X, Frame.Y);
                    pan = newCenter - origin - (oldCenter - origin - pan) * (next / scale);
                    scale = next;
                }
                else
                { pan += e.State.Position - firstPosition; firstPosition = e.State.Position; }
                Changed();
            }
            else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
            {
                // Remove ownership before ReleasePointer, which may synchronously deliver LostCapture.
                if (e.PointerId == first)
                { first = second; firstPosition = secondPosition; second = -1; }
                else
                    second = -1;
                if (e.Type != PointerEventType.LostCapture)
                    ReleasePointer(e.PointerId);
            }
        }
    }

    protected override void OnDeactivate()
    {
        int a = first, b = second;
        first = second = -1;
        if (a != -1)
            ReleasePointer(a);
        if (b != -1)
            ReleasePointer(b);
        base.OnDeactivate();
    }

    protected override void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
    {
        if (markerOpacity == markerTarget)
            return;
        fadeElapsed += (NFloat)NFloat.Max(0, (NFloat)(currentTime - previousTime).TotalSeconds);
        var t = NFloat.Min(1, fadeElapsed / (NFloat).22);
        markerOpacity = t == 1 ? markerTarget : markerFrom + (markerTarget - markerFrom) * Easing.EaseInOutCubic(t);
        InvalidateRender();
        if (t < 1)
            RequestAnimationFrame();
    }

    protected override void RenderCore(IContext context)
    {
        if (scale <= 0)
            return;
        context.Save();
        try
        {
            context.BeginPath();
            context.Rect(Frame);
            context.Clip();
            context.Translate((Frame.X + pan.X, Frame.Y + pan.Y));
            context.Scale((scale, scale));
            context.Translate((0, -120));
            Drawing.Render(context, DetailLevel, VisibleWorld, markerOpacity, DetailLevel >= 2);
        }
        finally { context.Restore(); }
    }

    private static Point Midpoint(Point a, Point b) => ((a.X + b.X) / 2, (a.Y + b.Y) / 2);
    private static NFloat Distance(Point a, Point b) { var d = a - b; return NFloat.Sqrt(d.X * d.X + d.Y * d.Y); }
}
