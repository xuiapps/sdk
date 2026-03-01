using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

public class ArcFlowerTest : View
{
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
        NFloat pi = NFloat.Pi;

        // 6 petals, each a group of concentric arc strokes at different sweep sizes.
        // Each petal faces outward from the center.
        // Inner arcs use CW, outer arcs use CCW — tests both windings.
        // Sweep sizes: 90°, 180°, 270° (quarter, half, three-quarter arcs)
        for (int i = 0; i < 6; i++)
        {
            NFloat angle = i * pi * 2 / 6;
            var color = PetalColors[i];

            context.Save();
            context.Translate((cx, cy));
            context.Rotate(angle);

            // Each petal: 3 concentric arcs at increasing radii,
            // centered at (55, 0) — offset from flower center.
            NFloat petalCx = 55;
            NFloat petalCy = 0;

            // Arc 1: small, 90° sweep, CCW (like signal strength inner ring)
            context.BeginPath();
            context.Arc((petalCx, petalCy), 12, pi * 1.75f, pi * 1.25f, Winding.CounterClockWise);
            context.SetStroke(color);
            context.LineWidth = 4;
            context.Stroke();

            // Arc 2: medium, 180° sweep, CCW
            context.BeginPath();
            context.Arc((petalCx, petalCy), 22, pi * 1.5f, pi * 0.5f, Winding.CounterClockWise);
            context.SetStroke(color);
            context.LineWidth = 3;
            context.Stroke();

            // Arc 3: large, 270° sweep, CW (default winding)
            context.BeginPath();
            context.Arc((petalCx, petalCy), 32, pi * 0.25f, pi * 1.75f, Winding.ClockWise);
            context.SetStroke(color);
            context.LineWidth = 2;
            context.Stroke();

            // Arc 4: filled 90° pie wedge, CW (default winding)
            context.BeginPath();
            context.MoveTo((petalCx, petalCy));
            context.Arc((petalCx, petalCy), 10, pi * 1.5f, pi * 2.0f, Winding.ClockWise);
            context.ClosePath();
            context.GlobalAlpha = 0.3f;
            context.SetFill(color);
            context.Fill();
            context.GlobalAlpha = 1.0f;

            // Guide: full circle outline for reference
            context.BeginPath();
            context.Arc((petalCx, petalCy), 32, 0, pi * 2);
            context.SetStroke(LightGray);
            context.LineWidth = 0.5f;
            context.Stroke();

            context.Restore();
        }
    }
}
