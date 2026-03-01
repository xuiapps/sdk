using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class RoundRectTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Uniform radius rounded rect (filled)
        context.BeginPath();
        context.RoundRect(new Rect(x + 10, y + 10, 120, 80), 15);
        context.SetFill(Blue);
        context.Fill();

        // Uniform radius rounded rect (stroked)
        context.BeginPath();
        context.RoundRect(new Rect(x + 150, y + 10, 120, 80), 15);
        context.SetStroke(Red);
        context.LineWidth = 3;
        context.Stroke();

        // Per-corner radii: top-left large, others small
        context.BeginPath();
        context.RoundRect(new Rect(x + 10, y + 110, 120, 80), new CornerRadius(30, 5, 5, 5));
        context.SetFill(Green);
        context.Fill();
        context.SetStroke(Black);
        context.LineWidth = 2;
        context.Stroke();

        // Per-corner radii: diagonal corners large
        context.BeginPath();
        context.RoundRect(new Rect(x + 150, y + 110, 120, 80), new CornerRadius(25, 0, 25, 0));
        context.SetFill(Orange);
        context.Fill();
        context.SetStroke(Black);
        context.LineWidth = 2;
        context.Stroke();

        // Large radius (pill shape)
        context.BeginPath();
        context.RoundRect(new Rect(x + 10, y + 220, 260, 50), 25);
        context.SetFill(Purple);
        context.Fill();
    }
}
