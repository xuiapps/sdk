namespace Xui.DevKit.UI.Design;

/// <summary>
/// Predefined shape profiles that control corner radius tokens across the design system.
/// </summary>
public enum ShapePreset
{
    /// <summary>All corners square (0 radius everywhere).</summary>
    Square,

    /// <summary>
    /// Classic desktop style: slight container radius (~7pt), square buttons and inputs.
    /// Typical of macOS and Windows window chrome.
    /// </summary>
    Desktop,

    /// <summary>
    /// Modern dialog style: large outer radius, smaller inner radius, small button radius (3–5pt).
    /// Similar to macOS alerts and Android Material dialogs.
    /// </summary>
    Rounded,

    /// <summary>
    /// Same as Rounded but with fully-round (pill) buttons.
    /// </summary>
    RoundedPill,

    /// <summary>
    /// Maximum roundness: large container radius, pill buttons. High visual softness.
    /// </summary>
    Soft,
}
