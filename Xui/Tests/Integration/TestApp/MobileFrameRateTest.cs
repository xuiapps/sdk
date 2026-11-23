using Xui.Apps.TestApp;

namespace Xui.Tests.Integration.TestApp;

public partial class MobileFrameRateTest
{

    [Fact]
    public void OnlyChangesBucketsAndKeepsItsReadingAcrossIdleGaps()
    {
        var rate = new MobileFrameRate();
        var changes = new List<int>();
        rate.Changed += changes.Add;
        double seconds = 0;
        Feed(60, SixtyHz); Feed(59, SixtyHz); Feed(61, SixtyHz);
        Assert.Equal(new[] { 60 }, changes);
        seconds += 10;
        rate.Observe(TimeSpan.FromSeconds(seconds), SixtyHz); // Unrequested tick after idle.
        Assert.Equal(new[] { 60 }, changes);
        Feed(120, TimeSpan.FromSeconds(1d / 120));
        Assert.Equal(new[] { 60, 120 }, changes);
        Feed(55, SixtyHz);
        Assert.Equal(55, rate.RoundedFramesPerSecond);

        void Feed(int fps, TimeSpan budget)
        {
            for (var i = 0; i < fps; i++)
            {
                rate.Request(TimeSpan.FromSeconds(seconds));
                seconds += 1d / fps;
                rate.Observe(TimeSpan.FromSeconds(seconds), budget);
            }
        }
    }

    [Theory]
    [InlineData(33)]
    [InlineData(200)]
    [InlineData(2000)]
    public void SparseInvalidationsDeliveredPromptlyAreNotSlowFrames(int spacingMilliseconds)
    {
        var rate = new MobileFrameRate();
        // Regularly delivered sparse input must not look like 30 or 5 FPS.
        // Across long idle gaps each new burst needs fresh samples.
        for (var burst = 0; burst < 8; burst++)
        {
            var start = TimeSpan.FromMilliseconds(burst * spacingMilliseconds);
            var requests = spacingMilliseconds > 500 ? 8 : 1;
            for (var i = 0; i < requests; i++)
            {
                var tick = start + TimeSpan.FromSeconds(i / 60d);
                rate.Request(tick);
                rate.Observe(tick + TimeSpan.FromMilliseconds(2), SixtyHz);
            }
        }
        Assert.Equal(60, rate.RoundedFramesPerSecond);
    }

    [Fact]
    public void DelayedPendingRequestCountsEvenAcrossALongPauseAndRepeatedInvalidations()
    {
        var rate = new MobileFrameRate();
        rate.Request(TimeSpan.Zero);
        rate.Request(TimeSpan.FromMilliseconds(990)); // Must not replace the original request.
        rate.Observe(TimeSpan.FromSeconds(1), SixtyHz);
        for (var i = 0; i < 5; i++)
        {
            var start = TimeSpan.FromSeconds(1 + i / 60d);
            rate.Request(start);
            rate.Observe(start + SixtyHz, SixtyHz);
        }
        Assert.Equal(5, rate.RoundedFramesPerSecond); // A real one-second blocked request.
    }

    [Fact]
    public void WarmupHidesTheNumberUntilEnoughRequestedFramesAreCollected()
    {
        var rate = new MobileFrameRate();
        rate.BeginWarmup();
        for (var i = 0; i < 30; i++)
        {
            var start = TimeSpan.FromSeconds(i / 60d);
            rate.Request(start);
            rate.Observe(start + SixtyHz, SixtyHz);
            Assert.Null(rate.RoundedFramesPerSecond);
        }
        rate.EndWarmup();
        Assert.Equal(60, rate.RoundedFramesPerSecond);
    }

    [Fact]
    public void AnimationJitterBalancesOutAndExtraRendersDoNotAcknowledgeTheNextTick()
    {
        var rate = new MobileFrameRate();
        var now = TimeSpan.Zero;
        var budget = TimeSpan.FromSeconds(1d / 120);
        for (var i = 0; i < 120; i++)
        {
            rate.RequestAnimation(now);
            rate.Observe(now + TimeSpan.FromMilliseconds(1), budget, animationTick: false);
            now += budget + TimeSpan.FromMilliseconds(i % 2 == 0 ? 1 : -1);
            rate.Observe(now, budget);
        }
        Assert.Equal(120, rate.RoundedFramesPerSecond);
        // Two-budget delivery while animation is pending is a genuine missed tick.
        for (var i = 0; i < 120; i++)
        {
            rate.RequestAnimation(now);
            now += budget * 2;
            rate.Observe(now, budget);
        }
        Assert.Equal(60, rate.RoundedFramesPerSecond);
    }

}
