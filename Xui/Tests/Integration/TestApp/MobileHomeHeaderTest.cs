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
using Xui.Core.Canvas.Composition;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Graphics;

namespace Xui.Tests.Integration.TestApp;

public class MobileHomeHeaderTest
{
    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    public void Header_BackdropIsRetainedBehindControlsWithoutAnOpaqueCover(TestRuntimeVariant variant)
    {
        using var app = CreateApp(variant);
        app.Render();
        var root = app.Window.RootView;
        var start = new Point(app.Window.SafeArea.Center.X, app.Window.SafeArea.Y + 500);
        app.PointerDown(start);
        app.PointerMove(start - new Vector(0, 350));
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.Render();

        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext(app.Window.DisplayArea.Size, stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame(app.Window.DisplayArea, 1);
        root.Render(scene);
        scene.EndFrame();

        var scroll = Assert.Single(Nodes(scene.Root), node => node.Count > 1 && ReferenceEquals(node.Options.Key, root.FindViewById("HomeScroll")));
        var backdrop = Assert.Single(Nodes(scroll), node =>
            node.RasterBounds.Width == app.Window.DisplayArea.Width &&
            node.RasterBounds.Height == app.Window.SafeArea.Y + 56);
        Assert.True(backdrop.Options.ViewBacking);
        if (variant == TestRuntimeVariant.IPhoneEmulator)
        {
            Assert.Equal(BackdropMaterial.Blur, backdrop.Options.Backdrop);
            Assert.Null(backdrop.Options.Background);
        }
        else
        {
            Assert.Equal(BackdropMaterial.None, backdrop.Options.Backdrop);
            Assert.InRange((double)backdrop.Options.Background!.Value.Alpha, 0.8999, 0.9001);
        }

        var backdropIndex = Enumerable.Range(0, scroll.Count).Single(i => ReferenceEquals(scroll[i], backdrop));
        var ids = new[] { "HomeLogo", "HomeTitle", "RateApp", "HomeFPS", "Settings" };
        for (var i = 0; i < ids.Length; i++)
            Assert.Same(root.FindViewById(ids[i])!.Parent, scroll[backdropIndex + 1 + i].Options.Key);
        // Only the separator is drawn on the backdrop; there must be no recorded white fill.
        using var drawing = new MemoryStream();
        using (var context = new SvgDrawingContext(app.Window.DisplayArea.Size, drawing, keepOpen: true))
            Assert.IsAssignableFrom<GraphicsNode>(backdrop).Commands.Replay(context);
        Assert.DoesNotContain("<rect", System.Text.Encoding.UTF8.GetString(drawing.ToArray()));

        static IEnumerable<Node> Nodes(Node parent)
        {
            yield return parent;
            for (var i = 0; i < parent.Count; i++)
                foreach (var node in Nodes(parent[i])) yield return node;
        }
    }

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
        app.MarkdownParagraph("The header is an overlay inside the scroll gesture route. Pulling moves it at 60% of the content's displacement. From offset 75 to 225 it collapses into a toolbar; the 0.7-second deadline starts a 0.56-second eased finish. Returning keeps the toolbar pinned until offset 175, then expands fully by offset 25, with the same deadline and easing. Expanded elements can move above the viewport. Heart, FPS and settings remain tappable in either layout.");
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
        app.PointerMove(start - new Vector(0, 145));
        app.AnimationFrame(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        app.Render();
        var partial = logo.LocalToGlobal(logo.Frame.Center);
        Assert.True(partial.Y < app.Window.SafeArea.Y); // No top-edge position clamp.
        app.Snapshot("PartlyCollapsed");
        app.AnimationFrame(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2.69));
        app.Render();
        Assert.Equal(partial, logo.LocalToGlobal(logo.Frame.Center));
        app.AnimationFrame(TimeSpan.FromSeconds(2.69), TimeSpan.FromSeconds(2.7));
        app.AnimationFrame(TimeSpan.FromSeconds(2.7), TimeSpan.FromSeconds(2.84));
        app.Render();
        var finishing = logo.LocalToGlobal(logo.Frame.Center);
        Assert.True(finishing.X < partial.X);
        app.AnimationFrame(TimeSpan.FromSeconds(2.84), TimeSpan.FromSeconds(3.4));
        app.PointerUp(start - new Vector(0, 145));
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
        app.AnimationFrame(TimeSpan.FromSeconds(3.4), TimeSpan.FromSeconds(4));
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
        app.PointerMove(start + new Vector(0, 36)); // Only 20 points of reverse travel.
        app.AnimationFrame(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4.5));
        app.Render();
        Assert.NotEqual(collapsed, logo.LocalToGlobal(logo.Frame.Center)); // Reverse travel is already inside the expansion interval.
        app.PointerMove(start + new Vector(0, 111)); // Continue through the 175-to-25 expansion interval.
        app.AnimationFrame(TimeSpan.FromSeconds(4.5), TimeSpan.FromSeconds(4.5));
        app.Render();
        var returning = logo.LocalToGlobal(logo.Frame.Center);
        Assert.True(returning.X > collapsed.X);
        Assert.True(returning.X < initial.X);
        app.Snapshot("PartlyExpanded");
        app.AnimationFrame(TimeSpan.FromSeconds(4.5), TimeSpan.FromSeconds(5.2));
        app.AnimationFrame(TimeSpan.FromSeconds(5.2), TimeSpan.FromSeconds(5.48));
        app.Render();
        Assert.NotEqual(initial, logo.LocalToGlobal(logo.Frame.Center)); // Only halfway through the slower finish.
        app.AnimationFrame(TimeSpan.FromSeconds(5.48), TimeSpan.FromSeconds(5.8));
        app.PointerMove(start + new Vector(0, 161));
        app.PointerUp(start + new Vector(0, 161));
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

    [Theory]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    public void Header_ReturnSpring_DoesNotRestartCollapse(TestRuntimeVariant variant)
    {
        using var app = CreateApp(variant);
        app.Render();
        var root = app.Window.RootView;
        var scroll = Assert.IsAssignableFrom<ScrollView>(root.FindViewById("HomeScroll"));
        var logo = root.FindViewById("HomeLogo")!;
        var initial = logo.LocalToGlobal(logo.Frame.Center);
        var start = new Point(app.Window.SafeArea.Center.X, app.Window.SafeArea.Y + 270);
        app.PointerDown(start);
        app.PointerMove(start - new Vector(0, 150));
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromSeconds(0.7));
        app.AnimationFrame(TimeSpan.FromSeconds(0.7), TimeSpan.FromSeconds(1.3));
        app.PointerUp(start - new Vector(0, 150));
        app.Render();
        Assert.NotEqual(initial, logo.LocalToGlobal(logo.Frame.Center));
        app.PointerDown(start);
        app.PointerMove(start + new Vector(0, 16));
        app.PointerMove(start + new Vector(0, 206));
        app.AnimationFrame(TimeSpan.FromSeconds(1.3), TimeSpan.FromSeconds(1.3));
        Assert.True(scroll.ScrollOffset.Y < 0);
        app.PointerUp(start + new Vector(0, 206));
        for (var i = 1; i <= 120; i++)
        {
            app.AnimationFrame(TimeSpan.FromSeconds(1.3 + (i - 1) / 60d), TimeSpan.FromSeconds(1.3 + i / 60d));
            app.Render();
        }
        Assert.Equal((nfloat)0, scroll.ScrollOffset.Y);
        Assert.Equal(initial, logo.LocalToGlobal(logo.Frame.Center));
    }

    [Theory]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator, false)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator, true)]
    [InlineData(TestRuntimeVariant.IPhoneEmulator, false)]
    [InlineData(TestRuntimeVariant.IPhoneEmulator, true)]
    public void Header_RapidReversals_AlwaysFinishTheAcceptedTransition(TestRuntimeVariant variant, bool duringFinish)
    {
        using var app = CreateApp(variant);
        app.Render();
        var root = app.Window.RootView;
        var scroll = Assert.IsAssignableFrom<ScrollView>(root.FindViewById("HomeScroll"));
        var items = new[] { "HomeLogo", "HomeTitle", "RateApp", "HomeFPS", "Settings" }
            .Select(id => root.FindViewById(id)!).ToArray();
        var expanded = items.Select(v => v.LocalToGlobal(v.Frame.Center)).ToArray();
        var start = new Point(app.Window.SafeArea.Center.X, app.Window.SafeArea.Y + 270);
        double time = 0;
        app.PointerDown(start);
        Move(-195);
        Advance(duringFinish ? 0.85 : 0.1);
        Move(-187); Move(-193); Move(-185); // None of these reversals crosses a new threshold.
        Advance(2);
        foreach (var view in items)
            Assert.InRange((double)view.LocalToGlobal(view.Frame.Center).Y,
                (double)app.Window.SafeArea.Y + 27.99, (double)app.Window.SafeArea.Y + 28.01);

        Move(-55); // Entering the 175-to-25 interval accepts expansion.
        Advance(duringFinish ? 0.85 : 0.1);
        Move(-63); Move(-57); Move(-64); // Small changes must not abandon expansion either.
        Advance(2);
        for (var i = 0; i < items.Length; i++)
        {
            var position = items[i].LocalToGlobal(items[i].Frame.Center);
            Assert.InRange((double)(position.X - expanded[i].X), -0.01, 0.01);
            Assert.InRange((double)(position.Y - expanded[i].Y + scroll.ScrollOffset.Y), -0.01, 0.01);
        }
        // Reverse the accepted target repeatedly, interrupting both pending and running finishes.
        Move(-180); Advance(0.85);
        Move(-55); Advance(0.85);
        Move(-195); Advance(0.85);
        Move(-50); Advance(0.1);
        Move(-190); Advance(0.1);
        Move(-184); // Last tiny reverse must still finish the accepted collapse.
        Advance(2);
        foreach (var view in items)
            Assert.InRange((double)view.LocalToGlobal(view.Frame.Center).Y,
                (double)app.Window.SafeArea.Y + 27.99, (double)app.Window.SafeArea.Y + 28.01);
        const View.ViewFlags animation = View.ViewFlags.Animated | View.ViewFlags.DescendantAnimated;
        Assert.Equal((View.ViewFlags)0, root.Flags & animation);
        app.PointerUp(start - new Vector(0, 184));

        void Move(int delta)
        {
            app.PointerMove(start + new Vector(0, delta));
            app.AnimationFrame(TimeSpan.FromSeconds(time), TimeSpan.FromSeconds(time));
            app.Render();
        }
        void Advance(double seconds)
        {
            var frames = (int)Math.Ceiling(seconds * 60);
            for (var i = 0; i < frames; i++)
            {
                app.AnimationFrame(TimeSpan.FromSeconds(time), TimeSpan.FromSeconds(time + 1d / 60));
                time += 1d / 60;
            }
            app.Render();
        }
    }

    [Theory]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    public void Header_OnlyExpandsNearTheTop(TestRuntimeVariant variant)
    {
        using var app = CreateApp(variant);
        app.Render();
        var root = app.Window.RootView;
        var scroll = Assert.IsAssignableFrom<ScrollView>(root.FindViewById("HomeScroll"));
        var items = new[] { "HomeLogo", "HomeTitle", "RateApp", "HomeFPS", "Settings" }
            .Select(id => root.FindViewById(id)!).ToArray();
        var expanded = items.Select(v => v.LocalToGlobal(v.Frame.Center)).ToArray();
        var start = new Point(app.Window.SafeArea.Center.X, app.Window.SafeArea.Y + 500);
        double time = 0;
        app.PointerDown(start);
        Move(-74); Settle();
        for (var i = 0; i < items.Length; i++)
        {
            var point = items[i].LocalToGlobal(items[i].Frame.Center);
            Assert.Equal(expanded[i].X, point.X);
            Assert.InRange((double)(point.Y - expanded[i].Y + scroll.ScrollOffset.Y), -0.01, 0.01);
        }
        Move(-76); Settle(); AssertToolbar();
        Move(-320); Settle(); AssertToolbar();
        Move(-240); Settle(); AssertToolbar();
        Move(-200); Settle(); AssertToolbar();
        Move(-190); Settle(); AssertToolbar();
        Move(-176); Settle(); AssertToolbar();
        Move(-170);
        // A long journey back must not jump straight to the expanded pose on entering the top band.
        Assert.InRange((double)items[0].LocalToGlobal(items[0].Frame.Center).X, 25, 40);
        Settle();
        for (var i = 0; i < items.Length; i++)
        {
            var point = items[i].LocalToGlobal(items[i].Frame.Center);
            Assert.InRange((double)(point.X - expanded[i].X), -0.01, 0.01);
            Assert.InRange((double)(point.Y - expanded[i].Y + scroll.ScrollOffset.Y), -0.01, 0.01);
        }
        Move(-180); Settle(); AssertToolbar(); // Downward travel accepts collapse again.
        app.PointerUp(start - new Vector(0, 180));

        void Move(int delta)
        {
            app.PointerMove(start + new Vector(0, delta));
            app.AnimationFrame(TimeSpan.FromSeconds(time), TimeSpan.FromSeconds(time));
            app.Render();
        }
        void Settle()
        {
            for (var i = 0; i < 120; i++)
            {
                app.AnimationFrame(TimeSpan.FromSeconds(time), TimeSpan.FromSeconds(time + 1d / 60));
                time += 1d / 60;
            }
            app.Render();
        }
        void AssertToolbar()
        {
            Assert.True(scroll.ScrollOffset.Y > 75);
            foreach (var view in items)
                Assert.InRange((double)view.LocalToGlobal(view.Frame.Center).Y,
                    (double)app.Window.SafeArea.Y + 27.99, (double)app.Window.SafeArea.Y + 28.01);
        }
    }

    [Theory]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    public void Header_ScrollRanges_CompleteAtSpecifiedOffsets(TestRuntimeVariant variant)
    {
        using var app = CreateApp(variant);
        app.Render();
        var root = app.Window.RootView;
        var items = new[] { "HomeLogo", "HomeTitle", "RateApp", "HomeFPS", "Settings" }
            .Select(id => root.FindViewById(id)!).ToArray();
        var expanded = items.Select(v => v.LocalToGlobal(v.Frame.Center)).ToArray();
        var start = new Point(app.Window.SafeArea.Center.X, app.Window.SafeArea.Y + 500);
        app.PointerDown(start);
        Move(75); AssertExpanded(75);
        Move(150); AssertBetween();
        Move(225); AssertToolbar();
        Move(175); AssertToolbar();
        Move(100); AssertBetween();
        Move(25); AssertExpanded(25);
        app.PointerUp(start - new Vector(0, 25));

        void Move(int offset)
        {
            app.PointerMove(start - new Vector(0, offset));
            // No elapsed time: completion must come from scroll distance, not the timer.
            app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
            app.Render();
        }
        void AssertExpanded(int offset)
        {
            for (var i = 0; i < items.Length; i++)
            {
                var point = items[i].LocalToGlobal(items[i].Frame.Center);
                Assert.InRange((double)(point.X - expanded[i].X), -0.01, 0.01);
                Assert.InRange((double)(point.Y - expanded[i].Y + offset), -0.01, 0.01);
            }
        }
        void AssertToolbar()
        {
            foreach (var view in items)
                Assert.InRange((double)view.LocalToGlobal(view.Frame.Center).Y,
                    (double)app.Window.SafeArea.Y + 27.99, (double)app.Window.SafeArea.Y + 28.01);
        }
        void AssertBetween()
        {
            var x = items[0].LocalToGlobal(items[0].Frame.Center).X;
            Assert.True(x > 26 && x < expanded[0].X);
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
