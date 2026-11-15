using System.Globalization;
using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Gestures.Widgets;

/// <summary>A horizontal temperature ruler that settles in half-degree increments.</summary>
public sealed class TemperatureDial : View
{
    public NFloat Temperature { get; private set; } = 21;
    private int pointer = -1;
    private NFloat startX, startValue, from, target;
    private bool settling;
    private TimeSpan? animationStart;
    private TimeSpan? sampleTime;
    private NFloat sampleValue, velocity;
    private NFloat? releaseVelocity;
    private ExponentialDecayCurve? coast;
    private string reading = "21.0°";
    public TemperatureDial() => Id = "TemperatureDial";
    private void SetTemperature(NFloat value)
    {
        Temperature = NFloat.Clamp(value, 16, 30);
        reading = ((double)Temperature).ToString("0.0", CultureInfo.InvariantCulture) + "°";
        InvalidateRender();
    }
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;
        if (e.Type == PointerEventType.Down && pointer == -1)
        {
            pointer = e.PointerId; startX = e.State.Position.X; startValue = Temperature; settling = false;
            coast = null; releaseVelocity = null; velocity = 0; sampleValue = Temperature; sampleTime = null;
            CapturePointer(pointer, PointerGestures.DragHorizontalTentative);
            RequestAnimationFrame();
        }
        else if (pointer == e.PointerId)
        {
            if (e.Type == PointerEventType.Move)
            {
                var dx = e.State.Position.X - startX;
                if (NFloat.Abs(dx) > 20) CapturePointer(pointer, PointerGestures.Drag);
                SetTemperature(startValue - dx / 32);
            }
            else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
            {
                var id = pointer; pointer = -1;
                if (e.Type == PointerEventType.Up && NFloat.Abs(velocity) > 0.5f)
                {
                    releaseVelocity = velocity; RequestAnimationFrame();
                }
                else BeginSnap();
                if (e.Type != PointerEventType.LostCapture) ReleasePointer(id);
            }
        }
    }
    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        if (pointer != -1)
        {
            // Sample motion with the frame clock: deterministic in tests and independent
            // of how many native move events arrive within a display frame.
            var dt = (NFloat)(current - (sampleTime ?? previous)).TotalSeconds;
            if (dt > 0)
            {
                var sample = NFloat.Clamp((Temperature - sampleValue) / dt, -24, 24);
                var weight = 1 - NFloat.Exp(-dt / 0.04f);
                velocity += (sample - velocity) * weight;
                sampleValue = Temperature;
            }
            sampleTime = current;
            RequestAnimationFrame();
            return;
        }
        if (releaseVelocity is { } released)
        {
            coast = new ExponentialDecayCurve(current, Temperature, released, ExponentialDecayCurve.Fast, 0.15);
            releaseVelocity = null;
        }
        if (coast is { } curve)
        {
            var value = curve[current];
            SetTemperature(value);
            if (current >= curve.EndTime || value <= 16 || value >= 30)
            {
                coast = null; BeginSnap();
            }
            else { RequestAnimationFrame(); return; }
        }
        if (!settling) return;
        animationStart ??= current;
        var t = NFloat.Clamp((NFloat)(current - animationStart.Value).TotalSeconds / 0.25f, 0, 1);
        SetTemperature(from + (target - from) * Easing.EaseInOutCubic(t));
        if (t == 1) settling = false; else RequestAnimationFrame();
    }
    private void BeginSnap()
    {
        from = Temperature; target = NFloat.Round(Temperature * 2) / 2;
        settling = true; animationStart = null; RequestAnimationFrame();
    }
    protected override void OnDeactivate()
    {
        var id = pointer; pointer = -1; settling = false;
        coast = null; releaseVelocity = null; velocity = 0;
        if (id != -1) ReleasePointer(id);
        base.OnDeactivate();
    }
    protected override void RenderCore(IContext ctx)
    {
        InputDrawing.Panel(ctx, Frame, new Color(0xF4F0E9FF)); InputDrawing.Clip(ctx, Frame);
        var center = Frame.Center.X;
        InputDrawing.Text(ctx, "LIVING ROOM", (center, Frame.Y + 35), 10, Muted, true, TextAlign.Center);
        InputDrawing.Text(ctx, reading, (center, Frame.Y + 104), 64, Ink, true, TextAlign.Center);
        InputDrawing.Text(ctx, Temperature < 20 ? "Fresh & cool" : Temperature > 24 ? "Warm & cozy" : "Comfort, found.",
            (center, Frame.Y + 157), 14, Teal, align: TextAlign.Center);
        var rulerY = Frame.Y + Frame.Height * 0.68f;
        for (var half = 32; half <= 60; half++)
        {
            var value = (NFloat)half / 2;
            var x = center + (value - Temperature) * 32;
            if (x < Frame.X || x > Frame.Right) continue;
            InputDrawing.Line(ctx, (x, rulerY), (x, rulerY + (half % 2 == 0 ? 26 : 14)), new Color(0xAAA69FFF), 2);
            if (half % 4 == 0)
                InputDrawing.Text(ctx, (half / 2).ToString(CultureInfo.InvariantCulture), (x, rulerY + 45), 12, Muted, align: TextAlign.Center);
        }
        InputDrawing.Line(ctx, (center, rulerY - 9), (center, rulerY + 30), Teal, 3);
        InputDrawing.Text(ctx, "←  SLIDE TO ADJUST  →", (center, Frame.Bottom - 24), 10, Muted, align: TextAlign.Center);
    }
}
