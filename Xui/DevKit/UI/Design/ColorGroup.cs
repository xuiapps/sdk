using Xui.Core.Canvas;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// A group of four semantically related colors derived from a single tonal palette,
/// together with the underlying OKLCH ramp that generated them. Shared by all consumers;
/// interaction colors are derived once on demand instead of copied into individual views.
/// </summary>
public sealed class ColorGroup
{
    private bool isDark;
    private Color background;
    private Color foreground;
    private Color container;
    private Color onContainer;
    private OklchRamp ramp;
    private nfloat backgroundLightness;
    private bool initialized;
    private nfloat foregroundLightness, containerLightness, onContainerLightness;
    private Color? hoverBackground, pressedBackground, pressedContainer, toggleHoverBackground, selectedPressedBackground;

    /// <summary>Whether this group belongs to a dark appearance palette.</summary>
    public bool IsDark { get => isDark; init => isDark = value; }

    /// <summary>Background for a hovered filled control.</summary>
    public Color HoverBackground => hoverBackground ??= Ramp[nfloat.Clamp(BackgroundLightness + (IsDark ? 0.08f : -0.06f), 0, 1)];

    /// <summary>Background for a pressed filled control.</summary>
    public Color PressedBackground => pressedBackground ??= Ramp[nfloat.Clamp(BackgroundLightness + (IsDark ? 0.15f : -0.12f), 0, 1)];

    /// <summary>Pressed state layer for an outline or text control.</summary>
    public Color PressedContainer => pressedContainer ??= Ramp[nfloat.Clamp(BackgroundLightness + (IsDark ? -0.06f : 0.06f), 0, 1)];

    /// <summary>Hovered active toggle track.</summary>
    public Color ToggleHoverBackground => toggleHoverBackground ??= Ramp[IsDark ? 0.85f : 0.48f];

    /// <summary>Pressed background within a segmented selection.</summary>
    public Color SelectedPressedBackground => selectedPressedBackground ??= Ramp[IsDark ? 0.40f : 0.85f];

    /// <summary>
    /// Strong, saturated action color (ramp at tone 0.56 light / 0.75 dark).
    /// Use as fill for filled buttons, active indicators, primary UI elements.
    /// </summary>
    public Color Background { get => background; init => background = value; }

    /// <summary>
    /// Black or white text/icon color on Background, selected by its actual luminance.
    /// </summary>
    public Color Foreground { get => foreground; init => foreground = value; }

    /// <summary>
    /// Light tinted fill from the same palette (ramp at tone 0.90 light / 0.30 dark).
    /// Use for tonal buttons, chips, and highlighted list rows.
    /// </summary>
    public Color Container { get => container; init => container = value; }

    /// <summary>
    /// Text/icon color on top of Container (ramp at tone 0.10 light / 0.90 dark).
    /// </summary>
    public Color OnContainer { get => onContainer; init => onContainer = value; }

    /// <summary>
    /// The full tonal ramp for this palette entry (Lightness 0 to 1 at the group's hue).
    /// Use for computing hover/press state colors.
    /// </summary>
    public OklchRamp Ramp { get => ramp; init => ramp = value; }

    /// <summary>
    /// The lightness value (0–1) used to produce <see cref="Background"/> from the ramp.
    /// Enables relative hover/press offset computation.
    /// </summary>
    public nfloat BackgroundLightness { get => backgroundLightness; init => backgroundLightness = value; }

    /// <summary>
    /// Creates a <see cref="ColorGroup"/> from a tonal ramp and color scheme.
    /// </summary>
    public static ColorGroup FromRamp(OklchRamp ramp, bool isDark)
    {
        var group = new ColorGroup();
        group.Update(ramp, isDark);
        return group;
    }

    internal void Update(OklchRamp ramp, bool isDark)
    {
        var tone = isDark ? 0.75f : 0.56f;
        Update(ramp, isDark, tone, ContrastingTone(ramp[tone]),
            isDark ? 0.30f : 0.90f, isDark ? 0.90f : 0.10f);
    }

    internal static nfloat ContrastingTone(Color color)
    {
        static nfloat Linear(nfloat value) => value <= 0.04045f ? value / 12.92f : nfloat.Pow((value + 0.055f) / 1.055f, 2.4f);
        var luminance = 0.2126f * Linear(color.Red) + 0.7152f * Linear(color.Green) + 0.0722f * Linear(color.Blue);
        return luminance > 0.179f ? 0 : 1;
    }

    internal void Update(OklchRamp nextRamp, bool dark, nfloat bg, nfloat fg, nfloat container, nfloat onContainer)
    {
        var unchanged = initialized && isDark == dark && backgroundLightness == bg &&
            foregroundLightness == fg && containerLightness == container && onContainerLightness == onContainer &&
            ramp.TargetChroma == nextRamp.TargetChroma && (ramp.Hue == nextRamp.Hue || nextRamp.TargetChroma == 0);
        ramp = nextRamp;
        if (unchanged) return;
        initialized = true;
        isDark = dark;
        backgroundLightness = bg;
        foregroundLightness = fg;
        containerLightness = container;
        onContainerLightness = onContainer;
        background = ramp[bg];
        foreground = ramp[fg];
        this.container = ramp[container];
        this.onContainer = ramp[onContainer];
        hoverBackground = pressedBackground = pressedContainer = toggleHoverBackground = selectedPressedBackground = null;
    }
}
