namespace Xui.DevKit.UI.Design;

/// <summary>
/// Predefined sizing profiles that control spacing tokens, minimum hit-test radius,
/// and overall density of the UI.
/// </summary>
public enum SizingPreset
{
    /// <summary>
    /// Compact desktop: small spacing, small hit targets (4pt), precise pointer assumed.
    /// Suitable for dense desktop applications with mouse input.
    /// </summary>
    Desktop,

    /// <summary>
    /// Touch-enabled desktop: same compact layout as Desktop but with larger hit targets (22pt).
    /// Good for laptop touchscreens and stylus-enabled desktops.
    /// </summary>
    TouchEnabled,

    /// <summary>
    /// Mobile: generous spacing, large hit targets (22pt), finger-friendly.
    /// Suitable for phones and tablets.
    /// </summary>
    Mobile,
}
