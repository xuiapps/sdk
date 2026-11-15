using System.Globalization;
using Xui.Apps.TestApp.Pages.Gestures.Widgets;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.Curves2D;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples;

/// <summary>Single-pointer pad with bounded, timestamped gesture recording.</summary>
public sealed class VibrationPad : View
{
    private readonly bool record;
    private readonly VibrationPoint[] points = new VibrationPoint[512];
    private const int ReturnSamples = 32;
    private static readonly TimeSpan MinimumReturnDuration = TimeSpan.FromMilliseconds(80);
    private CubicBezier? returnCurve;
    private int drawnCount;
    private IVibration? vibration;
    private TimeProvider clock = TimeProvider.System;
    private int pointer = -1, count;
    private long started, loopStarted;
    private NFloat strength, frequency = 0.5f;
    private bool looping;
    public bool IsLooping => looping;
    public bool IsPlaying => pointer != -1 || looping;
    public bool HasPattern => returnCurve.HasValue;
    public int RecordedPointCount => count;
    public event Action? PlaybackChanged;

    public VibrationPad(bool record)
    {
        this.record = record;
        Id = record ? "Vibration.Record.Pad" : "Vibration.Live.Pad";
    }

    // Values and status sit outside the square touch surface.
    public Rect ControlArea => (Frame.X + 16, Frame.Y + 16, NFloat.Max(1, Frame.Width - 32), NFloat.Max(1, Frame.Height - 32));

    protected override void OnActivate()
    {
        base.OnActivate();
        vibration = GetService(typeof(IVibration)) as IVibration;
        clock = GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;
        if (e.Type == PointerEventType.Down && pointer == -1)
        {
            vibration ??= GetService(typeof(IVibration)) as IVibration;
            StopLoop();
            pointer = e.PointerId; count = 0; started = clock.GetTimestamp();
            returnCurve = null; drawnCount = 0;
            CapturePointer(pointer, PointerGestures.Drag);
            SetPosition(e.State.Position);
            if (record) Append(TimeSpan.Zero);
            PlaybackChanged?.Invoke();
            RequestAnimationFrame();
        }
        else if (e.PointerId == pointer)
        {
            if (e.Type == PointerEventType.Move)
            {
                SetPosition(e.State.Position);
                if (record) RecordSample(false);
            }
            else if (e.Type == PointerEventType.Up)
            {
                SetPosition(e.State.Position);
                var id = pointer; pointer = -1;
                if (record)
                {
                    RecordSample(true);
                    ClosePattern();
                    PlayPattern();
                }
                else { vibration?.Stop(); strength = 0; }
                ReleasePointer(id);
                InvalidateRender();
            }
            else if (e.Type is PointerEventType.Cancel or PointerEventType.LostCapture) Stop();
        }
    }

    private void SetPosition(Point position)
    {
        var area = ControlArea;
        strength = NFloat.Clamp(1 - (position.Y - area.Y) / area.Height, 0, 1);
        frequency = NFloat.Clamp((position.X - area.X) / area.Width, 0, 1);
        vibration?.Play(strength, frequency);
        InvalidateRender();
    }

    private void RecordSample(bool final)
    {
        var elapsed = clock.GetElapsedTime(started);
        var limit = VibrationPattern.MaximumDuration - MinimumReturnDuration;
        if (elapsed > limit) elapsed = limit;
        if (final && elapsed < TimeSpan.FromMilliseconds(20)) elapsed = TimeSpan.FromMilliseconds(20);
        if (!final && count > 0 && elapsed - points[count - 1].Time < TimeSpan.FromMilliseconds(20)) return;
        Append(elapsed);
    }

    private void Append(TimeSpan time)
    {
        var point = new VibrationPoint(time, strength, frequency);
        if (count > 0 && time <= points[count - 1].Time)
        { points[count - 1] = point with { Time = points[count - 1].Time }; return; }
        if (count == points.Length - ReturnSamples)
        {
            // Preserve time zero and the newest endpoint while thinning older samples.
            var next = 1;
            for (var i = 2; i < count; i += 2) points[next++] = points[i];
            points[next++] = points[count - 1]; count = next;
        }
        points[count++] = point;
    }

    private void ClosePattern()
    {
        drawnCount = count;
        Point Position(int i) => (points[i].Frequency, points[i].Strength);
        var start = Position(0);
        var end = Position(count - 1);
        var departure = Vector.Zero;
        var arrival = Vector.Zero;
        // Ignore stationary samples when finding the drawing's endpoint directions.
        for (var i = count - 2; i >= 0 && departure.Magnitude < 0.001f; i--) departure = end - Position(i);
        for (var i = 1; i < count && arrival.Magnitude < 0.001f; i++) arrival = Position(i) - start;
        var reach = (start - end).Magnitude / 3;
        Point Clamp(Point p) => (NFloat.Clamp(p.X, 0, 1), NFloat.Clamp(p.Y, 0, 1));
        var cp1 = end + departure.Normalized * reach;
        var cp2 = start - arrival.Normalized * reach;
        var chord = start - end;
        NFloat Cross(Vector a, Vector b) => a.X * b.Y - a.Y * b.X;
        if (reach > 0 && NFloat.Abs(Cross(chord, cp1 - end)) + NFloat.Abs(Cross(chord, cp2 - end)) < 0.0001f)
        {
            // A straight stroke would otherwise retrace the same line. Give its
            // return a gentle bow so the closed loop remains visible.
            var bow = chord.Normalized.PerpendicularCCW * reach;
            cp1 = Point.Lerp(end, start, (NFloat)1 / 3) + bow;
            cp2 = Point.Lerp(end, start, (NFloat)2 / 3) + bow;
        }
        var curve = new CubicBezier(end, Clamp(cp1), Clamp(cp2), start);
        returnCurve = curve;
        var time = points[count - 1].Time;

        // Estimate velocity over short endpoint windows, including pauses rather
        // than treating an old movement as the release speed.
        var window = TimeSpan.FromTicks(Math.Min(time.Ticks, TimeSpan.FromMilliseconds(100).Ticks));
        var samples = points.AsSpan(0, count);
        var afterStart = VibrationPattern.Sample(samples, window);
        var beforeEnd = VibrationPattern.Sample(samples, time - window);
        Point afterStartPosition = (afterStart.Frequency, afterStart.Strength);
        Point beforeEndPosition = (beforeEnd.Frequency, beforeEnd.Strength);
        var initialSpeed = (afterStartPosition - start).Magnitude / (NFloat)window.TotalSeconds;
        var finalSpeed = (end - beforeEndPosition).Magnitude / (NFloat)window.TotalSeconds;

        Span<NFloat> lengths = stackalloc NFloat[ReturnSamples + 1];
        Span<NFloat> times = stackalloc NFloat[ReturnSamples + 1];
        lengths[0] = times[0] = 0;
        var previous = end;
        for (var i = 1; i <= ReturnSamples; i++)
        {
            var p = curve.Lerp((NFloat)i / ReturnSamples);
            lengths[i] = lengths[i - 1] + (p - previous).Magnitude;
            previous = p;
        }
        for (var i = 1; i <= ReturnSamples; i++)
        {
            var fraction = lengths[ReturnSamples] > 0 ? (lengths[i - 1] + lengths[i]) / (2 * lengths[ReturnSamples]) : (NFloat)i / ReturnSamples;
            var speed = NFloat.Max(0.15f, finalSpeed + (initialSpeed - finalSpeed) * fraction);
            times[i] = times[i - 1] + NFloat.Max(0.000001f, (lengths[i] - lengths[i - 1]) / speed);
        }
        // Bound very slow/degenerate gestures, and keep the complete loop within
        // the native pattern limit. Otherwise timing follows distance / speed.
        var duration = TimeSpan.FromSeconds((double)NFloat.Clamp(times[ReturnSamples], (NFloat)MinimumReturnDuration.TotalSeconds, 2));
        if (duration > VibrationPattern.MaximumDuration - time) duration = VibrationPattern.MaximumDuration - time;
        long previousTicks = 0;
        for (var i = 1; i <= ReturnSamples; i++)
        {
            var p = curve.Lerp((NFloat)i / ReturnSamples);
            var ticks = i == ReturnSamples ? duration.Ticks : Math.Clamp(
                (long)((NFloat)duration.Ticks * times[i] / times[ReturnSamples]), previousTicks + 1, duration.Ticks - (ReturnSamples - i));
            points[count++] = new(time + TimeSpan.FromTicks(ticks), p.Y, p.X);
            previousTicks = ticks;
        }
    }

    public void TogglePlayback()
    {
        if (IsPlaying) Stop();
        else if (HasPattern) PlayPattern();
    }

    private void PlayPattern()
    {
        vibration ??= GetService(typeof(IVibration)) as IVibration;
        vibration?.Play(points.AsSpan(0, count), repeat: true);
        looping = true; loopStarted = clock.GetTimestamp();
        strength = points[0].Strength; frequency = points[0].Frequency;
        PlaybackChanged?.Invoke();
        InvalidateRender(); RequestAnimationFrame();
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        if (pointer != -1 && record) RecordSample(false);
        if (looping && count > 1)
        {
            var elapsed = clock.GetElapsedTime(loopStarted);
            var time = TimeSpan.FromTicks(elapsed.Ticks % points[count - 1].Time.Ticks);
            var sample = VibrationPattern.Sample(points.AsSpan(0, count), time);
            strength = sample.Strength; frequency = sample.Frequency;
        }
        if (pointer != -1 || looping) { InvalidateRender(); RequestAnimationFrame(); }
    }

    public void StopLoop() { if (looping) Stop(); }

    public void Stop()
    {
        var id = pointer; pointer = -1;
        // An inactive page must not cancel playback owned by another page.
        if (id != -1 || looping) vibration?.Stop();
        looping = false; strength = 0;
        if (id != -1) ReleasePointer(id);
        PlaybackChanged?.Invoke();
        InvalidateRender();
    }

    protected override void OnDeactivate() { Stop(); vibration = null; base.OnDeactivate(); }

    protected override void RenderCore(IContext ctx)
    {
        InputDrawing.Panel(ctx, Frame, PaleBlue);
        var area = ControlArea;
        var values = string.Create(CultureInfo.InvariantCulture, $"Strength {strength:0.00}   Frequency {frequency:0.00}");
        InputDrawing.Text(ctx, values, (Frame.Center.X, Frame.Y - 22), 12, Ink, align: TextAlign.Center);
        for (var i = 0; i <= 4; i++)
        {
            var t = (NFloat)i / 4;
            InputDrawing.Line(ctx, (area.X + area.Width * t, area.Y), (area.X + area.Width * t, area.Bottom), new Color(0xD2D9F0FF));
            InputDrawing.Line(ctx, (area.X, area.Y + area.Height * t), (area.Right, area.Y + area.Height * t), new Color(0xD2D9F0FF));
        }
        Point Position(NFloat s, NFloat f) => (area.X + f * area.Width, area.Bottom - s * area.Height);
        if (record && count > 1)
        {
            ctx.BeginPath(); ctx.MoveTo(Position(points[0].Strength, points[0].Frequency));
            for (var i = 1; i < (HasPattern ? drawnCount : count); i++) ctx.LineTo(Position(points[i].Strength, points[i].Frequency));
            if (returnCurve is { } curve)
                ctx.CurveTo(Position(curve.P1.Y, curve.P1.X), Position(curve.P2.Y, curve.P2.X), Position(curve.P3.Y, curve.P3.X));
            ctx.SetStroke(Blue); ctx.LineWidth = 3; ctx.Stroke();
        }
        if (pointer != -1 || looping)
        {
            ctx.BeginPath(); ctx.Arc(Position(strength, frequency), 12, 0, NFloat.Pi * 2);
            ctx.SetFill(Teal); ctx.Fill();
        }
        else if (count == 0)
            InputDrawing.Text(ctx, "Touch and move", area.Center, 18, Blue, align: TextAlign.Center);
        InputDrawing.Text(ctx, "↑ strength      frequency →", (Frame.Center.X, Frame.Bottom + 20), 11, Muted, align: TextAlign.Center);
        var status = vibration?.IsSupported != true ? "Visual preview · no vibration hardware"
            : looping ? "Playing on repeat" : pointer != -1 ? record ? "Recording" : "Playing"
            : HasPattern ? "Stopped · press Start to repeat" : "Ready";
        if (vibration is { IsSupported: true, SupportsFrequency: false }) status += " · strength only";
        InputDrawing.Text(ctx, status, (Frame.Center.X, Frame.Bottom + 40), 10, Muted, align: TextAlign.Center);
    }
}
