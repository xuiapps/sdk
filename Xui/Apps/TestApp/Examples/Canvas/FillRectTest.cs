using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class FillRectTest : View
{
    protected override void RenderCore(IContext context)
    {
        // White background
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Red rectangle
        context.SetFill(Red);
        context.FillRect(new Rect(x + 10, y + 10, 100, 80));

        // Green rectangle
        context.SetFill(Green);
        context.FillRect(new Rect(x + 130, y + 10, 100, 80));

        // Blue rectangle
        context.SetFill(Blue);
        context.FillRect(new Rect(x + 250, y + 10, 100, 80));

        // Overlapping: orange over partial red
        context.SetFill(Orange);
        context.FillRect(new Rect(x + 60, y + 50, 100, 80));

        // Small black square
        context.SetFill(Black);
        context.FillRect(new Rect(x + 10, y + 150, 20, 20));
    }
}
