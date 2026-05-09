using Xui.Core.Canvas;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// An immutable snapshot of a single text style from the typography scale.
/// </summary>
public readonly struct TextStyle()
{
    /// <summary>Font family name.</summary>
    public string FontFamily { get; init; } = "Inter";

    /// <summary>Font size in points (pre-multiplied by accessibility scale).</summary>
    public nfloat FontSize { get; init; }

    /// <summary>Line height in points.</summary>
    public nfloat LineHeight { get; init; }

    /// <summary>Additional letter spacing in points.</summary>
    public nfloat LetterSpacing { get; init; }

    /// <summary>Font weight (100–900).</summary>
    public FontWeight FontWeight { get; init; }

    /// <summary>Font style (normal, italic, oblique).</summary>
    public FontStyle FontStyle { get; init; }
}
