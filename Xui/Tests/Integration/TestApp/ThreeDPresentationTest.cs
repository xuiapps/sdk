using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.ThreeD;
using Xui.Apps.TestApp.Pages.ThreeD.Tests;
using Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class ThreeDPresentationTest
{
    [Theory]
    [InlineData(375)]
    [InlineData(441)]
    public void SettingsSeparateTextFromTogglesAndGateSoftwareRenderer(int width)
    {
        using var app = CreateApp(width);
        app.Render();
        var settingsButton = app.Window.RootView.FindViewById("Settings")!;
        Assert.True(settingsButton.TryFindParent<NavigationView>(out var navigation));
        var defaultPage = new MobileExamplePage<ThreeDExample>();
        navigation.Push(defaultPage);
        Settle(app);
        Assert.IsType<GpuHardwareCubeTestView>(defaultPage.FindViewById("ThreeDSurface"));
        Assert.Null(defaultPage.FindViewById("ThreeDRendererSwitch"));
        navigation.Pop();
        Settle(app);
        Tap(app, settingsButton);
        Settle(app);
        var performance = Assert.IsType<Toggle>(app.Window.RootView.FindViewById("PerformanceOverlayToggle"));
        var developer = Assert.IsType<Toggle>(app.Window.RootView.FindViewById("DeveloperFeaturesToggle"));
        var symbols = Assert.IsType<Toggle>(app.RequireView("FloorplanSymbolsToggle", "checking the floorplan rendering preference"));
        var exampleSettings = Assert.IsAssignableFrom<IExampleSettings>(symbols.GetService(typeof(IExampleSettings)));
        Assert.True(symbols.IsOn);
        Tap(app, symbols);
        Assert.False(exampleSettings.UseFloorplanSymbols);
        Tap(app, symbols);
        Assert.True(exampleSettings.UseFloorplanSymbols);
        var settings = Assert.IsType<ExampleSettings>(developer.GetService(typeof(ExampleSettings)));
        Assert.False(settings.ShowDeveloperFeatures);
        Assert.True(developer.Frame.Top > performance.Frame.Bottom);
        foreach (var label in Descendants(performance.Parent!).OfType<Label>())
        {
            Assert.False(Overlaps(label.Frame, performance.Frame), label.Text);
            Assert.False(Overlaps(label.Frame, developer.Frame), label.Text);
            Assert.False(Overlaps(label.Frame, symbols.Frame), label.Text);
        }
        Tap(app, performance);
        Assert.True(performance.IsOn);
        Tap(app, developer);
        Assert.True(settings.ShowDeveloperFeatures);

        navigation.Pop();
        Settle(app);
        var page = new MobileExamplePage<ThreeDExample>();
        navigation.Push(page);
        var panel = Assert.IsType<ThreeDExample.ThreeDTestPanel>(Assert.IsType<ThreeDExample>(page[0]).Content);
        Assert.IsType<GpuHardwareCubeTestView>(panel.Demo);
        Assert.False(panel.Demo.IsLive);
        Settle(app);
        Assert.True(panel.Demo.IsLive);
        var renderer = Assert.IsType<Button>(page.FindViewById("ThreeDRendererSwitch"));
        Tap(app, renderer);
        Assert.IsType<RotatingCubeTest>(panel.Demo);
        app.Render();
        Tap(app, renderer);
        Assert.IsType<GpuHardwareCubeTestView>(panel.Demo);
        Tap(app, renderer);
        settings.ShowDeveloperFeatures = false;
        Assert.IsType<GpuHardwareCubeTestView>(panel.Demo);
        Assert.Null(page.FindViewById("ThreeDRendererSwitch"));
        // Default hardware must also work gracefully in a headless runtime.
        Assert.Contains("Hardware rendering unavailable", app.Render());
    }

    [Fact]
    public void TransitionRendersOnceThenReusesFrameUntilVisibleIncludingCancelledSwipe()
    {
        using var app = CreateApp(441);
        app.Render();
        var homeButton = app.Window.RootView.FindViewById("Settings")!;
        Assert.True(homeButton.TryFindParent<NavigationView>(out var navigation));
        var page = new MobileExamplePage<CountingExample>();
        var cube = ((CountingExample)page[0]).Cube;
        navigation.Push(page);
        Assert.False(cube.IsLive);
        app.Render();
        Assert.Equal(1, cube.Frames);
        cube.ChangeScene();
        app.Render();
        Assert.Equal(1, cube.Frames);
        Settle(app);
        Assert.True(cube.IsLive);
        app.Render();
        Assert.Equal(2, cube.Frames);
        app.Render(); // Unchanged scene doesn't need another pixel upload either.
        Assert.Equal(2, cube.Frames);

        var start = navigation.LocalToGlobal((navigation.Frame.X + 2, navigation.Frame.Center.Y));
        var drag = start + new Vector(55, 0);
        app.PointerDown(start);
        app.PointerMove(drag);
        Assert.True(navigation.IsTransitioning);
        Assert.False(cube.IsLive);
        cube.ChangeScene();
        app.Render();
        Assert.Equal(2, cube.Frames);
        // Losing capture cancels deterministically, independent of fling timing.
        navigation.ReleasePointer(0);
        app.PointerUp(drag);
        Settle(app);
        Assert.True(cube.IsLive);
        app.Render();
        Assert.Equal(3, cube.Frames);

        navigation.Push(new MobileExamplePage<CountingExample>());
        Assert.False(cube.IsLive);
        cube.ChangeScene();
        Settle(app);
        navigation.Pop();
        app.Render();
        Assert.False(cube.IsLive);
        Assert.Equal(3, cube.Frames);
        Settle(app);
        Assert.True(cube.IsLive);
        app.Render();
        Assert.Equal(4, cube.Frames);
        navigation.Pop();
        app.Render();
        Assert.False(cube.IsLive);
        Settle(app);
        Assert.False(cube.IsLive);
        Assert.Equal(4, cube.Frames);
    }

    [Fact]
    public void NativePresentationInvalidatesPortableCaptureEvenWhileNavigationIsFrozen()
    {
        using var app = CreateApp(441);
        app.Render();
        var button = app.Window.RootView.FindViewById("Settings")!;
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        var page = new MobileExamplePage<CountingExample>();
        var cube = ((CountingExample)page[0]).Cube;
        navigation.Push(page);
        app.Render();
        Assert.Equal(1, cube.Frames);
        cube.NativeFramePresented();
        Assert.False(cube.IsLive);
        app.Render();
        Assert.Equal(2, cube.Frames); // Capture must regenerate the image after native output changes.
        app.Render();
        Assert.Equal(2, cube.Frames);
    }

    public sealed class CountingExample : Example, INavigationAware
    {
        public CountingCube Cube { get; } = new();
        public CountingExample() => Content = Cube;
        public void OnNavigationStateChanged(in NavigationStateChange change) =>
            Cube.IsLive = change.Phase == NavigationPhase.Appeared;
    }

    public sealed class CountingCube : CubeSurface
    {
        public int Frames { get; private set; }
        protected override string BackendLabel => "Test";
        protected override bool RenderPixels(byte[] pixels) { Frames++; return true; }
        public void ChangeScene() => InvalidateScene();
        public void NativeFramePresented() { NativeSceneRendered(); InvalidateRender(); }
    }

    private static TestSinglePageApp<Application, MainWindow> CreateApp(int width) =>
        new((width, 916), runtimeVariant: TestRuntimeVariant.IPhoneEmulator,
            fontSource: Xui.Core.Fonts.Inter.Source);

    private static void Settle(TestSinglePageApp<Application, MainWindow> app)
    {
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(400));
        app.Render();
    }

    private static void Tap(TestSinglePageApp<Application, MainWindow> app, View view)
    {
        var point = view.LocalToGlobal(view.Frame.Center);
        app.PointerDown(point);
        app.PointerUp(point);
        app.Render();
    }

    private static bool Overlaps(Rect a, Rect b) =>
        a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;

    private static IEnumerable<View> Descendants(View root)
    {
        for (int i = 0; i < root.Count; i++)
        {
            yield return root[i];
            foreach (var child in Descendants(root[i])) yield return child;
        }
    }
}
