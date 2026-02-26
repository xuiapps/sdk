using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.FillRule;
using static Xui.Core.Canvas.Winding;

namespace Xui.Tests.Docs.Canvas.Views;

/// <summary>
/// Demonstrates LinearGradient and RadialGradient fills.
/// </summary>
public class GradientsView : View
{
    protected override void RenderCore(IContext context)
    {
        // Background
        context.SetFill(new Color(0xF5, 0xF5, 0xF5, 0xFF));
        context.FillRect(this.Frame);

        nfloat margin = 20, gap = 20;
        nfloat halfW = (this.Frame.Width - 2 * margin - gap) / 2;
        nfloat h = this.Frame.Height - 2 * margin;

        // ── Left: horizontal linear gradient ──────────────────────────────
        nfloat lx = margin, ly = margin;
        context.SetFill(new LinearGradient(
            start: new Point(lx, ly),
            end:   new Point(lx + halfW, ly),
            gradient: [
                new(0.0f, new Color(0xD6, 0x42, 0xCD, 0xFF)),
                new(0.5f, new Color(0x8A, 0x05, 0xFF, 0xFF)),
                new(1.0f, new Color(0x41, 0x96, 0xD0, 0xFF)),
            ]));
        context.BeginPath();
        context.RoundRect(new Rect(lx, ly, halfW, h), 14);
        context.Fill(NonZero);

        // ── Right: radial gradient ─────────────────────────────────────────
        nfloat rx = margin + halfW + gap, ry = margin;
        nfloat rcx = rx + halfW / 2, rcy = ry + h / 2;
        context.SetFill(new RadialGradient(
            startCenter: new Point(rcx - halfW * 0.15f, rcy - h * 0.15f),
            startRadius: 0,
            endCenter:   new Point(rcx, rcy),
            endRadius:   halfW * 0.6f,
            gradientStops: [
                new(0.0f, new Color(0xFF, 0xE0, 0x60, 0xFF)),
                new(0.6f, new Color(0xE0, 0x55, 0x55, 0xFF)),
                new(1.0f, new Color(0x60, 0x10, 0x40, 0xFF)),
            ]));
        context.BeginPath();
        context.RoundRect(new Rect(rx, ry, halfW, h), 14);
        context.Fill(NonZero);
    }
}
