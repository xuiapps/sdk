namespace Xui.Core.Canvas;

/// <summary>
/// Immutable description of one font face used for canvas text measurement and rendering.
/// </summary>
/// <remarks>
/// Xui currently selects one named family. Font fallback and per-run font selection are not
/// supported by this value and must bypass font-metric caches when introduced in the future.
/// </remarks>
public readonly partial struct Font : IEquatable<Font>
{
    /// <summary>Initializes the default Inter text font.</summary>
    public Font() : this(15, "Inter") { }

    /// <summary>Initializes a font face.</summary>
    public Font(
        nfloat fontSize,
        string fontFamily,
        FontWeight? fontWeight = null,
        FontStyle? fontStyle = null,
        FontStretch? fontStretch = null,
        nfloat? lineHeight = null)
    {
        FontFamily = fontFamily;
        FontSize = fontSize;
#if XUI_COMPILER_CANVAS
        // Keep the narrow Canvas profile's immutable defaults inline.
        FontWeight = fontWeight ?? new FontWeight(400);
#else
        FontWeight = fontWeight ?? FontWeight.Normal;
#endif
        FontStyle = fontStyle ?? FontStyle.Normal;
#if XUI_COMPILER_CANVAS
        FontStretch = fontStretch ?? new FontStretch(100);
#else
        FontStretch = fontStretch ?? FontStretch.Normal;
#endif
        LineHeight = lineHeight ?? fontSize * 1.2f;
    }

    /// <summary>The one selected font family name.</summary>
    public string FontFamily { get; init; }

    /// <summary>The font size in user-space units.</summary>
    public nfloat FontSize { get; init; }

    /// <summary>The font style.</summary>
    public FontStyle FontStyle { get; init; }

    /// <summary>The numeric font weight.</summary>
    public FontWeight FontWeight { get; init; }

    /// <summary>The font stretch.</summary>
    public FontStretch FontStretch { get; init; }

    /// <summary>The requested line height. Layout will own line-spacing semantics.</summary>
    public nfloat LineHeight { get; init; }

    /// <inheritdoc/>
    public bool Equals(Font other) =>
        string.Equals(FontFamily, other.FontFamily) &&
        FontSize.Equals(other.FontSize) &&
        FontWeight == other.FontWeight &&
        FontStyle.IsItalic == other.FontStyle.IsItalic &&
        FontStyle.IsOblique == other.FontStyle.IsOblique &&
        FontStyle.ObliqueAngle.Equals(other.FontStyle.ObliqueAngle) &&
        FontStretch == other.FontStretch &&
        LineHeight.Equals(other.LineHeight);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Font other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        (FontFamily ?? string.Empty).GetHashCode(),
        FontSize,
        FontWeight,
        FontStyle.IsItalic,
        FontStyle.IsOblique,
        FontStyle.ObliqueAngle,
        FontStretch,
        LineHeight);

    /// <summary>Returns whether two font descriptions are equal.</summary>
    public static bool operator ==(Font left, Font right) => left.Equals(right);

    /// <summary>Returns whether two font descriptions differ.</summary>
    public static bool operator !=(Font left, Font right) => !left.Equals(right);
}
