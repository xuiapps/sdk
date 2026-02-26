using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.Canvas.FillRule;

namespace Xui.Tests.Docs.Canvas.Views;

/// <summary>
/// Demonstrates solid fill colors, rounded rectangles, and stroke outlines.
/// </summary>
public class FillAndStrokeView : View
{
    protected override void RenderCore(IContext context)
    {
        // Background
        context.SetFill(new Color(0xF5, 0xF5, 0xF5, 0xFF));
        context.FillRect(this.Frame);

        nfloat margin = 20;
        nfloat blockW = 120, blockH = 80;

        // Three solid-color rectangles
        context.SetFill(new Color(0x41, 0x96, 0xD0, 0xFF));
        context.FillRect(new Rect(margin, margin, blockW, blockH));

        context.SetFill(new Color(0xE0, 0x55, 0x55, 0xFF));
        context.FillRect(new Rect(margin + blockW + 20, margin, blockW, blockH));

        context.SetFill(new Color(0x59, 0xB3, 0x5C, 0xFF));
        context.FillRect(new Rect(margin + 2 * (blockW + 20), margin, blockW, blockH));

        // Stroke outline on the first rect
        context.SetStroke(new Color(0x20, 0x20, 0x20, 0xFF));
        context.LineWidth = 3;
        context.StrokeRect(new Rect(margin, margin, blockW, blockH));

        // Rounded rect filling the bottom strip
        context.SetFill(new Color(0xA0, 0x60, 0xFF, 0xFF));
        context.BeginPath();
        context.RoundRect(new Rect(margin, margin + blockH + 24, this.Frame.Width - 2 * margin, 96), 16);
        context.Fill(NonZero);

        // Stroked rounded rect on top for contrast
        context.SetStroke(new Color(0x60, 0x20, 0xCC, 0xFF));
        context.LineWidth = 2;
        context.BeginPath();
        context.RoundRect(new Rect(margin + 8, margin + blockH + 32, this.Frame.Width - 2 * margin - 16, 80), 12);
        context.Stroke();
    }
}
