using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.FillRule;
using static Xui.Core.Canvas.Winding;

namespace Xui.Tests.Docs.Canvas.Views;

/// <summary>
/// Demonstrates clipping: coloured stripes clipped to a circle,
/// and an unclipped area outside showing how Save/Restore scope clipping.
/// </summary>
public class ClipView : View
{
    protected override void RenderCore(IContext context)
    {
        // Background
        context.SetFill(new Color(0xF5, 0xF5, 0xF5, 0xFF));
        context.FillRect(this.Frame);

        nfloat cx = 140, cy = 120, r = 100;

        // ── Left: striped circle via clip ──────────────────────────────────
        context.Save();

        context.BeginPath();
        context.Arc(new Point(cx, cy), r, 0, 2 * NFloat.Pi, ClockWise);
        context.Clip();

        // Vertical colour stripes (drawn over whole area, clip masks to circle)
        Color[] stripeColors =
        [
            new(0xD6, 0x42, 0xCD, 0xFF),
            new(0x8A, 0x05, 0xFF, 0xFF),
            new(0x41, 0x96, 0xD0, 0xFF),
            new(0x59, 0xB3, 0x5C, 0xFF),
            new(0xE0, 0x55, 0x55, 0xFF),
        ];
        nfloat stripeW = (r * 2) / stripeColors.Length;
        for (int i = 0; i < stripeColors.Length; i++)
        {
            context.SetFill(stripeColors[i]);
            context.FillRect(new Rect(cx - r + i * stripeW, cy - r, stripeW, r * 2));
        }

        context.Restore(); // clip released here

        // Circle outline to frame the clip region
        context.SetStroke(new Color(0x40, 0x40, 0x40, 0xFF));
        context.LineWidth = 2;
        context.BeginPath();
        context.Arc(new Point(cx, cy), r, 0, 2 * NFloat.Pi, ClockWise);
        context.Stroke();

        // ── Right: nested clip demonstration ──────────────────────────────
        nfloat bx = 290, by = 20, bw = 170, bh = 200;

        context.Save();

        // Outer clip: rounded rectangle
        context.BeginPath();
        context.RoundRect(new Rect(bx, by, bw, bh), 24);
        context.Clip();

        // Gradient background inside outer clip
        context.SetFill(new LinearGradient(
            start: new Point(bx, by),
            end:   new Point(bx, by + bh),
            gradient: [
                new(0f, new Color(0xE0, 0xF0, 0xFF, 0xFF)),
                new(1f, new Color(0xA0, 0xC0, 0xFF, 0xFF)),
            ]));
        context.FillRect(new Rect(bx, by, bw, bh));

        // Inner clip: smaller circle
        nfloat icx = bx + bw / 2, icy = by + bh / 2;
        context.Save();
        context.BeginPath();
        context.Arc(new Point(icx, icy), 60, 0, 2 * NFloat.Pi, ClockWise);
        context.Clip();

        context.SetFill(new Color(0x8A, 0x05, 0xFF, 0xC0));
        context.FillRect(new Rect(bx, by, bw, bh));

        context.Restore(); // inner clip released

        context.Restore(); // outer clip released

        // Outline of outer clip region
        context.SetStroke(new Color(0x40, 0x40, 0x80, 0xFF));
        context.LineWidth = 2;
        context.BeginPath();
        context.RoundRect(new Rect(bx, by, bw, bh), 24);
        context.Stroke();
    }
}
