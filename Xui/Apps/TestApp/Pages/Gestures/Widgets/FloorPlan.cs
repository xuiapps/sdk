using System.Globalization;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Gestures.Widgets;

/// <summary>A vector floor plan with independent pointer capture and anchored pinch zoom.</summary>
public sealed class FloorPlan : View
{
    private static readonly Rect[] Rooms = [new(0, 0, 210, 170), new(218, 0, 142, 120),
        new(218, 128, 142, 80), new(0, 178, 210, 122), new(218, 216, 142, 84)];
    private static readonly string[] Names = ["Living room", "Kitchen", "Bath", "Bedroom", "Entry"];
    private static readonly string[] Details = ["Living room · 24 m²", "Kitchen · 12 m²", "Bath · 6 m²", "Bedroom · 16 m²", "Entry · 5 m²"];
    private int first = -1, second = -1;
    private Point firstPosition, secondPosition, down;
    private bool moved;
    private Vector pan;
    private string zoomLabel = "100%";
    public NFloat Zoom { get; private set; } = 1;
    public Vector Pan => pan;
    public int SelectedRoom { get; private set; } = -1;
    public FloorPlan() => Id = "FloorPlan";
    private Rect MapRect => new(Frame.X, Frame.Y + 38, Frame.Width, NFloat.Max(1, Frame.Height - 94));
    private NFloat BaseScale => NFloat.Max(0.01f, NFloat.Min((MapRect.Width - 24) / 360, (MapRect.Height - 16) / 300));
    private static NFloat Distance(Point a, Point b)
    {
        var d = a - b; return NFloat.Sqrt(d.X * d.X + d.Y * d.Y);
    }
    private static Point Midpoint(Point a, Point b) => ((a.X + b.X) / 2, (a.Y + b.Y) / 2);

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;
        if (e.Type == PointerEventType.Down)
        {
            if (first == -1)
            { first = e.PointerId; firstPosition = down = e.State.Position; moved = false; }
            else if (second == -1)
            { second = e.PointerId; secondPosition = e.State.Position; moved = true; }
            else return;
            CapturePointer(e.PointerId, PointerGestures.Drag);
        }
        else if (e.PointerId == first || e.PointerId == second)
        {
            if (e.Type == PointerEventType.Move)
            {
                if (second != -1)
                {
                    var oldMid = Midpoint(firstPosition, secondPosition);
                    var oldDistance = Distance(firstPosition, secondPosition);
                    if (e.PointerId == first) firstPosition = e.State.Position; else secondPosition = e.State.Position;
                    var newMid = Midpoint(firstPosition, secondPosition);
                    var oldZoom = Zoom;
                    if (oldDistance > 1) Zoom = NFloat.Clamp(Zoom * Distance(firstPosition, secondPosition) / oldDistance, 1, 3);
                    // Keep the world point beneath the two-finger midpoint stationary.
                    pan = newMid - MapRect.Center - (oldMid - MapRect.Center - pan) * (Zoom / oldZoom);
                    zoomLabel = ((double)(Zoom * 100)).ToString("0", CultureInfo.InvariantCulture) + "%";
                }
                else
                {
                    pan += e.State.Position - firstPosition; firstPosition = e.State.Position;
                    moved |= Distance(down, firstPosition) > 6;
                }
                ClampPan(); InvalidateRender();
            }
            else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
            {
                if (e.Type == PointerEventType.Up && second == -1 && !moved)
                {
                    var local = (e.State.Position - MapRect.Center - pan) / (BaseScale * Zoom);
                    var world = new Point(local.X + 180, local.Y + 150);
                    SelectedRoom = Array.FindIndex(Rooms, r => r.Contains(world));
                }
                if (e.PointerId == first)
                { first = second; firstPosition = down = secondPosition; second = -1; }
                else second = -1;
                moved = true;
                if (e.Type != PointerEventType.LostCapture) ReleasePointer(e.PointerId);
                InvalidateRender();
            }
        }
    }
    private void ClampPan()
    {
        var scale = BaseScale * Zoom;
        var x = NFloat.Max(28, (360 * scale - MapRect.Width) / 2 + 20);
        var y = NFloat.Max(28, (300 * scale - MapRect.Height) / 2 + 20);
        pan = (NFloat.Clamp(pan.X, -x, x), NFloat.Clamp(pan.Y, -y, y));
    }
    protected override void OnDeactivate()
    {
        var a = first; var b = second; first = second = -1;
        if (a != -1) ReleasePointer(a);
        if (b != -1) ReleasePointer(b);
        base.OnDeactivate();
    }
    protected override void RenderCore(IContext ctx)
    {
        InputDrawing.Panel(ctx, Frame, new Color(0xEEF2F5FF));
        InputDrawing.Text(ctx, "THE COURTYARD / APT 04", (Frame.X + 16, Frame.Y + 22), 10, Muted, true);
        InputDrawing.Text(ctx, zoomLabel, (Frame.Right - 16, Frame.Y + 22), 10, Teal, true, TextAlign.Right);
        ctx.Save(); InputDrawing.Clip(ctx, MapRect); InputDrawing.Dots(ctx, MapRect);
        ctx.Translate(MapRect.Center + pan); ctx.Scale((BaseScale * Zoom, BaseScale * Zoom)); ctx.Translate((-180, -150));
        for (var i = 0; i < Rooms.Length; i++)
        {
            var color = i == SelectedRoom ? new Color(0xC9D6FAFF) : i switch { 1 => PaleTeal, 2 => PaleBlue, 3 => PaleRose, _ => Colors.White };
            InputDrawing.Panel(ctx, Rooms[i], color, 3);
            ctx.SetStroke(new Color(0x657185FF)); ctx.LineWidth = 3; ctx.BeginPath(); ctx.Rect(Rooms[i]); ctx.Stroke();
            var labelY = i == 0 ? 140 : i == 3 ? 282 : Rooms[i].Center.Y;
            InputDrawing.Text(ctx, Names[i], (Rooms[i].Center.X, labelY), 11, Ink, true, TextAlign.Center);
        }
        // Furniture and windows keep the plan readable at every zoom level.
        InputDrawing.Panel(ctx, new Rect(18, 18, 46, 84), PaleBlue, 5);
        InputDrawing.Panel(ctx, new Rect(24, 24, 34, 30), Colors.White, 4);
        InputDrawing.Panel(ctx, new Rect(24, 62, 34, 30), Colors.White, 4);
        InputDrawing.Panel(ctx, new Rect(86, 38, 62, 48), new Color(0xEEDCC5FF), 10);
        InputDrawing.Panel(ctx, new Rect(22, 193, 100, 68), Colors.White, 5);
        InputDrawing.Panel(ctx, new Rect(28, 216, 88, 39), new Color(0xE7BED1FF), 4);
        InputDrawing.Panel(ctx, new Rect(28, 198, 38, 14), PaleRose, 3);
        InputDrawing.Panel(ctx, new Rect(74, 198, 38, 14), PaleRose, 3);
        InputDrawing.Panel(ctx, new Rect(230, 10, 118, 24), new Color(0xAED2C2FF), 3);
        InputDrawing.Panel(ctx, new Rect(230, 140, 30, 50), Colors.White, 6);
        InputDrawing.Line(ctx, (22, 0), (148, 0), Blue, 5);
        InputDrawing.Line(ctx, (0, 190), (0, 265), Blue, 5);
        InputDrawing.Line(ctx, (360, 24), (360, 98), Blue, 5);
        ctx.Restore();
        InputDrawing.Panel(ctx, new Rect(Frame.X + 12, Frame.Bottom - 45, Frame.Width - 24, 33), Colors.White, 10);
        InputDrawing.Text(ctx, SelectedRoom < 0 ? "63 m² · Space for everyday life" : Details[SelectedRoom],
            (Frame.Center.X, Frame.Bottom - 28), 12, Ink, align: TextAlign.Center);
    }
}
