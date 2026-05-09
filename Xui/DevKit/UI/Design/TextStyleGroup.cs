namespace Xui.DevKit.UI.Design;

/// <summary>
/// A group of three related text styles at small, medium, and large sizes.
/// </summary>
public readonly struct TextStyleGroup
{
    /// <summary>Small variant.</summary>
    public TextStyle S { get; init; }

    /// <summary>Medium variant.</summary>
    public TextStyle M { get; init; }

    /// <summary>Large variant.</summary>
    public TextStyle L { get; init; }
}
