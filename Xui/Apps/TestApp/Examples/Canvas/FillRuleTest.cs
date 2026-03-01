using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class FillRuleTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;
        NFloat pi = NFloat.Pi;

        // NonZero fill rule (default) - concentric circles
        context.SetFill(Blue);
        context.BeginPath();
        context.Arc((x + 80, y + 80), 60, 0, pi * 2);
        context.Arc((x + 80, y + 80), 30, 0, pi * 2);
        context.Fill(FillRule.NonZero);

        // EvenOdd fill rule - concentric circles create a ring
        context.SetFill(Red);
        context.BeginPath();
        context.Arc((x + 220, y + 80), 60, 0, pi * 2);
        context.Arc((x + 220, y + 80), 30, 0, pi * 2);
        context.Fill(FillRule.EvenOdd);

        // NonZero - overlapping rectangles
        context.SetFill(Green);
        context.BeginPath();
        context.Rect(new Rect(x + 20, y + 170, 100, 100));
        context.Rect(new Rect(x + 50, y + 200, 100, 100));
        context.Fill(FillRule.NonZero);

        // EvenOdd - overlapping rectangles show intersection hole
        context.SetFill(Orange);
        context.BeginPath();
        context.Rect(new Rect(x + 180, y + 170, 100, 100));
        context.Rect(new Rect(x + 210, y + 200, 100, 100));
        context.Fill(FillRule.EvenOdd);
    }
}
