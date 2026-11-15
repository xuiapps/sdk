using Xui.Core.Animation;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

/// <summary>Scroll-driven in either direction until a deadline, then an eased finish.</summary>
internal sealed class MobileHomeHeaderMotion
{
    private const int CollapseStart = 75;
    private const int CollapseEnd = 225;
    private const int ExpansionStart = 175;
    private const int ExpansionEnd = 25;
    private const float SettleSeconds = 0.56f;
    private TimeSpan? deadline;
    private TimeSpan? settleStart;
    private NFloat settleFrom;
    private NFloat turnOffset;
    private NFloat turnProgress;
    private NFloat offset;
    private int direction = 1;
    private int targetDirection = 1;
    private bool transitionActive;
    public NFloat Progress { get; private set; }
    // Expanded content follows the scroll all the way past the top; only overscroll is parallax.
    public NFloat Translation => offset < 0 ? -offset * 0.6f : -offset;

    private NFloat Target => targetDirection > 0 ? 1 : 0;
    private bool CanTransition => direction > 0 ? offset > CollapseStart : offset <= ExpansionStart;
    private NFloat ScrollProgress
    {
        get
        {
            // A reversal inside the expansion interval uses only the remaining distance,
            // so offset 25 is always fully expanded, without counting deeper-page travel.
            var origin = NFloat.Clamp(turnOffset, ExpansionEnd, ExpansionStart);
            var fraction = direction > 0 ? NFloat.Clamp((offset - CollapseStart) / (CollapseEnd - CollapseStart), 0, 1) :
                origin > ExpansionEnd ? NFloat.Clamp((origin - offset) / (origin - ExpansionEnd), 0, 1) : 1;
            return turnProgress + ((direction > 0 ? 1 : 0) - turnProgress) * fraction;
        }
    }

    public void Scroll(NFloat value)
    {
        if (value == offset) return;
        // Springing back from negative overscroll is still a return to the expanded header.
        var nextDirection = value <= 0 ? -1 : value > offset ? 1 : -1;
        if (nextDirection != direction)
        {
            direction = nextDirection;
            turnOffset = offset;
            turnProgress = Progress;
            // A tentative reversal must not cancel an already accepted finish.
            // Keep its target and clock until this leg crosses its own threshold.
        }
        offset = value;
        // Returning from a pull must not count the overscroll distance toward collapse.
        if (offset <= 0 && Progress == 0)
        {
            turnOffset = 0;
            turnProgress = 0;
            targetDirection = -1;
            transitionActive = false;
            deadline = settleStart = null;
            return;
        }
        // Collapse past 75 on downward travel; expand at 175 on upward travel.
        // In the overlapping band, the current scroll direction selects the target.
        if (offset > CollapseStart && targetDirection < 0 && !CanTransition)
        {
            targetDirection = 1;
            transitionActive = true;
            deadline = settleStart = null;
        }
        if (CanTransition)
        {
            if (targetDirection != direction)
            {
                targetDirection = direction;
                // The previous finish may have advanced while the reverse travel
                // was below threshold. Turn from the current pose, not the old one.
                turnProgress = Progress;
                deadline = settleStart = null;
            }
            transitionActive = true;
            if (settleStart is null)
                Progress = direction > 0 ? NFloat.Max(Progress, ScrollProgress) : NFloat.Min(Progress, ScrollProgress);
        }
    }

    public bool Advance(TimeSpan now, bool reducedMotion)
    {
        if (reducedMotion)
        {
            Progress = offset <= CollapseStart ? 0 : offset > ExpansionStart ? 1 : Target;
            turnProgress = Progress;
            turnOffset = offset;
            deadline = settleStart = null;
            transitionActive = false;
            return false;
        }
        if (!transitionActive) return false;
        if (Progress == Target)
        {
            transitionActive = false;
            deadline = settleStart = null;
            return false;
        }
        deadline ??= now + TimeSpan.FromSeconds(0.7);
        if (settleStart is null && now >= deadline.Value)
        {
            settleFrom = Progress;
            settleStart = now;
        }
        if (settleStart is { } start)
        {
            var t = NFloat.Clamp((NFloat)(now - start).TotalSeconds / SettleSeconds, 0, 1);
            var eased = settleFrom + (Target - settleFrom) * Easing.EaseInOutCubic(t);
            var scroll = direction == targetDirection && CanTransition ? ScrollProgress : Progress;
            Progress = targetDirection > 0 ? NFloat.Max(scroll, eased) : NFloat.Min(scroll, eased);
            if (t >= 1)
            {
                Progress = Target;
                transitionActive = false;
                deadline = settleStart = null;
                return false;
            }
        }
        return Progress != Target;
    }
}
