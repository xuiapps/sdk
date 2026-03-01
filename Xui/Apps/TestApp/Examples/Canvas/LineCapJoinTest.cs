using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class LineCapJoinTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        context.LineWidth = 15;
        context.SetStroke(Blue);

        // LineCap: Butt
        context.LineCap = LineCap.Butt;
        context.BeginPath();
        context.MoveTo((x + 30, y + 30));
        context.LineTo((x + 130, y + 30));
        context.Stroke();

        // LineCap: Round
        context.LineCap = LineCap.Round;
        context.BeginPath();
        context.MoveTo((x + 30, y + 60));
        context.LineTo((x + 130, y + 60));
        context.Stroke();

        // LineCap: Square
        context.LineCap = LineCap.Square;
        context.BeginPath();
        context.MoveTo((x + 30, y + 90));
        context.LineTo((x + 130, y + 90));
        context.Stroke();

        // Reference lines to show cap extension
        context.SetStroke(LightGray);
        context.LineWidth = 1;
        context.LineCap = LineCap.Butt;
        context.BeginPath();
        context.MoveTo((x + 30, y + 15));
        context.LineTo((x + 30, y + 105));
        context.MoveTo((x + 130, y + 15));
        context.LineTo((x + 130, y + 105));
        context.Stroke();

        // LineJoin: Miter
        context.LineWidth = 10;
        context.SetStroke(Red);
        context.LineJoin = LineJoin.Miter;
        context.BeginPath();
        context.MoveTo((x + 20, y + 170));
        context.LineTo((x + 60, y + 130));
        context.LineTo((x + 100, y + 170));
        context.Stroke();

        // LineJoin: Round
        context.LineJoin = LineJoin.Round;
        context.BeginPath();
        context.MoveTo((x + 110, y + 170));
        context.LineTo((x + 150, y + 130));
        context.LineTo((x + 190, y + 170));
        context.Stroke();

        // LineJoin: Bevel
        context.LineJoin = LineJoin.Bevel;
        context.BeginPath();
        context.MoveTo((x + 200, y + 170));
        context.LineTo((x + 240, y + 130));
        context.LineTo((x + 280, y + 170));
        context.Stroke();

        // Thick line with different widths
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Round;
        NFloat[] widths = [2, 5, 10, 20];
        for (int i = 0; i < widths.Length; i++)
        {
            context.LineWidth = widths[i];
            context.SetStroke(new Color((byte)(50 + i * 50), 100, 200, 255));
            context.BeginPath();
            context.MoveTo((x + 20, y + 210 + i * 22));
            context.LineTo((x + 280, y + 210 + i * 22));
            context.Stroke();
        }
    }
}
