namespace Xui.DevKit.UI.Design;

/// <summary>
/// A set of spacing values on a geometric scale (S through XXXL).
/// Used for both passive (layout/content) and active (interactive leaf) spacing.
/// </summary>
public readonly struct SpacingScale
{
    /// <summary>Small spacing.</summary>
    public nfloat S    { get; init; }
    /// <summary>Medium spacing.</summary>
    public nfloat M    { get; init; }
    /// <summary>Large spacing.</summary>
    public nfloat L    { get; init; }
    /// <summary>Extra-large spacing.</summary>
    public nfloat XL   { get; init; }
    /// <summary>2× extra-large spacing.</summary>
    public nfloat XXL  { get; init; }
    /// <summary>3× extra-large spacing.</summary>
    public nfloat XXXL { get; init; }

    /// <summary>
    /// Minimum hit-test dimension for small interactive elements (icon buttons, tree toggles, etc.).
    /// On Desktop this is small (e.g. 20pt), on touch it's finger-sized (e.g. 44pt).
    /// Use as MinimumWidth/MinimumHeight on compact clickable views.
    /// </summary>
    public nfloat MinHitTarget { get; init; }
}
