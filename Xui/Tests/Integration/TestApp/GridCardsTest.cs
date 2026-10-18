using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.Grid;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class GridCardsTest
{
    [Fact]
    public void TallGridCardsSwipeAndScroll()
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
        var button = app.Window.RootView.FindViewById("GridLayout")!;
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(button).Clicked!();
        long time = 0;
        void Settle()
        {
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + 350));
            time += 350;
            app.Render();
        }
        Settle();
        var carousel = app.Window.RootView.FindViewById("GridLayoutCarousel")!;
        Assert.NotNull(carousel);
        Assert.True(carousel.TryFindParent<GridExample>(out var example));
        Assert.True(carousel.TryFindParent<MobileExamplePage<GridExample>>(out var page));
        Assert.Equal(2, page.Count);
        Assert.Equal(12, carousel.Count);
        Assert.Equal((nfloat)632, carousel[0].Frame.Height);
        var first = carousel[0];
        var parent = first.Parent;
        app.MarkdownHeading("Grid Layout cards", 1);
        app.MarkdownParagraph("Twelve grid examples in tall cards. Horizontal swipes snap one card into place with eased page dots; the scrollable grid also supports vertical dragging inside its card.");
        app.Snapshot("BasicFixedGrid");

        var y = carousel.Frame.Center.Y;
        app.PointerDown((330, y));
        app.PointerMove((155, y));
        app.Snapshot("Dragging");
        app.PointerUp((155, y));
        Settle();
        Assert.Equal(1, example.SelectedSectionIndex);
        Assert.Same(parent, first.Parent);

        for (var i = 1; i < example.SectionCount; i++)
        {
            example.SelectSection(i);
            Settle();
            Assert.Equal(i, example.SelectedSectionIndex);
            app.Snapshot(example.GetSectionName(i).Replace(" ", ""));
        }

        example.SelectSection(9); // Scrollable grid.
        Settle();
        var scroll = carousel[9];
        while (scroll is not ScrollView) scroll = scroll[0];
        Assert.Equal((nfloat)0, Assert.IsType<ScrollView>(scroll).ScrollbarEndInset);
        Assert.Equal(scroll.Parent!.Frame.Y, scroll.Frame.Y);
        Assert.Equal(scroll.Parent.Frame.Height, scroll.Frame.Height);
        var scrollingContent = Assert.IsAssignableFrom<TransformView>(scroll[0]);
        Assert.Equal((nfloat)0, scrollingContent.Transform.Ty);
        app.PointerDown((215, y + 100));
        app.PointerMove((215, y - 100));
        app.Snapshot("ScrolledGrid");
        Assert.True(scrollingContent.Transform.Ty < -100);
        Assert.Equal(9, example.SelectedSectionIndex);
        app.PointerUp((215, y - 100));
        // Horizontal swipes still work when the pointer starts inside the ScrollView.
        app.PointerDown((330, y));
        app.PointerMove((100, y));
        app.PointerUp((100, y));
        Settle();
        Assert.Equal(10, example.SelectedSectionIndex);

        app.PointerDown((10, y));
        app.PointerMove((280, y));
        Assert.True(navigation.IsTransitioning);
        app.PointerUp((280, y));
        Settle();
        Assert.False(navigation.CanGoBack);
    }

    private sealed class EmbeddedFonts : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) =>
            new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
