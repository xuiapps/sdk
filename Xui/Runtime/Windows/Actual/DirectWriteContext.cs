using System;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// A lightweight <see cref="ITextMeasureContext"/> implementation backed by DirectWrite.
/// Unlike <see cref="Direct2DContext"/>, this does not require a RenderTarget —
/// only a <see cref="DWrite.Factory"/> — making it suitable for use outside render passes
/// (e.g., during pointer event hit-testing for text selection).
/// </summary>
public class DirectWriteContext : ITextMeasureContext
{
    private readonly DWrite.Factory dwriteFactory;

    private DWrite.TextFormat.Ptr textFormat;
    private Core.Canvas.FontMetrics currentFontMetrics;

    public DirectWriteContext(DWrite.Factory dwriteFactory)
    {
        this.dwriteFactory = dwriteFactory;
        this.dwriteFactory.AddRef();
    }

    void ITextMeasureContext.SetFont(Font font)
    {
        string fontFamilyName = font.FontFamily[0];

        DWrite.FontWeight fontWeight = (DWrite.FontWeight)(uint)font.FontWeight;
        DWrite.FontStyle fontStyle =
            font.FontStyle.IsItalic ? DWrite.FontStyle.Italic :
            (font.FontStyle.IsOblique ? DWrite.FontStyle.Oblique : DWrite.FontStyle.Normal);

        float fontSize = (float)font.FontSize;
        DWrite.FontStretch fontStretch = DWrite.FontStretch.Normal;

        this.textFormat.Dispose();
        this.textFormat = this.dwriteFactory.CreateTextFormatPtr(
            fontFamilyName, null, fontWeight, fontStyle, fontStretch, fontSize, "en-US");

        this.currentFontMetrics = TryComputeFontMetrics(
            fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize);
    }

    TextMetrics ITextMeasureContext.MeasureText(string text)
    {
        if (this.textFormat.IsNull)
        {
            return new TextMetrics(
                new LineMetrics(0, 0, 0, 0, 0),
                new Core.Canvas.FontMetrics(0, 0, 0, 0, 0, 0, 0));
        }

        using var layout = this.dwriteFactory.CreateTextLayoutRef(
            text,
            this.textFormat,
            float.PositiveInfinity,
            float.PositiveInfinity);

        var tm = layout.GetTextMetrics();
        return BuildMetrics(tm, layout.TryGetFirstLineMetrics(out var fl) ? fl.Baseline : 0f, layout.GetOverhangMetrics(), tm.Height);
    }

    TextMetrics ITextMeasureContext.MeasureText(ReadOnlySpan<char> text)
    {
        if (this.textFormat.IsNull)
        {
            return new TextMetrics(
                new LineMetrics(0, 0, 0, 0, 0),
                new Core.Canvas.FontMetrics(0, 0, 0, 0, 0, 0, 0));
        }

        using var layout = this.dwriteFactory.CreateTextLayoutRef(
            text,
            this.textFormat,
            float.PositiveInfinity,
            float.PositiveInfinity);

        var tm = layout.GetTextMetrics();
        return BuildMetrics(tm, layout.TryGetFirstLineMetrics(out var fl) ? fl.Baseline : 0f, layout.GetOverhangMetrics(), tm.Height);
    }

    private TextMetrics BuildMetrics(DWrite.TextMetrics tm, float baseline, DWrite.OverhangMetrics overhang, float height)
    {
        float advanceWidth = tm.WidthIncludingTrailingWhitespace;

        float left = tm.Left;
        float right = tm.Left + tm.Width;

        float inkTop = -overhang.Top;
        float inkBottom = height + overhang.Bottom;

        float visualAscent = baseline - inkTop;
        float visualDescent = inkBottom - baseline;

        if (visualAscent < 0f) visualAscent = 0f;
        if (visualDescent < 0f) visualDescent = 0f;

        var line = new LineMetrics(
            width: advanceWidth,
            left: left,
            right: right,
            ascent: visualAscent,
            descent: visualDescent);

        return new TextMetrics(line, this.currentFontMetrics);
    }

    private Core.Canvas.FontMetrics TryComputeFontMetrics(
        string familyName,
        DWrite.FontWeight weight,
        DWrite.FontStyle style,
        DWrite.FontStretch stretch,
        float fontSizeDip)
    {
        Core.Canvas.FontMetrics fallback = new Core.Canvas.FontMetrics(
            fontAscent: fontSizeDip * 0.8f,
            fontDescent: fontSizeDip * 0.2f,
            emAscent: fontSizeDip * 0.8f,
            emDescent: fontSizeDip * 0.2f,
            alphabeticBaseline: 0f,
            hangingBaseline: -(fontSizeDip * 0.8f),
            ideographicBaseline: fontSizeDip * 0.2f);

        try
        {
            using var collection = this.dwriteFactory.GetSystemFontCollectionRef();

            collection.FindFamilyName(familyName, out uint familyIndex, out bool exists);
            if (!exists)
                return fallback;

            using var family = collection.GetFontFamily(familyIndex);
            using var dwFont = family.GetFirstMatchingFont(weight, stretch, style);

            using var face0 = dwFont.CreateFontFace();
            using var face1 = DWrite.FontFace1.Ref.FromFontFace(face0);

            face1.GetMetrics(out DWrite.FontMetrics1 m);

            float unitsPerEm = Math.Max(1f, m.DesignUnitsPerEm);
            float scale = fontSizeDip / unitsPerEm;

            float emAscent = m.Ascent * scale;
            float emDescent = m.Descent * scale;

            float fontAscent = emAscent;
            float fontDescent = emDescent;

            return new Core.Canvas.FontMetrics(
                fontAscent: fontAscent,
                fontDescent: fontDescent,
                emAscent: emAscent,
                emDescent: emDescent,
                alphabeticBaseline: 0f,
                hangingBaseline: -emAscent,
                ideographicBaseline: emDescent);
        }
        catch
        {
            return fallback;
        }
    }
}
