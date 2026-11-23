using Xui.Core.Canvas;
using Xunit;
using CanvasFont = Xui.Core.Canvas.Font;

namespace Xui.Core.Tests.Canvas;

public partial class TextMetricsCacheTests
{
    [Fact]
    public void HitsAndEvictionsAllocateNoManagedObjectsAfterWarmup()
    {
        var cache = new TextMetricsCache(2);
        string[] labels = ["A", "B", "C"];
        var metrics = Metrics(12);
        void Exercise()
        {
            for (int i = 0; i < 1000; i++)
            {
                string text = labels[i % labels.Length];
                cache.Set(text, Font, TextAlign.Left, TextBaseline.Alphabetic, metrics);
                cache.TryGet(text, Font, TextAlign.Left, TextBaseline.Alphabetic, out _);
            }
        }
        Exercise();
        long before = GC.GetAllocatedBytesForCurrentThread();
        Exercise();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
