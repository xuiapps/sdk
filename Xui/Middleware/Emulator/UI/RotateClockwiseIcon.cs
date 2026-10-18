using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator.UI;

/// <summary>Renders a clockwise rotation glyph.</summary>
internal sealed class RotateClockwiseIcon : IToolButtonIcon
{
    public void Render(IContext context, Point center)
    {
        context.Save();
        try
        {
            context.Translate(center);
            RenderAtOrigin(context);
        }
        finally
        {
            context.Restore();
        }
    }

    internal static void RenderAtOrigin(IContext context)
    {
        context.BeginPath();
        context.Arc((0, 0), 6.5f, -NFloat.Pi * .18f, NFloat.Pi * 1.4f, Winding.ClockWise);
        context.SetStroke(new Color(0x4A4A4AFF));
        context.LineWidth = 1.5f;
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Round;
        context.Stroke();

        var tipAngle = NFloat.Pi * 1.4f;
        var tip = new Point(NFloat.Cos(tipAngle) * 6.5f, NFloat.Sin(tipAngle) * 6.5f);
        context.BeginPath();
        context.MoveTo(tip);
        context.LineTo(tip + (4f, -1f));
        context.LineTo(tip + (1f, 3.5f));
        context.ClosePath();
        context.SetFill(new Color(0x4A4A4AFF));
        context.Fill();
    }
}
