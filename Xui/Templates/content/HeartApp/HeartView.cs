using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace NewBlankApp;

public class HeartView : View
{
    protected override Size MeasureCore(Size available, IMeasureContext context) => (150, 150);

    protected override void RenderCore(IContext context)
    {
        NFloat cx = this.Frame.X + this.Frame.Width * 0.5f;
        NFloat cy = this.Frame.Y + this.Frame.Height * 0.5f;
        NFloat size = NFloat.Min(this.Frame.Width, this.Frame.Height);

        context.BeginPath();
        DrawHeart(context, cx, cy, size);
        context.SetFill(new LinearGradient(
            start: (cx, cy - size * 0.5f),
            end: (cx, cy + size * 0.5f),
            gradient: [
                (0.0f, 0xD642CDFF),
                (1.0f, 0x8A05FFFF),
            ]));
        context.Fill();
    }

    private static void DrawHeart(IContext ctx, NFloat cx, NFloat cy, NFloat size)
    {
        NFloat s = size * 0.5f;

        Point top    = (cx,             cy - 0.25f * s);
        Point bottom = (cx,             cy + 0.95f * s);
        Point c1     = (cx + 0.50f * s, cy - 0.95f * s);
        Point c2     = (cx + 1.20f * s, cy - 0.05f * s);
        Point c3     = (cx - 1.20f * s, cy - 0.05f * s);
        Point c4     = (cx - 0.50f * s, cy - 0.95f * s);

        ctx.MoveTo(top);
        ctx.CurveTo(c1, c2, bottom);
        ctx.CurveTo(c3, c4, top);
        ctx.ClosePath();
    }
}
