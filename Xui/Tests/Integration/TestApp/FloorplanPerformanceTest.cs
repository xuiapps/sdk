using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Xml.Linq;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Test;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Tests.Integration.TestApp;

public class FloorplanPerformanceTest
{
    [Fact]
    public void SharedDrawingResolvesAllFourLevelsAndReusesSymbolPaths()
    {
        var drawing = FloorplanDrawing.Shared;
        Assert.Same(drawing, FloorplanDrawing.Shared);
        Assert.True(drawing.PathCount > 200);
        Assert.True(drawing.PrimitiveCount(0) > 1000);
        Assert.True(drawing.PrimitiveCount(3) > drawing.PrimitiveCount(0) * 2);
        Assert.True(drawing.PrimitiveCount(3) > drawing.PathCount);
    }

    [Fact]
    public void RepeatedPanAndZoomRenderWithoutManagedAllocationsOrCompositionChildren()
    {
        using var measure = new SvgDrawingContext((430, 800), Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        var map = new FloorplanView();
        map.Measure((430, 800), measure); map.Arrange((0, 0, 430, 800), measure);
        void Frame()
        {
            recorder.BeginFrame((0, 0, 430, 800), 1);
            map.Render(recorder);
            recorder.EndFrame(); recorder.AcceptFrame();
        }
        // Include all symbol/style/gradient/text combinations before measuring steady state.
        var drawing = FloorplanDrawing.Shared;
        for (int level = 0; level < 4; level++)
        {
            recorder.BeginFrame((0, 0, 430, 800), 1);
            drawing.Render(recorder, level, (0, 120, 1000, 760), 1);
            recorder.EndFrame(); recorder.AcceptFrame();
        }
        for (int i = 0; i < 8; i++) Frame();
        long allocated = 0;
        for (int i = 0; i < 80; i++)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            map.ZoomAt((150, 350), (i % 20 < 10) ? (NFloat)1.2 : (NFloat)(1 / 1.2));
            Frame();
            allocated += GC.GetAllocatedBytesForCurrentThread() - before;
        }
        Assert.Equal(0, allocated);
        Assert.Equal(0, map.Count);
        Assert.Equal(0, recorder.Root.Count);
        Assert.True(recorder.Root.Commands.Count > 1000);
    }

    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    public void PinchKeepsAnchorThenRemainingFingerPansAndFreshTouchWorks(TestRuntimeVariant variant)
    {
        using var scene = new Scene(variant);
        var map = scene.Map;
        map.ZoomAt(map.Frame.Center, 3); scene.App.Render();
        var center = map.Parent!.LocalToGlobal(map.Frame.Center);
        var first = center + new Vector(-40, 0); var second = center + new Vector(40, 0);
        var before = map.VisibleWorld;
        scene.App.PointerDown(first, 0); scene.App.PointerDown(second, 1);
        scene.App.PointerMove(first + new Vector(-40, 0), 0);
        scene.App.PointerMove(second + new Vector(40, 0), 1);
        var after = map.VisibleWorld;
        Assert.InRange((double)(after.Width / before.Width), .4999, .5001);
        Assert.InRange((double)NFloat.Abs(after.Center.X - before.Center.X), 0, .001);
        Assert.InRange((double)NFloat.Abs(after.Center.Y - before.Center.Y), 0, .001);
        scene.App.PointerUp(first + new Vector(-40, 0), 0);
        scene.App.PointerMove(second + new Vector(60, 20), 1);
        Assert.True(map.VisibleWorld.X < after.X);
        Assert.True(map.VisibleWorld.Y < after.Y);
        scene.App.PointerUp(second + new Vector(60, 20), 1);
        var next = map.VisibleWorld;
        scene.App.PointerDown(center, 2); scene.App.PointerMove(center + new Vector(-20, 0), 2); scene.App.PointerUp(center, 2);
        Assert.True(map.VisibleWorld.X > next.X);
        Assert.Null(map.GetCapturedGesture(0)); Assert.Null(map.GetCapturedGesture(1)); Assert.Null(map.GetCapturedGesture(2));
        map.ZoomAt(map.Frame.Center, 100);
        Assert.InRange((double)map.ZoomPercent, 999.99, 1000.01);
        Assert.Equal(3, map.DetailLevel);
    }

    [Fact]
    public void CullingSkipsOffscreenRooms()
    {
        using var measure = new SvgDrawingContext((430, 800), Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        int Commands(Rect visible)
        {
            recorder.BeginFrame((0, 0, 430, 800), 1);
            FloorplanDrawing.Shared.Render(recorder, 3, visible);
            recorder.EndFrame(); recorder.AcceptFrame();
            return recorder.Root.Commands.Count;
        }
        var all = Commands((0, 120, 1000, 760));
        var room = Commands((80, 370, 20, 20));
        Assert.InRange(room, 1, all / 4);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WheelUsesPointerAnchorWithAndWithoutAnExplicitPosition(bool explicitPosition)
    {
        using var scene = new Scene();
        var map = scene.Map;
        // Enlarge beyond both viewport axes so the fit-centering clamp is inactive.
        map.ZoomAt(map.Frame.Center, 4); scene.App.Render();
        var anchor = map.Frame.Center + new Vector(40, 30);
        var global = map.LocalToGlobal(anchor);
        // Send an external mouse to the app; the phone harness intentionally suppresses hover.
        var move = new MouseMoveEventRef { Position = global };
        scene.App.Window.OnMouseMove(ref move);
        var before = WorldAt();
        var wheel = new ScrollWheelEventRef { HasPosition = explicitPosition, Position = global, Delta = (0, 80) };
        scene.App.Window.OnScrollWheel(ref wheel);
        var after = WorldAt();
        Assert.InRange((double)NFloat.Abs(after.X - before.X), 0, .001);
        Assert.InRange((double)NFloat.Abs(after.Y - before.Y), 0, .001);
        Point WorldAt() => (map.VisibleWorld.X + (anchor.X - map.Frame.X) / map.Frame.Width * map.VisibleWorld.Width,
            map.VisibleWorld.Y + (anchor.Y - map.Frame.Y) / map.Frame.Height * map.VisibleWorld.Height);
    }

    [Fact]
    public void WheelZoomClampsAndConstellationsFadeInBothDirections()
    {
        using var measure = new SvgDrawingContext((500, 500), Stream.Null, Xui.Core.Fonts.Inter.Source);
        var map = new FloorplanView(); map.Measure((500, 500), measure); map.Arrange((0, 0, 500, 500), measure);
        Assert.Equal(0, map.DetailLevel);
        var wheel = new ScrollWheelEventRef { HasPosition = true, Position = (250, 250), Delta = (0, 1000) };
        map.OnScrollWheel(ref wheel);
        Assert.True(wheel.Handled); Assert.InRange((double)map.ZoomPercent, 499.99, 500.01);
        Assert.Equal(3, map.DetailLevel);
        map.Animate(TimeSpan.Zero, TimeSpan.FromMilliseconds(110));
        Assert.InRange((double)map.MarkerOpacity, .49, .51);
        map.Animate(TimeSpan.FromMilliseconds(110), TimeSpan.FromMilliseconds(220));
        Assert.Equal((NFloat)0, map.MarkerOpacity);
        map.Fit(); map.Animate(TimeSpan.FromMilliseconds(220), TimeSpan.FromMilliseconds(330));
        Assert.InRange((double)map.MarkerOpacity, .49, .51);
        map.Animate(TimeSpan.FromMilliseconds(330), TimeSpan.FromMilliseconds(440));
        Assert.Equal((NFloat)1, map.MarkerOpacity);
        Assert.Equal(0, map.DetailLevel);
    }

    [Fact]
    public void Floorplan_RenderReference()
    {
        using var scene = new Scene();
        scene.App.SvgFontResolver = new EmbeddedFonts();
        scene.App.MarkdownHeading("Office floorplan: one vector map view", 1);
        scene.App.MarkdownParagraph("Drag to pan; pinch or use the mouse wheel to zoom around the pointer. " +
            "The embedded SVG is resolved once into shared paths and four detail levels. " +
            "On phones, overview markers fade at 320%; furniture changes at 250%, 440%, and 800%, up to 1000%. " +
            "Offscreen rooms are culled. The map creates no composition children or bitmap snapshots. " +
            "Allocation coverage measures warmed managed drawing/recording, not native rasterization.");
        scene.App.MarkdownParagraph("The map extends beneath the header and status bar. iOS uses the native backdrop blur " +
            "without an app-supplied background color; SVG approximates that native material.");
        scene.App.Snapshot("Overview");
        scene.Map.ZoomAt(scene.Map.Frame.Center, 6); scene.Advance();
        scene.App.Snapshot("Interior");
        scene.Map.ZoomAt(scene.Map.Frame.Center, 2); scene.Advance();
        scene.App.Snapshot("Detail");
        Assert.Equal(scene.App.Window.DisplayArea, scene.Map.Frame);
        using var measure = new SvgDrawingContext(scene.App.Window.DisplayArea.Size, Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        recorder.BeginFrame(scene.App.Window.DisplayArea, 1);
        scene.App.Window.RootView.Render(recorder);
        recorder.EndFrame();
        var nodes = Nodes(recorder.Root).ToArray();
        var header = Assert.Single(nodes, n => n.Options.Key is MobileExampleHeader);
        var content = Assert.Single(nodes, n => n.Options.Key is FloorplanExample);
        Assert.Equal(scene.App.Window.DisplayArea, content.RasterBounds);
        Assert.Equal(scene.App.Window.DisplayArea.Y, header.RasterBounds.Y);
        Assert.Equal(BackdropMaterial.Blur, header.Options.Backdrop);
        Assert.Null(header.Options.Background);
        Assert.True(Array.IndexOf(nodes, content) < Array.IndexOf(nodes, header));

        static IEnumerable<Node> Nodes(Node node)
        {
            yield return node;
            for (int i = 0; i < node.Count; i++)
                foreach (var child in Nodes(node[i])) yield return child;
        }
    }

    [Fact]
    public void ImporterHandlesRelativeCurvesArcsSymbolTransformsAndGradientCoordinates()
    {
        const string svg = """
            <svg xmlns="http://www.w3.org/2000/svg"><defs>
              <linearGradient id="metal" gradientUnits="userSpaceOnUse" x1="0" y1="-10" x2="0" y2="10">
                <stop offset="0" stop-color="#fff"/><stop offset="1" stop-color="#888"/>
              </linearGradient>
              <symbol id="piece" viewBox="-10 -10 20 20">
                <path d="M-10 0h2v2q2 4 4 0t4 0c1 2 2 3 4 0s3-2 4 0a5 5 0 0 1-5 5z" fill="#abc"/>
                <rect x="-2" y="-10" width="4" height="20" fill="url(#metal)"/>
              </symbol>
            </defs><use href="#piece" x="10" y="20" width="40" height="40" transform="rotate(30 10 20)"/></svg>
            """;
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var drawing = FloorplanDrawing.Load(input);
        using var output = new MemoryStream();
        using (var ctx = new SvgDrawingContext((200, 200), output, Xui.Core.Fonts.Inter.Source, keepOpen: true))
            drawing.Render(ctx, 3, (0, 0, 1000, 1000));
        var doc = XDocument.Parse(Encoding.UTF8.GetString(output.ToArray()));
        var ns = XNamespace.Get("http://www.w3.org/2000/svg");
        Assert.NotEmpty(doc.Descendants(ns + "path"));
        var gradient = Assert.Single(doc.Descendants(ns + "linearGradient"));
        Assert.Equal("-10", (string?)gradient.Attribute("y1"));
        Assert.Equal("10", (string?)gradient.Attribute("y2"));
        Assert.Equal(2, drawing.PathCount);
    }

    private sealed class EmbeddedFonts : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(Xui.Runtime.Software.Font.FontFace face, Uri? uri) =>
            new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }

    private sealed class Scene : IDisposable
    {
        public TestSinglePageApp<Application, MainWindow> App { get; }
        public FloorplanView Map { get; }
        private int time;
        public Scene(TestRuntimeVariant variant = TestRuntimeVariant.IPhoneEmulator)
        {
            App = new((478, 996), configure: s => s.AddSingleton<ITestSettings>(new TestSettings()),
                runtimeVariant: variant, fontSource: Xui.Core.Fonts.Inter.Source, testName: nameof(Floorplan_RenderReference));
            App.Render();
            var link = App.Window.RootView.FindViewById("OfficeFloorplan")!;
            Assert.NotNull(link);
            Assert.True(link.TryFindParent<NavigationView>(out var navigation));
            navigation.Push(new MobileExamplePage<FloorplanExample>());
            Advance();
            Map = Assert.IsType<FloorplanView>(App.Window.RootView.FindViewById("Floorplan.Map"));
        }
        public void Advance()
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + 500));
            time += 500; App.Render();
        }
        public void Dispose() => App.Dispose();
    }
}
