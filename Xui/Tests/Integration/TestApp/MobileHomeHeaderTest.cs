using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Apps.TestApp;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class MobileHomeHeaderTest
{
    private static TestSinglePageApp<Application, MainWindow> CreateApp(TestRuntimeVariant variant,
        [CallerFilePath] string callerPath = "", [CallerMemberName] string testName = "") =>
        IntegrationRuntimeVariants.CreateApp<Application, MainWindow>((600, 400), variant,
            callerPath, testName, fontSource: Xui.Core.Fonts.Inter.Source);

    [Theory]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    public void HomeHeader_Parallax_Deadline_And_Rating(TestRuntimeVariant variant)
    {
        using var app = CreateApp(variant);
        app.SvgFontResolver = new EmbeddedFontResolver();
        app.MarkdownHeading("Home header", 1);
        app.MarkdownParagraph("The header is an overlay inside the scroll gesture route. Pulling moves it at 60% of the content's displacement. After the first 20 points, it collapses into a toolbar; a partial collapse finishes with easing after 0.7 seconds. Heart, FPS and settings remain tappable in either layout.");
        app.Snapshot("Expanded");
        var root = app.Window.RootView;
        var scroll = Assert.IsAssignableFrom<ScrollView>(root.FindViewById("HomeScroll"));
        var logo = root.FindViewById("HomeLogo")!;
        var initial = logo.LocalToGlobal(logo.Frame.Center);
        var start = new Point(app.Window.SafeArea.Center.X, app.Window.SafeArea.Y + 270);
        app.PointerDown(start);
        app.PointerMove(start + new Vector(0, 120));
        app.Render();
        Assert.True(scroll.ScrollOffset.Y < 0);
        var pulled = logo.LocalToGlobal(logo.Frame.Center);
        Assert.InRange((double)(pulled.Y - initial.Y + scroll.ScrollOffset.Y * 0.6f), -0.01, 0.01);
        app.Snapshot("Pulled");
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(250));
        app.PointerUp(start + new Vector(0, 120));
        app.AnimationFrame(TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250));
        app.AnimationFrame(TimeSpan.FromMilliseconds(250), TimeSpan.FromSeconds(2));
        app.Render();
        Assert.Equal((nfloat)0, scroll.ScrollOffset.Y);

        // Keep the finger still halfway through: the deadline must finish even without another move event.
        app.PointerDown(start);
        app.PointerMove(start - new Vector(0, 90));
        app.AnimationFrame(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        app.Render();
        var partial = logo.LocalToGlobal(logo.Frame.Center);
        app.Snapshot("PartlyCollapsed");
        app.AnimationFrame(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2.69));
        app.Render();
        Assert.Equal(partial, logo.LocalToGlobal(logo.Frame.Center));
        app.AnimationFrame(TimeSpan.FromSeconds(2.69), TimeSpan.FromSeconds(2.7));
        app.AnimationFrame(TimeSpan.FromSeconds(2.7), TimeSpan.FromSeconds(2.84));
        app.Render();
        var finishing = logo.LocalToGlobal(logo.Frame.Center);
        Assert.True(finishing.X < partial.X);
        app.AnimationFrame(TimeSpan.FromSeconds(2.84), TimeSpan.FromSeconds(3.1));
        app.PointerUp(start - new Vector(0, 90));
        app.Render();
        var collapsed = logo.LocalToGlobal(logo.Frame.Center);
        Assert.InRange((double)collapsed.X, 25, 27);
        Assert.InRange((double)collapsed.Y, (double)app.Window.SafeArea.Y + 27, (double)app.Window.SafeArea.Y + 29);
        app.Snapshot("Toolbar");

        Tap("RateApp");
        app.Render();
        var ratingPanel = root.FindViewById("RatingPanel")!;
        Assert.NotNull(ratingPanel);
        Assert.Equal(app.Window.DisplayArea.Center.X, ratingPanel.Frame.Center.X);
        Tap("Rating4");
        app.AnimationFrame(TimeSpan.FromSeconds(3.1), TimeSpan.FromSeconds(4));
        Assert.Contains("4 of 5 stars", app.Snapshot("Rating"));
        Tap("RatingDone");
        app.Render();
        Assert.Null(root.FindViewById("RatingPanel"));
        Tap("HomeFPS");
        Assert.Contains("Alloc", app.Render());
        Tap("HomeFPS");
        Assert.DoesNotContain(">Alloc<", app.Render());

        // Pull back to the top and let the header expand; it must not stay latched in the toolbar.
        app.PointerDown(start);
        app.PointerMove(start + new Vector(0, 16)); // Negotiate away from the menu button first.
        app.PointerMove(start + new Vector(0, 106));
        app.AnimationFrame(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4));
        app.AnimationFrame(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4.4));
        app.PointerUp(start + new Vector(0, 106));
        app.Render();
        Assert.Equal((nfloat)0, scroll.ScrollOffset.Y);
        Assert.Equal(initial, logo.LocalToGlobal(logo.Frame.Center));
        app.Snapshot("Returned");

        Tap("RateApp");
        app.Render();
        var outside = new Point(app.Window.DisplayArea.Right - 4, app.Window.SafeArea.Bottom - 8);
        app.PointerDown(outside); app.PointerUp(outside);
        Assert.Null(root.FindViewById("RatingPanel"));
        Tap("RateApp");
        app.Render();
        Assert.Equal(app.Window.DisplayArea.Center.X, root.FindViewById("RatingPanel")!.Frame.Center.X);
        var thirdStar = root.FindViewById("Rating3")!;
        var starPoint = thirdStar.LocalToGlobal(thirdStar.Frame.Center);
        app.MouseDown(starPoint); app.MouseUp(starPoint);
        Assert.Contains("3 of 5 stars", app.Render());
        Tap("RatingDone");
        Assert.Equal(1, root.Count); // Closed popup content is detached, including its subscriptions.


        void Tap(string id)
        {
            var view = root.FindViewById(id)!;
            var point = view.LocalToGlobal(view.Frame.Center);
            app.PointerDown(point);
            app.PointerUp(point);
        }
    }

    [Fact]
    public void FpsBadge_SamplesExistingFrames_WithoutRequestingAnimation()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>((496, 1072),
            configure: services => services.AddSingleton<ITestSettings>(new TestSettings(isDebugOverlayEnabled: true)),
            runtimeVariant: TestRuntimeVariant.AndroidPunchHoleEmulator,
            emulatorDevice: DeviceCatalog.All.Single(d => d.Model == "Pixel 8 Pro"),
            fontSource: Xui.Core.Fonts.Inter.Source);
        app.SvgFontResolver = new EmbeddedFontResolver();
        app.Render();
        var root = app.Window.RootView;
        var badge = root.FindViewById("HomeFPS")!;
        const View.ViewFlags animation = View.ViewFlags.Animated | View.ViewFlags.DescendantAnimated;
        Assert.NotEqual((View.ViewFlags)0, root.Flags & animation); // Finite startup spinner.
        for (var i = 1; i <= 180; i++)
        {
            app.AnimationFrame(TimeSpan.FromSeconds((i - 1) / 60d), TimeSpan.FromSeconds(i / 60d));
            if (i % 2 == 0) app.Render(); // Retained composition need not redraw on every tick.
            if (i == 12) Assert.DoesNotContain(">60</text>", app.Render()); // Number stays hidden during warmup.
            if (i < 120) Assert.NotEqual((View.ViewFlags)0, root.Flags & animation);
            if (i >= 125) Assert.Equal((View.ViewFlags)0, root.Flags & animation);
        }
        app.MarkdownHeading("Passive FPS badge", 1);
        app.MarkdownParagraph("A two-second startup spinner collects requested-frame timings before showing the number. Idle gaps are excluded; animation ticks remain 60 Hz even when retained content only redraws at 30 Hz. After warmup the badge requests no animation frames. The detailed overlay remains an explicit continuous-rendering mode.");
        var svg = app.Snapshot("PassiveBadge");
        Assert.Contains(">60</text>", svg);
        Assert.Contains(">fps</text>", svg);
        Assert.DoesNotContain(">Alloc</text>", svg);
        var point = badge.LocalToGlobal(badge.Frame.Center);
        app.PointerDown(point); app.PointerUp(point);
        app.AnimationFrame(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4));
        Assert.Contains(">Alloc</text>", app.Render());
        Assert.NotEqual((View.ViewFlags)0, root.Flags & animation);
        app.PointerDown(point); app.PointerUp(point);
        app.AnimationFrame(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4));
        app.AnimationFrame(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(5));
        app.Render();
        Assert.Equal((View.ViewFlags)0, root.Flags & animation);
    }

    [Theory]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    public void StatusBar_Appearance_ControlsEmulatedForeground(TestRuntimeVariant variant)
    {
        using var app = CreateApp(variant);
        var statusBar = Assert.IsAssignableFrom<Xui.Core.Actual.IStatusBar>(
            app.Window.GetService(typeof(Xui.Core.Actual.IStatusBar)));
        Assert.Equal(Xui.Core.Actual.StatusBarAppearance.Dark, statusBar.Appearance);
        static string NetworkColor(string svg) => System.Xml.Linq.XDocument.Parse(svg)
            .Descendants().Single(e => e.Name.LocalName == "text" && e.Value == "5G")
            .Attribute("fill")!.Value;
        Assert.Equal("#000000", NetworkColor(app.Render()));
        statusBar.Appearance = Xui.Core.Actual.StatusBarAppearance.Light;
        Assert.Equal("#FFFFFF", NetworkColor(app.Render()));
        statusBar.Appearance = Xui.Core.Actual.StatusBarAppearance.Auto;
        Assert.Equal("#000000", NetworkColor(app.Render())); // Emulator's device theme is light.
        Assert.Throws<ArgumentOutOfRangeException>(() => statusBar.Appearance = (Xui.Core.Actual.StatusBarAppearance)99);
    }

    private sealed class EmbeddedFontResolver : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) => new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
