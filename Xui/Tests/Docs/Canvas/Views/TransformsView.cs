using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.Canvas.FillRule;

namespace Xui.Tests.Docs.Canvas.Views;

/// <summary>
/// Demonstrates Save/Restore with Translate, Rotate and Scale.
/// Each panel draws the same 60×60 square under a different transform.
/// </summary>
public class TransformsView : View
{
    protected override void RenderCore(IContext context)
    {
        // Background
        context.SetFill(new Color(0xF5, 0xF5, 0xF5, 0xFF));
        context.FillRect(this.Frame);

        NFloat squareSize = 60;
        var fill = new Color(0x41, 0x96, 0xD0, 0xFF);
        var stroke = new Color(0x20, 0x60, 0xA0, 0xFF);

        // ── Panel 1: no transform (identity) ──────────────────────────────
        DrawLabel(context, "identity", 80, 20);
        context.Save();
        context.SetFill(fill);
        context.FillRect(new Rect(50, 50, squareSize, squareSize));
        context.SetStroke(stroke);
        context.LineWidth = 2;
        context.StrokeRect(new Rect(50, 50, squareSize, squareSize));
        context.Restore();

        // ── Panel 2: Translate ─────────────────────────────────────────────
        DrawLabel(context, "Translate(190,30)", 240, 20);
        context.Save();
        context.Translate((190, 30));
        context.SetFill(new Color(0xE0, 0x55, 0x55, 0xFF));
        context.FillRect(new Rect(50, 50, squareSize, squareSize));
        context.SetStroke(new Color(0xA0, 0x20, 0x20, 0xFF));
        context.LineWidth = 2;
        context.StrokeRect(new Rect(50, 50, squareSize, squareSize));
        context.Restore();

        // ── Panel 3: Translate + Rotate ────────────────────────────────────
        DrawLabel(context, "Rotate(45°)", 380, 20);
        context.Save();
        context.Translate((410, 120));  // pivot point
        context.Rotate(NFloat.Pi / 4);
        context.SetFill(new Color(0x59, 0xB3, 0x5C, 0xFF));
        context.FillRect(new Rect(-squareSize / 2, -squareSize / 2, squareSize, squareSize));
        context.SetStroke(new Color(0x20, 0x80, 0x30, 0xFF));
        context.LineWidth = 2;
        context.StrokeRect(new Rect(-squareSize / 2, -squareSize / 2, squareSize, squareSize));
        context.Restore();

        // ── Panel 4: Scale ─────────────────────────────────────────────────
        DrawLabel(context, "Scale(0.5)", 370, 140);
        context.Save();
        context.Translate((370, 180));
        context.Scale((0.5f, 0.5f));
        context.SetFill(new Color(0xA0, 0x60, 0xFF, 0xFF));
        context.FillRect(new Rect(-squareSize / 2, -squareSize / 2, squareSize, squareSize));
        context.SetStroke(new Color(0x60, 0x20, 0xCC, 0xFF));
        context.LineWidth = 4;   // compensate for scale
        context.StrokeRect(new Rect(-squareSize / 2, -squareSize / 2, squareSize, squareSize));
        context.Restore();
    }

    private static void DrawLabel(IContext context, string text, NFloat x, NFloat y)
    {
        context.SetFill(new Color(0x60, 0x60, 0x60, 0xFF));
        context.SetFont(new Font(12, ["Inter"], FontWeight.Normal));
        context.TextAlign = TextAlign.Center;
        context.TextBaseline = TextBaseline.Top;
        context.FillText(text, new Point(x, y));
    }
}
