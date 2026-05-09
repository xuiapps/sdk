namespace Xui.DevKit.UI.Design;

/// <summary>
/// Provides the typography scale.
/// All FontSize values are pre-multiplied by the accessibility font scale.
/// </summary>
public interface ITypographySystem
{
    /// <summary>57 pt — Hero, marketing, splash.</summary>
    TextStyle Display { get; }

    /// <summary>24–32 pt — Page and section titles.</summary>
    TextStyleGroup Headline { get; }

    /// <summary>14–22 pt — Card headers, toolbars, tabs.</summary>
    TextStyleGroup Title { get; }

    /// <summary>12–16 pt — Reading and UI text.</summary>
    TextStyleGroup Body { get; }

    /// <summary>11–14 pt — Buttons, badges, captions.</summary>
    TextStyleGroup Label { get; }

    /// <summary>The default font family used across the application.</summary>
    string DefaultFontFamily { get; }
}
