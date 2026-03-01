using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class DashPatternTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        context.SetStroke(Black);
        context.LineWidth = 3;

        // Simple dash: 10 on, 10 off
        context.SetLineDash([(NFloat)10, (NFloat)10]);
        context.BeginPath();
        context.MoveTo((x + 20, y + 20));
        context.LineTo((x + 280, y + 20));
        context.Stroke();

        // Long dash, short gap
        context.SetLineDash([(NFloat)20, (NFloat)5]);
        context.BeginPath();
        context.MoveTo((x + 20, y + 50));
        context.LineTo((x + 280, y + 50));
        context.Stroke();

        // Dot pattern
        context.LineCap = LineCap.Round;
        context.SetLineDash([(NFloat)1, (NFloat)10]);
        context.BeginPath();
        context.MoveTo((x + 20, y + 80));
        context.LineTo((x + 280, y + 80));
        context.Stroke();

        // Dash-dot pattern
        context.LineCap = LineCap.Butt;
        context.SetLineDash([(NFloat)15, (NFloat)5, (NFloat)3, (NFloat)5]);
        context.BeginPath();
        context.MoveTo((x + 20, y + 110));
        context.LineTo((x + 280, y + 110));
        context.Stroke();

        // Dashed rectangle
        context.SetLineDash([(NFloat)8, (NFloat)4]);
        context.SetStroke(Blue);
        context.LineWidth = 2;
        context.StrokeRect(new Rect(x + 20, y + 140, 120, 60));

        // Dashed circle
        context.SetStroke(Red);
        context.BeginPath();
        context.Arc((x + 220, y + 170), 30, 0, NFloat.Pi * 2);
        context.Stroke();

        // Dash offset demo: same pattern, different offsets
        context.SetStroke(Green);
        context.LineWidth = 3;
        context.SetLineDash([(NFloat)10, (NFloat)10]);
        for (int i = 0; i < 5; i++)
        {
            context.LineDashOffset = i * 4;
            context.BeginPath();
            context.MoveTo((x + 20, y + 230 + i * 14));
            context.LineTo((x + 280, y + 230 + i * 14));
            context.Stroke();
        }
    }
}
