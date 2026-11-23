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

public class FloorplanPerformanceTest(Xunit.Abstractions.ITestOutputHelper output)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MapUpdatesWaitForNavigationToFinish(bool cancelBackSwipe)
    {
        using var scene = new Scene(finishNavigation: cancelBackSwipe);
        var map = scene.Map;
        Assert.True(map.TryFindParent<NavigationView>(out var navigation));
        using var measure = new SvgDrawingContext(map.Frame.Size, Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        GraphicsNode Record()
        {
            recorder.BeginFrame(scene.App.Window.DisplayArea, 3);
            scene.App.Window.RootView.Render(recorder);
            recorder.EndFrame();
            return Find(recorder.Root)!;
        }
        static GraphicsNode? Find(Node node)
        {
            if (node.Options.Key is FloorplanExample) return (GraphicsNode)node;
            for (var i = 0; i < node.Count; i++)
                if (Find(node[i]) is { } found) return found;
            return null;
        }
        var surface = Record();
        Assert.NotNull(surface);
        recorder.AcceptFrame();
        var start = navigation.LocalToGlobal((navigation.Frame.Left + 2, navigation.Frame.Center.Y));
        if (cancelBackSwipe)
        {
            scene.App.PointerDown(start);
            scene.App.PointerMove(start + new Vector(55, 0));
            Record(); recorder.AcceptFrame();
        }
        Assert.True(navigation.IsTransitioning);
        // A camera change also starts a marker fade; neither may repaint during the slide.
        map.ZoomAt(map.Frame.Center, 6);
        for (var i = 0; i < 8; i++)
        {
            scene.Tick(16);
            Assert.True(navigation.IsTransitioning);
            Assert.Same(surface, Record());
            Assert.False(surface.Updating);
            Assert.Equal((NFloat)1, map.MarkerOpacity);
            recorder.AcceptFrame();
        }
        if (cancelBackSwipe)
        {
            navigation.ReleasePointer(0);
            scene.App.PointerUp(start + new Vector(55, 0));
        }
        for (var i = 0; navigation.IsTransitioning && i < 60; i++)
        {
            scene.Tick(16);
            if (navigation.IsTransitioning)
            {
                Assert.Same(surface, Record());
                Assert.False(surface.Updating);
                recorder.AcceptFrame();
            }
        }
        Assert.False(navigation.IsTransitioning);
        scene.Tick(16);
        Assert.Same(surface, Record());
        Assert.True(surface.Updating);
        Assert.True(map.MarkerOpacity < 1);
        recorder.AcceptFrame();
        scene.Tick(250);
        Assert.Equal((NFloat)0, map.MarkerOpacity);
    }

    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    public void IdleFrameTailAndInteractiveBackSwipeReuseTheFloorplanSurface(TestRuntimeVariant variant)
    {
        using var scene = new Scene(variant);
        var map = scene.Map;
        Assert.True(map.TryFindParent<NavigationView>(out var navigation));
        using var measure = new SvgDrawingContext(map.Frame.Size, Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        GraphicsNode Record()
        {
            recorder.BeginFrame(scene.App.Window.DisplayArea, 3);
            scene.App.Window.RootView.Render(recorder);
            recorder.EndFrame();
            return Find(recorder.Root)!;
        }
        static GraphicsNode? Find(Node node)
        {
            if (node.Options.Key is FloorplanExample) return (GraphicsNode)node;
            for (var i = 0; i < node.Count; i++)
                if (Find(node[i]) is { } found) return found;
            return null;
        }
        var surface = Record();
        Assert.NotNull(surface);
        Assert.True(surface.Updating);
        recorder.AcceptFrame();
        // FPS sampling may request frames after activation without repainting a stationary map.
        for (var i = 0; i < 8; i++)
        {
            scene.Tick(16);
            Assert.Same(surface, Record());
            Assert.False(surface.Updating);
            recorder.AcceptFrame();
        }
        var world = map.VisibleWorld;
        var start = navigation.LocalToGlobal((navigation.Frame.Left + 2, navigation.Frame.Center.Y));
        scene.App.PointerDown(start);
        scene.App.PointerMove(start + new Vector(55, 0));
        Assert.True(navigation.IsTransitioning);
        Record(); recorder.AcceptFrame(); // Allow gesture acquisition/first exposure to settle.
        foreach (var distance in new[] { 100, 160, 120, 180, 80, 55 })
        {
            scene.App.PointerMove(start + new Vector(distance, 0));
            scene.Tick(16);
            Assert.Same(surface, Record());
            Assert.False(surface.Updating);
            Assert.Equal(world, map.VisibleWorld);
            recorder.AcceptFrame();
        }
        // Cancel without depending on wall-clock fling velocity, then animate the page back.
        navigation.ReleasePointer(0);
        scene.App.PointerUp(start + new Vector(55, 0));
        for (var i = 0; i < 40; i++)
        {
            scene.Tick(16);
            Assert.Same(surface, Record());
            Assert.False(surface.Updating);
            recorder.AcceptFrame();
        }
        Assert.False(navigation.IsTransitioning);
        // Camera changes must still repaint the retained map.
        map.ZoomAt(map.Frame.Center, 3);
        Assert.Same(surface, Record());
        Assert.True(surface.Updating);
        recorder.AcceptFrame();
    }

    [Fact]
    public void EmbeddedAssetUsesCompiledAttributesAndLoadsWithinAllocationBudget()
    {
        using var input = typeof(FloorplanDrawing).Assembly.GetManifestResourceStream("TestApp.Floorplan.Office.svg")!;
        var document = XDocument.Load(input);
        Assert.Equal("office-attributes-v1", (string?)document.Root!.Attribute("data-compiled"));
        Assert.DoesNotContain(document.Descendants(), e => e.Name.LocalName == "style" || e.Attribute("style") is not null);
        Assert.DoesNotContain(document.Descendants().Attributes(), a => a.Value.Contains("var(") || a.Value == "currentColor");
        using var load = typeof(FloorplanDrawing).Assembly.GetManifestResourceStream("TestApp.Floorplan.Office.svg")!;
        var before = GC.GetAllocatedBytesForCurrentThread();
        var drawing = FloorplanDrawing.Load(load);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        // Includes XML, all four levels, paths and symbols. The CSS importer used >1.5 GiB.
        Assert.InRange(allocated, 0, 32 * 1024 * 1024);
        Assert.Equal(new[] { 1121, 1268, 2795, 7014 }, Enumerable.Range(0, 4).Select(drawing.PrimitiveCount));
    }

    [Theory]
    [InlineData("<style>path { fill: #fff; }</style><path d='M0 0L1 1'/>")]
    [InlineData("<path style='fill: #fff' d='M0 0L1 1'/>")]
    [InlineData("<path fill='var(--ink)' d='M0 0L1 1'/>")]
    public void ImporterRejectsUncompiledCss(string content)
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes($"<svg xmlns='http://www.w3.org/2000/svg'>{content}</svg>"));
        Assert.Throws<NotSupportedException>(() => FloorplanDrawing.Load(input));
    }

    [Fact]
    public void PanCoastsWithDecayAndNewTouchStopsIt()
    {
        using var scene = new Scene();
        var map = scene.Map;
        map.ZoomAt(map.Frame.Center, 6);
        var start = map.Parent!.LocalToGlobal(map.Frame.Center);
        scene.App.PointerDown(start);
        for (int i = 1; i <= 5; i++)
        {
            scene.Tick(16);
            scene.App.PointerMove(start + new Vector(i * 4, 0));
        }
        scene.App.PointerUp(start + new Vector(20, 0));
        var release = map.VisibleWorld.X;
        scene.Tick(16); scene.Tick(100);
        var first = map.VisibleWorld.X;
        scene.Tick(100);
        var second = map.VisibleWorld.X;
        Assert.True(first < release);
        Assert.InRange((double)(first - second), .001, (double)(release - first));
        scene.App.PointerDown(start);
        var stopped = map.VisibleWorld;
        scene.Tick(200);
        Assert.Equal(stopped, map.VisibleWorld);
        scene.App.PointerUp(start);
        scene.Tick(16); scene.Tick(1000);
        Assert.True((map.Flags & View.ViewFlags.Animated) != 0);
        scene.Tick(1100);
        Assert.Equal(stopped, map.VisibleWorld);
        Assert.False((map.Flags & View.ViewFlags.Animated) != 0);
    }

    [Fact]
    public void PinchZoomCoastsAroundItsAnchorThenKeepsSamplingFramesForTwoSeconds()
    {
        using var scene = new Scene();
        var map = scene.Map;
        map.ZoomAt(map.Frame.Center, 5);
        var center = map.Parent!.LocalToGlobal(map.Frame.Center);
        scene.App.PointerDown(center + new Vector(-40, 0), 0);
        scene.App.PointerDown(center + new Vector(40, 0), 1);
        for (int i = 1; i <= 4; i++)
        {
            scene.Tick(16);
            scene.App.PointerMove(center + new Vector(-40 - i * 3, 0), 0);
            scene.App.PointerMove(center + new Vector(40 + i * 3, 0), 1);
        }
        scene.App.PointerUp(center + new Vector(-52, 0), 0);
        scene.App.PointerUp(center + new Vector(52, 0), 1);
        var released = map.ZoomPercent;
        var anchor = map.VisibleWorld.Center;
        scene.Tick(16); scene.Tick(100);
        var first = map.ZoomPercent;
        scene.Tick(100);
        var second = map.ZoomPercent;
        Assert.InRange((double)NFloat.Abs(map.VisibleWorld.Center.X - anchor.X), 0, .001);
        Assert.InRange((double)NFloat.Abs(map.VisibleWorld.Center.Y - anchor.Y), 0, .001);
        Assert.True(first > released);
        Assert.InRange((double)(second - first), .001, (double)(first - released));
        scene.Tick(600);
        var settled = map.ZoomPercent;
        scene.Tick(600);
        Assert.Equal(settled, map.ZoomPercent);
        Assert.True((map.Flags & View.ViewFlags.Animated) != 0);
        scene.Tick(700);
        Assert.False((map.Flags & View.ViewFlags.Animated) != 0);
    }

    [Fact]
    public void HoldingBeforeReleaseOrLosingCaptureDoesNotFling()
    {
        using var scene = new Scene();
        var map = scene.Map;
        map.ZoomAt(map.Frame.Center, 6);
        var center = map.Parent!.LocalToGlobal(map.Frame.Center);
        foreach (var cancel in new[] { false, true })
        {
            scene.App.PointerDown(center);
            scene.Tick(16);
            scene.App.PointerMove(center + new Vector(20, 10));
            if (cancel) map.ReleasePointer(0);
            else scene.Tick(200);
            scene.App.PointerUp(center + new Vector(20, 10));
            var released = map.VisibleWorld;
            scene.Tick(16); scene.Tick(300);
            Assert.Equal(released, map.VisibleWorld);
        }
    }

    [Fact]
    public void SharedSettingsSwitchExistingMapBetweenSymbolsAndDirectDrawing()
    {
        using var scene = new Scene();
        var settings = Assert.IsAssignableFrom<IExampleSettings>(scene.Map.GetService(typeof(IExampleSettings)));
        Assert.True(settings.UseFloorplanSymbols);
        Assert.True(scene.Map.UseSymbols);
        Assert.Contains("<use href=\"#symbol", scene.App.Render());
        settings.UseFloorplanSymbols = false;
        Assert.False(scene.Map.UseSymbols);
        Assert.True(scene.Map.Flags.HasFlag(View.ViewFlags.RenderChanged));
        Assert.DoesNotContain("<use href=\"#symbol", scene.App.Render());
        settings.UseFloorplanSymbols = true;
        Assert.Contains("<use href=\"#symbol", scene.App.Render());
    }

    [Fact]
    public void InertiaAndFrameTailAllocateOnlyWhenCompositionVisibilityChanges()
    {
        using var scene = new Scene();
        var map = scene.Map;
        map.ZoomAt(map.Frame.Center, 6);
        var start = map.Parent!.LocalToGlobal(map.Frame.Center);
        scene.App.PointerDown(start);
        for (int i = 1; i <= 5; i++)
        {
            scene.Tick(16);
            scene.App.PointerMove(start + new Vector(i * 4, 0));
        }
        scene.App.PointerUp(start + new Vector(20, 0));
        using var measure = new SvgDrawingContext(map.Frame.Size, Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        // Compile every visible symbol before timing; coasting can reveal new rooms.
        recorder.BeginFrame(map.Frame, 1);
        map.Drawing.Render(recorder, map.DetailLevel, (0, 120, 1000, 760));
        recorder.EndFrame(); recorder.AcceptFrame();
        int frame = 0;
        void RenderFrame()
        {
            map.Animate(TimeSpan.FromMilliseconds(frame * 16), TimeSpan.FromMilliseconds(++frame * 16));
            recorder.BeginFrame(map.Frame, 1);
            map.Render(recorder);
            recorder.EndFrame(); recorder.AcceptFrame();
        }
        for (int i = 0; i < 10; i++) RenderFrame();
        var nodes = Nodes(recorder.Root).ToArray();
        var stableFrames = 0;
        for (int i = 0; i < 100; i++)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            RenderFrame();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            var next = Nodes(recorder.Root).ToArray();
            if (nodes.SequenceEqual(next))
            {
                Assert.Equal(0, allocated);
                stableFrames++;
            }
            nodes = next;
        }
        Assert.InRange(stableFrames, 80, 100);
        static IEnumerable<Node> Nodes(Node node)
        {
            yield return node;
            for (var i = 0; i < node.Count; i++)
                foreach (var child in Nodes(node[i])) yield return child;
        }
    }

    [Fact]
    public void SymbolsReduceRecordedInstructionsAtEveryDetailLevel()
    {
        using var measure = new SvgDrawingContext((1000, 760), Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        var drawing = FloorplanDrawing.Shared;
        int Record(int level, bool symbols)
        {
            recorder.BeginFrame((0,0,1000,760), 1);
            drawing.Render(recorder, level, (0,120,1000,760), useSymbols:symbols);
            recorder.EndFrame(); recorder.AcceptFrame();
            return recorder.Root.Commands.Count;
        }
        for (int level=0; level<4; level++)
        {
            var direct=Record(level,false); var symbols=Record(level,true);
            output.WriteLine($"L{level}: {drawing.SymbolPlacementCount(level)} symbol placements; {direct:N0} -> {symbols:N0} commands ({100d*(direct-symbols)/direct:F1}% fewer).");
            Assert.True(symbols < direct/2, $"LoD {level}: {symbols} cached commands vs {direct} direct commands.");
        }
    }

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
    public void SteadyZoomWithinADetailLevelReusesCompositionWithoutManagedAllocationsOrChildViews()
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
        // First exposure and LoD changes may allocate native surfaces. Stable placement
        // changes within a warmed level must reuse them without per-frame managed allocation.
        for (int i = 0; i < 8; i++) Frame();
        long allocated = 0;
        for (int i = 0; i < 80; i++)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            map.ZoomAt((150, 350), (i % 2 == 0) ? (NFloat)1.0001 : (NFloat)(1 / 1.0001));
            Frame();
            allocated += GC.GetAllocatedBytesForCurrentThread() - before;
        }
        Assert.Equal(0, allocated);
        Assert.Equal(0, map.Count);
        Assert.Equal(1, recorder.Root.Count);
        Assert.Equal(1, recorder.Root[0][0].Count);
        Assert.True(recorder.Root.Commands.Count < 100);
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
        var initialZoom = map.ZoomPercent;
        scene.Tick(16); scene.Tick(100); scene.Tick(100);
        Assert.True(map.ZoomPercent > initialZoom);
        after = WorldAt();
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
            "The embedded SVG has build-time CSS resolution and explicit presentation attributes; it loads into shared paths, reusable furniture symbols, and four detail levels. " +
            "On phones, overview markers fade at 320%; furniture changes at 250%, 440%, and 800%, up to 1000%. " +
            "Levels 0 and 1 each retain one whole-floorplan surface; levels 2 and 3 retain and cull individual rooms. Stable composition keys reuse native surfaces without adding Xui child views. " +
            "Allocation coverage measures warmed managed drawing/recording, not native rasterization.");
        scene.App.MarkdownParagraph("The map extends beneath the header and status bar. iOS uses the native backdrop blur " +
            "without an app-supplied background color; SVG approximates that native material. " +
            "The header FPS badge uses the shared rounded frame-rate sample and opens the detailed overlay when tapped.");
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
        Assert.Contains(nodes, n => n.Options.Key is FloorplanDrawing.Region { IsRoom: true });
        Assert.Contains(nodes, n => n.Options.Key is FloorplanDrawing.DetailKey { Level: 3 });
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
        public Scene(TestRuntimeVariant variant = TestRuntimeVariant.IPhoneEmulator, bool finishNavigation = true)
        {
            App = new((478, 996), configure: s => s.AddSingleton<ITestSettings>(new TestSettings()),
                runtimeVariant: variant, fontSource: Xui.Core.Fonts.Inter.Source, testName: nameof(Floorplan_RenderReference));
            App.Render();
            var link = App.RequireView("OfficeFloorplan", "opening the floorplan performance example");
            Assert.True(link.TryFindParent<NavigationView>(out var navigation));
            navigation.Push(new MobileExamplePage<FloorplanExample>(new(Task.FromResult(FloorplanDrawing.Shared))));
            if (finishNavigation) Advance();
            else App.Render();
            Map = Assert.IsType<FloorplanView>(App.Window.RootView.FindViewById("Floorplan.Map"));
        }
        public void Advance()
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + 500));
            time += 500; App.Render();
        }
        public void Tick(int milliseconds)
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + milliseconds));
            time += milliseconds;
        }
        public void Dispose() => App.Dispose();
    }
}
