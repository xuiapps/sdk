using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Text;
using Xui.Core.Canvas;

namespace Xui.Runtime.Software.Font;

public partial class TrueTypeFont
{
    public readonly ReadOnlyMemory<byte> Blob;
    public readonly Uri? SourceUri;

    public IReadOnlyDictionary<string, TableRecord> Tables { get; }

    public HeaderTable? Head { get; private set; }
    public MaxProfileTable? Maxp { get; private set; }
    public CMapTable? Cmap { get; private set; }
    public PostTable? Post { get; private set; }
    public LocaTable? Loca { get; private set; }
    public GlyfTable? Glyf { get; private set; }
    public OS2Table? OS2 { get; private set; }
    public NameTable? Name { get; private set; }
    public HorizontalHeaderTable? Hhea { get; private set; }
    public HorizontalMetricsTable? Hmtx { get; private set; }
    public KernTable? Kern { get; private set; }
    public GPosTable? GPos { get; private set; }

    public FontFace Face { get; private set; }

    public KerningQuery Kerning => new(this);

    public TrueTypeFont(ReadOnlyMemory<byte> blob, Uri? SourceUri = null)
    {
        this.Blob = blob;
        this.SourceUri = SourceUri;

        Tables = ParseTableDirectory(blob.Span);

        // You can optionally parse critical tables here or defer
        if (Tables.TryGetValue("head", out var headRec))
            Head = new HeaderTable(GetTableSpan(headRec));

        if (Tables.TryGetValue("maxp", out var maxpRec))
            Maxp = new MaxProfileTable(GetTableSpan(maxpRec));

        if (Tables.TryGetValue("hhea", out var hheaRec))
            Hhea = new HorizontalHeaderTable(GetTableSpan(hheaRec));

        if (Tables.TryGetValue("OS/2", out var os2Rec))
            OS2 = new OS2Table(GetTableSpan(os2Rec));

        if (Tables.TryGetValue("post", out var postRec))
            Post = new PostTable(GetTableSpan(postRec));

        if (Tables.TryGetValue("cmap", out var cmapRec))
            Cmap = new CMapTable(GetTableSpan(cmapRec));

        if (Tables.TryGetValue("name", out var nameRec))
            Name = new NameTable(GetTableSpan(nameRec));

        if (Tables.TryGetValue("loca", out var locaRec) && Maxp is not null && Head is not null)
            Loca = new LocaTable(GetTableSpan(locaRec), Maxp.NumGlyphs, Head.IndexToLocFormat);

        if (Tables.TryGetValue("glyf", out var glyfRec) && Loca is not null)
            Glyf = new GlyfTable(GetTableMemory(glyfRec), Loca);

        if (Tables.TryGetValue("hmtx", out var hmtxRec) && Hhea is not null)
            Hmtx = new HorizontalMetricsTable(GetTableSpan(hmtxRec), Hhea.NumberOfHMetrics);

        if (Tables.TryGetValue("kern", out var kernRec) && Cmap is not null)
            Kern = new KernTable(GetTableSpan(kernRec), Cmap);

        if (Tables.TryGetValue("GPOS", out var gPosRec) && Cmap is not null)
            GPos = new GPosTable(GetTableSpan(gPosRec), Cmap);

        Face = FontFace(Head, Name, OS2);
    }

    public TextMetrics MeasureText(string text, Xui.Core.Canvas.Font font, TextAlign textAlign = TextAlign.Left, TextBaseline textBaseline = TextBaseline.Alphabetic)
    {
        return new TextMetrics(MeasureLine(text, font, textAlign, textBaseline), this.Metrics(font));
    }

    /// <summary>Measures metrics which depend on the text's glyphs and shaping.</summary>
    public LineMetrics MeasureLine(string text, Xui.Core.Canvas.Font font, TextAlign textAlign = TextAlign.Left, TextBaseline textBaseline = TextBaseline.Alphabetic)
        => new TextLayout(this, text, font.FontSize, textAlign, textBaseline).LineMetrics;

    public FontMetrics Metrics(Xui.Core.Canvas.Font font)
    {
        if (Head is null || Hhea is null)
            throw new InvalidOperationException("Missing required font tables (head or hhea).");

        nfloat unitsPerEm = Head.UnitsPerEm;
        nfloat scale = font.FontSize / unitsPerEm;

        nfloat emAscent = Hhea.Ascender * scale;
        nfloat emDescent = -Hhea.Descender * scale;

        // nfloat fontAscent = Head.YMax * scale;
        // nfloat fontDescent = -Head.YMin * scale;
        nfloat fontAscent = Hhea.Ascender * scale;
        nfloat fontDescent = -Hhea.Descender * scale;

        nfloat alphaBaseline = 0;
        nfloat hangingBaseline;
        nfloat ideographicBaseline;

        // OpenType's core metric tables do not define script baselines. Until BASE
        // support is added, use the CSS Canvas fallback: hanging is 0.8em above
        // alphabetic and ideographic is at the bottom of the em box.
        hangingBaseline = -font.FontSize * 0.8f;
        ideographicBaseline = emDescent;

        return new FontMetrics(
            fontAscent,
            fontDescent,
            emAscent,
            emDescent,
            alphaBaseline,
            hangingBaseline,
            ideographicBaseline
        );
    }

    private ReadOnlySpan<byte> GetTableSpan(TableRecord record)
    {
        return Blob.Span.Slice((int)record.Offset, (int)record.Length);
    }

    private ReadOnlyMemory<byte> GetTableMemory(TableRecord record)
    {
        return Blob.Slice((int)record.Offset, (int)record.Length);
    }

    public bool TryGetGlyph(int glyphId, out GlyphShape shape)
    {
        if (Glyf != null)
            return Glyf.TryGetGlyph(glyphId, out shape);

        shape = default;
        return false;
    }

    public int? GetGlyphIndex(Rune rune) => Cmap?.GetGlyphIndex(rune);

    public string? GetGlyphName(int glyphId) => Post?.GetGlyphName(glyphId);

    public readonly record struct TableRecord(string Tag, uint Offset, uint Length, uint Checksum);

    public readonly struct KerningQuery
    {
        private readonly TrueTypeFont _font;

        public KerningQuery(TrueTypeFont font) => _font = font;

        public ValueRecord this[Rune left, Rune right]
        {
            get
            {
                if (_font.GPos is { } gpos)
                    return gpos[left, right];

                if (_font.Kern is { } kern)
                    return kern[left, right];

                return default;
            }
        }
    }

    /// <summary>
    /// Extracts a <see cref="FontFace"/> from minimal parsed font tables.
    /// </summary>
    /// <param name="name">The <see cref="NameTable"/> containing font naming information.</param>
    /// <param name="os2">The <see cref="OS2Table"/> containing weight and width data.</param>
    /// <param name="head">The <see cref="HeaderTable"/> used to determine italic style (via macStyle flags).</param>
    /// <returns>A constructed <see cref="FontFace"/>.</returns>
    public static FontFace FontFace(HeaderTable? head, NameTable? name, OS2Table? os2)
    {
        // Legacy families group only regular/bold/italic faces; intermediate
        // weights may be named "Inter SemiBold" there. The typographic family
        // groups all weights under the family requested by canvas callers.
        string family = name?.PreferredFamily ?? name?.FamilyName ?? "Unknown";

        var weight = os2 != null
            ? new FontWeight(os2.WeightClass)
            : FontWeight.Normal;

        var stretch = os2 != null
            ? new FontStretch(os2.WidthClass switch
            {
                1 => FontStretch.UltraCondensed,
                2 => FontStretch.ExtraCondensed,
                3 => FontStretch.Condensed,
                4 => FontStretch.SemiCondensed,
                5 => FontStretch.Normal,
                6 => FontStretch.SemiExpanded,
                7 => FontStretch.Expanded,
                8 => FontStretch.ExtraExpanded,
                9 => FontStretch.UltraExpanded,
                _ => FontStretch.Normal
            })
            : FontStretch.Normal;

        var style = head is not null && (head.MacStyle & 0b10) != 0
            ? FontStyle.Italic
            : FontStyle.Normal;

        return new FontFace(family, weight, style, stretch);
    }

    public static FontFace FontFace(ReadOnlySpan<byte> fontData)
    {
        var tables = ParseTableDirectory(fontData);

        HeaderTable? head = null;
        NameTable? name = null;
        OS2Table? os2 = null;

        if (tables.TryGetValue("head", out var headRec))
        {
            var span = fontData.Slice((int)headRec.Offset, (int)headRec.Length);
            head = new HeaderTable(span);
        }

        if (tables.TryGetValue("name", out var nameRec))
        {
            var span = fontData.Slice((int)nameRec.Offset, (int)nameRec.Length);
            name = new NameTable(span);
        }

        if (tables.TryGetValue("OS/2", out var os2Rec))
        {
            var span = fontData.Slice((int)os2Rec.Offset, (int)os2Rec.Length);
            os2 = new OS2Table(span);
        }

        return FontFace(head, name, os2);
    }

    private static Dictionary<string, TableRecord> ParseTableDirectory(ReadOnlySpan<byte> span)
    {
        ushort tableCount = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4, 2));
        var tables = new Dictionary<string, TableRecord>(tableCount);

        for (int i = 0; i < tableCount; i++)
        {
            int offset = 12 + i * 16;
            string tag = Encoding.ASCII.GetString(span.Slice(offset, 4));
            uint checksum = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(offset + 4, 4));
            uint tableOffset = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(offset + 8, 4));
            uint length = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(offset + 12, 4));

            tables[tag] = new TableRecord(tag, tableOffset, length, checksum);
        }

        return tables;
    }
}
