using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class TransformTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;
        NFloat pi = NFloat.Pi;

        // Translate
        context.Save();
        context.Translate((x + 50, y + 50));
        context.SetFill(Blue);
        context.FillRect(new Rect(0, 0, 40, 40));
        context.Restore();

        // Rotate 45 degrees around center of square
        context.Save();
        context.Translate((x + 150, y + 50));
        context.Rotate(pi / 4);
        context.SetFill(Red);
        context.FillRect(new Rect(-20, -20, 40, 40));
        context.Restore();

        // Scale
        context.Save();
        context.Translate((x + 250, y + 50));
        context.Scale((2, 0.5f));
        context.SetFill(Green);
        context.FillRect(new Rect(-20, -20, 40, 40));
        context.Restore();

        // Combined: translate + rotate + scale
        context.Save();
        context.Translate((x + 80, y + 170));
        context.Rotate(pi / 6);
        context.Scale((1.5f, 0.8f));
        context.SetFill(Orange);
        context.FillRect(new Rect(-25, -25, 50, 50));
        context.SetStroke(Black);
        context.LineWidth = 2;
        context.StrokeRect(new Rect(-25, -25, 50, 50));
        context.Restore();

        // Nested transforms
        context.Save();
        context.Translate((x + 220, y + 180));
        for (int i = 0; i < 6; i++)
        {
            context.Rotate(pi / 3);
            context.SetFill(new Color((byte)(i * 40), (byte)(100 + i * 20), 200, 180));
            context.FillRect(new Rect(20, -5, 40, 10));
        }
        context.Restore();

        // Transform affects stroke
        context.Save();
        context.Translate((x + 80, y + 270));
        context.Scale((3, 1));
        context.SetStroke(Purple);
        context.LineWidth = 2;
        context.BeginPath();
        context.Arc((0, 0), 15, 0, pi * 2);
        context.Stroke();
        context.Restore();
    }
}
