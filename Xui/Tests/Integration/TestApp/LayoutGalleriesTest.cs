using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages.Layout;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class LayoutGalleriesTest
{
    [Fact]
    public void BasicsAndFlexboxCards()
    {
        var device = DeviceCatalog.All.Single(d => d.Model == "iPhone 15 Pro") with
        {
            LogicalResolution = new Size(430, 932), NotchFrame = new Rect(152, 11, 126, 36),
        };
        using var app = new TestSinglePageApp<Application, MainWindow>(new Size(478, 996),
            configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
            runtimeVariant: TestRuntimeVariant.IPhoneEmulator, emulatorDevice: device,
            snapshotSet: "Scenarios.Emulator.largeIPhone", fontSource: Xui.Core.Fonts.Inter.Source);
        app.SvgFontResolver = new EmbeddedFonts();
        app.Render();
        var button = app.Window.RootView.FindViewById("Basics")!;
        Assert.NotNull(button);
        Assert.Null(app.Window.RootView.FindViewById("Layers"));
        Assert.Null(app.Window.RootView.FindViewById("ViewCollectionAlignment"));
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(button).Clicked!();
        long time = 0;
        void Advance(int milliseconds = 350)
        {
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + milliseconds));
            time += milliseconds;
            app.Render();
        }
        Advance();
        var basicsCarousel = app.Window.RootView.FindViewById("BasicsCarousel")!;
        Assert.True(basicsCarousel.TryFindParent<BasicsExample>(out var basics));
        app.MarkdownHeading("Layout galleries", 1);
        app.MarkdownParagraph("Basics explores text hierarchy, nested stacks, and nine-point alignment. Flexbox demonstrates live wrapping as its container narrows, proportional growth, and main-axis justification. Each gallery uses retained, snapping cards.");
        app.Snapshot("TextLayout");
        var y = basicsCarousel.Frame.Center.Y;
        app.PointerDown((330, y)); app.PointerMove((100, y)); app.PointerUp((100, y));
        Advance();
        Assert.Equal(1, basics.SelectedSectionIndex);
        app.Snapshot("NestedStacks");
        basics.SelectSection(2); Advance();
        app.Snapshot("Alignment");
        navigation.Pop(); Advance();
        var flexButton = app.Window.RootView.FindViewById("FlexboxLayout")!;
        Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(flexButton).Clicked!();
        Advance();
        var flexCarousel = app.Window.RootView.FindViewById("FlexboxCarousel")!;
        Assert.True(flexCarousel.TryFindParent<FlexboxExample>(out var flex));
        app.Snapshot("WrappingWide");
        var panel = app.Window.RootView.FindViewById("WrappingPanel")!;
        var chips = app.Window.RootView.FindViewById("WrappingChips")!;
        var wideWidth = panel.Frame.Width;
        var wideLastY = chips[chips.Count - 1].Frame.Y;
        Advance(2650);
        Assert.True(panel.Frame.Width < wideWidth);
        Assert.True(chips[chips.Count - 1].Frame.Y > wideLastY);
        app.Snapshot("WrappingNarrow");
        flex.SelectSection(1); Advance(); app.Snapshot("FlexibleSpace");
        flex.SelectSection(2); Advance(); app.Snapshot("JustifyContent");
        app.PointerDown((10, y)); app.PointerMove((280, y)); app.PointerUp((280, y));
        Advance();
        Assert.False(navigation.CanGoBack);
    }

    private sealed class EmbeddedFonts : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) =>
            new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
