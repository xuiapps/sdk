using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Examples;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class CanvasAnimationsTest
{
    [Fact]
    public void HeartMorphAndDownloadAnimateInsideSwipeableCards()
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
        var button = app.Window.RootView.FindViewById("AnimatedHeart")!;
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(button).Clicked!();

        double time = 0;
        void At(double seconds)
        {
            app.AnimationFrame(TimeSpan.FromSeconds(time), TimeSpan.FromSeconds(seconds));
            time = seconds;
            app.Render();
        }

        At(0);
        At(0.35);
        var carousel = app.Window.RootView.FindViewById("CanvasAnimationsCarousel")!;
        Assert.NotNull(carousel);
        Assert.True(carousel.TryFindParent<CanvasAnimationsExample>(out var example));
        Assert.Equal(3, carousel.Count);
        var first = Assert.IsType<TransformView>(carousel[0]);
        var firstFrame = first.Frame;
        var parent = first.Parent;
        app.MarkdownHeading("Canvas Animations", 1);
        app.MarkdownParagraph("Three vector animations driven by frame time: a double-pulse heart, a rotating circle/square/flower morph, and a download arrow becoming a completion check. Cards retain their geometry and snap to the center after a swipe.");

        At(0.7);
        var rest = app.Snapshot("HeartRest");
        At(0.95);
        var pulse = app.Snapshot("HeartBeat");
        Assert.NotEqual(rest, pulse);
        Assert.Equal(firstFrame, first.Frame);
        Assert.Same(parent, first.Parent);

        var y = carousel.Frame.Center.Y;
        app.PointerDown((330, y));
        app.PointerMove((100, y));
        app.PointerUp((100, y));
        At(0.95);
        At(1.3);
        Assert.Equal(1, example.SelectedSectionIndex);
        At(2 / 1.5);
        var square = app.Snapshot("RoundedSquare");
        At(3.6 / 1.5);
        var flower = app.Snapshot("Flower");
        Assert.NotEqual(square, flower);
        Assert.Equal((nfloat)1, Assert.IsType<TransformView>(carousel[1]).Transform.A);
        Assert.InRange((double)first.Transform.A, 0.799, 0.801);

        app.PointerDown((330, y));
        app.PointerMove((100, y));
        app.PointerUp((100, y));
        At(3.6 / 1.5);
        At(3.6 / 1.5 + 0.35);
        Assert.Equal(2, example.SelectedSectionIndex);
        At(6.5 / 1.5);
        var downloading = app.Snapshot("Downloading");
        At(8.15 / 1.5);
        var transforming = app.Snapshot("ArrowToCheck");
        At(8.8 / 1.5);
        var complete = app.Snapshot("Complete");
        Assert.NotEqual(downloading, transforming);
        Assert.NotEqual(transforming, complete);
        Assert.Equal((nfloat)0, Assert.IsType<TransformView>(carousel[2]).Transform.Tx);

        navigation.Pop();
        At(8.8 / 1.5);
        At(8.8 / 1.5 + 0.35);
        Assert.False(navigation.CanGoBack);
        Assert.Null(app.Window.RootView.FindViewById("CanvasAnimationsCarousel"));
    }

    private sealed class EmbeddedFonts : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) =>
            new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
