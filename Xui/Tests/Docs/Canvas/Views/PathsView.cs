using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.Canvas.FillRule;
using static Xui.Core.Canvas.Winding;

namespace Xui.Tests.Docs.Canvas.Views;

/// <summary>
/// Demonstrates path building: LineTo for a star, CurveTo for a heart,
/// and line cap / line join options.
/// </summary>
public class PathsView : View
{
    protected override void RenderCore(IContext context)
    {
        // Background
        context.SetFill(new Color(0xF5, 0xF5, 0xF5, 0xFF));
        context.FillRect(this.Frame);

        // ── Left: five-pointed star filled with EvenOdd rule ──────────────
        nfloat cx = 130, cy = 120, outer = 90, inner = 36;
        context.SetFill(new Color(0xE0, 0x55, 0x55, 0xFF));
        context.BeginPath();
        for (int i = 0; i < 10; i++)
        {
            double angle = i * NFloat.Pi / 5 - NFloat.Pi / 2;
            nfloat r = i % 2 == 0 ? outer : inner;
            var pt = new Point(cx + r * (nfloat)Math.Cos(angle), cy + r * (nfloat)Math.Sin(angle));
            if (i == 0) context.MoveTo(pt);
            else context.LineTo(pt);
        }
        context.ClosePath();
        context.Fill(EvenOdd);

        // ── Right: heart drawn with cubic Bézier curves ───────────────────
        nfloat hx = 340, hy = 70, s = 1.6f;
        context.SetFill(new Color(0xD6, 0x42, 0xCD, 0xFF));
        context.BeginPath();
        context.MoveTo(new Point(hx, hy + 20 * s));
        context.CurveTo(
            new Point(hx, hy),
            new Point(hx - 40 * s, hy),
            new Point(hx - 40 * s, hy + 20 * s));
        context.CurveTo(
            new Point(hx - 40 * s, hy + 50 * s),
            new Point(hx, hy + 70 * s),
            new Point(hx, hy + 90 * s));
        context.CurveTo(
            new Point(hx, hy + 70 * s),
            new Point(hx + 40 * s, hy + 50 * s),
            new Point(hx + 40 * s, hy + 20 * s));
        context.CurveTo(
            new Point(hx + 40 * s, hy),
            new Point(hx, hy),
            new Point(hx, hy + 20 * s));
        context.Fill(NonZero);

        // Round-cap dashed stroke under the star
        context.SetStroke(new Color(0x41, 0x96, 0xD0, 0xFF));
        context.LineWidth = 4;
        context.LineCap = LineCap.Round;
        context.SetLineDash([12f, 6f]);
        context.BeginPath();
        context.MoveTo(new Point(40, 220));
        context.LineTo(new Point(220, 220));
        context.Stroke();
        context.SetLineDash([]);
    }
}
