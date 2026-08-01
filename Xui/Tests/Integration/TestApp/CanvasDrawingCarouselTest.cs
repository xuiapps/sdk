using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.Canvas;
using Xui.Core.Math2D;
using Xui.Core.DI;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class CanvasDrawingCarouselTest
{
    [Fact]
    public void CardsSwipeSnapCancelAndPreserveBackNavigation()
    {
        var device = DeviceCatalog.All.Single(d => d.Model == "iPhone 15 Pro") with
        {
            LogicalResolution = new Size(430, 932),
            NotchFrame = new Rect(152, 11, 126, 36),
        };
        using var app = new TestSinglePageApp<Application, MainWindow>(new Size(478, 996),
            configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
            runtimeVariant: TestRuntimeVariant.IPhoneEmulator, emulatorDevice: device,
            snapshotSet: "Scenarios.Emulator.largeIPhone", fontSource: Xui.Core.Fonts.Inter.Source);
        app.SvgFontResolver = new EmbeddedFonts();
        app.Render();
        var button = app.Window.RootView.FindViewById("CanvasTests")!;
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        var page = new MobileExamplePage<CanvasTestsExample>();
        navigation.Push(page);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(350));
        app.Render();
        var example = Assert.IsType<CanvasTestsExample>(page[0]);
        var carousel = page.FindViewById("CanvasDrawingCarousel")!;
        var first = Assert.IsType<TransformView>(carousel[0]);
        var second = Assert.IsType<TransformView>(carousel[1]);
        var originalParent = first.Parent;
        var y = carousel.Frame.Center.Y;
        long time = 350;

        void Settle()
        {
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + 1000));
            time += 1000;
            app.Render();
        }

        void Swipe(nfloat from, nfloat to)
        {
            app.PointerDown((from, y));
            app.PointerMove((to, y));
            app.PointerUp((to, y));
            Settle();
        }

        app.MarkdownHeading("Canvas Drawing cards", 1);
        app.MarkdownParagraph("Swipe horizontally between retained drawing cards. Release settles one card in the center; the left screen edge remains available for back navigation.");
        var initial = app.Snapshot("FirstCard");
        Assert.Contains("Canvas Drawing", initial);
        Assert.Contains("Fill Rect", initial);
        Assert.Equal(2, page.Count); // Header and content, without the old footer/menu.
        Assert.Equal((nfloat)0, first.Transform.Tx);
        Assert.Equal((nfloat)1, first.Transform.A);
        Assert.InRange((double)second.Transform.A, 0.799, 0.801);
        var document = XDocument.Parse(initial);
        XNamespace ns = "http://www.w3.org/2000/svg";
        Assert.DoesNotContain(document.Descendants(ns + "rect"), r =>
            (string?)r.Attribute("fill") == "#FFFFFF" &&
            (string?)r.Attribute("width") == "600" && (string?)r.Attribute("height") == "300");

        app.PointerDown((330, y));
        app.PointerMove((155, y));
        app.Snapshot("Dragging");
        Assert.True(first.Transform.Tx < 0);
        Assert.InRange((double)first.Transform.A, 0.8, 1);
        Assert.InRange((double)second.Transform.A, 0.8, 1);
        Assert.Same(originalParent, first.Parent);
        app.PointerUp((155, y));
        Settle();
        app.Snapshot("SecondCard");
        Assert.Equal(1, example.SelectedSectionIndex);
        Assert.Equal((nfloat)0, second.Transform.Tx);

        // Cancel an active drag: return to the selected card without detaching it.
        app.PointerDown((300, y));
        app.PointerMove((200, y));
        var cancel = new PointerEventRef(PointerEventType.Cancel, 0, 0, true, default, [], []);
        carousel.OnPointerEvent(ref cancel, EventPhase.Bubble);
        app.PointerUp((200, y));
        Settle();
        Assert.Equal(1, example.SelectedSectionIndex);
        Assert.Equal((nfloat)0, second.Transform.Tx);

        Swipe(100, 320);
        Assert.Equal(0, example.SelectedSectionIndex);
        Assert.Equal((nfloat)0, first.Transform.Tx);
        Swipe(100, 320); // No wrapping before the first card.
        Assert.Equal(0, example.SelectedSectionIndex);

        example.SelectSection(example.SectionCount - 1);
        Settle();
        Swipe(330, 100); // No wrapping beyond the last card.
        Assert.Equal(example.SectionCount - 1, example.SelectedSectionIndex);
        Assert.Equal((nfloat)0, Assert.IsType<TransformView>(carousel[carousel.Count - 1]).Transform.Tx);
        app.Snapshot("LastCard");

        foreach (var name in new[] { "HeartCurve", "Star", "Text" })
        {
            var index = Enumerable.Range(0, example.SectionCount).Single(i => example.GetSectionName(i) == name);
            example.SelectSection(index);
            Settle();
            app.Snapshot(name);

        }

        // The carousel must not capture a swipe that begins at the navigation edge.
        app.PointerDown((10, y));
        app.PointerMove((280, y));
        Assert.True(navigation.IsTransitioning);
        app.PointerUp((280, y));
        Settle();
        Assert.False(navigation.CanGoBack);

        app.MarkdownHeading("Standalone Text Metrics", 2);
        app.MarkdownParagraph("Five separate samples show the actual glyph box, font bounding box, and alphabetic, ideographic, and hanging baselines. Each sample has one guide and a small heading.");
        var metricsButton = app.Window.RootView.FindViewById("TextMetrics")!;
        Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(metricsButton).Clicked!();
        Settle();
        var metricsPage = XDocument.Parse(app.Snapshot("TextMetricsPage"));
        foreach (var label in new[] { "Actual Glyph Box", "Font Bounding Box", "Alphabetic Baseline", "Ideographic Baseline", "Hanging Baseline" })
            Assert.Contains(metricsPage.Descendants(ns + "text"), t => t.Value == label);
        Assert.Equal(5, metricsPage.Descendants(ns + "text").Count(t => t.Value == "Hello World!"));
        Assert.DoesNotContain(metricsPage.Descendants(ns + "text"), t => t.Value.Contains("Em square"));
    }

    [Theory]
    [InlineData(DevicePlatform.iOS, true)]
    [InlineData(DevicePlatform.MacOS, true)]
    [InlineData(DevicePlatform.Windows, false)]
    [InlineData(DevicePlatform.Android, false)]
    public void CardEdgesFollowDeviceAndScaleEasesNearCenter(DevicePlatform platform, bool elastic)
    {
        using var app = new TestSinglePageApp<Application, MainWindow>((430, 932),
            configure: services =>
            {
                services.AddSingleton<ITestSettings>(new TestSettings());
                services.AddSingleton<IDeviceInfo>(new Device(platform));
                services.AddSingleton<TimeProvider>(new FrozenClock());
            }, fontSource: Xui.Core.Fonts.Inter.Source);
        app.Render();
        var button = app.Window.RootView.FindViewById("CanvasTests")!;
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        var page = new MobileExamplePage<CanvasTestsExample>();
        navigation.Push(page);
        long time = 0;
        void Advance(int ms)
        {
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + ms));
            time += ms; app.Render();
        }
        Advance(350);
        var example = Assert.IsType<CanvasTestsExample>(page[0]);
        var carousel = page.FindViewById("CanvasDrawingCarousel")!;
        var first = Assert.IsType<TransformView>(carousel[0]);
        nfloat Offset(TransformView card) => card.InverseTransformPoint(card.Frame.Center).X - card.Frame.Center.X;
        var y = carousel.Frame.Center.Y;
        app.PointerDown((120, y)); app.PointerMove((320, y));
        if (elastic) Assert.InRange((double)Offset(first), 1, 199);
        else Assert.Equal((nfloat)0, Offset(first));
        app.PointerUp((320, y)); Advance(80);
        if (elastic) Assert.True(Offset(first) > 0);
        // Touching catches the rebound without jumping; release resumes it.
        app.PointerDown((120, y)); var caught = Offset(first); Advance(80);
        Assert.Equal(caught, Offset(first));
        app.PointerUp((120, y)); Advance(1000);
        Assert.Equal((nfloat)0, Offset(first));
        Assert.Equal(0, example.SelectedSectionIndex);

        app.PointerDown((330, y)); app.PointerMove((260, y));
        Assert.InRange((double)first.Transform.A, 0.98, 0.9999); // Cubic scale holds near 100% as it leaves center.
        var cancel = new PointerEventRef(PointerEventType.Cancel, 0, 0, true, default, [], []);
        carousel.OnPointerEvent(ref cancel, EventPhase.Bubble);
        app.PointerUp((260, y)); Advance(1000);
        Assert.Equal((nfloat)1, first.Transform.A);

        example.SelectSection(example.SectionCount - 1); Advance(1000);
        var last = Assert.IsType<TransformView>(carousel[carousel.Count - 1]);
        app.PointerDown((330, y)); app.PointerMove((110, y));
        if (elastic) Assert.InRange((double)Offset(last), -219, -1);
        else Assert.Equal((nfloat)0, Offset(last));
        app.PointerUp((110, y)); Advance(1000);
        Assert.Equal((nfloat)0, Offset(last));
        Assert.Equal(example.SectionCount - 1, example.SelectedSectionIndex);
    }

    private sealed class FrozenClock : TimeProvider
    {
        public override long GetTimestamp() => 0;
    }

    private sealed class Device(DevicePlatform platform) : IDeviceInfo
    {
        public event Action? Changed { add { } remove { } }
        public DevicePlatform Platform => platform;
        public DeviceFormFactor FormFactor => DeviceFormFactor.Mobile;
        public PointerModel PointerModel => PointerModel.Touch;
        public nfloat AccessibilityFontScale => 1;
        public bool PrefersReducedMotion => false;
        public bool PrefersHighContrast => false;
        public ColorScheme ColorScheme => ColorScheme.Light;
    }

    private sealed class EmbeddedFonts : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) =>
            new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
