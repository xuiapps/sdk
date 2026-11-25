using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Xml.Linq;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.Market;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public partial class MarketPerformanceTest
{
    [Fact]
    public void OpeningMarketDefersRowsAndOpeningStockLoadsOnlyItsDetails()
    {
        _ = new MarketData(42); // Exclude static/JIT initialization from the allocation budget.
        var before = GC.GetAllocatedBytesForCurrentThread();
        var data = new MarketData(42);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.InRange(allocated, 0, 100_000);
        Assert.Equal(0, data.LoadedCount);
        var last = data[4999];
        var first = data[0];
        Assert.Equal(2, data.LoadedCount);
        Assert.False(first.HasDetails);
        Assert.False(last.HasDetails);
        Assert.Same(last, data[4999]);
        var detail = new MarketDetailExample(last);
        Assert.Same(last, detail.Stock);
        Assert.True(last.HasDetails);
        Assert.False(first.HasDetails);
        Assert.Equal(2, data.LoadedCount);
        var bars = last.Bars;
        _ = new MarketDetailExample(last);
        Assert.Same(bars, last.Bars);
    }

    [Theory]
    [InlineData(320, 568)]
    [InlineData(430, 932)]
    [InlineData(1024, 768)]
    public void LoadingRowsStaysViewportBoundAndCachedFramesRecordZeroManagedBytes(int width, int height)
    {
        using var measure = new SvgDrawingContext((width, height), Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        var data = new MarketData(0x584D4152);
        var grid = new MarketGrid(data);
        var scroll = new ScrollView { Content = grid };
        scroll.Measure((width, height), measure);
        scroll.Arrange((0, 0, width, height), measure);
        Assert.Equal(0, data.LoadedCount);
        void Frame()
        {
            recorder.BeginFrame((0, 0, width, height), 1);
            scroll.Render(recorder);
            recorder.EndFrame(); recorder.AcceptFrame();
        }
        Frame();
        Assert.Equal(grid.RenderedRowCount, data.LoadedCount);
        // Warm reusable command/storage buffers, including the maximum partially visible row count.
        for (var i = 0; i < 32; i++)
        {
            var wheel = new ScrollWheelEventRef { Delta = (0, -137) };
            scroll.OnScrollWheel(ref wheel); Frame();
        }
        long allocated = 0;
        for (var i = 0; i < 128; i++)
        {
            // Move far enough to encounter entirely new text, logos and paths on every frame.
            var wheel = new ScrollWheelEventRef { Delta = (0, -1307) };
            scroll.OnScrollWheel(ref wheel);
            var loaded = data.LoadedCount;
            Frame(); // First visibility prepares only this viewport's missing row data.
            Assert.InRange(data.LoadedCount - loaded, 0, grid.RenderedRowCount);
            for (var row = grid.FirstRenderedRow; row < grid.FirstRenderedRow + grid.RenderedRowCount; row++)
                Assert.False(data[row].HasDetails);
            var before = GC.GetAllocatedBytesForCurrentThread();
            Frame();
            allocated += GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.InRange(grid.RenderedRowCount, 1, height / 100 + 2);
            Assert.Equal(0, grid.Count); // no row/cell views at any scroll depth
            Assert.True(recorder.Root.Commands.Count > 100);
        }
        Assert.True(grid.FirstRenderedRow > 1000);
        Assert.Equal(0, allocated);
        var end = new ScrollWheelEventRef { Delta = (0, -1_000_000) };
        scroll.OnScrollWheel(ref end); Frame();
        Assert.Equal(5000, grid.FirstRenderedRow + grid.RenderedRowCount);
    }
}
