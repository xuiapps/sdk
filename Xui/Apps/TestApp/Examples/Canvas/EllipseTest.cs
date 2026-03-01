using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class EllipseTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;
        NFloat pi = NFloat.Pi;

        // Full ellipse (horizontal)
        context.BeginPath();
        context.Ellipse((x + 80, y + 60), 60, 30, 0, 0, pi * 2);
        context.SetFill(Blue);
        context.Fill();
        context.SetStroke(Black);
        context.LineWidth = 2;
        context.Stroke();

        // Full ellipse (vertical)
        context.BeginPath();
        context.Ellipse((x + 220, y + 60), 30, 50, 0, 0, pi * 2);
        context.SetFill(Green);
        context.Fill();
        context.SetStroke(Black);
        context.LineWidth = 2;
        context.Stroke();

        // Rotated ellipse (45 degrees)
        context.BeginPath();
        context.Ellipse((x + 80, y + 180), 60, 25, pi / 4, 0, pi * 2);
        context.SetFill(Orange);
        context.Fill();
        context.SetStroke(Black);
        context.LineWidth = 2;
        context.Stroke();

        // Half ellipse
        context.BeginPath();
        context.Ellipse((x + 220, y + 180), 50, 30, 0, 0, pi);
        context.ClosePath();
        context.SetFill(Red);
        context.Fill();
        context.SetStroke(Black);
        context.LineWidth = 2;
        context.Stroke();

        // Small circle via Ellipse (equal radii)
        context.BeginPath();
        context.Ellipse((x + 150, y + 270), 20, 20, 0, 0, pi * 2);
        context.SetFill(Purple);
        context.Fill();
    }
}
