using System.Text;
using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Core.Canvas.Composition;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Tests.Integration.TestApp;

public class FloorplanCompositionTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void OverviewLevelsReuseOneWholeFloorplanSurfaceAcrossCameraAndDetailChanges(int level)
    {
        using var measure = new SvgDrawingContext((430, 800), Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        var composition = FloorplanDrawing.Shared.CreateComposition();
        void Render(int detail, Rect visible, AffineTransform camera)
        {
            recorder.BeginFrame((0, 0, 430, 800), 3);
            composition.Render(recorder, (0, 0, 430, 800), camera, detail, visible);
            recorder.EndFrame();
        }
        Rect full = (0, 120, 1000, 760);
        Render(level, full, AffineTransform.Identity);
        var world = recorder.Root[0][0];
        var surface = Assert.IsType<GraphicsNode>(Assert.Single(Nodes(world).Skip(1)));
        Assert.True(surface.HasDrawing);
        Assert.Equal(level, Assert.IsType<FloorplanDrawing.DetailKey>(surface.Options.Key).Level);
        var commandCount = surface.Commands.Count;
        recorder.AcceptFrame();

        // A new visible slice must move the same complete backing, without recording it.
        Render(level, (800, 600, 100, 100), AffineTransform.Translate((-800, -600)));
        Assert.Same(surface, world[0]);
        Assert.False(surface.Updating);
        Assert.Equal(commandCount, surface.Commands.Count);
        recorder.AcceptFrame();
        Render(1 - level, full, AffineTransform.Identity);
        Assert.NotSame(surface, world[0]);
        recorder.AcceptFrame();
        Render(level, full, AffineTransform.Identity);
        Assert.Same(surface, world[0]);
        Assert.False(surface.Updating);
        recorder.AcceptFrame();

        Render(2, full, AffineTransform.Identity);
        Assert.Contains(Nodes(world), n => n.Options.Key is FloorplanDrawing.Region { IsRoom: true });
        recorder.AcceptFrame();
        Render(level, full, AffineTransform.Identity);
        Assert.Same(surface, Assert.Single(Nodes(world).Skip(1)));
        Assert.False(surface.Updating);
        recorder.AcceptFrame();
    }

    [Fact]
    public void PanAndZoomReuseRoomDrawingAndKeepMarkerBackingBoundedToViewport()
    {
        using var measure = new SvgDrawingContext((430, 800), Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        var drawing = FloorplanDrawing.Shared;
        var composition = drawing.CreateComposition();
        void Render(AffineTransform camera)
        {
            recorder.BeginFrame((0, 0, 430, 800), 3);
            composition.Render(recorder, (0, 0, 430, 800), camera, 2, (0, 120, 1000, 760), 1, rasterScale: 4);
            recorder.EndFrame();
        }
        Render(AffineTransform.Scale(((NFloat).4, (NFloat).4)));
        var nodes = Nodes(recorder.Root).ToArray();
        var rooms = nodes.Where(n => n.Options.Key is FloorplanDrawing.Region { IsRoom: true }).ToArray();
        var drawingNodes = nodes.OfType<GraphicsNode>().Where(n => n.Options.Key is FloorplanDrawing.DetailKey).ToArray();
        var marker = Assert.IsType<GraphicsNode>(recorder.Root[0][1]);
        Assert.Equal(new Rect(0, 0, 430, 800), marker.RasterBounds);
        Assert.True(marker.HasDrawing);
        Assert.True(rooms.Length > 50);
        Assert.Equal(drawing.RegionCount(2), nodes.Count(n => n.Options.Key is FloorplanDrawing.DetailKey));
        Assert.All(rooms, n => Assert.IsNotAssignableFrom<View>(n.Options.Key));
        Assert.All(drawingNodes, n => Assert.True(n.Updating));
        var world = recorder.Root[0][0];
        var previousPlacement = world.Placement;
        recorder.AcceptFrame();
        Render(AffineTransform.Translate((30, 80)) * AffineTransform.Scale(((NFloat).45, (NFloat).45)));
        Assert.NotEqual(previousPlacement, world.Placement);
        Assert.Equal(nodes, Nodes(recorder.Root));
        Assert.All(drawingNodes, n => Assert.False(n.Updating));
        Assert.True(marker.Updating);
        Assert.All(drawingNodes, n => Assert.True(n.Commands.Count > 0));
        Assert.True(recorder.Root[0].Options.ClipToBounds);
        recorder.AcceptFrame();
        Render(AffineTransform.Translate((30, 80)) * AffineTransform.Scale(((NFloat).45, (NFloat).45)));
        Assert.False(marker.Updating);
        recorder.AcceptFrame();
    }

    [Fact]
    public void DetailLevelsModesAndRecentlyCulledRoomsKeepIndependentCachedSurfaces()
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes("""
            <svg xmlns="http://www.w3.org/2000/svg">
              <g id="west" data-kind="room" data-bounds="0 120 100 100">
                <rect x="0" y="120" width="100" height="100" fill="#fff"/>
                <text x="20" y="150">West</text>
              </g>
              <g id="east" data-kind="room" data-bounds="200 120 100 100">
                <rect x="200" y="120" width="100" height="100" fill="#eee"/>
              </g>
            </svg>
            """));
        var composition = FloorplanDrawing.Load(input).CreateComposition();
        using var measure = new SvgDrawingContext((430, 800), Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        GraphicsNode Render(int level, Rect visible, bool labels = true, bool symbols = true)
        {
            recorder.BeginFrame((0, 0, 430, 800), 3);
            composition.Render(recorder, (0, 0, 430, 800), AffineTransform.Identity, level, visible,
                showLabels: labels, useSymbols: symbols);
            recorder.EndFrame();
            return Assert.IsType<GraphicsNode>(Assert.Single(Nodes(recorder.Root),
                n => n.Options.Key is FloorplanDrawing.DetailKey));
        }
        Rect west = (10, 130, 20, 20), east = (210, 130, 20, 20);
        var overview = Render(2, west);
        recorder.AcceptFrame();
        var detail = Render(3, west);
        Assert.NotSame(overview, detail);
        Assert.True(detail.Updating);
        recorder.AcceptFrame();
        Assert.Same(overview, Render(2, west));
        Assert.False(overview.Updating);
        recorder.AcceptFrame();
        var noLabels = Render(2, west, labels: false);
        Assert.NotSame(overview, noLabels);
        recorder.AcceptFrame();
        var expanded = Render(2, west, symbols: false);
        Assert.NotSame(overview, expanded);
        recorder.AcceptFrame();
        var otherRoom = Render(2, east);
        Assert.Equal("east", ((FloorplanDrawing.DetailKey)otherRoom.Options.Key).Region.Id);
        Assert.DoesNotContain(Nodes(recorder.Root), n => n.Options.Key is FloorplanDrawing.Region { Id: "west" });
        Assert.Single(((GraphicsNode)recorder.Root[0][0]).HiddenChildren);
        recorder.AcceptFrame();
        Assert.Same(overview, Render(2, west));
        Assert.False(overview.Updating);
        recorder.AcceptFrame();
    }

    [Fact]
    public void OfficeViewportCullsMostRegionsAtHighZoom()
    {
        using var measure = new SvgDrawingContext((430, 800), Stream.Null, Xui.Core.Fonts.Inter.Source);
        using var recorder = new RetainedCanvasContext(measure);
        var drawing = FloorplanDrawing.Shared;
        var composition = drawing.CreateComposition();
        recorder.BeginFrame((0, 0, 430, 800), 3);
        composition.Render(recorder, (0, 0, 430, 800), AffineTransform.Identity, 3, (80, 370, 20, 20));
        recorder.EndFrame();
        var regions = Nodes(recorder.Root).Where(n => n.Options.Key is FloorplanDrawing.Region).ToArray();
        Assert.InRange(regions.Length, 1, drawing.RegionCount(3) / 4);
        Assert.Contains(regions, n => n.Options.Key is FloorplanDrawing.Region { Id: "room-manager-a" });
        recorder.AcceptFrame();
    }

    private static IEnumerable<Node> Nodes(Node node)
    {
        yield return node;
        for (var i = 0; i < node.Count; i++)
            foreach (var child in Nodes(node[i])) yield return child;
    }
}
