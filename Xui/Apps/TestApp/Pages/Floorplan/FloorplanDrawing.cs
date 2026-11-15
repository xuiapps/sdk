using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

/// <summary>Immutable load-time drawing data; rendering visits arrays and reusable paths.</summary>
public sealed partial class FloorplanDrawing
{
    private static readonly Lazy<FloorplanDrawing> shared = new(LoadEmbedded);
    public static FloorplanDrawing Shared => shared.Value;
    private readonly Item[][] levels;
    private readonly Item[] markers;
    public int PathCount { get; }
    public int PrimitiveCount(int level) => levels[level].Length;
    private FloorplanDrawing(Item[][] levels, Item[] markers, int pathCount)
    { this.levels = levels; this.markers = markers; PathCount = pathCount; }
    private sealed record Paint(Color Color, GradientStop[]? Stops = null, Point Start = default, Point End = default, NFloat Radius = default)
    {
        public void Apply(IContext ctx, bool stroke)
        {
            if (Stops is null)
            { if (stroke) ctx.SetStroke(Color); else ctx.SetFill(Color); }
            else if (Radius > 0)
            { var gradient = new RadialGradient(Start, Radius, Stops); if (stroke) ctx.SetStroke(gradient); else ctx.SetFill(gradient); }
            else
            { var gradient = new LinearGradient(Start, End, Stops); if (stroke) ctx.SetStroke(gradient); else ctx.SetFill(gradient); }
        }
    }
    private sealed record Item(Path2D? Path, string? Text, Point Position, Font Font, TextAlign Align,
        AffineTransform Transform, Rect Bounds, Paint? Fill, Paint? Stroke, NFloat Width,
        LineCap Cap, LineJoin Join, FillRule Rule, NFloat Alpha, bool StrokeFirst, bool Label);

    public void Render(IContext ctx, int level, Rect visibleWorld, NFloat markerOpacity = default, bool showLabels = true)
    {
        Draw(ctx, levels[level], visibleWorld, 1, showLabels);
        if (markerOpacity > 0)
            Draw(ctx, markers, visibleWorld, markerOpacity, true);
    }
    private static bool Intersects(Rect a, Rect b) => a.X <= b.Right && a.Right >= b.X && a.Y <= b.Bottom && a.Bottom >= b.Y;
    private static void Draw(IContext ctx, Item[] items, Rect visible, NFloat opacity, bool labels)
    {
        ctx.Save();
        AffineTransform last = default;
        bool transformed = false;
        try
        {
            foreach (var item in items)
            {
                if ((!labels && item.Label) || !Intersects(item.Bounds, visible))
                    continue;
                var m = item.Transform;
                if (!transformed || m.A != last.A || m.B != last.B || m.C != last.C || m.D != last.D || m.Tx != last.Tx || m.Ty != last.Ty)
                {
                    if (transformed)
                        ctx.Restore();
                    ctx.Save();
                    ctx.Transform(m);
                    last = m;
                    transformed = true;
                }
                ctx.GlobalAlpha = item.Alpha * opacity;
                ctx.LineWidth = item.Width;
                ctx.LineCap = item.Cap;
                ctx.LineJoin = item.Join;
                if (item.Text is not null)
                {
                    ctx.SetFont(item.Font);
                    ctx.TextAlign = item.Align;
                    ctx.TextBaseline = TextBaseline.Alphabetic;
                    // IContext has FillText only; the white constellation halo supplies contrast.
                    item.Fill?.Apply(ctx, false);
                    if (item.Fill is not null)
                        ctx.FillText(item.Text, item.Position);
                    continue;
                }
                ctx.BeginPath();
                item.Path!.Visit(ctx);
                if (item.StrokeFirst && item.Stroke is not null)
                { item.Stroke.Apply(ctx, true); ctx.Stroke(); }
                if (item.Fill is not null)
                { item.Fill.Apply(ctx, false); ctx.Fill(item.Rule); }
                if (!item.StrokeFirst && item.Stroke is not null)
                { item.Stroke.Apply(ctx, true); ctx.Stroke(); }
            }
        }
        finally { if (transformed) ctx.Restore(); ctx.Restore(); }
    }
}
