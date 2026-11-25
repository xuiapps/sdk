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

public partial class FloorplanPerformanceTest
{
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
}
