using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;
namespace Xui.Tests.Component;

public class RetainedCompositionTest
{
    private static GraphicsNode SingleChild(GraphicsNode node)
    {
        Assert.Equal(1, node.Count);
        return Assert.IsAssignableFrom<GraphicsNode>(node[0]);
    }

    [Fact]
    public void NodeEnumerationUsesPresentationOrderWithoutAllocations()
    {
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), output, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame((0, 0, 200, 200), 1);
        for (var i = 0; i < 3; i++)
            using (scene.Group(new GroupOptions { Key = new object(), ViewBacking = true, Bounds = (0, 0, 100, 100) }))
                scene.FillRect((0, 0, 10, 10));
        scene.EndFrame();
        Node root = scene.Root;
        var visits = 0;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var repeat = 0; repeat < 100; repeat++)
        {
            var index = 0;
            foreach (var child in root)
            {
                if (!ReferenceEquals(root[index++], child)) throw new InvalidOperationException("Unexpected child order.");
                visits++;
            }
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(300, visits);
        Assert.Equal(0, allocated);
        Assert.Equal(0, root[0].Count);
        Assert.False(root[0].GetEnumerator().MoveNext());
    }

    [Fact]
    public void BaseNodeExposesPresentationTreeAndDisposesEachResourceOwnerOnce()
    {
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), output, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure, graphicsNodeFactory: static () => new TestNode());
        var key = new object();
        scene.BeginFrame((0, 0, 200, 200), 2);
        using (scene.Group(new GroupOptions { Key = key, ViewBacking = true, Bounds = (10, 20, 100, 100) }))
        {
            using (scene.Group(new GroupOptions { Key = new object(), ViewBacking = true, Bounds = (10, 20, 30, 40) }))
                scene.FillRect((10, 20, 30, 40));
        }
        scene.EndFrame();
        Node root = scene.Root;
        Assert.Equal(1, root.Count);
        var parent = Assert.IsType<TestNode>(root[0]);
        Assert.Equal(1, parent.Count);
        var child = Assert.IsType<TestNode>(parent[0]);
        Assert.True(typeof(Node.Enumerator).IsByRefLike);
        var enumerator = root.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Same(parent, enumerator.Current);
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        var traversed = 0;
        foreach (var node in root) { Assert.Same(parent, node); traversed++; }
        Assert.Equal(1, traversed);
        Assert.Same(key, parent.Options.Key);
        Assert.Equal(new Rect(10, 20, 100, 100), parent.RasterBounds);
        Assert.Equal((nfloat)2, parent.Scale);
        var parentOwner = new CountingResource();
        var childOwner = new CountingResource();
        parent.Resource = parentOwner;
        child.Resource = childOwner;
        scene.Dispose();
        scene.Dispose();
        Assert.Equal(1, parentOwner.DisposeCount);
        Assert.Equal(1, childOwner.DisposeCount);
        Assert.Null(parent.Resource);
        Assert.Null(child.Resource);
        Assert.Equal(0, root.Count);
    }

    [Fact]
    public void CulledSurfacesReuseTheirResourcesAndEvictOldestBeyondTheBound()
    {
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), output, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure, graphicsNodeFactory: static () => new TestNode());
        var keys = Enumerable.Range(0, 9).Select(_ => new object()).ToArray();
        var nodes = new GraphicsNode[9];
        var resources = new Resource[9];
        for (var i = 0; i < keys.Length; i++)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            using (var group = scene.Group(new GroupOptions
            { Key = keys[i], ViewBacking = true, Bounds = (0, 0, 100, 100), HasUpdates = false, CacheWhenHidden = true }))
            {
                Assert.True(group.NeedsRedraw);
                scene.FillRect((0, 0, 100, 100));
            }
            scene.EndFrame();
            nodes[i] = SingleChild(scene.Root);
            ((TestNode)nodes[i]).Resource = resources[i] = new Resource();
            scene.AcceptFrame();
        }
        scene.BeginFrame((0, 0, 200, 200), 1); scene.EndFrame(); scene.AcceptFrame();
        Assert.Equal(0, scene.Root.Count);
        Assert.Equal(8, scene.Root.HiddenChildren.Count);
        Assert.True(resources[0].Disposed);
        Assert.All(resources.Skip(1), resource => Assert.False(resource.Disposed));
        scene.BeginFrame((0, 0, 200, 200), 1);
        using (var group = scene.Group(new GroupOptions
        { Key = keys[1], ViewBacking = true, Bounds = (0, 0, 100, 100), HasUpdates = false, CacheWhenHidden = true }))
            Assert.False(group.NeedsRedraw);
        scene.EndFrame();
        Assert.Same(nodes[1], SingleChild(scene.Root));
        Assert.Same(resources[1], ((TestNode)nodes[1]).Resource);
        scene.Dispose();
        Assert.All(resources, resource => Assert.True(resource.Disposed));
    }

    [Fact]
    public void AncestorScrollTransformMovesSurfaceWithoutRedrawingItsContent()
    {
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), output, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var key = new object();
        for (var i = 0; i < 3; i++)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            scene.Save(); scene.Translate((0, -i * 30));
            using (var group = scene.Group(new GroupOptions
            { Key = key, ViewBacking = true, Bounds = (0, 0, 100, 100), HasUpdates = false }))
            {
                Assert.Equal(i == 0, group.NeedsRedraw);
                if (group.NeedsRedraw) scene.FillRect((0, 0, 100, 100));
            }
            scene.Restore(); scene.EndFrame();
            Assert.Equal((nfloat)(-i * 30), SingleChild(scene.Root).Placement.Ty);
            scene.AcceptFrame();
        }
    }

    [Fact]
    public void TextMetricsGlyphBoxMatchesImmediateRendering()
    {
        XDocument Render(bool retained)
        {
            var size = new Size(402, 700);
            using var output = new MemoryStream();
            using (var svg = new SvgDrawingContext(size, output, Xui.Core.Fonts.Inter.URIs, keepOpen: true))
            {
                using var scene = new RetainedCanvasContext(svg);
                IContext context = retained ? scene : svg;
                if (retained) scene.BeginFrame((0, 0, size.Width, size.Height), 3);
                var view = new Xui.Apps.TestApp.Examples.TextMetricsExample.TextMetricsView();
                var frame = new LayoutFrameContext(TimeSpan.Zero, TimeSpan.Zero, context, context, default);
                var update = new LayoutUpdate(
                    LayoutPass.Measure | LayoutPass.Arrange | LayoutPass.Render,
                    new MeasureConstraints(size, LayoutSizeMode.Exact, LayoutSizeMode.Exact),
                    new ArrangeConstraints(size, default, (0, 0)));
                LayoutMeasurements measurements = default;
                view.Update(in frame, in update, ref measurements);
                if (retained)
                {
                    scene.EndFrame();
                    scene.Root.Commands.Replay(svg);
                }
            }
            output.Position = 0;
            return XDocument.Load(output);
        }

        XNamespace ns = "http://www.w3.org/2000/svg";
        var direct = Render(false);
        var recorded = Render(true);
        // The example's first rectangle is computed from ActualBoundingBoxLeft/Right.
        Assert.Equal(direct.Descendants(ns + "rect").First().ToString(),
            recorded.Descendants(ns + "rect").First().ToString());
        Assert.Equal(direct.Descendants(ns + "text").Select(e => e.ToString()),
            recorded.Descendants(ns + "text").Select(e => e.ToString()));
    }

    [Fact]
    public void MeasurementTextStateRestoresAcrossSavesGroupsAndFrames()
    {
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), output, Xui.Core.Fonts.Inter.URIs, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var options = new GroupOptions { Key = new object(), ViewBacking = true, Bounds = (0, 0, 200, 200), HasUpdates = false };
        for (var i = 0; i < 2; i++)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            Assert.Equal(TextAlign.Start, measure.TextAlign);
            Assert.Equal(TextBaseline.Alphabetic, measure.TextBaseline);
            scene.SetFont(new Xui.Core.Canvas.Font(32, "Inter"));
            var left = scene.MeasureText("Hello World!");
            scene.TextAlign = TextAlign.Center;
            var center = scene.MeasureText("Hello World!".AsSpan());
            Assert.Equal(left.Line.ActualBoundingBoxLeft + left.Line.Width / 2, center.Line.ActualBoundingBoxLeft);
            Assert.Equal(left.Line.ActualBoundingBoxRight - left.Line.Width / 2, center.Line.ActualBoundingBoxRight);
            scene.Save();
            scene.TextAlign = TextAlign.Right;
            scene.TextBaseline = TextBaseline.Top;
            scene.SetFont(new Xui.Core.Canvas.Font(16, "Inter"));
            scene.Restore();
            Assert.Equal(center.Line, scene.MeasureText("Hello World!").Line);
            Assert.Equal(TextBaseline.Alphabetic, measure.TextBaseline);
            using (var group = scene.Group(options))
            {
                Assert.Equal(i == 0, group.NeedsRedraw);
                Assert.Equal(center.Line, scene.MeasureText("Hello World!").Line);
                if (group.NeedsRedraw)
                {
                    scene.TextAlign = TextAlign.Left;
                    scene.TextBaseline = TextBaseline.Bottom;
                    scene.SetFont(new Xui.Core.Canvas.Font(12, "Inter"));
                }
            }
            Assert.Equal(center.Line, scene.MeasureText("Hello World!").Line);
            Assert.Equal(TextBaseline.Alphabetic, measure.TextBaseline);
            scene.EndFrame();
            scene.AcceptFrame();
            // Native replay can leave the shared text service in another state.
            measure.TextAlign = TextAlign.Right;
            measure.TextBaseline = TextBaseline.Bottom;
        }
    }

    [Fact]
    public void TransformAndMaterialChangesReuseContentAndOmittedKeysReleaseResources()
    {
        using var output=new MemoryStream();
        using var measure=new SvgDrawingContext((200,200),output,keepOpen:true);
        using var scene=new RetainedCanvasContext(measure, graphicsNodeFactory: static () => new TestNode());
        var key=new object();
        var options=new GroupOptions { Key=key, ViewBacking=true, Bounds=(0,0,100,100), HasUpdates=false };
        scene.BeginFrame((0,0,200,200),2);
        using(var group=scene.Group(options))
        {
            Assert.True(group.NeedsRedraw);
            scene.SetFill(Colors.Red); scene.FillRect((0,0,100,100));
        }
        scene.EndFrame();
        var first=SingleChild(scene.Root);
        var resource=new Resource(); ((TestNode)first).Resource=resource;
        var count=first.Commands.Count;
        scene.AcceptFrame();
        scene.BeginFrame((0,0,200,200),2);
        using(var group=scene.Group(options with { Transform=AffineTransform.Translate((30,0)), Backdrop=BackdropMaterial.LiquidGlass }))
            Assert.False(group.NeedsRedraw);
        scene.EndFrame();
        Assert.Same(first,SingleChild(scene.Root));
        Assert.Equal(count,first.Commands.Count);
        Assert.Equal((nfloat)30,first.Placement.Tx);
        scene.AcceptFrame();
        scene.BeginFrame((0,0,200,200),2); scene.EndFrame();
        Assert.True(resource.Disposed);
        Assert.Equal(0, scene.Root.Count);
    }

    [Fact]
    public void UpdatesResizeScaleAndAbortedFrameForceFreshDrawing()
    {
        using var stream=new MemoryStream();
        using var measure=new SvgDrawingContext((200,200),stream,keepOpen:true);
        using var scene=new RetainedCanvasContext(measure);
        var options=new GroupOptions { Key=new object(), ViewBacking=true, Bounds=(0,0,100,100), HasUpdates=false };
        void Frame(GroupOptions o,nfloat scale,bool expected)
        {
            scene.BeginFrame((0,0,200,200),scale);
            using(var group=scene.Group(o)) Assert.Equal(expected,group.NeedsRedraw);
            scene.EndFrame(); scene.AcceptFrame();
        }
        Frame(options,1,true); Frame(options,1,false); Frame(options with { HasUpdates=true },1,true);
        Frame(options with { Bounds=(0,0,110,100) },1,true);
        Frame(options,2,true); Frame(options,2,false);
        scene.BeginFrame((0,0,200,200),2); scene.AbortFrame();
        Frame(options,2,true);
    }

    [Fact]
    public void DrawingAfterChildrenIsRejectedAndAnOverlayGroupPreservesOrder()
    {
        using var stream=new MemoryStream();
        using var measure=new SvgDrawingContext((200,200),stream,keepOpen:true);
        using var scene=new RetainedCanvasContext(measure);
        scene.BeginFrame((0,0,200,200),1);
        scene.FillRect((0,0,200,200));
        using(scene.Group(new GroupOptions { Key=new object(), ViewBacking=true, Bounds=(0,0,100,100) })) scene.FillRect((0,0,100,100));
        Assert.Throws<InvalidOperationException>(()=>scene.FillRect((0,0,20,20)));
        using(scene.Group(new GroupOptions { Key=new object(), ViewBacking=true, Bounds=(0,0,20,20) })) scene.FillRect((0,0,20,20));
        scene.EndFrame();
        Assert.Equal(2,scene.Root.Count);
        using var svgOutput=new MemoryStream();
        using var svg=new SvgDrawingContext((200,200),svgOutput,keepOpen:true);
        IContext context=svg;
        using var parent=context.Group(new GroupOptions { Key=new object(),ViewBacking=true,Bounds=(0,0,200,200) });
        using(context.Group(new GroupOptions { Key=new object(),ViewBacking=true,Bounds=(0,0,100,100) })) context.FillRect((0,0,10,10));
        Assert.Throws<InvalidOperationException>(()=>context.FillRect((0,0,20,20)));
    }

    [Fact]
    public void InheritedPaintInvalidatesCachedContentAndSaveRestorePreservesReuse()
    {
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), output, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var options = new GroupOptions { Key = new object(), ViewBacking = true, Bounds = (0, 0, 100, 100), HasUpdates = false };
        void Frame(Color color, bool expected)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            scene.SetFill(color);
            scene.Save(); scene.SetFill(Colors.Green); scene.Restore();
            using (var group = scene.Group(options))
            {
                Assert.Equal(expected, group.NeedsRedraw);
                if (group.NeedsRedraw) scene.FillRect((0, 0, 100, 100));
            }
            scene.EndFrame(); scene.AcceptFrame();
        }
        Frame(Colors.Red, true);
        Frame(Colors.Red, false);
        Frame(Colors.Blue, true);
    }

    [Fact]
    public void ArbitraryInheritedClipFlattensContentAndRequestsDrawing()
    {
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), output, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var options = new GroupOptions { Key = new object(), ViewBacking = true, Bounds = (0, 0, 100, 100), HasUpdates = false };
        for (var i = 0; i < 2; i++)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            scene.Save(); scene.BeginPath(); scene.Arc((50, 50), 40, 0, nfloat.Pi * 2); scene.Clip();
            using (var group = scene.Group(options))
            {
                Assert.True(group.NeedsRedraw);
                scene.FillRect((0, 0, 100, 100));
            }
            scene.Restore(); scene.EndFrame();
            Assert.Equal(0, scene.Root.Count);
            Assert.True(scene.Root.HasDrawing);
            scene.AcceptFrame();
        }
    }

    [Fact]
    public void RecordedGradientAndTextOwnTheirSpanData()
    {
        using var stream=new MemoryStream();
        using var measure=new SvgDrawingContext((200,200),stream,Xui.Core.Fonts.Inter.URIs,keepOpen:true);
        using var scene=new RetainedCanvasContext(measure);
        scene.BeginFrame((0,0,200,200),1);
        using(scene.Group(new GroupOptions { Key=new object(),ViewBacking=true,Bounds=(0,0,100,100) }))
        {
            Span<GradientStop> stops=stackalloc GradientStop[] { new(0,Colors.Red),new(1,Colors.Blue) };
            scene.SetFill(new LinearGradient((0,0),(100,0),stops)); scene.FillRect((0,0,100,100));
            stops[0]=new(0,Colors.Green);
            Span<char> label=stackalloc char[] { 'H','i' };
            scene.SetFill(Colors.Black);
            scene.SetFont(new Xui.Core.Canvas.Font(15,"Inter")); scene.FillText(label,(10,20)); label.Fill('X');
        }
        scene.EndFrame();
        using var output=new MemoryStream();
        using(var context=new SvgDrawingContext((200,200),output,Xui.Core.Fonts.Inter.URIs,keepOpen:true))
        {
            var node=SingleChild(scene.Root); node.Inherited.Replay(context); node.Commands.Replay(context);
        }
        output.Position=0; var doc=XDocument.Load(output); XNamespace ns="http://www.w3.org/2000/svg";
        Assert.Equal("Hi",Assert.Single(doc.Descendants(ns+"text")).Value);
        Assert.Equal("#FF0000",(string?)doc.Descendants(ns+"stop").First().Attribute("stop-color"));
    }

    [Fact]
    public void ShadowUsesAnOutlineAndGlassDoesNotBlurForegroundText()
    {
        using var output=new MemoryStream();
        using(var svg=new SvgDrawingContext((200,200),output,Xui.Core.Fonts.Inter.URIs,keepOpen:true))
        {
            IContext context=svg;
            using var group=context.Group(new GroupOptions { Key=new object(),ViewBacking=true,Bounds=(10,10,180,60),CornerRadius=30,
                Shadow=new OutlineShadow(new Color(0,0,0,(nfloat).2),8,(0,4)),Backdrop=BackdropMaterial.LiquidGlass });
            context.SetFont(new Xui.Core.Canvas.Font(15,"Inter")); context.FillText("Clear text",(30,40));
        }
        output.Position=0; var doc=XDocument.Load(output); XNamespace ns="http://www.w3.org/2000/svg";
        Assert.Equal(2,doc.Descendants(ns+"feGaussianBlur").Count());
        var text=Assert.Single(doc.Descendants(ns+"text"));
        Assert.DoesNotContain(text.AncestorsAndSelf(),e=>e.Attribute("filter") is not null);
        Assert.Single(doc.Descendants(ns+"rect"),e=>(string?)e.Attribute("data-xui-material")=="LiquidGlass");
    }
    private sealed class TestNode : GraphicsNode
    {
        internal IDisposable? Resource;
        protected override void DisposeCore()
        {
            Resource?.Dispose();
            Resource = null;
            base.DisposeCore();
        }
    }
    private sealed class Resource : IDisposable { public bool Disposed; public void Dispose()=>Disposed=true; }
    private sealed class CountingResource : IDisposable { public int DisposeCount; public void Dispose() => DisposeCount++; }
}
