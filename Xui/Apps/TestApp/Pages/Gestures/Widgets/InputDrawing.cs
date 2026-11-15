using Xui.Core.Canvas;
using Xui.Core.Math2D;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Gestures.Widgets;

internal static class InputDrawing
{
    public static void Panel(IContext ctx, Rect rect, Color color, NFloat radius = default)
    {
        ctx.BeginPath(); ctx.RoundRect(rect, radius == 0 ? 16 : radius); ctx.SetFill(color); ctx.Fill();
    }

    public static void Text(IContext ctx, string text, Point at, NFloat size, Color color, bool bold = false,
        TextAlign align = TextAlign.Left)
    {
        ctx.SetFont(new Font(size, "Inter", bold ? FontWeight.SemiBold : FontWeight.Normal));
        ctx.TextAlign = align; ctx.TextBaseline = TextBaseline.Middle;
        ctx.SetFill(color); ctx.FillText(text, at);
    }

    public static void Line(IContext ctx, Point from, Point to, Color color, NFloat width = default)
    {
        ctx.SetStroke(color); ctx.LineWidth = width == 0 ? 1 : width;
        ctx.BeginPath(); ctx.MoveTo(from); ctx.LineTo(to); ctx.Stroke();
    }

    public static void Clip(IContext ctx, Rect rect)
    {
        ctx.BeginPath(); ctx.RoundRect(rect, 16); ctx.Clip();
    }

    public static void Dots(IContext ctx, Rect rect)
    {
        ctx.SetFill(new Color(0xDCE2EEFF));
        for (var y = rect.Y + 16; y < rect.Bottom; y += 24)
            for (var x = rect.X + 16; x < rect.Right; x += 24)
            { ctx.BeginPath(); ctx.Arc((x, y), 1, 0, NFloat.Pi * 2); ctx.Fill(); }
    }
}
