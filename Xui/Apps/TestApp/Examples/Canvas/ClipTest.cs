using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class ClipTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;
        NFloat pi = NFloat.Pi;

        // Clip to circle, then draw rectangles
        context.Save();
        context.BeginPath();
        context.Arc((x + 80, y + 80), 60, 0, pi * 2);
        context.Clip();

        context.SetFill(Red);
        context.FillRect(new Rect(x + 20, y + 20, 60, 120));
        context.SetFill(Green);
        context.FillRect(new Rect(x + 80, y + 20, 60, 120));
        context.SetFill(Blue);
        context.FillRect(new Rect(x + 20, y + 60, 120, 40));
        context.Restore();

        // Clip to rectangle, then draw circle
        context.Save();
        context.BeginPath();
        context.Rect(new Rect(x + 170, y + 20, 110, 110));
        context.Clip();

        context.SetFill(Orange);
        context.BeginPath();
        context.Arc((x + 225, y + 75), 70, 0, pi * 2);
        context.Fill();
        context.Restore();

        // Nested clips
        context.Save();
        context.BeginPath();
        context.Rect(new Rect(x + 20, y + 170, 120, 120));
        context.Clip();

        context.BeginPath();
        context.Arc((x + 80, y + 230), 50, 0, pi * 2);
        context.Clip();

        context.SetFill(Purple);
        context.FillRect(new Rect(x, y + 160, 200, 200));
        context.Restore();

        // Clip with stroked content
        context.Save();
        context.BeginPath();
        context.Arc((x + 220, y + 230), 50, 0, pi * 2);
        context.Clip();

        context.SetStroke(DarkRed);
        context.LineWidth = 4;
        for (NFloat i = 170; i < 290; i += 10)
        {
            context.BeginPath();
            context.MoveTo((x + i, y + 170));
            context.LineTo((x + i, y + 290));
            context.Stroke();
        }
        context.Restore();
    }
}
