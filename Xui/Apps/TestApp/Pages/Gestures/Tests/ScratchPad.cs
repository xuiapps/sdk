using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Apps.TestApp.Pages.Gestures.Widgets;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;

namespace Xui.Apps.TestApp.Pages.Gestures.Tests;

/// <summary>A bounded sketch surface whose firm drag keeps its parent feed still.</summary>
public class ScratchPad : View
{
    private readonly List<Point> points = new(512);
    private int pointer = -1;
    protected override Size MeasureCore(Size available, IMeasureContext context) => (available.Width, 156);
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;
        if (e.Type == PointerEventType.Down && pointer == -1)
        {
            pointer = e.PointerId; points.Clear();
            points.Add(new Point(e.State.Position.X - Frame.X, e.State.Position.Y - Frame.Y));
            CapturePointer(pointer, PointerGestures.Drag); InvalidateRender();
        }
        else if (e.PointerId == pointer)
        {
            if (e.Type == PointerEventType.Move && points.Count < 512)
            {
                points.Add(new Point(e.State.Position.X - Frame.X, e.State.Position.Y - Frame.Y)); InvalidateRender();
            }
            else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
            {
                var id = pointer; pointer = -1;
                if (e.Type != PointerEventType.LostCapture) ReleasePointer(id);
            }
        }
    }
    protected override void OnDeactivate()
    {
        var id = pointer; pointer = -1;
        if (id != -1) ReleasePointer(id);
        base.OnDeactivate();
    }
    protected override void RenderCore(IContext ctx)
    {
        InputDrawing.Panel(ctx, Frame, PaleBlue); InputDrawing.Clip(ctx, Frame); InputDrawing.Dots(ctx, Frame);
        if (points.Count == 0)
        {
            InputDrawing.Text(ctx, "Your next big idea starts here.", Frame.Center, 12, Blue, align: TextAlign.Center);
            InputDrawing.Text(ctx, "DRAW WITH YOUR FINGER", Frame.Center + new Vector(0, 23), 9, Muted, align: TextAlign.Center);
        }
        ctx.Translate(Frame.TopLeft); ctx.SetStroke(Blue); ctx.LineWidth = 3;
        ctx.LineCap = LineCap.Round; ctx.LineJoin = LineJoin.Round;
        if (points.Count > 1)
        {
            ctx.BeginPath(); ctx.MoveTo(points[0]);
            for (var i = 1; i < points.Count; i++) ctx.LineTo(points[i]);
            ctx.Stroke();
        }
    }
}
