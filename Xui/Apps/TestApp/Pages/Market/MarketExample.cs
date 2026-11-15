using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Apps.TestApp.Examples;
using NFloat = System.Runtime.InteropServices.NFloat;
using static Xui.Apps.TestApp.Pages.Market.MarketDrawing;

namespace Xui.Apps.TestApp.Pages.Market;

public sealed class MarketExample : Example, IMobileExamplePresentation
{
    private readonly MarketListPage list;
    public MarketExample()
    {
        Title = "Market Pulse";
        Content = list = new MarketListPage(MarketData.Shared);
    }

    public bool DrawsUnderHeader => true;
    public void UseMobilePresentation() => list.Scroller.ClipToViewport = false;
}

/// <summary>The list remains on the application's navigation stack while a stock page is pushed.</summary>
public sealed class MarketListPage : ViewCollection
{
    public MarketGrid Grid { get; }
    public ScrollView Scroller { get; }
    public MarketListPage(MarketData data)
    {
        Id = "Market.List";
        Grid = new MarketGrid(data) { Selected = Open };
        // The page header/safe area already inset this viewport from the screen corners.
        Add(Scroller = new ScrollView { Id = "Market.Scroll", Content = Grid, ScrollbarEndInset = 0 });
    }

    private void Open(MarketStock stock)
    {
        if (this.TryFindExampleNavigation(out var navigation))
            navigation!.PushExamplePage(new MarketDetailExample(stock));
    }
}

public sealed class MarketDetailExample : Example
{
    public MarketStock Stock { get; }
    public MarketDetailExample(MarketStock stock)
    {
        Stock = stock;
        Id = "Market.Detail";
        Title = stock.Symbol + " · Price history";
        Content = new ScrollView { Id = "Market.DetailScroll", Content = new MarketDetailCanvas(stock), ScrollbarEndInset = 0 };
    }
}

internal sealed class MarketDetailCanvas(MarketStock stock) : View
{
    private static readonly Font Prose = new(14, "Inter");
    private static readonly string[] OhlcNames = ["OPEN", "HIGH", "LOW", "CLOSE"];
    private readonly string exchange = stock.Symbol + " / XSIM";
    private readonly string sectorTrend = stock.Trend + " · " + stock.Sector;
    private string[] lines = [];
    private NFloat wrappedWidth = -1;

    protected override Size MeasureCore(Size available, IMeasureContext ctx)
    {
        var width = NFloat.IsFinite(available.Width) ? available.Width : 390;
        if (wrappedWidth != width)
        {
            // Text layout is prepared only when width changes, never during a render frame.
            wrappedWidth = width;
            ctx.SetFont(Prose);
            var words = stock.Description.Split(' ');
            var wrapped = new List<string>();
            var line = "";
            foreach (var word in words)
            {
                var candidate = line.Length == 0 ? word : line + " " + word;
                if (line.Length > 0 && ctx.MeasureText(candidate).Size.Width > width - 40)
                { wrapped.Add(line); line = word; }
                else line = candidate;
            }
            if (line.Length > 0) wrapped.Add(line);
            lines = wrapped.ToArray();
        }
        return (width, 570 + lines.Length * 22);
    }

    protected override void RenderCore(IContext ctx)
    {
        var x = Frame.X + 20; var y = Frame.Y; var width = Frame.Width - 40;
        ctx.SetFill(Colors.White); ctx.FillRect(Frame);
        Logo(ctx, stock, (x, y + 8, 52, 52));
        Text(ctx, exchange, (x + 66, y + 12), Strong, Ink);
        Text(ctx, stock.Name, (x + 66, y + 36), Body, Muted);
        Text(ctx, stock.Price, (x, y + 82), Price, Ink);
        Text(ctx, stock.Change, (Frame.Right - 20, y + 88), Strong, stock.IsPositive ? Up : Down, TextAlign.Right);
        Text(ctx, "60-session return", (Frame.Right - 20, y + 111), Small, Muted, TextAlign.Right);
        Text(ctx, sectorTrend, (x, y + 127), Body, Muted);
        Text(ctx, "PRICE HISTORY", (x, y + 166), Caption, Purple);
        Text(ctx, "60 daily candles · USD", (Frame.Right - 20, y + 166), Small, Muted, TextAlign.Right);
        DrawChart(ctx, (x, y + 199, width - 56, 214));
        Text(ctx, "SESSION 01", (x, y + 426), Small, Muted);
        Text(ctx, "SESSION 60", (Frame.Right - 76, y + 426), Small, Muted, TextAlign.Right);
        for (var i = 0; i < 4; i++)
        {
            var cellX = x + i * width / 4;
            Text(ctx, OhlcNames[i], (cellX, y + 464), Caption, Muted);
            Text(ctx, stock.OhlcLabels[i], (cellX, y + 483), Strong, Ink);
        }
        Rule(ctx, (x, y + 517), (Frame.Right - 20, y + 517), Line);
        Text(ctx, "THE COMPANY BRIEF", (x, y + 537), Caption, Purple);
        for (var i = 0; i < lines.Length; i++) Text(ctx, lines[i], (x, y + 565 + i * 22), Prose, Ink);
    }

    private void DrawChart(IContext ctx, Rect r)
    {
        var span = stock.High - stock.Low;
        for (var i = 0; i < 5; i++)
        {
            var y = r.Y + r.Height * i / 4;
            Rule(ctx, (r.X, y), (r.Right, y), Line);
            Text(ctx, stock.AxisLabels[i], (r.Right + 8, y - 6), Small, Muted);
        }
        var step = r.Width / stock.Bars.Length;
        for (var i = 0; i < stock.Bars.Length; i++)
        {
            var bar = stock.Bars[i]; var x = r.X + step * (i + .5f);
            var color = bar.Close >= bar.Open ? Up : Down;
            NFloat Y(NFloat value) => r.Bottom - (value - stock.Low) / span * r.Height;
            Rule(ctx, (x, Y(bar.High)), (x, Y(bar.Low)), color);
            var top = NFloat.Min(Y(bar.Open), Y(bar.Close));
            ctx.SetFill(color);
            ctx.FillRect((x - step * .3f, top, NFloat.Max(1, step * .6f), NFloat.Max(1, NFloat.Abs(Y(bar.Close) - Y(bar.Open)))));
        }
    }
}
