using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class HeartCurveTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Heart shape using two cubic bezier curves
        // Centered in 300x300, heart centered at (150, 150)
        NFloat cx = x + 150;
        NFloat cy = y + 140;
        NFloat s = 120;

        Point top = (cx, cy - 0.25f * s);
        Point bottom = (cx, cy + 0.95f * s);

        // Right half control points
        Point rc1 = (cx + 0.50f * s, cy - 0.95f * s);
        Point rc2 = (cx + 1.20f * s, cy - 0.05f * s);

        // Left half control points
        Point lc1 = (cx - 1.20f * s, cy - 0.05f * s);
        Point lc2 = (cx - 0.50f * s, cy - 0.95f * s);

        // Fill heart
        context.BeginPath();
        context.MoveTo(top);
        context.CurveTo(rc1, rc2, bottom);
        context.CurveTo(lc1, lc2, top);
        context.ClosePath();
        context.SetFill(Red);
        context.Fill();

        // Stroke heart outline
        context.SetStroke(Black);
        context.LineWidth = 2;
        context.BeginPath();
        context.MoveTo(top);
        context.CurveTo(rc1, rc2, bottom);
        context.CurveTo(lc1, lc2, top);
        context.ClosePath();
        context.Stroke();
    }
}
