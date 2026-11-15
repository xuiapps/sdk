using Xui.Core.DI;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// Concrete implementation of <see cref="IDesignSystem"/> that derives a complete set of
/// design tokens from a small number of seed properties and the current <see cref="IDeviceInfo"/>.
/// </summary>
public class XuiDesignSystem : IDesignSystem
{
    private XuiColorSystem colors = null!;
    private XuiTypographySystem typography = null!;
    private XuiSpacingSystem spacing = null!;
    private XuiShapeSystem shape = null!;
    private XuiMotionSystem motion = null!;
    private XuiDesignSystemOptions previousOptions;
    private ColorScheme previousColorScheme;
    private nfloat previousFontScale;
    private bool previousReducedMotion, previousHighContrast;

    /// <summary>
    /// Creates a new design system from the given options and device info.
    /// </summary>
    public XuiDesignSystem(XuiDesignSystemOptions options, IDeviceInfo device)
    {
        UpdateTokens(options, device, DesignSystemChange.All);
    }

    /// <summary>Updates the retained palette in place and rebuilds only other changed token groups.</summary>
    public void Update(XuiDesignSystemOptions options, IDeviceInfo device)
    {
        var changes = GetChanges(options, device);
        if (changes == DesignSystemChange.None) return;
        UpdateTokens(options, device, changes);
        Changed?.Invoke(changes);
    }

    private DesignSystemChange GetChanges(XuiDesignSystemOptions options, IDeviceInfo device)
    {
        var old = previousOptions;
        var changes = DesignSystemChange.None;
        if (old.PrimaryHue != options.PrimaryHue || old.SecondaryHue != options.SecondaryHue ||
            old.TertiaryHue != options.TertiaryHue || old.NeutralHue != options.NeutralHue ||
            old.PrimaryLightness != options.PrimaryLightness || old.Harmony != options.Harmony || old.Chroma != options.Chroma || old.NeutralStyle != options.NeutralStyle ||
            previousColorScheme != device.ColorScheme || previousHighContrast != device.PrefersHighContrast)
            changes |= DesignSystemChange.Colors;
        if (old.DefaultFontFamily != options.DefaultFontFamily || previousFontScale != device.AccessibilityFontScale)
            changes |= DesignSystemChange.Typography;
        if (old.Sizing != options.Sizing) changes |= DesignSystemChange.Spacing;
        if (old.Shape != options.Shape || old.RoundnessFactor != options.RoundnessFactor) changes |= DesignSystemChange.Shape;
        if (old.Motion != options.Motion || old.MotionPreference != options.MotionPreference || previousReducedMotion != device.PrefersReducedMotion)
            changes |= DesignSystemChange.Motion;
        return changes;
    }

    private void UpdateTokens(XuiDesignSystemOptions options, IDeviceInfo device, DesignSystemChange changes)
    {
        if ((changes & DesignSystemChange.Colors) != 0)
        {
            if (colors is null) colors = new XuiColorSystem(options, device);
            else colors.Update(options, device);
        }
        if ((changes & DesignSystemChange.Typography) != 0) typography = new XuiTypographySystem(options, device);
        if ((changes & DesignSystemChange.Spacing) != 0) spacing = new XuiSpacingSystem(options.Sizing);
        if ((changes & DesignSystemChange.Shape) != 0) shape = new XuiShapeSystem(options);
        if ((changes & DesignSystemChange.Motion) != 0) motion = new XuiMotionSystem(options, device);
        previousOptions = options;
        // Device services are mutable: retain values, not the device object, for comparison.
        previousColorScheme = device.ColorScheme;
        previousFontScale = device.AccessibilityFontScale;
        previousReducedMotion = device.PrefersReducedMotion;
        previousHighContrast = device.PrefersHighContrast;
    }

    /// <inheritdoc/>
    public IColorSystem Colors => colors;

    /// <inheritdoc/>
    public ITypographySystem Typography => typography;

    /// <inheritdoc/>
    public ISpacingSystem Spacing => spacing;

    /// <inheritdoc/>
    public IShapeSystem Shape => shape;

    /// <inheritdoc/>
    public IMotionSystem Motion => motion;

    /// <inheritdoc/>
    public event Action<DesignSystemChange>? Changed;

    /// <summary>
    /// Raises the <see cref="Changed"/> event, causing widgets to re-query tokens.
    /// </summary>
    public void NotifyChanged(DesignSystemChange changes = DesignSystemChange.All)
    {
        if (changes != DesignSystemChange.None) Changed?.Invoke(changes);
    }
}

/// <summary>
/// Value-type options for constructing or updating a <see cref="XuiDesignSystem"/> without allocating a configuration object.
/// </summary>
public readonly struct XuiDesignSystemOptions
{
    /// <summary>Initializes the default design-system settings.</summary>
    public XuiDesignSystemOptions() { }

    /// <summary>Primary brand hue in degrees (0–360). Required.</summary>
    public nfloat PrimaryHue { get; init; }

    /// <summary>Optional secondary hue override. If null, derived from <see cref="Harmony"/>.</summary>
    public nfloat? SecondaryHue { get; init; }

    /// <summary>Optional tertiary/accent hue override. If null, derived from <see cref="Harmony"/>.</summary>
    public nfloat? TertiaryHue { get; init; }

    /// <summary>Optional neutral hue override. If null, derived as primary hue desaturated.</summary>
    public nfloat? NeutralHue { get; init; }

    /// <summary>Color harmony used to derive secondary/tertiary hues. Default: SplitComplementary.</summary>
    public ColorHarmony Harmony { get; init; } = ColorHarmony.SplitComplementary;

    /// <summary>Target chroma for the primary palette (typical range 0.10–0.35). Default: 0.15.</summary>
    public nfloat Chroma { get; init; } = (nfloat)0.15;

    /// <summary>Optional primary action lightness (0–1). Null uses the appearance's default action tone.</summary>
    public nfloat? PrimaryLightness { get; init; }

    /// <summary>Global corner-radius multiplier (0.0 = square, 1.0 = default, 2.0 = very round). Default: 1.0.</summary>
    public nfloat RoundnessFactor { get; init; } = 1;

    /// <summary>Shape preset controlling corner radius tokens. Default: Soft.</summary>
    public ShapePreset Shape { get; init; } = ShapePreset.Soft;

    /// <summary>Sizing preset controlling spacing and hit-test targets. Default: Mobile.</summary>
    public SizingPreset Sizing { get; init; } = SizingPreset.Mobile;

    /// <summary>Controls the hue relationship between Application and Surface backgrounds. Default: Monochrome.</summary>
    public NeutralStyle NeutralStyle { get; init; } = NeutralStyle.Monochrome;

    /// <summary>Preferred animation style. Default: Curve.</summary>
    public MotionPreference MotionPreference { get; init; } = MotionPreference.Curve;

    /// <summary>Animation timing preset. Default: Normal.</summary>
    public MotionPreset Motion { get; init; } = MotionPreset.Normal;

    /// <summary>Default font family for all text styles. Default: system-ui.</summary>
    public string DefaultFontFamily { get; init; } = "Inter";
}
