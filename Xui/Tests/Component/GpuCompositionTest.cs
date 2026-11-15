using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Composition.GPU;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;
using Xui.GPU.Hardware;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public class GpuCompositionTest
{
    [Fact]
    public void GraphicsGroupsContainTypedGpuLeavesAndFactoriesPreserveCachedNodes()
    {
        using var stream = new MemoryStream();
        using var svg = new SvgDrawingContext((300, 300), stream, keepOpen: true);
        var surfaces = new Factory();
        var graphicsNodes = new List<TestGraphicsNode>();
        var gpuNodes = new List<TestGpuNode>();
        using var scene = new RetainedCanvasContext(svg, surfaces,
            () => { var node = new TestGraphicsNode(); graphicsNodes.Add(node); return node; },
            () => { var node = new TestGpuNode(); gpuNodes.Add(node); return node; });
        var parentOptions = new GroupOptions { Key=new object(), Bounds=(0, 0, 200, 200), ViewBacking=true, HasUpdates=false };
        var gpuOptions = new GroupOptions { Key=new Content(), Bounds=(10, 20, 100, 100), HasUpdates=false };
        for (var frame = 0; frame < 2; frame++)
        {
            scene.BeginFrame((0, 0, 300, 300), 2);
            using (var group = scene.Group(parentOptions))
            {
                Assert.Equal(frame == 0, group.NeedsRedraw);
                if (group.NeedsRedraw)
                {
                    scene.SetFill(Colors.Red);
                    scene.FillRect((0, 0, 200, 200));
                    Assert.True(scene.GpuSurface(gpuOptions));
                }
            }
            scene.EndFrame();
            Assert.Equal(2, graphicsNodes.Count); // Root and 2D parent only.
            var gpu = Assert.Single(gpuNodes);
            Assert.Same(gpu, scene.Root[0][0]);
            Assert.Equal(0, gpu.Count);
            Assert.Same(Assert.Single(surfaces.Created), gpu.Surface);
            surfaces.Created[0].HasContent = true;
            scene.AcceptFrame();
        }
        Assert.Equal(1, surfaces.Created[0].Requests);
        scene.Dispose();
        Assert.All(graphicsNodes, node => Assert.Equal(1, node.Disposals));
        Assert.Equal(1, gpuNodes[0].Disposals);
        Assert.True(surfaces.Created[0].Disposed);
    }

    [Fact]
    public void KeyOwnsOneSurfaceAndPlacementAndCanvasPaintDoNotRedrawTheCube()
    {
        using var stream = new MemoryStream();
        using var svg = new SvgDrawingContext((300, 300), stream, keepOpen: true);
        var factory = new Factory();
        using var scene = new RetainedCanvasContext(svg, factory);
        var content = new Content();
        var options = new GroupOptions { Key=content, Bounds=(0, 0, 300, 300), HasUpdates=false };
        for (int i=0; i<3; i++)
        {
            scene.BeginFrame((0, 0, 300, 300), 2);
            scene.SetFilter(new Brightness((nfloat)(1 - i * .2)));
            scene.Translate((i * 20, 0));
            Assert.True(scene.GpuSurface(options));
            scene.EndFrame();
            var native = Assert.Single(factory.Created);
            Assert.Same(content, native.Content);
            Assert.Equal(1, native.Requests);
            Assert.Equal(0, scene.Root[0].Count);
            Assert.IsType<GpuNode>(scene.Root[0]);
            Assert.Equal((nfloat)(i * 20), scene.Root[0].Placement.Tx);
            native.HasContent=true; // A successful native presentation.
            scene.AcceptFrame();
        }
        Assert.Equal(0, content.Renders); // Declaring a node does not render before native layout.
    }

    [Fact]
    public void FailedFirstFrameResizeScaleAndSceneUpdatesRequestFreshFrames()
    {
        using var stream = new MemoryStream();
        using var svg = new SvgDrawingContext((300, 300), stream, keepOpen: true);
        var factory = new Factory();
        using var scene = new RetainedCanvasContext(svg, factory);
        var options = new GroupOptions { Key=new Content(), Bounds=(0, 0, 300, 300), HasUpdates=false };
        void Frame(GroupOptions o, nfloat scale)
        {
            scene.BeginFrame((0, 0, 300, 300), scale);
            Assert.True(scene.GpuSurface(o)); scene.EndFrame(); scene.AcceptFrame();
        }
        Frame(options, 1);
        Frame(options, 1); // Native drawable was unavailable: retry, even with no scene updates.
        var native = Assert.Single(factory.Created);
        Assert.Equal(2, native.Requests);
        native.HasContent=true;
        Frame(options, 1); Assert.Equal(2, native.Requests);
        Frame(options with { HasUpdates=true }, 1); Assert.Equal(3, native.Requests);
        Frame(options with { Bounds=(0, 0, 250, 300) }, 1); Assert.Equal(4, native.Requests);
        Frame(options with { Bounds=(0, 0, 250, 300) }, 2); Assert.Equal(5, native.Requests);
    }

    [Fact]
    public void OmittedGpuSurfacesReleaseAndCachedSurfacesRetainTheirNativeContent()
    {
        using var stream = new MemoryStream();
        using var svg = new SvgDrawingContext((300, 300), stream, keepOpen: true);
        var factory = new Factory();
        using var scene = new RetainedCanvasContext(svg, factory);
        var kept = new GroupOptions { Key=new Content(), Bounds=(0, 0, 100, 100), HasUpdates=false, CacheWhenHidden=true };
        scene.BeginFrame((0, 0, 300, 300), 1);
        scene.GpuSurface(kept);
        scene.GpuSurface(kept with { Key=new Content(), CacheWhenHidden=false });
        scene.EndFrame(); scene.AcceptFrame();
        factory.Created[0].HasContent=true;
        scene.BeginFrame((0, 0, 300, 300), 1); scene.EndFrame(); scene.AcceptFrame();
        Assert.False(factory.Created[0].Disposed);
        Assert.True(factory.Created[1].Disposed);
        scene.BeginFrame((0, 0, 300, 300), 1); scene.GpuSurface(kept); scene.EndFrame(); scene.AcceptFrame();
        Assert.Equal(2, factory.Created.Count);
        Assert.Equal(1, factory.Created[0].Requests);
        scene.Dispose();
        Assert.All(factory.Created, s => Assert.True(s.Disposed));
    }

    [Fact]
    public void UnsupportedAndClippedSurfacesLeaveRoomForPortableDrawing()
    {
        using var stream = new MemoryStream();
        using var svg = new SvgDrawingContext((300, 300), stream, keepOpen: true);
        var options = new GroupOptions { Key=new Content(), Bounds=(0, 0, 100, 100) };
        Assert.False(((IContext)svg).GpuSurface(options));
        foreach (bool supported in new[] { false, true })
        {
            var factory = new Factory { IsSupported=supported };
            using var scene = new RetainedCanvasContext(svg, factory);
            scene.BeginFrame((0, 0, 300, 300), 1);
            if (supported) { scene.BeginPath(); scene.Rect((0, 0, 50, 50)); scene.Clip(); }
            Assert.False(scene.GpuSurface(options));
            scene.FillRect((0, 0, 100, 100));
            scene.EndFrame();
            Assert.Equal(0, scene.Root.Count);
            Assert.True(scene.Root.HasDrawing);
            Assert.Empty(factory.Created);
        }
    }

    [Fact]
    public void GpuKeysRequireRendererInterfaceAndBoundsAndMustBeUniqueAmongSiblings()
    {
        using var stream = new MemoryStream();
        using var svg = new SvgDrawingContext((300, 300), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(svg, new Factory());
        scene.BeginFrame((0, 0, 300, 300), 1);
        Assert.Throws<ArgumentException>(() => scene.GpuSurface(new GroupOptions { Key=new object(), Bounds=(0, 0, 20, 20) }));
        Assert.Throws<ArgumentException>(() => scene.GpuSurface(new GroupOptions { Key=new Content() }));
        var options = new GroupOptions { Key=new Content(), Bounds=(0, 0, 100, 100) };
        Assert.True(scene.GpuSurface(options));
        Assert.Throws<InvalidOperationException>(() => scene.GpuSurface(options));
        Assert.Throws<InvalidOperationException>(() => scene.FillRect((0, 0, 10, 10)));
        scene.AbortFrame();
    }

    [Fact]
    public void SwitchingToCanvasAtTheSameKeyDisposesTheGpuResource()
    {
        using var stream = new MemoryStream();
        using var svg = new SvgDrawingContext((300, 300), stream, keepOpen: true);
        var factory = new Factory();
        using var scene = new RetainedCanvasContext(svg, factory);
        var options = new GroupOptions { Key=new Content(), Bounds=(0, 0, 100, 100), ViewBacking=true, HasUpdates=false };
        scene.BeginFrame((0, 0, 300, 300), 1); scene.GpuSurface(options); scene.EndFrame(); scene.AcceptFrame();
        var oldGpu = Assert.IsType<GpuNode>(scene.Root[0]);
        scene.BeginFrame((0, 0, 300, 300), 1);
        using (var group = scene.Group(options))
        {
            Assert.True(group.NeedsRedraw);
            scene.FillRect((0, 0, 100, 100));
        }
        scene.EndFrame();
        Assert.True(factory.Created[0].Disposed);
        Assert.Null(oldGpu.Surface);
        var graphics = Assert.IsType<GraphicsNode>(scene.Root[0]);
        Assert.True(graphics.HasDrawing);
        scene.AcceptFrame();
        scene.BeginFrame((0, 0, 300, 300), 1);
        scene.GpuSurface(options);
        scene.EndFrame();
        Assert.IsType<GpuNode>(scene.Root[0]);
        Assert.NotSame(oldGpu, scene.Root[0]);
        Assert.Equal(2, factory.Created.Count);
        Assert.Throws<ObjectDisposedException>(() => graphics.Commands.Clear());
    }

    private sealed class TestGraphicsNode : GraphicsNode
    {
        public int Disposals;
        protected override void DisposeCore() { Disposals++; base.DisposeCore(); }
    }
    private sealed class TestGpuNode : GpuNode
    {
        public int Disposals;
        protected override void DisposeCore() { Disposals++; base.DisposeCore(); }
    }

    private sealed class Content : IGpuSurfaceContent
    {
        public int Renders;
        public void Render(IGpuRenderTarget target) => Renders++;
    }
    private sealed class Factory : IGpuSurfaceFactory
    {
        public bool IsSupported { get; set; } = true;
        public List<Surface> Created { get; } = new();
        public ICompositionGpuSurface CreateSurface() { var s = new Surface(); Created.Add(s); return s; }
    }
    private sealed class Surface : ICompositionGpuSurface
    {
        public bool HasContent { get; set; }
        public int Requests;
        public bool Disposed;
        public IGpuSurfaceContent? Content;
        public void RequestRender(IGpuSurfaceContent content) { Requests++; Content=content; }
        public void Dispose() => Disposed=true;
    }
}
