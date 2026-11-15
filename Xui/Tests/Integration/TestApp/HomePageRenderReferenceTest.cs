using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xui.Apps.TestApp;
using Xui.Core.Math2D;
using Xui.Runtime.Test;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Microsoft.Extensions.DependencyInjection;
using Xui.Core.UI;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Core.Canvas;

namespace Xui.Tests.Integration.TestApp;

public class HomePageRenderReferenceTest
{
    private static TestSinglePageApp<Application, MainWindow> CreateApp(
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string testName = "")
    {
        // Larger phone surface, retaining the existing emulator's device chrome
        // and safe-area behavior. This is a reference profile, not a new device claim.
        var device = DeviceCatalog.All.Single(d => d.Model == "iPhone 15 Pro") with
        {
            Model = "Large iPhone reference",
            LogicalResolution = new Size(430, 932),
            NotchFrame = new Rect(152, 11, 126, 36),
        };
        return new TestSinglePageApp<Application, MainWindow>(
            new Size(478, 996),
            configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
            runtimeVariant: TestRuntimeVariant.IPhoneEmulator,
            emulatorDevice: device,
            snapshotSet: "Scenarios.Emulator.largeIPhone",
            callerPath: callerPath,
            testName: testName,
            fontSource: Xui.Core.Fonts.Inter.Source);
    }

    [Fact]
    public void HomePage_RenderReference_WithEmbeddedFonts()
    {
        using var app = CreateApp();
        app.SvgFontResolver = new EmbeddedFontResolver();

        app.MarkdownHeading("TestApp home-screen rendering reference", level: 1);
        app.MarkdownParagraph("Mobile TestApp on a 430×932 logical-point iPhone reference surface, " +
            "including emulator frame, safe areas, and deterministic status bar (478×980 SVG). " +
            "Initial home screen, before navigation or pointer input.");
        app.MarkdownParagraph("Rendered by the deterministic software SVG context with embedded Inter font subsets. " +
            "The SVG records fills, strokes, text, transforms, and clipping for a future standalone drawing experiment; " +
            "it is a visual reference, not a recording of native CGContext calls or an iOS timing measurement.");

        var svg = app.Snapshot("HomePage");
        var document = XDocument.Parse(svg);
        XNamespace ns = "http://www.w3.org/2000/svg";
        Assert.Equal(ns + "svg", document.Root!.Name);
        Assert.Equal("478", (string?)document.Root.Attribute("width"));
        Assert.Equal("980", (string?)document.Root.Attribute("height"));
        Assert.Equal((nfloat)430, app.Window.DisplayArea.Width);
        Assert.Equal((nfloat)932, app.Window.DisplayArea.Height);
        Assert.True(app.Window.SafeArea.Height < app.Window.DisplayArea.Height);
        Assert.NotEmpty(document.Descendants(ns + "text"));
        var styles = string.Join("\n", document.Descendants(ns + "style").Select(style => style.Value));
        Assert.Contains("@font-face", styles);
        Assert.Contains("font-family: 'Inter'", styles);
        Assert.Contains("url(data:font/ttf;base64,", styles);
        Assert.DoesNotContain("url('http", styles);
        // The title, category heading, and navigation labels use different faces.
        // Checking each weight prevents silently falling back for Medium/SemiBold.
        foreach (var weight in new[] { 500, 600, 700 })
            Assert.Contains($"font-weight: {weight};", styles);

        foreach (var label in new[] { "Canvas", "Canvas Drawing", "Canvas Animations", "Layout", "Text Metrics", "Basics", "Grid Layout", "Flexbox Layout" })
        {
            var text = Assert.Single(document.Descendants(ns + "text"), t => t.Value == label);
            Assert.Equal("Inter", (string?)text.Attribute("font-family"));
        }
        Assert.DoesNotContain(document.Descendants(ns + "text"), t => t.Value == "Typography & Text");
        foreach (var id in new[] { "CanvasTests", "AnimatedHeart" })
        {
            var button = app.Window.RootView.FindViewById(id);
            Assert.NotNull(button);
            Assert.True(button.Frame.Width > 0 && button.Frame.Height > 0);
            Assert.True(button.Frame.Y >= app.Window.SafeArea.Y);
            Assert.True(button.Frame.Y + button.Frame.Height <= app.Window.SafeArea.Y + app.Window.SafeArea.Height);
        }
        Assert.Contains(document.Descendants(ns + "path"), p =>
            (string?)p.Attribute("fill") == "#F8F8F8" && p.Attribute("fill-opacity") is null);
        Assert.Single(FullScreenFills(document));
        // Only the emulator's physical display clip remains rounded at rest.
        Assert.Single(RoundedScreenClips(document));
        Assert.DoesNotContain(document.Descendants(ns + "linearGradient"), g =>
            (string?)g.Attribute("x2") == "430" && (string?)g.Attribute("y2") == "932");

        var firstButton = app.Window.RootView.FindViewById("CanvasTests")!;
        Assert.True(firstButton.TryFindParent<NavigationView>(out var navigation));
        Assert.True(((IFullScreenView)app.Window.RootView.Content!).DrawsOpaqueFullScreen);
        navigation.DefaultPageBackground = new Color(1f, 1f, 1f, 0.5f);
        Assert.False(navigation.DrawsOpaqueFullScreen);
        Assert.False(((IFullScreenView)app.Window.RootView.Content!).DrawsOpaqueFullScreen);
        Assert.Equal(2, FullScreenFills(XDocument.Parse(app.Render())).Count());
        navigation.DefaultPageBackground = Colors.White;

        app.MarkdownHeading("Moving top page");
        app.MarkdownParagraph("Push and pop a simple probe page. Only the moving top page gets a rounded clip; " +
            "one stationary background covers the display and back-page content is clipped to the exposed region. " +
            "Settled navigation draws only its active page. Opaque navigation keeps host clearing disabled throughout. " +
            "The probe uses linear easing so the midpoint snapshots show the top page halfway across the display.");
        navigation.TransitionEasing = new((0, 0), (1, 1));
        navigation.Push(new Label { Text = "Transition probe", FontFamily = "Inter", FontSize = 24 });
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(175));
        Assert.True(navigation.IsTransitioning);
        Assert.True(navigation.DrawsOpaqueFullScreen);
        var push = XDocument.Parse(app.Snapshot("PushHalfway"));
        Assert.Equal(2, FullScreenFills(push).Count());
        Assert.Equal(2, RoundedScreenClips(push).Count());
        app.AnimationFrame(TimeSpan.FromMilliseconds(175), TimeSpan.FromMilliseconds(350));
        Assert.False(navigation.IsTransitioning);
        Assert.True(navigation.DrawsOpaqueFullScreen);
        var settled = XDocument.Parse(app.Snapshot("Pushed"));
        Assert.Single(FullScreenFills(settled));
        Assert.DoesNotContain(settled.Descendants(ns + "text"), t => t.Value == "Canvas Drawing");
        Assert.Single(RoundedScreenClips(settled));

        navigation.Pop();
        app.AnimationFrame(TimeSpan.FromMilliseconds(350), TimeSpan.FromMilliseconds(350));
        app.AnimationFrame(TimeSpan.FromMilliseconds(350), TimeSpan.FromMilliseconds(525));
        var pop = XDocument.Parse(app.Snapshot("PopHalfway"));
        Assert.True(navigation.DrawsOpaqueFullScreen);
        Assert.Equal(2, FullScreenFills(pop).Count());
        Assert.Equal(2, RoundedScreenClips(pop).Count());
        app.AnimationFrame(TimeSpan.FromMilliseconds(525), TimeSpan.FromMilliseconds(700));
        Assert.False(navigation.IsTransitioning);
        Assert.True(navigation.DrawsOpaqueFullScreen);
        var restored = XDocument.Parse(app.Snapshot("Returned"));
        Assert.Single(FullScreenFills(restored));
        Assert.Single(RoundedScreenClips(restored));
        Assert.Equal(document.Descendants(ns + "text").Select(t => t.ToString()),
            restored.Descendants(ns + "text").Select(t => t.ToString()));
    }

    [Fact]
    public void HostClear_FollowsStationaryBackgroundCoverage()
    {
        using var app = CreateApp();
        app.Render();
        var button = app.Window.RootView.FindViewById("CanvasTests")!;
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        var coverage = (IFullScreenView)app.Window.RootView.Content!;

        navigation.DefaultPageBackground = new Color(1f, 1f, 1f, 0.5f);
        Assert.False(coverage.DrawsOpaqueFullScreen);
        Assert.Equal(2, FullScreenFills(XDocument.Parse(app.Render())).Count());

        // An opaque moving page cannot guarantee coverage at its exposed edges.
        navigation.Push(new OpaquePage());
        Assert.False(coverage.DrawsOpaqueFullScreen);
        Assert.Equal(3, FullScreenFills(XDocument.Parse(app.Render())).Count());
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(350));
        Assert.True(coverage.DrawsOpaqueFullScreen);
        Assert.Single(FullScreenFills(XDocument.Parse(app.Render())));

        // Popping exposes the translucent root again, so host clearing resumes.
        navigation.Pop();
        Assert.False(coverage.DrawsOpaqueFullScreen);
        Assert.Equal(3, FullScreenFills(XDocument.Parse(app.Render())).Count());
        app.AnimationFrame(TimeSpan.FromMilliseconds(350), TimeSpan.FromMilliseconds(350));
        app.AnimationFrame(TimeSpan.FromMilliseconds(350), TimeSpan.FromMilliseconds(700));
        Assert.False(coverage.DrawsOpaqueFullScreen);
        Assert.Equal(2, FullScreenFills(XDocument.Parse(app.Render())).Count());

        // Ordinary root content has no coverage promise and retains the host fill.
        app.Window.RootView.Content = new Label { Text = "Uncovered root" };
        Assert.Single(FullScreenFills(XDocument.Parse(app.Render())));
    }

    private sealed class OpaquePage : View, IPage
    {
        public PagePresentation Presentation => new() { Background = Colors.White };
    }

    private static IEnumerable<XElement> FullScreenFills(XDocument document) =>
        document.Descendants(XName.Get("rect", "http://www.w3.org/2000/svg"))
            .Where(r => r.Attribute("fill") is not null && (string?)r.Attribute("width") == "430" && (string?)r.Attribute("height") == "932");

    private static IEnumerable<XElement> RoundedScreenClips(XDocument document) =>
        document.Descendants(XName.Get("clipPath", "http://www.w3.org/2000/svg"))
            .Where(c => c.Elements().Any(p => ((string?)p.Attribute("d"))?.Contains("A 44 44") == true));

    private sealed class EmbeddedFontResolver : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) =>
            new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
