using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.DevKit.UI.Widgets;

public partial class Button
{
    private const int RippleCapacity = 8;
    private Ripple[]? ripples;
    private int activeRipple = -1;
    private int pressedPointer = -1;
    private TimeProvider? feedbackClock;
    private long feedbackEpoch;

    private struct Ripple
    {
        public bool Active, Released, Cancelled;
        public Point Origin;
        public nfloat ShowAt, FadeAt;
    }

    private nfloat FeedbackTime => feedbackClock is null ? 0 : (nfloat)feedbackClock.GetElapsedTime(feedbackEpoch).TotalSeconds;

    /// <inheritdoc/>
    protected override void OnActivate()
    {
        base.OnActivate();
        if (feedbackClock is null) return;
        var clock = GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
        if (!ReferenceEquals(clock, feedbackClock)) ClearPressFeedback();
        else if (ripples is not null && Array.Exists(ripples, ripple => ripple.Active))
        {
            InvalidateRender(); RequestAnimationFrame();
        }
    }

    private void BeginPress(int pointer, Point position, bool delay)
    {
        if (feedbackClock is null)
        {
            feedbackClock = GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
            feedbackEpoch = feedbackClock.GetTimestamp();
        }
        ripples ??= new Ripple[RippleCapacity];
        var now = FeedbackTime;
        var slot = 0;
        for (var i = 0; i < ripples.Length; i++)
        {
            if (!ripples[i].Active || Opacity(ripples[i], now) == 0 && ripples[i].Released) { slot = i; break; }
            if (ripples[i].ShowAt < ripples[slot].ShowAt) slot = i;
        }
        ripples[slot] = new Ripple
        {
            Active = true, ShowAt = now + (delay ? 0.1f : 0),
            Origin = (position.X - Frame.X, position.Y - Frame.Y),
        };
        activeRipple = slot;
        pressedPointer = pointer;
        InvalidateRender(); RequestAnimationFrame();
    }

    private void EndPress(bool tapped)
    {
        pressedPointer = -1; // ReleasePointer synchronously sends LostCapture.
        if (activeRipple < 0 || ripples is null) return;
        ref var ripple = ref ripples[activeRipple];
        var now = FeedbackTime;
        if (!tapped && now < ripple.ShowAt) ripple.Active = false;
        else
        {
            // A quick tap bypasses the recognition delay and becomes visible now.
            ripple.ShowAt = nfloat.Min(ripple.ShowAt, now);
            ripple.Released = true;
            ripple.Cancelled = !tapped;
            ripple.FadeAt = now + (tapped ? 0.2f : 0);
        }
        activeRipple = -1;
        InvalidateRender(); RequestAnimationFrame();
    }

    private static nfloat Opacity(in Ripple ripple, nfloat now)
    {
        if (!ripple.Active || now < ripple.ShowAt) return 0;
        if (!ripple.Released || now <= ripple.FadeAt) return 1;
        var duration = ripple.Cancelled ? 0.08f : 0.18f;
        return 1 - Easing.EaseInOutCubic(nfloat.Clamp((now - ripple.FadeAt) / duration, 0, 1));
    }

    private nfloat FeedbackOpacity
    {
        get
        {
            var opacity = (nfloat)0;
            var now = FeedbackTime;
            if (ripples is not null)
                foreach (ref readonly var ripple in ripples.AsSpan()) opacity = nfloat.Max(opacity, Opacity(ripple, now));
            return opacity;
        }
    }

    private void RenderRipples(IContext context)
    {
        if (ripples is null) return;
        var now = FeedbackTime;
        var saved = false;
        foreach (ref readonly var ripple in ripples.AsSpan())
        {
            var opacity = Opacity(ripple, now);
            if (opacity <= 0) continue;
            if (!saved)
            {
                context.Save();
                context.BeginPath(); context.RoundRect(Frame, cornerRadius); context.Clip();
                saved = true;
            }
            var x = nfloat.Clamp(ripple.Origin.X, 0, Frame.Width);
            var y = nfloat.Clamp(ripple.Origin.Y, 0, Frame.Height);
            var dx = nfloat.Max(x, Frame.Width - x);
            var dy = nfloat.Max(y, Frame.Height - y);
            var progress = nfloat.Clamp((now - ripple.ShowAt) / 0.35f, 0, 1);
            var growth = Easing.EaseOutCubic(progress);
            var radius = 3 + nfloat.Sqrt(dx * dx + dy * dy) * growth;
            var color = textColor; color.Alpha *= opacity * 0.12f;
            context.SetFill(color); context.BeginPath();
            context.Arc((Frame.X + x, Frame.Y + y), radius, 0, nfloat.Pi * 2); context.Fill();
        }
        if (saved) context.Restore();
    }

    /// <inheritdoc/>
    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        if (ripples is null) return;
        var now = FeedbackTime;
        var animate = false;
        var changed = false;
        foreach (ref var ripple in ripples.AsSpan())
        {
            if (!ripple.Active) continue;
            changed = true;
            if (ripple.Released && now > ripple.FadeAt && Opacity(ripple, now) == 0) ripple.Active = false;
            else if (ripple.Released || now < ripple.ShowAt + 0.35f) animate = true;
        }
        if (changed) InvalidateRender();
        if (animate) RequestAnimationFrame();
    }

    private void ClearPressFeedback()
    {
        var pointer = pressedPointer;
        pressedPointer = activeRipple = -1;
        if (ripples is not null) Array.Clear(ripples);
        feedbackClock = null;
        if (pointer != -1) ReleasePointer(pointer);
    }

    private static Color Blend(Color a, Color b, nfloat t) => new(
        a.Red + (b.Red - a.Red) * t, a.Green + (b.Green - a.Green) * t,
        a.Blue + (b.Blue - a.Blue) * t, a.Alpha + (b.Alpha - a.Alpha) * t);
}
