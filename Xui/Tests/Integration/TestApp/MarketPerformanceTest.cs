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

public class MarketPerformanceTest
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
    [InlineData(0, "$54.80", "-21.85%", "Vol 24.9M · XSIM")]
    [InlineData(1, "$61.54", "+25.67%", "Vol 66.9M · XSIM")]
    [InlineData(42, "$17.80", "-18.03%", "Vol 4.3M · XSIM")]
    [InlineData(100, "$131.23", "-25.56%", "Vol 28.0M · XSIM")]
    [InlineData(4999, "$209.80", "-18.97%", "Vol 99.1M · XSIM")]
    public void DirectAccessPreservesOriginalSeedSequence(int index, string price, string change, string volume)
    {
        var data = new MarketData(0x584D4152);
        var stock = data[index];
        Assert.Equal(price, stock.Price);
        Assert.Equal(change, stock.Change);
        Assert.Equal(volume, stock.Volume);
        Assert.Equal(1, data.LoadedCount);
        Assert.False(stock.HasDetails);
    }

    [Fact]
    public void FreshDownReplacesUnfinishedRowPress()
    {
        using var scene = new Scene();
        MarketStock? selected = null;
        scene.List.Grid.Selected = stock => selected = stock;
        var first = scene.FirstRowPoint;
        var second = first + new Vector(0, MarketGrid.RowHeight);
        scene.App.PointerDown(first); scene.Advance(101);
        scene.App.PointerMove(first + new Vector(40, 0));
        // Recovery when a new Down arrives without the previous sequence's Up.
        scene.App.PointerDown(second); scene.Advance(101);
        Assert.Single(scene.Ripples());
        scene.App.PointerUp(second);
        Assert.Same(MarketData.Shared[1], selected);
    }

    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator, false)]
    [InlineData(TestRuntimeVariant.IPhoneEmulator, true)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator, false)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator, true)]
    public void VerticalScrollWorksAfterHorizontalMovementAndNextPress(TestRuntimeVariant variant, bool liftBetween)
    {
        using var scene = new Scene(variant: variant);
        var clicks = 0;
        scene.List.Grid.Selected = _ => clicks++;
        var point = scene.FirstRowPoint;
        scene.App.PointerDown(point); scene.Advance(101);
        scene.App.PointerMove(point + new Vector(40, 0));
        if (liftBetween)
        {
            scene.App.PointerUp(point + new Vector(40, 0));
            scene.App.PointerDown(point);
        }
        var previousClicks = clicks;
        scene.App.PointerMove(point + new Vector(40, -90));
        Assert.IsAssignableFrom<IDrag>(scene.List.Grid.GetCapturedGesture(0));
        scene.App.PointerMove(point + new Vector(40, -160));
        scene.App.PointerUp(point + new Vector(40, -160));
        scene.Advance(400);
        Assert.True(scene.List.Scroller.ScrollOffset.Y > 0);
        Assert.Equal(previousClicks, clicks);
        Assert.Empty(scene.Ripples());
        // Capture cancellation must leave the grid ready for a fresh press on a visible row.
        var next = scene.List.Scroller.Parent!.LocalToGlobal(scene.List.Scroller.Frame.Center);
        scene.App.PointerDown(next); scene.Advance(101);
        Assert.Single(scene.Ripples());
        scene.App.PointerUp(next);
        Assert.Equal(previousClicks + 1, clicks);
    }

    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    public void HorizontalMovementWithinRowKeepsFeedbackAndSameRowCanBePressedAgain(TestRuntimeVariant variant)
    {
        using var scene = new Scene(variant: variant);
        var clicks = 0;
        scene.List.Grid.Selected = _ => clicks++;
        var point = scene.FirstRowPoint;
        var offset = scene.List.Scroller.ScrollOffset;
        scene.App.PointerDown(point); scene.Advance(101);
        Assert.Single(scene.Ripples());
        scene.App.PointerMove(point + new Vector(80, 0)); scene.Advance(100);
        Assert.IsAssignableFrom<ITap>(scene.List.Grid.GetCapturedGesture(0));
        Assert.Equal(offset, scene.List.Scroller.ScrollOffset);
        Assert.Single(scene.Ripples());
        scene.App.PointerMove(point); scene.App.PointerUp(point);
        Assert.Equal(1, clicks);
        scene.Advance(400);
        // No visit to another row should be needed to reset capture or feedback.
        scene.App.PointerDown(point); scene.Advance(101);
        Assert.Single(scene.Ripples());
        scene.App.PointerUp(point);
        Assert.Equal(2, clicks);
        scene.Advance(400);
        Assert.Empty(scene.Ripples());
    }

    [Fact]
    public void RowFeedbackDelaysForScrollingAndRetainsSeparateRipplesForQuickTaps()
    {
        using var scene = new Scene();
        var clicks = 0;
        scene.List.Grid.Selected = _ => clicks++;
        var point = scene.FirstRowPoint;
        var idle = scene.DrawList();
        scene.App.PointerDown(point);
        scene.Advance(50);
        Assert.Equal(idle, scene.DrawList());
        scene.App.PointerUp(point);
        Assert.Equal(1, clicks);
        Assert.Single(scene.Ripples());
        scene.Advance(50);
        var second = point + new Vector(0, MarketGrid.RowHeight);
        scene.App.PointerDown(second); scene.App.PointerUp(second);
        Assert.Equal(2, clicks);
        Assert.Equal(2, scene.Ripples().Count());
        Assert.All(scene.Ripples(), ripple => Assert.NotNull(ripple.Parent!.Attribute("clip-path")));
        scene.Advance(199);
        Assert.Equal(2, scene.Ripples().Count());
        scene.Advance(200);
        Assert.Equal(idle, scene.DrawList());
    }

    [Fact]
    public void RowFeedbackNeverFlashesForEarlyScrollAndCancelsAfterAHold()
    {
        using var scene = new Scene();
        var clicks = 0;
        scene.List.Grid.Selected = _ => clicks++;
        var point = scene.FirstRowPoint;
        scene.App.PointerDown(point); scene.Advance(30);
        scene.App.PointerMove(point + new Vector(0, -80));
        scene.App.PointerUp(point + new Vector(0, -80));
        scene.Advance(500);
        Assert.Empty(scene.Ripples());
        // Use a visible row after inertia; feedback must follow logical row identity, not a recycled slot.
        point = scene.List.Scroller.Parent!.LocalToGlobal(scene.List.Scroller.Frame.Center);
        scene.App.PointerDown(point); scene.Advance(101);
        Assert.Single(scene.Ripples());
        scene.App.PointerMove(point + new Vector(0, -80));
        scene.App.PointerUp(point + new Vector(0, -80));
        scene.Advance(90);
        Assert.Empty(scene.Ripples());
        Assert.Equal(0, clicks);
    }

    [Fact]
    public void SeedProduces5000DistinctStableCompaniesAndValidOhlc()
    {
        var first = new MarketData(42);
        var again = new MarketData(42);
        Assert.Equal(5000, first.Count);
        var symbols = new HashSet<string>();
        var names = new HashSet<string>();
        // Random access must not change the seeded data or require loading earlier rows.
        for (var i = again.Count - 1; i >= 0; i--) _ = again[i];
        for (var i = 0; i < first.Count; i++)
        {
            var a = first[i]; var b = again[i];
            Assert.True(symbols.Add(a.Symbol)); Assert.True(names.Add(a.Name));
            Assert.Equal(a.Name, b.Name); Assert.Equal(a.Description, b.Description);
            Assert.Equal(a.Price, b.Price); Assert.Equal(a.Change, b.Change);
            Assert.Equal(a.Logo, b.Logo); Assert.Equal(a.Bars, b.Bars); Assert.Equal(a.Sparkline, b.Sparkline);
            Assert.Equal(60, a.Bars.Length); Assert.NotEmpty(a.Logo);
            foreach (var bar in a.Bars)
            {
                Assert.True(bar.Low > 0 && bar.Low <= bar.Open && bar.Low <= bar.Close);
                Assert.True(bar.High >= bar.Open && bar.High >= bar.Close);
            }
        }
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

    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    public void TapSelectsScrolledRowAndBackRestoresPositionWhileDragDoesNotNavigate(TestRuntimeVariant variant)
    {
        using var scene = new Scene(variant: variant);
        var list = scene.List;
        var scroll = list.Scroller;
        var wheel = new ScrollWheelEventRef { Delta = (0, -(MarketGrid.IntroHeight + 100 * MarketGrid.RowHeight) * 1.5f) };
        scroll.OnScrollWheel(ref wheel); scene.App.Render();
        var offset = scroll.ScrollOffset;
        var point = scroll.Parent!.LocalToGlobal((scroll.Frame.X + 100, scroll.Frame.Y + 50));
        scene.App.PointerDown(point); scene.App.PointerUp(point);
        Assert.True(scene.Navigation.IsTransitioning);
        scene.Advance();
        var detail = Assert.IsType<MarketDetailExample>(scene.App.Window.RootView.FindViewById("Market.Detail"));
        Assert.Same(MarketData.Shared[100], detail.Stock);
        var back = scene.App.Window.RootView.FindViewById("Back")!;
        var backPoint = back.Parent!.LocalToGlobal(back.Frame.Center);
        scene.App.PointerDown(backPoint); scene.App.PointerUp(backPoint); scene.Advance();
        Assert.Null(scene.App.Window.RootView.FindViewById("Market.Detail"));
        Assert.Same(list, scene.App.Window.RootView.FindViewById("Market.List"));
        Assert.Equal(offset, scroll.ScrollOffset);
        scene.App.PointerDown(point);
        scene.App.PointerMove(point + new Vector(0, -120));
        scene.App.PointerMove(point + new Vector(0, -200));
        scene.App.PointerUp(point + new Vector(0, -200));
        scene.App.Render();
        Assert.Null(scene.App.Window.RootView.FindViewById("Market.Detail"));
        Assert.True(scroll.ScrollOffset.Y > offset.Y);
        // A second pop returns to home: home -> market -> stock really are three stack entries.
        scene.Navigation.Pop(); scene.Advance();
        Assert.False(scene.Navigation.CanGoBack);
        Assert.NotNull(scene.App.Window.RootView.FindViewById("MarketPulse"));
    }

    [Fact]
    public void MarketListAndDetails_RenderReference()
    {
        using var scene = new Scene(snapshot: true);
        scene.App.SvgFontResolver = new EmbeddedFonts();
        scene.App.MarkdownHeading("Market Pulse: a layer-composed 5,000-row grid", 1);
        scene.App.MarkdownParagraph("Fixed-seed fictional companies, compiled vector logos, 60-session sparklines and OHLC candles. " +
            "One grid view composes struct layers for visible rows. No row views or recycler. " +
            "Rows load on first visibility; full details load when opened. Allocation tests measure the managed canvas recording path for cached rows; native rasterization is outside that measurement.");
        scene.App.Snapshot("MarketList");
        var scroll = scene.List.Scroller;
        var p = scroll.Parent!.LocalToGlobal((scroll.Frame.X + 100, scroll.Frame.Y + MarketGrid.IntroHeight + 50));
        scene.App.PointerDown(p); scene.App.PointerUp(p); scene.Advance();
        scene.App.Snapshot("CompanyDetails");
        Assert.NotNull(scene.App.Window.RootView.FindViewById("Market.Detail"));
        scene.Navigation.Pop(); scene.Advance();
        var wheel = new ScrollWheelEventRef { Delta = (0, -363) };
        scroll.OnScrollWheel(ref wheel); scene.App.Render();
        scene.App.Snapshot("RowsUnderHeader");
    }

    [Fact]
    public void DesktopAdapterAlsoReturnsThroughTheListBeforeHome()
    {
        var home = new View();
        var navigation = new SdkNavigation(home);
        navigation.PushExamplePage<MarketExample>();
        var listPage = Assert.IsType<SdkExamplePage<MarketExample>>(navigation[0]);
        var stock = new MarketDetailExample(MarketData.Shared[42]);
        navigation.PushExamplePage(stock);
        Assert.Same(stock, navigation[0][0]);
        navigation.PopExample();
        Assert.Same(listPage, navigation[0]);
        navigation.PopExample();
        Assert.Same(home, navigation[0]);
    }

    [Fact]
    public void ScrollingPaintsColorfulRowsBehindTheNativeHeaderSurface()
    {
        using var scene = new Scene();
        var scroll = scene.List.Scroller;
        var wheel = new ScrollWheelEventRef { Delta = (0, -363) };
        scroll.OnScrollWheel(ref wheel); scene.App.Render();
        Assert.False(scroll.ClipToViewport);
        Assert.Equal(0, scene.List.Grid.Count);
        // First logo straddles the header, above the scroll view's nominal hit-test viewport.
        var logoTop = scroll.Frame.Y + MarketGrid.IntroHeight - scroll.ScrollOffset.Y + 26;
        Assert.True(logoTop < scroll.Frame.Y && logoTop + 40 > scroll.Frame.Y - 48);
        using var measure = new SvgDrawingContext(scene.App.Window.DisplayArea.Size, Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        recorder.BeginFrame(scene.App.Window.DisplayArea, 1);
        scene.App.Window.RootView.Render(recorder);
        recorder.EndFrame();
        var nodes = Nodes(recorder.Root).ToArray();
        var header = Assert.Single(nodes, n => n.Options.Key is MobileExampleHeader);
        var content = Assert.Single(nodes, n => n.Options.Key is MarketExample);
        Assert.Equal(scene.App.Window.DisplayArea, content.RasterBounds);
        Assert.Equal(BackdropMaterial.Blur, header.Options.Backdrop);
        Assert.Null(header.Options.Background);
        Assert.True(Array.IndexOf(nodes, content) < Array.IndexOf(nodes, header));
        Assert.True(scene.List.Grid.RenderedRowCount < 12);

        static IEnumerable<Node> Nodes(Node node)
        {
            yield return node;
            for (var i = 0; i < node.Count; i++)
                foreach (var child in Nodes(node[i])) yield return child;
        }
    }

    private sealed class EmbeddedFonts : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(Xui.Runtime.Software.Font.FontFace face, Uri? uri) =>
            new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }

    private sealed class Scene : IDisposable
    {
        public readonly TestSinglePageApp<Application, MainWindow> App;
        public readonly MarketListPage List;
        public readonly NavigationView Navigation;
        private int time;
        public Scene(bool snapshot = false, TestRuntimeVariant variant = TestRuntimeVariant.IPhoneEmulator)
        {
            App = new((478, 996), configure: s => s.AddSingleton<ITestSettings>(new TestSettings()),
                runtimeVariant: variant, fontSource: Xui.Core.Fonts.Inter.Source,
                testName: snapshot ? nameof(MarketListAndDetails_RenderReference) : "MarketInteraction");
            App.Render();
            var link = App.Window.RootView.FindViewById("MarketPulse")!;
            Assert.True(link.TryFindParent<NavigationView>(out var navigation));
            Navigation = navigation;
            navigation.Push(new MobileExamplePage<MarketExample>());
            Advance();
            List = Assert.IsType<MarketListPage>(App.Window.RootView.FindViewById("Market.List"));
        }
        public Point FirstRowPoint => List.Scroller.Parent!.LocalToGlobal(
            (List.Scroller.Frame.X + 100, List.Scroller.Frame.Y + MarketGrid.IntroHeight + 50));
        public string DrawList()
        {
            using var stream = new MemoryStream();
            using (var context = new SvgDrawingContext((1000, 1500), stream, Xui.Core.Fonts.Inter.Source, keepOpen: true))
                List.Scroller.Render(context);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
        public IEnumerable<XElement> Ripples() => XDocument.Parse(DrawList())
            .Descendants(XName.Get("path", "http://www.w3.org/2000/svg"))
            .Where(p => (double?)p.Attribute("fill-opacity") is > 0 and <= .12);
        public void Advance(int milliseconds = 500)
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + milliseconds));
            time += milliseconds; App.Render();
        }
        public void Dispose() => App.Dispose();
    }
}
