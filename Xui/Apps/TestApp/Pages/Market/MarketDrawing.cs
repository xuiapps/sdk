using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Market;

internal static class MarketDrawing
{
    internal static readonly Color Ink = new(0x20233CFF), Muted = new(0x7C8198FF), Line = new(0xEBEDF4FF),
        Paper = new(0xF6F7FBFF), Up = new(0x00876EFF), Down = new(0xC74B67FF), Purple = new(0x7946E8FF);
    internal static readonly Font Small = new(10, "Inter"), Body = new(12, "Inter"),
        Strong = new(14, "Inter", FontWeight.SemiBold), Caption = new(10, "Inter", FontWeight.SemiBold),
        Title = new(26, "Inter", FontWeight.Bold), Price = new(32, "Inter", FontWeight.Bold);

    internal static void Text(IContext ctx, string text, Point point, Font font, Color color, TextAlign align = TextAlign.Left)
    {
        ctx.SetFont(font); ctx.TextAlign = align; ctx.TextBaseline = TextBaseline.Top;
        ctx.SetFill(color); ctx.FillText(text, point);
    }

    internal static void Rule(IContext ctx, Point a, Point b, Color color, NFloat width = default)
    {
        ctx.SetStroke(color); ctx.LineWidth = width == 0 ? 1 : width;
        ctx.BeginPath(); ctx.MoveTo(a); ctx.LineTo(b); ctx.Stroke();
    }

    internal static void Logo(IContext ctx, MarketStock stock, Rect rect)
    {
        ctx.Save(); ctx.Translate(rect.TopLeft); ctx.Scale((rect.Width / 44, rect.Height / 44));
        ctx.SetFill(stock.Paper); ctx.BeginPath(); ctx.RoundRect((0, 0, 44, 44), 12); ctx.Fill();
        ctx.SetFill(stock.Ink);
        foreach (var tile in stock.Logo) ctx.FillRect(tile);
        ctx.Restore();
    }

    internal static void Sparkline(IContext ctx, MarketStock stock, Rect rect)
    {
        ctx.SetFill(new Color(0xF0F1F5FF)); ctx.FillRect(rect);
        ctx.SetStroke(stock.IsPositive ? Up : Down); ctx.LineWidth = 1.5f;
        ctx.LineJoin = LineJoin.Round; ctx.BeginPath();
        for (var i = 0; i < stock.Sparkline.Length; i++)
        {
            var p = stock.Sparkline[i];
            Point point = (rect.X + p.X * rect.Width, rect.Y + p.Y * rect.Height);
            if (i == 0) ctx.MoveTo(point); else ctx.LineTo(point);
        }
        ctx.Stroke();
    }
}
