using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class ArcTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;
        NFloat pi = NFloat.Pi;

        // Full circle
        context.BeginPath();
        context.Arc((x + 60, y + 60), 40, 0, pi * 2);
        context.SetFill(Blue);
        context.Fill();

        // Half circle (clockwise)
        context.BeginPath();
        context.Arc((x + 170, y + 60), 40, 0, pi, Winding.ClockWise);
        context.ClosePath();
        context.SetFill(Red);
        context.Fill();

        // Quarter arc stroke
        context.BeginPath();
        context.Arc((x + 60, y + 170), 40, 0, pi / 2);
        context.SetStroke(Green);
        context.LineWidth = 3;
        context.Stroke();

        // Three-quarter arc (counter-clockwise from 0 to PI/2 = 3/4 arc)
        context.BeginPath();
        context.Arc((x + 170, y + 170), 40, 0, pi / 2, Winding.CounterClockWise);
        context.SetStroke(Purple);
        context.LineWidth = 3;
        context.Stroke();

        // Concentric arcs with different start/end
        context.SetStroke(Orange);
        context.LineWidth = 2;
        for (int i = 0; i < 4; i++)
        {
            context.BeginPath();
            context.Arc((x + 240, y + 100), 20 + i * 12, pi * i / 4, pi * i / 4 + pi);
            context.Stroke();
        }
    }
}
