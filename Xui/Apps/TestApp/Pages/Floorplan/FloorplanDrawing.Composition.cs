using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

public sealed partial class FloorplanDrawing
{
    /// <summary>A stable composition key for a room, underlay, or structural section; never a UI view.</summary>
    public sealed record Region(string Id, Rect Bounds, bool IsRoom);

    /// <summary>Separate immutable drawing identities for each region's detail and rendering mode.</summary>
    public sealed record DetailKey(Region Region, int Level, bool ShowLabels, bool UseSymbols);

    private sealed class Surface(Region region)
    {
        public readonly Region Region = region;
        public readonly Item[][] Levels = new Item[4][];
        public readonly DetailKey[,,] Keys = CreateKeys(region);
        private static DetailKey[,,] CreateKeys(Region region)
        {
            var keys = new DetailKey[4, 2, 2];
            for (var level = 0; level < 4; level++)
                for (var labels = 0; labels < 2; labels++)
                    for (var symbols = 0; symbols < 2; symbols++)
                        keys[level, labels, symbols] = new(region, level, labels != 0, symbols != 0);
            return keys;
        }
    }

    private static Surface[][] BuildSurfaces(Item[][] levels)
    {
        var lookup = new Dictionary<(Region, int), Surface>();
        var fallback = new Region("floorplan", (0, 120, 1000, 760), false);
        var overview = new Surface(fallback);
        var result = new Surface[4][];
        for (var level = 0; level < 4; level++)
        {
            if (level < 2)
            {
                // At overview scale, native room surfaces cost more to compose than
                // they save through culling. Retain the whole level in painter order.
                overview.Levels[level] = levels[level];
                result[level] = [overview];
                continue;
            }
            var surfaces = new List<Surface>();
            var occurrences = new Dictionary<Region, int>();
            var items = levels[level];
            for (var start = 0; start < items.Length;)
            {
                var region = items[start].Region ?? fallback;
                var end = start + 1;
                while (end < items.Length && ReferenceEquals(items[end].Region ?? fallback, region)) end++;
                occurrences.TryGetValue(region, out var occurrence);
                occurrences[region] = occurrence + 1;
                // Keep discontiguous runs separate: moving an underlay or a foreground wall
                // across another room would change the SVG's painter order.
                if (!lookup.TryGetValue((region, occurrence), out var surface))
                    lookup[(region, occurrence)] = surface = new(occurrence == 0 ? region : region with { });
                surface.Levels[level] = items[start..end];
                surfaces.Add(surface);
                start = end;
            }
            result[level] = surfaces.ToArray();
        }
        return result;
    }

    public int RegionCount(int level) => surfaces[level].Length;
    public Composition CreateComposition() => new(this);

    /// <summary>Per-map viewport/camera identity and presentation state, independent of the Xui view tree.</summary>
    public sealed class Composition(FloorplanDrawing drawing)
    {
        private readonly object viewportKey = new(), cameraKey = new(), markerKey = new();
        private IContext? markerContext;
        private NFloat lastMarkerOpacity;
        private AffineTransform lastMarkerCamera;

        public void Render(IContext context, Rect viewport, AffineTransform camera, int level, Rect visibleWorld,
            NFloat markerOpacity = default, bool showLabels = true, bool useSymbols = true, NFloat rasterScale = default)
        {
            if (rasterScale == 0) rasterScale = 1;
            using var clip = context.Group(new GroupOptions
            {
                Key = viewportKey, Bounds = viewport, ViewBacking = true, ClipToBounds = true,
            });
            if (!clip.NeedsRedraw) return;
            using (var world = context.Group(new GroupOptions
            {
                Key = cameraKey, Bounds = (0, 120, 1000, 760), ViewBacking = true, Transform = camera,
            }))
            {
                if (world.NeedsRedraw)
                {
                    foreach (var surface in drawing.surfaces[level])
                    {
                        var bounds = surface.Region.Bounds.Expand(8);
                        if (!Intersects(bounds, visibleWorld)) continue;
                        if (level < 2)
                        {
                            DrawSurface(context, surface, level, bounds, rasterScale, showLabels, useSymbols);
                            continue;
                        }
                        using var room = context.Group(new GroupOptions
                        {
                            Key = surface.Region, Bounds = bounds, ViewBacking = true, CacheWhenHidden = true,
                        });
                        if (!room.NeedsRedraw) continue;
                        DrawSurface(context, surface, level, bounds, rasterScale, showLabels, useSymbols);
                    }
                }
            }
            if (markerOpacity > 0)
            {
                // Bound this transient overlay to the screen: a building-sized marker
                // texture at maximum detail/device scale would consume hundreds of MB.
                using var markers = context.Group(new GroupOptions
                {
                    Key = markerKey, Bounds = viewport, ViewBacking = true,
                    HasUpdates = !ReferenceEquals(markerContext, context) || lastMarkerOpacity != markerOpacity
                        || !SameTransform(lastMarkerCamera, camera),
                });
                if (markers.NeedsRedraw)
                {
                    context.Transform(camera);
                    Draw(context, drawing.markers, default, markerOpacity, true, false, cull: false);
                }
                markerContext = context;
                lastMarkerOpacity = markerOpacity;
                lastMarkerCamera = camera;
            }
        }

        private static void DrawSurface(IContext context, Surface surface, int level, Rect bounds,
            NFloat rasterScale, bool showLabels, bool useSymbols)
        {
            using var detail = context.Group(new GroupOptions
            {
                Key = surface.Keys[level, showLabels ? 1 : 0, useSymbols ? 1 : 0],
                Bounds = ScaleBounds(bounds, rasterScale),
                Transform = AffineTransform.Scale((1 / rasterScale, 1 / rasterScale)),
                ViewBacking = true, HasUpdates = false, CacheWhenHidden = true,
            });
            if (!detail.NeedsRedraw) return;
            context.Scale((rasterScale, rasterScale));
            // Retain the entire level/room, including geometry exposed by later pans.
            Draw(context, surface.Levels[level], bounds, 1, showLabels, useSymbols, cull: false);
        }

        private static bool SameTransform(in AffineTransform a, in AffineTransform b) =>
            a.A == b.A && a.B == b.B && a.C == b.C && a.D == b.D && a.Tx == b.Tx && a.Ty == b.Ty;

        private static Rect ScaleBounds(Rect bounds, NFloat scale) =>
            new(bounds.X * scale, bounds.Y * scale, bounds.Width * scale, bounds.Height * scale);
    }
}
