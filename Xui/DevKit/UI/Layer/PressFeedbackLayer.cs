using System.Runtime.CompilerServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;
using Xui.Core.UI.Layer;

namespace Xui.DevKit.UI.Layer;

/// <summary>
/// Delayed press feedback and bounded, overlapping ripples. The host owns gesture capture and
/// calls Begin/End when a press starts, commits or is cancelled. One layer can paint many logical
/// cells by selecting a Target before arranging/rendering; it never allocates per-cell state.
/// </summary>
public struct PressFeedbackLayer<TView> : ILayer<TView> where TView : class, ILayerHost
{
    private const int Capacity = 8;
    [InlineArray(Capacity)]
    private struct RippleBuffer { private Ripple first; }
    private struct Ripple
    {
        public bool Active, Released, Cancelled;
        public int Target;
        public Point Origin;
        public nfloat ShowAt, FadeAt;
    }

    private RippleBuffer ripples;
    private int activeSlot; // One-based, so a default layer has no active press.
    private TimeProvider? clock;
    private long epoch;
    private Rect bounds;

    /// <summary>Logical cell to begin or render. Leave at zero for a single button.</summary>
    public int Target;
    /// <summary>Optional pressed background; transparent by default.</summary>
    public Color Background;
    /// <summary>Ripple color, rendered at twelve percent opacity.</summary>
    public Color RippleColor;
    /// <summary>Outline used to clip feedback to the arranged bounds.</summary>
    public CornerRadius CornerRadius;

    private readonly nfloat Now => clock is null ? 0 : (nfloat)clock.GetElapsedTime(epoch).TotalSeconds;

    /// <summary>Whether a competing ancestor drag gesture warrants a touch recognition delay.</summary>
    public static bool ShouldDelay(TView view, ref PointerEventRef e)
    {
        if (e.State.PointerType == PointerType.Touch)
            foreach (var candidate in e.PotentialGestures)
                if (!ReferenceEquals(candidate.Handler, view) && candidate.Gesture is IDrag) return true;
        return false;
    }

    /// <summary>Starts feedback at a point in the same coordinate space as the arranged bounds.</summary>
    public void Begin(TView view, Point position, bool delay)
    {
        if (clock is null)
        {
            clock = view.GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
            epoch = clock.GetTimestamp();
        }
        var now = Now;
        var slot = 0;
        for (var i = 0; i < Capacity; i++)
        {
            if (!ripples[i].Active || ripples[i].Released && Opacity(ripples[i], now) == 0) { slot = i; break; }
            if (ripples[i].ShowAt < ripples[slot].ShowAt) slot = i;
        }
        ripples[slot] = new Ripple
        {
            Active = true, Target = Target, ShowAt = now + (delay ? .1f : 0),
            Origin = (position.X - bounds.X, position.Y - bounds.Y),
        };
        activeSlot = slot + 1;
        view.InvalidateRender(); view.RequestAnimationFrame();
    }

    /// <summary>Quick taps show immediately and hold for 200ms; early scroll cancellation stays invisible.</summary>
    public void End(TView view, bool tapped)
    {
        if (activeSlot == 0) return;
        ref var ripple = ref ripples[activeSlot - 1];
        var now = Now;
        if (!tapped && now < ripple.ShowAt) ripple.Active = false;
        else
        {
            ripple.ShowAt = nfloat.Min(ripple.ShowAt, now);
            ripple.Released = true;
            ripple.Cancelled = !tapped;
            ripple.FadeAt = now + (tapped ? .2f : 0);
        }
        activeSlot = 0;
        view.InvalidateRender(); view.RequestAnimationFrame();
    }

    /// <summary>Resumes completed feedback after reparenting, or clears it if the clock changed.</summary>
    public void Activate(TView view)
    {
        if (clock is null) return;
        var nextClock = view.GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
        if (!ReferenceEquals(clock, nextClock)) { ripples = default; activeSlot = 0; clock = null; return; }
        for (var i = 0; i < Capacity; i++)
            if (ripples[i].Active) { view.InvalidateRender(); view.RequestAnimationFrame(); break; }
    }

    private static nfloat Opacity(in Ripple ripple, nfloat now)
    {
        if (!ripple.Active || now < ripple.ShowAt) return 0;
        if (!ripple.Released || now <= ripple.FadeAt) return 1;
        return 1 - Easing.EaseInOutCubic(nfloat.Clamp((now - ripple.FadeAt) / (ripple.Cancelled ? .08f : .18f), 0, 1));
    }

    /// <summary>Current background strength for Target, for hosts that blend their own themed fill.</summary>
    public nfloat FeedbackOpacity
    {
        get
        {
            nfloat opacity = 0;
            var now = Now;
            for (var i = 0; i < Capacity; i++)
                if (ripples[i].Target == Target) opacity = nfloat.Max(opacity, Opacity(ripples[i], now));
            return opacity;
        }
    }

    /// <inheritdoc/>
    public Size Measure(TView view, Size availableSize, IMeasureContext context) => Size.Empty;
    /// <inheritdoc/>
    public void Arrange(TView view, Rect rect, IMeasureContext context) => bounds = rect;

    /// <summary>Sets bounds before input, without requiring a drawing/measurement context.</summary>
    public void SetBounds(Rect rect) => bounds = rect;

    /// <inheritdoc/>
    public void Render(TView view, IContext context)
    {
        var opacity = FeedbackOpacity;
        if (opacity <= 0) return;
        context.Save();
        context.BeginPath(); context.RoundRect(bounds, CornerRadius); context.Clip();
        if (!Background.IsTransparent)
        {
            var background = Background; background.Alpha *= opacity;
            context.SetFill(background); context.FillRect(bounds);
        }
        var now = Now;
        for (var i = 0; i < Capacity; i++)
        {
            ref readonly var ripple = ref ripples[i];
            if (ripple.Target != Target) continue;
            opacity = Opacity(ripple, now);
            if (opacity <= 0) continue;
            var x = nfloat.Clamp(ripple.Origin.X, 0, bounds.Width);
            var y = nfloat.Clamp(ripple.Origin.Y, 0, bounds.Height);
            var dx = nfloat.Max(x, bounds.Width - x);
            var dy = nfloat.Max(y, bounds.Height - y);
            var growth = Easing.EaseOutCubic(nfloat.Clamp((now - ripple.ShowAt) / .35f, 0, 1));
            var radius = 3 + nfloat.Sqrt(dx * dx + dy * dy) * growth;
            var color = RippleColor; color.Alpha *= opacity * .12f;
            context.SetFill(color); context.BeginPath();
            context.Arc((bounds.X + x, bounds.Y + y), radius, 0, nfloat.Pi * 2); context.Fill();
        }
        context.Restore();
    }

    /// <inheritdoc/>
    public void Animate(TView view, TimeSpan previousTime, TimeSpan currentTime)
    {
        var now = Now;
        var animate = false;
        var changed = false;
        for (var i = 0; i < Capacity; i++)
        {
            ref var ripple = ref ripples[i];
            if (!ripple.Active) continue;
            changed = true;
            if (ripple.Released && now > ripple.FadeAt && Opacity(ripple, now) == 0) ripple.Active = false;
            else if (ripple.Released || now < ripple.ShowAt + .35f) animate = true;
        }
        if (changed) view.InvalidateRender();
        if (animate) view.RequestAnimationFrame();
    }

    /// <inheritdoc/>
    public void OnPointerEvent(TView view, ref PointerEventRef e, EventPhase phase) { }
    /// <inheritdoc/>
    public void OnKeyDown(TView view, ref KeyEventRef e) { }
    /// <inheritdoc/>
    public void OnChar(TView view, ref KeyEventRef e) { }
    /// <inheritdoc/>
    public void OnFocus(TView view) { }
    /// <inheritdoc/>
    public void OnBlur(TView view) { }
}
