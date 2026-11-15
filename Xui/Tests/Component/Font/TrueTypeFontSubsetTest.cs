using System.Text.RegularExpressions;
using System.Text;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using CanvasFont = Xui.Core.Canvas.Font;

namespace Xui.Tests.Component.Font;

public sealed class TrueTypeFontSubsetTest
{
    [Fact]
    public void FontUsage_CollectsUniqueCharactersAndAdjacentPairs()
    {
        var usage = new FontUsage();

        usage.Add("Hello");
        usage.Add("World!");

        Assert.Equal([R('!'), R('H'), R('W'), R('d'), R('e'), R('l'), R('o'), R('r')], usage.Characters.Order());
        Assert.Contains(new FontUsage.RunePair(R('H'), R('e')), usage.Pairs);
        Assert.Contains(new FontUsage.RunePair(R('l'), R('o')), usage.Pairs);
        Assert.Contains(new FontUsage.RunePair(R('W'), R('o')), usage.Pairs);
        Assert.DoesNotContain(new FontUsage.RunePair(R('o'), R('W')), usage.Pairs);
    }

    [Fact]
    public void Create_PreservesRenderedGlyphsAndTextMetrics()
    {
        var original = LoadInter();
        var usage = new FontUsage();
        const string text = "Hello World! AVATAR";
        usage.Add(text);

        var bytes = TrueTypeFontSubset.Create(original, usage);
        var subset = new TrueTypeFont(bytes);
        var font = new CanvasFont(32, "Inter");

        Assert.True(bytes.Length < original.Blob.Length);
        Assert.Contains(usage.Pairs, pair => original.Kerning[pair.Left, pair.Right].XAdvance is not null and not 0);
        foreach (var character in usage.Characters)
        {
            Assert.NotNull(subset.GetGlyphIndex(character));
            var originalGlyph = original.GetGlyphIndex(character)!.Value;
            if (original.TryGetGlyph(originalGlyph, out _))
                Assert.True(subset.TryGetGlyph(subset.GetGlyphIndex(character)!.Value, out _));
        }
        foreach (var pair in usage.Pairs)
        {
            var originalAdvance = original.Kerning[pair.Left, pair.Right].XAdvance ?? 0;
            var subsetAdvance = subset.Kerning[pair.Left, pair.Right].XAdvance ?? 0;
            Assert.Equal(originalAdvance, subsetAdvance);
        }
        Assert.Equal(original.MeasureText(text, font).Line.Width, subset.MeasureText(text, font).Line.Width);
    }

    [Fact]
    public void MeasureText_UsesTightGlyphBoundsInsteadOfAdvanceBounds()
    {
        var font = LoadInter();
        var leftAligned = font.MeasureText("Hello World!", new CanvasFont(64, "Inter"));
        var centered = font.MeasureText("Hello World!", new CanvasFont(64, "Inter"), TextAlign.Center);

        Assert.True(leftAligned.Line.ActualBoundingBoxLeft < 0);
        Assert.True(leftAligned.Line.ActualBoundingBoxRight < leftAligned.Line.Width);
        Assert.True(centered.Line.ActualBoundingBoxLeft < centered.Line.Width / 2);
        Assert.True(centered.Line.ActualBoundingBoxRight < centered.Line.Width / 2);
    }

    [Fact]
    public void Create_IncludesAndRenumbersCompositeGlyphComponents()
    {
        var original = LoadInter();
        var usage = new FontUsage();
        usage.Add("·");

        var subset = new TrueTypeFont(TrueTypeFontSubset.Create(original, usage));

        Assert.True(subset.Maxp!.NumGlyphs > usage.Characters.Count + 1);
        Assert.NotNull(subset.GetGlyphIndex(R('·')));
    }

    [Fact]
    public void CMapSubset_RoundTripsNonBmpRunes()
    {
        var emoji = new Rune(0x1F600);
        var cmap = new CMapTable(CMapTable.CreateSubset([new CMapTable.GlyphMapping(emoji, 42)]));

        Assert.Equal(42, cmap.GetGlyphIndex(emoji));
    }

    [Fact]
    public void SvgDrawingContext_TracksUsageAndEmbedsTheCompactFont()
    {
        using var stream = new MemoryStream();
        using (var context = new SvgDrawingContext(
            new Size(300, 100), stream, Xui.Core.Fonts.Inter.Source,
            resolver: SvgDrawingContext.SvgFontResolver.Embedded, keepOpen: true))
        {
            ((ITextMeasureContext)context).SetFont(new CanvasFont(32, "Inter"));
            ((ITextDrawingContext)context).FillText("Hello World!", (10, 50));

            var usage = Assert.Single(context.FontUsage);
            Assert.Equal("Inter", usage.Key.Family);
            Assert.Contains(R('!'), usage.Value.Characters);
            Assert.Contains(new FontUsage.RunePair(R('W'), R('o')), usage.Value.Pairs);
        }

        var svg = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        var match = Regex.Match(svg, "data:font/ttf;base64,([^)]*)");
        Assert.True(match.Success);
        var subset = new TrueTypeFont(Convert.FromBase64String(match.Groups[1].Value));
        Assert.NotNull(subset.GetGlyphIndex(R('H')));
        Assert.NotNull(subset.GetGlyphIndex(R('!')));
    }

    [Fact]
    public void FontSource_ResolvesDeclaredFaceWithoutEagerLoading()
    {
        var request = new CanvasFont(16, "Inter", FontWeight.Normal);
        Assert.True(Xui.Core.Fonts.Inter.Source.TryGet(in request, out var resource));
        Assert.Equal("Inter", resource.Face.Family);

        var catalog = new Catalog(Xui.Core.Fonts.Inter.Source);
        Assert.NotNull(catalog.FontForFace(new FontFace("Inter", FontWeight.Normal, FontStyle.Normal, FontStretch.Normal)));
    }

    private static TrueTypeFont LoadInter()
    {
        var catalog = new Catalog(Xui.Core.Fonts.Inter.URIs);
        return catalog.FontForFace(new FontFace("Inter", FontWeight.Normal, FontStyle.Normal, FontStretch.Normal))!;
    }

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(300)]
    [InlineData(400)]
    [InlineData(500)]
    [InlineData(600)]
    [InlineData(700)]
    [InlineData(800)]
    [InlineData(900)]
    public void UriCatalog_ResolvesAndMeasuresEveryInterWeight(int weight)
    {
        var catalog = new Catalog(Xui.Core.Fonts.Inter.URIs);
        foreach (var style in new[] { FontStyle.Normal, FontStyle.Italic })
        {
            var face = new FontFace("Inter", new FontWeight(weight), style, FontStretch.Normal);
            var resolved = catalog.FontForFace(face);
            Assert.NotNull(resolved);
            Assert.True(FontFace.Comparer.Equals(face, resolved.Face));
            var metrics = catalog.MeasureText(new CanvasFont(20, "Inter", face.Weight, style), "Hello World!");
            Assert.True(metrics.Size.Width > 0);
            Assert.True(metrics.Size.Height > 0);
        }
    }

    private static Rune R(char value) => new(value);

}
