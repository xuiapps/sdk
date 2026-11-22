using Xui.Core.Animation;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

public abstract partial class FloorplanCameraView
{
    private TimeProvider clock = TimeProvider.System;
    private long sampleTick, lastMotionTick;
    private Vector velocity, samplePan;
    private NFloat zoomVelocity, sampleZoom;
    private Point zoomAnchor;
    private ExponentialDecayCurve? coastX, coastY, coastZoom;
    private Vector coastPosition;
    private NFloat coastLogZoom;
    private bool pendingCoast, restartMotionClock;
    private NFloat wheelQuietRemaining, framesRemaining;

    private void KeepFrames()
    {
        // Keep sampling frames after a quick tap/short zoom. A stationary map can
        // reuse its retained surface while the FPS sample continues to settle.
        framesRemaining = 2;
        restartMotionClock = true;
        RequestAnimationFrame();
    }

    private void StopMotion()
    {
        coastX = coastY = coastZoom = null;
        pendingCoast = false;
        velocity = samplePan = default;
        zoomVelocity = sampleZoom = 0;
        wheelQuietRemaining = framesRemaining = 0;
    }

    private void BeginInteraction()
    {
        StopMotion();
        clock = GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
        sampleTick = lastMotionTick = clock.GetTimestamp();
        KeepFrames();
    }

    private void SampleMotion(Vector delta, NFloat logZoom)
    {
        samplePan += delta;
        sampleZoom += logZoom;
        if (delta.X != 0 || delta.Y != 0 || logZoom != 0)
            lastMotionTick = clock.GetTimestamp();
        KeepFrames();
    }

    private void UpdateVelocity()
    {
        var dt = (NFloat)clock.GetElapsedTime(sampleTick).TotalSeconds;
        // Sample once per frame, like the temperature dial. Both pinch contacts
        // contribute before sampling: symmetric zoom must not create a pan fling.
        if (dt >= (NFloat).001)
        {
            var weight = 1 - NFloat.Exp(-dt / (NFloat).04);
            var next = new Vector(NFloat.Clamp(samplePan.X / dt, -8000, 8000),
                NFloat.Clamp(samplePan.Y / dt, -8000, 8000));
            velocity += (next - velocity) * weight;
            zoomVelocity += (NFloat.Clamp(sampleZoom / dt, -6, 6) - zoomVelocity) * weight;
            samplePan = default; sampleZoom = 0;
            sampleTick = clock.GetTimestamp();
        }
    }

    private void EndInteraction(bool released)
    {
        if (!released)
        {
            StopMotion();
            KeepFrames();
            return;
        }
        if (first == -1)
        {
            UpdateVelocity();
            // Holding still before lifting a finger must not launch an old fling.
            pendingCoast = clock.GetElapsedTime(lastMotionTick).TotalMilliseconds < 100;
            KeepFrames();
        }
        else
        {
            // The remaining finger can continue panning without a position jump.
            // Preserve pinch velocity for two Up events delivered in the same frame;
            // an actual single-finger Move clears the zoom velocity instead.
            samplePan = default; sampleZoom = 0;
            sampleTick = clock.GetTimestamp();
        }
    }

    private void WheelZoom(Point anchor, NFloat logDelta)
    {
        StopMotion();
        ZoomAt(anchor, NFloat.Exp(logDelta));
        zoomAnchor = anchor;
        zoomVelocity = NFloat.Clamp(logDelta / (NFloat).04, -6, 6);
        pendingCoast = true;
        // A wheel/trackpad burst owns the scale until it stops delivering input.
        wheelQuietRemaining = (NFloat).08;
        KeepFrames();
    }

    private void AnimateMotion(TimeSpan previous, TimeSpan current)
    {
        var dt = restartMotionClock ? (NFloat)0 : NFloat.Max(0, (NFloat)(current - previous).TotalSeconds);
        restartMotionClock = false;
        if (first != -1)
        {
            UpdateVelocity();
            RequestAnimationFrame();
            return;
        }
        if (framesRemaining <= 0) return;
        framesRemaining = NFloat.Max(0, framesRemaining - dt);
        wheelQuietRemaining = NFloat.Max(0, wheelQuietRemaining - dt);
        if (pendingCoast && wheelQuietRemaining == 0)
        {
            coastPosition = default; coastLogZoom = 0;
            // The same exponential motion as ScrollView, in screen points.
            coastX = NFloat.Abs(velocity.X) > 50
                ? new(current, 0, velocity.X, ExponentialDecayCurve.Normal) : null;
            coastY = NFloat.Abs(velocity.Y) > 50
                ? new(current, 0, velocity.Y, ExponentialDecayCurve.Normal) : null;
            // Log scale makes zoom-in and zoom-out symmetric and preserves the anchor.
            // Fast is the same decay used by the temperature dial.
            coastZoom = NFloat.Abs(zoomVelocity) > (NFloat).02
                ? new(current, 0, zoomVelocity, ExponentialDecayCurve.Fast, .01) : null;
            pendingCoast = false;
        }
        if (coastZoom is { } zoom)
        {
            var next = zoom[current];
            var before = scale;
            ZoomAt(zoomAnchor, NFloat.Exp(next - coastLogZoom));
            coastLogZoom = next;
            if (current >= zoom.EndTime || (before == scale && current > zoom.StartTime)) coastZoom = null;
        }
        var movement = new Vector(
            coastX is { } x ? x[current] - coastPosition.X : 0,
            coastY is { } y ? y[current] - coastPosition.Y : 0);
        coastPosition += movement;
        pan += movement;
        zoomAnchor += movement;
        var requested = pan;
        if (movement.X != 0 || movement.Y != 0)
            Changed();
        if (coastX is { } cx && (current >= cx.EndTime || pan.X != requested.X)) coastX = null;
        if (coastY is { } cy && (current >= cy.EndTime || pan.Y != requested.Y)) coastY = null;
        // Keep frame sampling flowing for two seconds without repainting a settled map.
        // Stop both motion and frame requests afterwards or on deactivation.
        if (framesRemaining > 0) RequestAnimationFrame();
        else StopMotion();
    }
}
