namespace Xui.DevKit.UI.Design;

/// <summary>
/// Color-scheme relationship used to derive secondary/tertiary hues from a primary hue.
/// </summary>
public enum ColorHarmony
{
    /// <summary>Secondary = H + 180°.</summary>
    Complementary,

    /// <summary>Secondary = H + 30°, Tertiary = H + 60°.</summary>
    Analogous,

    /// <summary>Secondary = H + 150°, Tertiary = H + 210°.</summary>
    SplitComplementary,

    /// <summary>Secondary = H + 120°, Tertiary = H + 240°.</summary>
    Triadic,

    /// <summary>Secondary = H + 90°, Tertiary = H + 180°, Quaternary = H + 270°.</summary>
    Tetradic,
}
