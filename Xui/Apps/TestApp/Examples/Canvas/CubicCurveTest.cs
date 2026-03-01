using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class CubicCurveTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Cubic bezier curve
        context.BeginPath();
        context.MoveTo((x + 30, y + 200));
        context.CurveTo((x + 50, y + 30), (x + 250, y + 30), (x + 270, y + 200));
        context.SetStroke(Red);
        context.LineWidth = 3;
        context.Stroke();

        // Draw control point lines for visualization
        context.BeginPath();
        context.MoveTo((x + 30, y + 200));
        context.LineTo((x + 50, y + 30));
        context.MoveTo((x + 270, y + 200));
        context.LineTo((x + 250, y + 30));
        context.SetStroke(LightGray);
        context.LineWidth = 1;
        context.Stroke();

        // Mark end points (red)
        context.SetFill(Red);
        context.BeginPath();
        context.Arc((x + 30, y + 200), 4, 0, NFloat.Pi * 2);
        context.Fill();

        context.BeginPath();
        context.Arc((x + 270, y + 200), 4, 0, NFloat.Pi * 2);
        context.Fill();

        // Mark control points (green)
        context.SetFill(Green);
        context.BeginPath();
        context.Arc((x + 50, y + 30), 4, 0, NFloat.Pi * 2);
        context.Fill();

        context.BeginPath();
        context.Arc((x + 250, y + 30), 4, 0, NFloat.Pi * 2);
        context.Fill();
    }
}
