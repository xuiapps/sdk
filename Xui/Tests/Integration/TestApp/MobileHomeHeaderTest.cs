using System.Runtime.CompilerServices;
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
        Assert.NotNull(root.FindViewById("RatingPanel"));
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

        void Tap(string id)
        {
            var view = root.FindViewById(id)!;
            var point = view.LocalToGlobal(view.Frame.Center);
            app.PointerDown(point);
            app.PointerUp(point);
        }
    }

    private sealed class EmbeddedFontResolver : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) => new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
