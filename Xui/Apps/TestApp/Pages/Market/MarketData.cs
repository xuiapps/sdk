using System.Globalization;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Market;

/// <summary>Fixed-seed fictional market. Rows are cached on demand; detail payloads load when opened.</summary>
public sealed class MarketData
{
    public const int CompanyCount = 5_000;
    public static MarketData Shared => SharedData.Value;
    private static class SharedData { internal static readonly MarketData Value = new(0x584D4152); }
    private const int SessionCount = 60;
    // Each company consumes a fixed PRNG sequence: sentiment/start, OHLC sessions,
    // three commentary choices, palette, eight logo cells, and two volume digits.
    private const int RandomStepsPerCompany = 2 + SessionCount * 4 + 3 + 1 + 8 + 2;
    private readonly MarketStock?[] stocks = new MarketStock?[CompanyCount];
    private readonly uint[] seeds = new uint[CompanyCount];
    public int Count => stocks.Length;
    public int LoadedCount { get; private set; }
    public MarketStock this[int index]
    {
        get
        {
            if (stocks[index] is { } stock) return stock;
            stock = Create(index, seeds[index]);
            stocks[index] = stock;
            LoadedCount++;
            return stock;
        }
    }

    public MarketData(uint seed)
    {
        var random = new SeededRandom(seed);
        // Checkpoint integer state only, preserving the original dataset regardless of
        // access order without constructing 5,000 histories, descriptions, or logos.
        for (var i = 0; i < seeds.Length; i++)
        {
            seeds[i] = random.State;
            random.Skip(RandomStepsPerCompany);
        }
    }

    private static readonly string[] roots = ["Aster", "Nova", "Vertex", "Lumen", "Cedar", "Orbit", "Atlas", "Ember", "Helix", "Opal",
            "Solace", "Vela", "Cobalt", "Alto", "Pine", "Nexus", "Arbor", "Prism", "Flux", "Vale",
            "Echo", "Sable", "Kite", "Onyx", "Lyra"];
    private static readonly string[] businesses = ["Cloud", "Energy", "Health", "Capital", "Robotics", "Foods", "Systems", "Labs", "Mobility", "Media",
            "Solar", "Networks", "Materials", "Retail", "Logistics", "Digital", "Water", "Devices", "Homes", "Aerospace"];
    private static readonly string[] series = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"];

    private static readonly Color[] inks = [new(0x7147EBFF), new(0x007F80FF), new(0xC05235FF), new(0x3468C7FF), new(0xAE4080FF)];
    private static readonly Color[] papers = [new(0xEEE8FFFF), new(0xDCF3EEFF), new(0xFCEAE0FF), new(0xE5EEFFFF), new(0xF9E5F0FF)];

    private static MarketStock Create(int index, uint seed)
    {
        var random = new SeededRandom(seed);
        Span<char> ticker = stackalloc char[4];
        var code = (index * 7919 + 113) % 456976;
        for (var j = 3; j >= 0; j--) { ticker[j] = (char)('A' + code % 26); code /= 26; }
        var name = $"{roots[index % 25]} {businesses[index / 25 % 20]} {series[index / 500]}";
        // Only normalized close points survive in the list item. Full OHLC history is
        // regenerated from the same seed when the detail page asks for it.
        Span<MarketBar> bars = stackalloc MarketBar[SessionCount];
        History(ref random, bars, out var initial, out var low, out var high);
        var close = bars[^1].Close;
        var change = (close / initial - 1) * 100;
        var trend = change > 1 ? "Bullish" : change < -1 ? "Bearish" : "Sideways";
        random.Skip(3); // Deferred commentary choices still occupy their original seed positions.
        var palette = random.Int(5);
        // Compile a symmetric mosaic once. The canvas only consumes its immutable rectangles.
        Span<Rect> logo = stackalloc Rect[16];
        var logoCount = 0;
        for (var y = 0; y < 4; y++)
            for (var x = 0; x < 2; x++)
                if (random.Int(3) != 0 || x == 0 && y == 0)
                {
                    logo[logoCount++] = new Rect(7 + x * 8, 7 + y * 8, 6, 6);
                    logo[logoCount++] = new Rect(7 + (3 - x) * 8, 7 + y * 8, 6, 6);
                }
        var spark = new Point[bars.Length];
        for (var j = 0; j < spark.Length; j++) spark[j] = new((NFloat)j / (spark.Length - 1), 1 - (bars[j].Close - low) / (high - low));
        return new MarketStock(new string(ticker), name, businesses[index / 25 % 20],
            Money(close), change.ToString("+0.00;-0.00;0.00", CultureInfo.InvariantCulture) + "%",
            trend, $"Vol {(1 + random.Int(99)).ToString(CultureInfo.InvariantCulture)}.{random.Int(10)}M · XSIM",
            change >= 0, inks[palette], papers[palette], logo[..logoCount].ToArray(), spark, low, high) { Seed = seed };
    }

    private static void History(ref SeededRandom random, Span<MarketBar> bars,
        out NFloat initial, out NFloat low, out NFloat high)
    {
        var sentiment = random.Int(3); // bullish / bearish / sideways; copy and chart agree.
        NFloat drift = sentiment == 0 ? .004f : sentiment == 1 ? -.004f : 0;
        var close = 20 + random.Unit() * 380;
        initial = close;
        low = NFloat.MaxValue; high = 0;
        for (var j = 0; j < bars.Length; j++)
        {
            var open = close * (1 + (random.Unit() - .5f) * .006f);
            close = open * (1 + drift + (random.Unit() - .5f) * .012f);
            var barLow = NFloat.Min(open, close) * (1 - .002f - random.Unit() * .009f);
            var barHigh = NFloat.Max(open, close) * (1 + .002f + random.Unit() * .009f);
            bars[j] = new(open, barHigh, barLow, close);
            low = NFloat.Min(low, barLow); high = NFloat.Max(high, barHigh);
        }
    }

    internal static MarketDetails CreateDetails(MarketStock stock)
    {
        var random = new SeededRandom(stock.Seed);
        var bars = new MarketBar[SessionCount];
        History(ref random, bars, out var initial, out var low, out var high);
        var change = (bars[^1].Close / initial - 1) * 100;
        var trendGroup = change > 1 ? 0 : change < -1 ? 10 : 20;
        var first = Commentary[trendGroup + random.Int(10)];
        var secondIndex = 30 + random.Int(20);
        var thirdIndex = 30 + random.Int(19);
        if (thirdIndex >= secondIndex) thirdIndex++;
        var description = $"{stock.Name} is a fictional {stock.Sector.ToLowerInvariant()} company. {first} {Commentary[secondIndex]} {Commentary[thirdIndex]}";
        var axis = new string[5];
        for (var j = 0; j < axis.Length; j++) axis[j] = Money(high - (high - low) * j / 4);
        var last = bars[^1];
        return new(description, bars, axis,
            [Money(last.Open), Money(last.High), Money(last.Low), Money(last.Close)]);
    }

    private static string Money(NFloat value) => "$" + value.ToString("0.00", CultureInfo.InvariantCulture);

    // 50 reusable sentences: ten per direction and twenty neutral observations.
    private static readonly string[] Commentary =
    [
        "Buyers have steadily lifted the trading range over this sample period.",
        "The upward trend is supported by a sequence of higher closing prices.",
        "Bullish momentum has survived several brief rounds of profit taking.",
        "Recent sessions suggest growing confidence in the company's outlook.",
        "Demand has strengthened while pullbacks have remained relatively shallow.",
        "The price has advanced as buyers returned near earlier support levels.",
        "A constructive trend has emerged after a quiet opening stretch.",
        "Higher lows suggest that investors are gradually accepting richer valuations.",
        "The latest recovery has carried the stock beyond its early trading range.",
        "Positive price action points to a market willing to reward steady progress.",
        "Sellers have pushed the trading range lower over this sample period.",
        "Bearish momentum has kept recent rebounds short lived.",
        "A sequence of lower closes reflects a more cautious market mood.",
        "The stock has struggled to hold gains after brief recovery attempts.",
        "Selling pressure has outweighed demand through much of the period.",
        "Recent weakness suggests investors are reassessing near term expectations.",
        "Lower highs indicate that buyers are waiting for clearer signals.",
        "The downward drift has continued despite occasional positive sessions.",
        "A defensive tone has emerged as the share price tests lower levels.",
        "The market has priced in a more restrained outlook during this window.",
        "The stock has traded broadly sideways across this sample period.",
        "Neither buyers nor sellers have established a lasting directional advantage.",
        "The closing price remains close to the opening trading range.",
        "Short rallies and pullbacks have largely balanced each other out.",
        "A flat trend suggests the market is waiting for a new catalyst.",
        "The price has consolidated while investors weigh competing signals.",
        "Recent sessions have favored range trading over a sustained breakout.",
        "Balanced demand has kept the stock near familiar price levels.",
        "The sideways pattern reflects a pause in directional conviction.",
        "Muted net movement masks a series of smaller swings within the range.",
        "Management continues to emphasize operating discipline and recurring revenue.",
        "The next earnings update may offer a clearer view of underlying demand.",
        "Margins remain a useful measure of how efficiently growth is being delivered.",
        "Investors are watching the balance between expansion and cash generation.",
        "Customer retention will help shape the longer term revenue profile.",
        "Capital spending is aimed at improving capacity and service reliability.",
        "The business is exploring adjacent markets without abandoning its core offering.",
        "A diversified customer base can soften changes in individual end markets.",
        "Execution on the current product roadmap remains a central focus.",
        "Input costs and pricing discipline will influence the next operating cycle.",
        "Working capital trends may reveal more than a single headline growth figure.",
        "The company is balancing investment in new products with cost control.",
        "Seasonal demand can make comparisons between neighboring quarters uneven.",
        "Longer contracts provide some visibility into future activity.",
        "Distribution partnerships could broaden access to new customers.",
        "Supply chain flexibility remains an important operational priority.",
        "A steady balance sheet gives the business room to respond to changing conditions.",
        "Product mix will play a role in the quality of future earnings.",
        "Competitive positioning depends on consistent service as well as pricing.",
        "The broader industry cycle remains a useful context for company results."
    ];

    private struct SeededRandom(uint seed)
    {
        private uint state = seed == 0 ? 1 : seed;
        private uint Next() { var x = state; x ^= x << 13; x ^= x >> 17; x ^= x << 5; return state = x; }
        public uint State => state;
        public void Skip(int count) { for (var i = 0; i < count; i++) Next(); }
        public int Int(int limit) => (int)(Next() % (uint)limit);
        public NFloat Unit() => (NFloat)(Next() & 0xFFFFFF) / 0x1000000;
    }
}

public readonly record struct MarketBar(NFloat Open, NFloat High, NFloat Low, NFloat Close);

public sealed record MarketStock(string Symbol, string Name, string Sector, string Price, string Change,
    string Trend, string Volume, bool IsPositive, Color Ink, Color Paper,
    Rect[] Logo, Point[] Sparkline, NFloat Low, NFloat High)
{
    internal uint Seed { get; init; }
    private MarketDetails? details;
    private MarketDetails Details => details ??= MarketData.CreateDetails(this);
    internal void PrepareDetails() => _ = Details;
    public bool HasDetails => details is not null;
    public string Description => Details.Description;
    public MarketBar[] Bars => Details.Bars;
    public string[] AxisLabels => Details.AxisLabels;
    public string[] OhlcLabels => Details.OhlcLabels;
}

internal sealed record MarketDetails(string Description, MarketBar[] Bars, string[] AxisLabels, string[] OhlcLabels);
