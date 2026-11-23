using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class FloorplanLoadingTest
{
    [Fact]
    public async Task BackgroundCompletionKeepsPlaceholderCachedUntilForwardTransitionEnds()
    {
        using var app = CreateApp();
        app.Render();
        var button = app.RequireView("OfficeFloorplan", "opening the loading floorplan");
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        var source = new TaskCompletionSource<FloorplanDrawing>(TaskCreationOptions.RunContinuationsAsynchronously);
        var page = new FloorplanExample(source.Task);
        navigation.Push(new MobileExamplePage<FloorplanExample>(page));
        app.Render();
        var placeholder = app.RequireView("Floorplan.Loading", "showing the loading placeholder");
        Assert.Null(app.Window.RootView.FindViewById("Floorplan.Map"));
        using var measure = new SvgDrawingContext(placeholder.Frame.Size, Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        GraphicsNode Record()
        {
            recorder.BeginFrame(app.Window.DisplayArea, 3);
            app.Window.RootView.Render(recorder);
            recorder.EndFrame();
            return Find(recorder.Root)!;
        }
        GraphicsNode? Find(Node node)
        {
            if (ReferenceEquals(node.Options.Key, page)) return (GraphicsNode)node;
            for (var i = 0; i < node.Count; i++)
                if (Find(node[i]) is { } found) return found;
            return null;
        }
        var surface = Record();
        Assert.NotNull(surface);
        recorder.AcceptFrame();
        var drawing = FloorplanDrawing.Shared;
        await Task.Run(() => source.SetResult(drawing));
        Assert.Null(app.Window.RootView.FindViewById("Floorplan.Map")); // Worker never mutates the UI tree.
        var time = 0;
        for (var i = 0; i < 8; i++)
        {
            Tick();
            Assert.True(navigation.IsTransitioning);
            Assert.Null(app.Window.RootView.FindViewById("Floorplan.Map"));
            Assert.Same(surface, Record());
            Assert.False(surface.Updating);
            recorder.AcceptFrame();
        }
        for (var i = 0; navigation.IsTransitioning && i < 60; i++) Tick();
        Assert.False(navigation.IsTransitioning);
        Tick(); Record();
        Assert.NotNull(app.Window.RootView.FindViewById("Floorplan.Map"));
        Assert.Null(app.Window.RootView.FindViewById("Floorplan.Loading"));
        Assert.True(surface.Updating);
        void Tick()
        {
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + 16));
            time += 16;
        }
    }

    [Fact]
    public void SlowLoadShimmersAfterTransitionAndFailureStopsLoading()
    {
        using var app = CreateApp();
        app.Render();
        var button = app.RequireView("OfficeFloorplan", "opening the loading floorplan");
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        var source = new TaskCompletionSource<FloorplanDrawing>();
        navigation.Push(new MobileExamplePage<FloorplanExample>(new(source.Task)));
        app.Render();
        app.Advance(TimeSpan.FromMilliseconds(500));
        Assert.False(navigation.IsTransitioning);
        var placeholder = app.RequireView("Floorplan.Loading", "checking the shimmer");
        string Draw()
        {
            using var stream = new MemoryStream();
            using (var context = new SvgDrawingContext(placeholder.Frame.Size, stream, Xui.Core.Fonts.Inter.Source, keepOpen: true))
                placeholder.Render(context);
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        var first = Draw();
        var xml = System.Xml.Linq.XDocument.Parse(first);
        Assert.Equal(4, xml.Descendants(System.Xml.Linq.XName.Get("path", "http://www.w3.org/2000/svg")).Count());
        app.Advance(TimeSpan.FromMilliseconds(100));
        Assert.NotEqual(first, Draw());
        source.SetException(new FormatException("Invalid SVG"));
        app.Advance(TimeSpan.FromMilliseconds(16));
        Assert.NotNull(app.Window.RootView.FindViewById("Floorplan.LoadError"));
        Assert.Null(app.Window.RootView.FindViewById("Floorplan.Loading"));
        Assert.False((placeholder.Flags & (View.ViewFlags.Animated | View.ViewFlags.DescendantAnimated)) != 0);
    }

    [Fact]
    public async Task PreloadingSharesOneDrawingAndWarmPagesNeedNoPlaceholder()
    {
        var first = FloorplanDrawing.LoadSharedAsync();
        FloorplanDrawing.Preload();
        Assert.Same(first, FloorplanDrawing.LoadSharedAsync());
        var drawing = await first;
        Assert.Same(FloorplanDrawing.Shared, drawing);
        var page = new FloorplanExample();
        Assert.NotNull(page.FindViewById("Floorplan.Map"));
        Assert.Null(page.FindViewById("Floorplan.Loading"));
    }

    private static TestSinglePageApp<Application, MainWindow> CreateApp() => new((478, 996),
        configure: s => s.AddSingleton<ITestSettings>(new TestSettings()),
        runtimeVariant: TestRuntimeVariant.IPhoneEmulator, fontSource: Xui.Core.Fonts.Inter.Source);
}
