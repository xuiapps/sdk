using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Small vector diagrams that describe each choice without font glyphs or bitmaps.</summary>
internal static class DesignSystemIcons
{
    public static void Draw(IContext ctx, Point center, MobilePresetVisual kind, int index, Color color)
    {
        ctx.Save();
        ctx.Translate((center.X, center.Y));
        ctx.SetStroke(color); ctx.SetFill(color); ctx.LineWidth = (NFloat)1.5;
        ctx.LineCap = LineCap.Round; ctx.LineJoin = LineJoin.Round;
        switch (kind)
        {
            case MobilePresetVisual.Shape:
                var radius = index switch { 0 => 0, 1 => 2, 2 => 6, 3 => 12, _ => 9 };
                Box(ctx, (-12, -9, 24, 18), radius);
                Line(ctx, (-6, 0), (6, 0));
                break;
            case MobilePresetVisual.Sizing:
                Box(ctx, index == 0 ? new Rect(-13, -9, 26, 17) : index == 1 ? new Rect(-10, -12, 20, 24) : new Rect(-7, -12, 14, 24), 2);
                if (index == 0) { Line(ctx, (0, 8), (0, 12)); Line(ctx, (-5, 12), (5, 12)); }
                else Line(ctx, (-3, 8), (3, 8));
                break;
            case MobilePresetVisual.Motion:
                if (index == 0) { Box(ctx, (-7, -7, 14, 14), 3); Line(ctx, (-12, 12), (12, -12)); }
                else
                {
                    for (var i = 0; i < index; i++) Line(ctx, (-12, (i - 1) * 5), (-5 + i * 2, (i - 1) * 5));
                    ctx.BeginPath(); ctx.Arc((6, 0), 6, 0, NFloat.Pi * 2); ctx.Stroke();
                }
                break;
            case MobilePresetVisual.Harmony:
                ctx.BeginPath(); ctx.Arc((0, 0), 11, 0, NFloat.Pi * 2); ctx.Stroke();
                var count = index switch { 0 or 1 => 2, 2 or 3 => 3, _ => 4 };
                for (var i = 0; i < count; i++)
                {
                    var angle = -NFloat.Pi / 2 + (index == 1 ? (NFloat)0.65 * i : index == 2 ? (i == 0 ? 0 : NFloat.Pi + (i == 1 ? -(NFloat)0.5 : (NFloat)0.5)) : NFloat.Pi * 2 * i / count);
                    var point = new Point(NFloat.Cos(angle) * 11, NFloat.Sin(angle) * 11);
                    Line(ctx, (0, 0), point);
                    ctx.BeginPath(); ctx.Arc(point, (NFloat)2.5, 0, NFloat.Pi * 2); ctx.Fill();
                }
                break;
            default:
                Box(ctx, (-12, -11, 24, 22), 3);
                if (index is 1 or 3) { var tint = color; tint.Alpha *= (NFloat)0.2; ctx.SetFill(tint); ctx.FillRect((-11, -10, 22, 20)); }
                Box(ctx, (-7, -4, 14, 10), 2);
                if (index is 2 or 4) { var tint = color; tint.Alpha *= (NFloat)0.3; ctx.SetFill(tint); ctx.FillRect((-6, -3, 12, 8)); }
                Line(ctx, (-7, -7), (2, -7));
                break;
        }
        ctx.Restore();
    }

    private static void Box(IContext ctx, Rect rect, NFloat radius)
    { ctx.BeginPath(); ctx.RoundRect(rect, radius); ctx.Stroke(); }
    private static void Line(IContext ctx, Point from, Point to)
    { ctx.BeginPath(); ctx.MoveTo(from); ctx.LineTo(to); ctx.Stroke(); }
}
