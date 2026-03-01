using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class ArcToFlowerTest : View
{
    // Petal outline: asymmetric zigzag shape to exercise various arcTo corner angles.
    // Upper edge has a concave notch at P3, producing a CW turn (all others are CCW).
    // Corner angles range from ~54° (sharp) to ~173° (nearly straight).
    private static readonly Point[] PetalPoints =
    [
        (12, 0),      // P0: base
        (25, -10),    // P1: upper edge
        (45, -22),    // P2: continuing up
        (52, -3),     // P3: notch down (concave — CW turn)
        (62, -18),    // P4: back up
        (78, -8),     // P5: approaching tip
        (88, 0),      // P6: tip
        (78, 12),     // P7: lower edge from tip
        (58, 22),     // P8: lower
        (38, 18),     // P9: continuing lower
        (25, 8),      // P10: approaching base
    ];

    private static readonly Color[] PetalColors =
    [
        new(0xe7, 0x4c, 0x3c, 0xFF),  // red
        new(0x34, 0x98, 0xdb, 0xFF),  // blue
        new(0x2e, 0xcc, 0x71, 0xFF),  // green
        new(0xf3, 0x9c, 0x12, 0xFF),  // orange
        new(0x9b, 0x59, 0xb6, 0xFF),  // purple
        new(0x1a, 0xbc, 0x9c, 0xFF),  // teal
    ];

    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat cx = this.Frame.X + 150;
        NFloat cy = this.Frame.Y + 150;
        NFloat r = 5;

        for (int i = 0; i < 6; i++)
        {
            NFloat angle = i * NFloat.Pi * 2 / 6;
            var color = PetalColors[i];

            context.Save();
            context.Translate((cx, cy));
            context.Rotate(angle);

            // Guide polygon (un-rounded outline)
            context.BeginPath();
            context.MoveTo(PetalPoints[0]);
            for (int j = 1; j < PetalPoints.Length; j++)
                context.LineTo(PetalPoints[j]);
            context.ClosePath();
            context.SetStroke(LightGray);
            context.LineWidth = 1;
            context.Stroke();

            // ArcTo path (rounded corners)
            context.BeginPath();
            context.MoveTo(PetalPoints[0]);
            for (int j = 1; j < PetalPoints.Length; j++)
            {
                var next = PetalPoints[(j + 1) % PetalPoints.Length];
                context.ArcTo(PetalPoints[j], next, r);
            }
            context.ClosePath();

            context.GlobalAlpha = 0.2f;
            context.SetFill(color);
            context.Fill();
            context.GlobalAlpha = 1.0f;
            context.SetStroke(color);
            context.LineWidth = 2;
            context.Stroke();

            context.Restore();
        }
    }
}
