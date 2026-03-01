using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class QuadraticCurveTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Simple quadratic curve
        context.BeginPath();
        context.MoveTo((x + 30, y + 200));
        context.CurveTo((x + 150, y + 20), (x + 270, y + 200));
        context.SetStroke(Blue);
        context.LineWidth = 3;
        context.Stroke();

        // Draw control point and lines for visualization
        context.BeginPath();
        context.MoveTo((x + 30, y + 200));
        context.LineTo((x + 150, y + 20));
        context.LineTo((x + 270, y + 200));
        context.SetStroke(LightGray);
        context.LineWidth = 1;
        context.Stroke();

        // Mark points
        context.SetFill(Red);
        context.BeginPath();
        context.Arc((x + 30, y + 200), 4, 0, NFloat.Pi * 2);
        context.Fill();

        context.BeginPath();
        context.Arc((x + 270, y + 200), 4, 0, NFloat.Pi * 2);
        context.Fill();

        context.SetFill(Green);
        context.BeginPath();
        context.Arc((x + 150, y + 20), 4, 0, NFloat.Pi * 2);
        context.Fill();
    }
}
