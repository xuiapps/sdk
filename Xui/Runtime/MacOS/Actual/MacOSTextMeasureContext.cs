using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using static Xui.Runtime.MacOS.CoreFoundation;
using static Xui.Runtime.MacOS.CoreText;
using static Xui.Runtime.MacOS.Foundation;

namespace Xui.Runtime.MacOS.Actual;

internal sealed class MacOSTextMeasureContext : ITextMeasureContext
{
    internal nint nsFont;
    private readonly FontMetricsCache fontMetrics = new();
    private readonly CoreTextFontCache coreTextFonts = new();
    private readonly MacOSFontRegistrar fontRegistrar;
    private Font currentFont;
    private bool hasCurrentFont;

    public TextAlign TextAlign { get; set; }

    public MacOSTextMeasureContext(IFontSource? fontSource = null) => fontRegistrar = new MacOSFontRegistrar(fontSource);

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

    private TextMetrics MeasureTextCore(CFStringRef nsStringRef)
    {
        using var attributes = new CFMutableDictionaryRef();

        if (nsFont != 0)
            attributes.SetValue(NSAttributedString.Key.Font, nsFont);

        using var attrString = new NSAttributedStringRef(nsStringRef, attributes);
        using var line = CTLineRef.Create(attrString);

        Rect glyphBounds = line.GetBounds(CTLineRef.BoundsOptions.UseGlyphPathBounds);
        NFloat typographicWidth = line.GetWidth();

        NFloat alignOffset = this.TextAlign switch
        {
            TextAlign.Center => typographicWidth * 0.5f,
            TextAlign.Right  => typographicWidth,
            _ => 0f
        };

        var lineMetrics = new LineMetrics(
            width: typographicWidth,
            // CTLine bounds are relative to the left-aligned line origin. Xui's
            // Canvas-style left/right values are distances from the aligned
            // drawing origin to the actual glyph-path bounds.
            left: alignOffset - glyphBounds.X,
            right: glyphBounds.X + glyphBounds.Width - alignOffset,
            ascent: glyphBounds.Height + glyphBounds.Y,
            descent: -glyphBounds.Y
        );

        FontMetrics font;
        if (nsFont != 0 && hasCurrentFont)
        {
            if (!fontMetrics.TryGet(currentFont, out font))
            {
                var ct = new CTFontRef(nsFont);
                font = ct.FontMetrics;
                fontMetrics.Set(currentFont, font);
            }
        }
        else
        {
            font = default;
        }

        return new TextMetrics(lineMetrics, font);
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
                using var fontFamilyNameRef = new CFStringRef(font.FontFamily);
                attributes.SetValue(CTFontDescriptor.FontAttributes.FamilyName, fontFamilyNameRef);
            }

            using var fontSizeRef = new CFNumberRef(font.FontSize);
            attributes.SetValue(CTFontDescriptor.FontAttributes.Size, fontSizeRef);

            using var fontTraits = new CFMutableDictionaryRef();

            // 400 is regular, 700 is bold set through symbolic traits
            if (font.FontWeight != 400 && font.FontWeight != 700)
            {
                NFloat webWeightToCTWeight = MapWebWeightToCTWeight(font.FontWeight);
                using var fontWeight = new CFNumberRef(webWeightToCTWeight);
                fontTraits.SetValue(CTFontDescriptor.FontTrait.Weight, fontWeight);
            }

            if (font.FontStyle.IsItalic || font.FontWeight == 700)
            {
                int symTrait = 0;
                if (font.FontStyle.IsItalic)
                    symTrait |= (int)CTFontDescriptor.SymbolicTrait.Italic;
                if (font.FontWeight == 700)
                    symTrait |= (int)CTFontDescriptor.SymbolicTrait.Bold;

                using var symTraitRef = new CFNumberRef(symTrait);
                fontTraits.SetValue(CTFontDescriptor.FontTrait.Symbolic, symTraitRef);
            }

            if (font.FontStyle.IsOblique)
            {
                var slant = new CFNumberRef(NFloat.Clamp(font.FontStyle.ObliqueAngle / 90f, -1f, 1f));
                fontTraits.SetValue(CTFontDescriptor.FontTrait.Slant, slant);
            }

            attributes.SetValue(CTFontDescriptor.FontAttributes.Traits, fontTraits);

            using var descriptor = CTFontDescriptorRef.WithAttributes(attributes);
            nint options = 0;

            // Ownership transfers to the cache. It releases this handle on eviction or disposal.
            var ctFont = new CTFontRef(descriptor, options);
            nsFont = ctFont.Self;
            coreTextFonts.Set(font, nsFont);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    /// <summary>
    /// <code>
    /// NSFontWeightUltraLight: -0.800000 - 100 Thin (Hairline)
    /// NSFontWeightThin: -0.600000       - 200 Extra Light (Ultra Light)
    /// NSFontWeightLight: -0.400000      - 300 Light
    /// NSFontWeightRegular: 0.000000     - 400 Normal (Regular)
    /// NSFontWeightMedium: 0.230000      - 500 Medium
    /// NSFontWeightSemibold: 0.300000    - 600 Semi Bold (Demi Bold)
    /// NSFontWeightBold: 0.400000        - 700 Bold
    ///                                   - 800 Extra Bold (Ultra Bold)
    /// NSFontWeightHeavy: 0.560000       - 900 Black (Heavy)
    ///                                   - 950 Extra Black (Ultra Black)
    /// </code>
    /// </summary>
    private static NFloat MapWebWeightToCTWeight(NFloat webWeight)
    {
        if (webWeight == 400) return 0;
        webWeight = NFloat.Clamp(webWeight, 1, 1000);
        if (webWeight < 100f) return NFloat.Lerp(-1, -0.8f, (webWeight - 1f) / 100f);
        if (webWeight <= 300f) return NFloat.Lerp(-0.8f, -0.4f, (webWeight - 100f) / 200f);
        if (webWeight <= 400f) return NFloat.Lerp(-0.4f, 0f, (webWeight - 300f) / 100f);
        if (webWeight <= 500f) return NFloat.Lerp(0f, 0.23f, (webWeight - 400f) / 100f);
        if (webWeight <= 600f) return NFloat.Lerp(0.23f, 0.3f, (webWeight - 500f) / 100f);
        if (webWeight <= 700f) return NFloat.Lerp(0.3f, 0.4f, (webWeight - 600f) / 100f);
        if (webWeight <= 900f) return NFloat.Lerp(0.4f, 0.56f, (webWeight - 700f) / 200f);
        return NFloat.Lerp(0.56f, 1f, (webWeight - 900f) / 100f);
    }
}
