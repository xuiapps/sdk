using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class ArcToTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // ArcTo: rounded corner between two lines
        context.BeginPath();
        context.MoveTo((x + 20, y + 20));
        context.ArcTo((x + 150, y + 20), (x + 150, y + 100), 30);
        context.LineTo((x + 150, y + 100));
        context.SetStroke(Blue);
        context.LineWidth = 3;
        context.Stroke();

        // Guide lines
        context.BeginPath();
        context.MoveTo((x + 20, y + 20));
        context.LineTo((x + 150, y + 20));
        context.LineTo((x + 150, y + 100));
        context.SetStroke(LightGray);
        context.LineWidth = 1;
        context.Stroke();

        // ArcTo with small radius
        context.BeginPath();
        context.MoveTo((x + 20, y + 140));
        context.ArcTo((x + 100, y + 140), (x + 100, y + 220), 10);
        context.LineTo((x + 100, y + 220));
        context.SetStroke(Red);
        context.LineWidth = 3;
        context.Stroke();

        // ArcTo with large radius
        context.BeginPath();
        context.MoveTo((x + 160, y + 140));
        context.ArcTo((x + 280, y + 140), (x + 280, y + 280), 60);
        context.LineTo((x + 280, y + 280));
        context.SetStroke(Green);
        context.LineWidth = 3;
        context.Stroke();

        // Guide lines for large radius
        context.BeginPath();
        context.MoveTo((x + 160, y + 140));
        context.LineTo((x + 280, y + 140));
        context.LineTo((x + 280, y + 280));
        context.SetStroke(LightGray);
        context.LineWidth = 1;
        context.Stroke();
    }
}
