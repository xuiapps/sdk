using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class GlobalAlphaTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Opaque base rectangles
        context.SetFill(Red);
        context.FillRect(new Rect(x + 20, y + 20, 100, 100));
        context.SetFill(Blue);
        context.FillRect(new Rect(x + 80, y + 20, 100, 100));

        // Semi-transparent overlapping circle
        context.GlobalAlpha = 0.5f;
        context.SetFill(Green);
        context.BeginPath();
        context.Arc((x + 100, y + 70), 50, 0, NFloat.Pi * 2);
        context.Fill();

        // Alpha gradient: rectangles with decreasing opacity
        context.SetFill(Black);
        for (int i = 0; i < 10; i++)
        {
            context.GlobalAlpha = 1.0f - i * 0.1f;
            context.FillRect(new Rect(x + 20 + i * 26, y + 150, 24, 40));
        }

        // Alpha affects stroke too
        context.GlobalAlpha = 0.3f;
        context.SetStroke(Red);
        context.LineWidth = 8;
        context.BeginPath();
        context.MoveTo((x + 20, y + 220));
        context.LineTo((x + 280, y + 220));
        context.Stroke();

        context.GlobalAlpha = 0.6f;
        context.SetStroke(Blue);
        context.BeginPath();
        context.MoveTo((x + 20, y + 245));
        context.LineTo((x + 280, y + 245));
        context.Stroke();

        context.GlobalAlpha = 1.0f;
        context.SetStroke(Green);
        context.BeginPath();
        context.MoveTo((x + 20, y + 270));
        context.LineTo((x + 280, y + 270));
        context.Stroke();
    }
}
