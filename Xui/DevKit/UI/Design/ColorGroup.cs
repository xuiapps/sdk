using Xui.Core.Canvas;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// A group of four semantically related colors derived from a single tonal palette,
/// together with the underlying OKLCH ramp that generated them.
/// </summary>
public readonly struct ColorGroup
{
    /// <summary>
    /// Strong, saturated action color (ramp at tone 0.40 light / 0.80 dark).
    /// Use as fill for filled buttons, active indicators, primary UI elements.
    /// </summary>
    public Color Background { get; init; }

    /// <summary>
    /// High-contrast text/icon color on top of Background (ramp at tone 1.00 light / 0.20 dark).
    /// </summary>
    public Color Foreground { get; init; }

    /// <summary>
    /// Light tinted fill from the same palette (ramp at tone 0.90 light / 0.30 dark).
    /// Use for tonal buttons, chips, selected segment items, highlighted list rows.
    /// </summary>
    public Color Container { get; init; }

    /// <summary>
    /// Text/icon color on top of Container (ramp at tone 0.10 light / 0.90 dark).
    /// </summary>
    public Color OnContainer { get; init; }

    /// <summary>
    /// The full tonal ramp for this palette entry (Lightness 0 to 1 at the group's hue).
    /// Use for computing hover/press state colors.
    /// </summary>
    public OklchRamp Ramp { get; init; }

    /// <summary>
    /// The lightness value (0–1) used to produce <see cref="Background"/> from the ramp.
    /// Enables relative hover/press offset computation.
    /// </summary>
    public nfloat BackgroundLightness { get; init; }

    /// <summary>
    /// Creates a <see cref="ColorGroup"/> from a tonal ramp and color scheme.
    /// </summary>
    public static ColorGroup FromRamp(OklchRamp ramp, bool isDark)
    {
        if (isDark)
        {
            return new ColorGroup
            {
                Background  = ramp[0.80f],
                Foreground  = ramp[0.20f],
                Container   = ramp[0.30f],
                OnContainer = ramp[0.90f],
                Ramp        = ramp,
                BackgroundLightness = 0.80f,
            };
        }

        return new ColorGroup
        {
            Background  = ramp[0.40f],
            Foreground  = ramp[1.00f],
            Container   = ramp[0.90f],
            OnContainer = ramp[0.10f],
            Ramp        = ramp,
            BackgroundLightness = 0.40f,
        };
    }
}
