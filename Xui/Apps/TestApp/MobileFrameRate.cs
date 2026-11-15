namespace Xui.Apps.TestApp;

/// <summary>Frame responsiveness while work is pending; idle time is not a missed frame.</summary>
public sealed class MobileFrameRate
{
    private readonly long[] intervals = new long[120];
    private int count;
    private int next;
    private long total;
    private TimeSpan? requestedAt;
    private TimeSpan? completedAt;
    private bool animationRequest;
    private TimeSpan frameInterval = TimeSpan.FromSeconds(1d / 60);

    public int? RoundedFramesPerSecond { get; private set; }
    public bool IsWarmingUp { get; private set; }
    public event Action<int>? Changed;

    /// <summary>Records the oldest outstanding request. Repeated invalidations cannot hide a delay.</summary>
    public void Request(TimeSpan now)
    {
        if (requestedAt.HasValue) return;
        if (completedAt is { } last && (now < last || now - last > TimeSpan.FromSeconds(0.5)))
            ClearSamples();
        requestedAt = now;
        animationRequest = false;
    }

    /// <summary>Requests the next animation tick, preserving its full tick-to-tick interval.</summary>
    public void RequestAnimation(TimeSpan now)
    {
        Request(now);
        animationRequest = true;
    }

    /// <summary>A render or animation callback satisfies a request; unsolicited ticks add no samples.</summary>
    public void Observe(TimeSpan now, TimeSpan targetInterval, bool animationTick = true)
    {
        // Native display-link deltas describe the refresh budget, not the time since the last draw.
        if (targetInterval >= TimeSpan.FromMilliseconds(1) && targetInterval <= TimeSpan.FromMilliseconds(50))
            frameInterval = targetInterval;
        if (requestedAt is not { } request) return;
        if (animationRequest && (!animationTick || now == request)) return;
        requestedAt = null;
        completedAt = now;
        if (now < request) { ClearSamples(); return; }
        // Input may arrive just before vsync. Cap that estimate at the display cadence.
        // Continuous animation uses actual deltas, so ordinary scheduling jitter cancels out.
        var duration = animationRequest ? (now - request).Ticks : Math.Max(frameInterval.Ticks, (now - request).Ticks);
        if (count == intervals.Length) total -= intervals[next];
        else count++;
        intervals[next] = duration;
        next = (next + 1) % intervals.Length;
        total += duration;
        // Keep about half a second of requested work. A long delayed request remains
        // in the window instead of being discarded as an "idle" gap.
        while (count > 1)
        {
            var oldest = (next - count + intervals.Length) % intervals.Length;
            if (total - intervals[oldest] < TimeSpan.TicksPerSecond / 2) break;
            total -= intervals[oldest];
            count--;
        }
        Publish();
    }

    public void BeginWarmup()
    {
        IsWarmingUp = true;
        requestedAt = completedAt = null;
        ClearSamples();
    }

    public void EndWarmup()
    {
        IsWarmingUp = false;
        Publish();
    }

    private void ClearSamples() { count = next = 0; total = 0; }

    private void Publish()
    {
        if (IsWarmingUp || count < 6 || total < TimeSpan.TicksPerSecond / 10) return;
        var rate = count * (double)TimeSpan.TicksPerSecond / total;
        var rounded = (int)(Math.Round(rate / 5, MidpointRounding.AwayFromZero) * 5);
        if (rounded == RoundedFramesPerSecond) return;
        RoundedFramesPerSecond = rounded;
        Changed?.Invoke(rounded);
    }
}
