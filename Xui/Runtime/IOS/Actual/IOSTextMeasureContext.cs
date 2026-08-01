using System;
using System.Diagnostics;
using NFloat = System.Runtime.InteropServices.NFloat;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using static Xui.Runtime.IOS.CoreFoundation;
using static Xui.Runtime.IOS.CoreText;
using static Xui.Runtime.IOS.Foundation;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Resolves and retains a bounded set of CoreText fonts for an iOS drawing context.</summary>
internal sealed class IOSTextMeasureContext : ITextMeasureContext, IDisposable
{
    private const int FontCacheCapacity = 64;

    private sealed class CoreTextFontCache : FontCache<nint>, IDisposable
    {
        public CoreTextFontCache() : base(FontCacheCapacity) { }

        protected override void OnEvicted(nint ctFont)
        {
            if (ctFont != 0)
                CFRelease(ctFont);
        }

        public void Dispose() => Clear();
    }

    internal nint nsFont;
    private readonly FontMetricsCache fontMetrics = new();
    private readonly CoreTextFontCache coreTextFonts = new();
    private readonly IOSFontRegistrar fontRegistrar;
    private Font currentFont;
    private bool hasCurrentFont;

    public TextAlign TextAlign { get; set; }

    public IOSTextMeasureContext(IFontSource? fontSource = null) => fontRegistrar = new IOSFontRegistrar(fontSource);

    public void Dispose()
    {
        nsFont = 0;
        coreTextFonts.Dispose();
    }

    public TextMetrics MeasureText(string text)
    {
        using var nsStringRef = new CFStringRef(text);
        return MeasureTextCore(nsStringRef);
    }

    public TextMetrics MeasureText(ReadOnlySpan<char> text)
    {
        using var nsStringRef = new CFStringRef(text);
        return MeasureTextCore(nsStringRef);
    }

    private TextMetrics MeasureTextCore(CFStringRef text)
    {
        using var attributes = new CFMutableDictionaryRef();
        if (nsFont != 0)
            attributes.SetValue(NSAttributedString.Key.Font, nsFont);

        using var attributedText = new NSAttributedStringRef(text, attributes);
        using var line = CTLineRef.Create(attributedText);
        var glyphBounds = line.GetBounds(CTLineRef.BoundsOptions.UseGlyphPathBounds);
        var typographicWidth = line.GetWidth();
        var alignmentOffset = TextAlign switch
        {
            TextAlign.Center => typographicWidth * 0.5f,
            TextAlign.Right => typographicWidth,
            _ => 0f,
        };

        var lineMetrics = new LineMetrics(
            width: typographicWidth,
            left: alignmentOffset - glyphBounds.Origin.X,
            right: glyphBounds.Origin.X + glyphBounds.Size.Width - alignmentOffset,
            ascent: glyphBounds.Size.Height + glyphBounds.Origin.Y,
            descent: -glyphBounds.Origin.Y);

        FontMetrics metrics = default;
        if (nsFont != 0 && hasCurrentFont)
        {
            if (!fontMetrics.TryGet(currentFont, out metrics))
            {
                metrics = new CTFontRef(nsFont).FontMetrics;
                fontMetrics.Set(currentFont, metrics);
            }
        }

        return new TextMetrics(lineMetrics, metrics);
    }

    public void SetFont(Font font)
    {
        if (hasCurrentFont && currentFont == font)
            return;

        currentFont = font;
        hasCurrentFont = true;
        if (coreTextFonts.TryGet(font, out nsFont))
            return;

        fontRegistrar.EnsureRegistered(in font);

        nsFont = 0;
        try
        {
            using var attributes = new CFMutableDictionaryRef();
            if (!string.IsNullOrEmpty(font.FontFamily))
            {
                using var family = new CFStringRef(font.FontFamily);
                attributes.SetValue(CTFontDescriptor.FontAttributes.FamilyName, family);
            }

            using var size = new CFNumberRef(font.FontSize);
            attributes.SetValue(CTFontDescriptor.FontAttributes.Size, size);
            using var traits = new CFMutableDictionaryRef();
            if (font.FontWeight != 400 && font.FontWeight != 700)
            {
                using var weight = new CFNumberRef(MapWebWeightToCTWeight(font.FontWeight));
                traits.SetValue(CTFontDescriptor.FontTrait.Weight, weight);
            }

            if (font.FontStyle.IsItalic || font.FontWeight == 700)
            {
                var symbolic = (font.FontStyle.IsItalic ? (int)CTFontDescriptor.SymbolicTrait.Italic : 0)
                    | (font.FontWeight == 700 ? (int)CTFontDescriptor.SymbolicTrait.Bold : 0);
                using var symbolicRef = new CFNumberRef(symbolic);
                traits.SetValue(CTFontDescriptor.FontTrait.Symbolic, symbolicRef);
            }

            if (font.FontStyle.IsOblique)
            {
                using var slant = new CFNumberRef(NFloat.Clamp(font.FontStyle.ObliqueAngle / 90f, -1f, 1f));
                traits.SetValue(CTFontDescriptor.FontTrait.Slant, slant);
            }

            attributes.SetValue(CTFontDescriptor.FontAttributes.Traits, traits);
            using var descriptor = CTFontDescriptorRef.WithAttributes(attributes);
            nsFont = new CTFontRef(descriptor, 0).Self;
            coreTextFonts.Set(font, nsFont);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private static NFloat MapWebWeightToCTWeight(NFloat weight)
    {
        if (weight == 400) return 0;
        weight = NFloat.Clamp(weight, 1, 1000);
        if (weight < 100f) return NFloat.Lerp(-1, -0.8f, (weight - 1f) / 100f);
        if (weight <= 300f) return NFloat.Lerp(-0.8f, -0.4f, (weight - 100f) / 200f);
        if (weight <= 400f) return NFloat.Lerp(-0.4f, 0f, (weight - 300f) / 100f);
        if (weight <= 500f) return NFloat.Lerp(0f, 0.23f, (weight - 400f) / 100f);
        if (weight <= 600f) return NFloat.Lerp(0.23f, 0.3f, (weight - 500f) / 100f);
        if (weight <= 700f) return NFloat.Lerp(0.3f, 0.4f, (weight - 600f) / 100f);
        if (weight <= 900f) return NFloat.Lerp(0.4f, 0.56f, (weight - 700f) / 200f);
        return NFloat.Lerp(0.56f, 1f, (weight - 900f) / 100f);
    }
}
