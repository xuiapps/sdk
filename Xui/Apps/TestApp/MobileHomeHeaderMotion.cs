using Xui.Core.Animation;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

/// <summary>Scroll-driven until the deadline, then a frame-clock-driven finish. No wall-clock timers.</summary>
internal sealed class MobileHomeHeaderMotion
{
    private TimeSpan? deadline;
    private TimeSpan? settleStart;
    private NFloat settleFrom;
    private bool committed;
    private bool expanding;
    private NFloat offset;
    public NFloat Progress { get; private set; }
    public NFloat Translation => offset < 0 ? -offset * 0.6f : -NFloat.Min(offset, 20);

    public void Scroll(NFloat value)
    {
        offset = value;
        if (offset <= 20)
        {
            deadline = null;
            if (!expanding && (committed || settleStart.HasValue))
            {
                expanding = true;
                settleStart = null;
                settleFrom = Progress;
            }
            committed = false;
            if (!expanding) Progress = 0;
        }
        else if (!committed)
        {
            expanding = false;
            settleStart = null;
            Progress = ScrollProgress;
        }
    }

    private NFloat ScrollProgress => NFloat.Clamp((offset - 20) / 160, 0, 1);

    public bool Advance(TimeSpan now, bool reducedMotion)
    {
        if (reducedMotion)
        {
            Progress = ScrollProgress;
            deadline = settleStart = null;
            committed = expanding = false;
            return false;
        }
        if (expanding)
        {
            settleStart ??= now;
            var t = NFloat.Clamp((NFloat)(now - settleStart.Value).TotalSeconds / 0.28f, 0, 1);
            Progress = settleFrom * (1 - Easing.EaseOutCubic(t));
            if (t < 1) return true;
            expanding = false;
            settleStart = null;
        }
        if (offset <= 20) return false;
        deadline ??= now + TimeSpan.FromSeconds(0.7);
        if (!committed && now >= deadline.Value)
        {
            committed = true;
            settleFrom = Progress;
            settleStart = now;
        }
        if (committed && settleStart is { } start)
        {
            var t = NFloat.Clamp((NFloat)(now - start).TotalSeconds / 0.28f, 0, 1);
            Progress = NFloat.Max(ScrollProgress, settleFrom + (1 - settleFrom) * Easing.EaseInOutCubic(t));
            if (t < 1) return true;
            settleStart = null;
        }
        return !committed && Progress < 1;
    }
}
