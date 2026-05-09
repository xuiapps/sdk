using Xui.Core.Canvas;
using Xui.Core.DI;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// Concrete typography system with a 13-level named scale grouped into S/M/L.
/// All font sizes are pre-multiplied by the accessibility font scale.
/// </summary>
internal class XuiTypographySystem : ITypographySystem
{
    public XuiTypographySystem(XuiDesignSystemOptions options, IDeviceInfo device)
    {
        DefaultFontFamily = options.DefaultFontFamily;
        var scale = device.AccessibilityFontScale;
        var family = DefaultFontFamily;

        Display = MakeStyle(family, 57, FontWeight.Normal, scale);

        Headline = new TextStyleGroup
        {
            S = MakeStyle(family, 24, FontWeight.Normal, scale),
            M = MakeStyle(family, 28, FontWeight.Normal, scale),
            L = MakeStyle(family, 32, FontWeight.Normal, scale),
        };

        Title = new TextStyleGroup
        {
            S = MakeStyle(family, 14, FontWeight.Medium, scale),
            M = MakeStyle(family, 16, FontWeight.Medium, scale),
            L = MakeStyle(family, 22, FontWeight.Normal, scale),
        };

        Body = new TextStyleGroup
        {
            S = MakeStyle(family, 12, FontWeight.Normal, scale),
            M = MakeStyle(family, 14, FontWeight.Normal, scale),
            L = MakeStyle(family, 16, FontWeight.Normal, scale),
        };

        Label = new TextStyleGroup
        {
            S = MakeStyle(family, 11, FontWeight.Medium, scale),
            M = MakeStyle(family, 12, FontWeight.Medium, scale),
            L = MakeStyle(family, 14, FontWeight.Medium, scale),
        };
    }

    /// <inheritdoc/>
    public TextStyle Display { get; }

    /// <inheritdoc/>
    public TextStyleGroup Headline { get; }

    /// <inheritdoc/>
    public TextStyleGroup Title { get; }

    /// <inheritdoc/>
    public TextStyleGroup Body { get; }

    /// <inheritdoc/>
    public TextStyleGroup Label { get; }

    /// <inheritdoc/>
    public string DefaultFontFamily { get; }

    private static TextStyle MakeStyle(string family, nfloat baseSize, FontWeight weight, nfloat scale)
    {
        var size = baseSize * scale;
        return new TextStyle
        {
            FontFamily    = family,
            FontSize      = size,
            LineHeight    = size * (nfloat)1.4,
            LetterSpacing = 0,
            FontWeight    = weight,
            FontStyle     = FontStyle.Normal,
        };
    }
}
