using Xui.Apps.TestApp;

namespace Xui.Tests.Integration.TestApp;

public partial class MobileFrameRateTest
{
    [Fact]
    public void SteadySamplingDoesNotAllocate()
    {
        var rate = new MobileFrameRate();
        for (var i = 0; i < 300; i++) Sample(i);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 300; i < 1000; i++) Sample(i);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        void Sample(int i)
        {
            rate.Request(TimeSpan.FromTicks(i * 166667L));
            rate.Observe(TimeSpan.FromTicks((i + 1) * 166667L), SixtyHz);
        }
    }
}
