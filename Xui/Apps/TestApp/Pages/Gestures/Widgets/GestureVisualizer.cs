using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Gestures.Widgets;

/// <summary>Bounded touch particles with independent pointer capture.</summary>
public sealed class GestureVisualizer : View
{
    private struct Bubble
    {
        public bool Visible, Held;
        public int Pointer;
        public Point Position;
        public NFloat Age;
        public string Device, Phase;
        public Color Color;
    }
    private readonly Bubble[] bubbles = new Bubble[64];
    private static readonly Color[] Palette = [new(0x5266DEFF), new(0x208B80FF), new(0xBD5287FF), new(0xC67B36FF)];
    private int next;
    private TimeSpan? previousFrame;
    public int VisibleBubbleCount
    {
        get { var count = 0; foreach (ref var bubble in bubbles.AsSpan()) if (bubble.Visible) count++; return count; }
    }

    public GestureVisualizer() => Id = "GestureVisualizer";

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;
        if (e.Type == PointerEventType.Down)
        {
            var slot = -1;
            // Never evict a held contact, even when the fading particle pool fills up.
            for (var i = 0; i < bubbles.Length; i++)
            {
                var candidate = (next + i) % bubbles.Length;
                if (!bubbles[candidate].Held) { slot = candidate; break; }
            }
            if (slot < 0) return;
            bubbles[slot] = new Bubble { Visible = true, Held = true, Pointer = e.PointerId,
                Position = (Point)(e.State.Position - Frame.TopLeft), Age = -1, Phase = "DOWN",
                Device = (e.State.PointerType switch { PointerType.Touch => "TOUCH", PointerType.Pen => "PEN", PointerType.Mouse => "MOUSE", _ => "POINTER" })
                    + " #" + e.PointerId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Color = Palette[next % Palette.Length] };
            next = (slot + 1) % bubbles.Length;
            CapturePointer(e.PointerId, PointerGestures.Drag);
        }
        else
        {
            for (var i = 0; i < bubbles.Length; i++)
            {
                ref var bubble = ref bubbles[i];
                if (!bubble.Held || bubble.Pointer != e.PointerId) continue;
                if (e.Type == PointerEventType.Move)
                {
                    bubble.Position = (Point)(e.State.Position - Frame.TopLeft); bubble.Phase = "MOVE";
                }
                else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
                {
                    if (e.Type == PointerEventType.Up) bubble.Position = (Point)(e.State.Position - Frame.TopLeft);
                    bubble.Held = false; bubble.Age = -1;
                    bubble.Phase = e.Type == PointerEventType.Up ? "UP" : "CANCEL";
                    if (e.Type != PointerEventType.LostCapture) ReleasePointer(e.PointerId);
                    RequestAnimationFrame();
                }
            }
        }
        InvalidateRender();
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        var dt = previousFrame.HasValue ? NFloat.Max(0, (NFloat)(current - previousFrame.Value).TotalSeconds) : 0;
        previousFrame = current;
        var fading = false;
        for (var i = 0; i < bubbles.Length; i++)
        {
            ref var bubble = ref bubbles[i];
            if (!bubble.Visible || bubble.Held) continue;
            bubble.Age = bubble.Age < 0 ? 0 : bubble.Age + dt;
            if (bubble.Age >= 2) bubble.Visible = false;
            else fading = true;
        }
        InvalidateRender();
        if (fading) RequestAnimationFrame();
    }

    protected override void OnDeactivate()
    {
        for (var i = 0; i < bubbles.Length; i++)
        {
            if (!bubbles[i].Held) continue;
            bubbles[i].Held = false; ReleasePointer(bubbles[i].Pointer);
        }
        Array.Clear(bubbles); previousFrame = null;
        base.OnDeactivate();
    }

    protected override void RenderCore(IContext ctx)
    {
        InputDrawing.Panel(ctx, Frame, new Color(0xF0F3FAFF));
        InputDrawing.Dots(ctx, Frame);
        if (VisibleBubbleCount == 0)
        {
            InputDrawing.Text(ctx, "Touch something wonderful.", Frame.Center, 15, Muted, align: TextAlign.Center);
            InputDrawing.Text(ctx, "One finger. Many fingers. Your turn.", Frame.Center + new Vector(0, 26), 11, Muted, align: TextAlign.Center);
        }
        // Draw every circle first so another contact cannot paint over a label.
        for (var pass = 0; pass < 2; pass++)
        {
            ctx.Save();
            if (pass == 0) InputDrawing.Clip(ctx, Frame);
            for (var i = 0; i < bubbles.Length; i++)
            {
                ref var bubble = ref bubbles[i];
                if (!bubble.Visible) continue;
                var t = bubble.Held ? 0 : NFloat.Clamp(bubble.Age / 2, 0, 1);
                var eased = Easing.EaseInOutCubic(t);
                var center = Frame.TopLeft + new Vector(bubble.Position.X, bubble.Position.Y - 18 * eased);
                // 112 logical points: a generous halo around a finger on phone screens.
                var radius = 56 * (1 + 0.28f * eased);
                ctx.Save(); ctx.GlobalAlpha = 1 - eased;
                if (pass == 0)
                {
                    ctx.BeginPath(); ctx.Arc(center, radius, 0, NFloat.Pi * 2);
                    ctx.SetFill(new Color(bubble.Color.Red, bubble.Color.Green, bubble.Color.Blue, 0.16f)); ctx.Fill();
                    ctx.SetStroke(bubble.Color); ctx.LineWidth = 2; ctx.Stroke();
                }
                else
                {
                    var x = NFloat.Clamp(center.X, Frame.X + 60, NFloat.Max(Frame.X + 60, Frame.Right - 60));
                    var top = center.Y - radius - 50;
                    // Labels may extend above the touch surface, but remain inside the card.
                    InputDrawing.Panel(ctx, new Rect(x - 58, top, 116, 42), Colors.White, 8);
                    InputDrawing.Text(ctx, bubble.Device, (x, top + 12), 10, bubble.Color, true, TextAlign.Center);
                    InputDrawing.Text(ctx, bubble.Phase, (x, top + 29), 17, bubble.Color, true, TextAlign.Center);
                }
                ctx.Restore();
            }
            ctx.Restore();
        }
    }
}
