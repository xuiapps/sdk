using Xui.Core.Canvas;
using Xui.Core.DI;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// Concrete color system that derives all color roles from seed hues using OKLCH tonal ramps.
/// </summary>
internal class XuiColorSystem : IColorSystem
{
    private static readonly nfloat WarningHue = 85;         // Yellow-orange
    private static readonly nfloat WarningChroma = (nfloat)0.18;
    private static readonly nfloat ErrorHue = 29;            // Red-orange
    private static readonly nfloat ErrorChroma = (nfloat)0.20;
    private static readonly nfloat NeutralChroma = (nfloat)0.015;

    public XuiColorSystem(XuiDesignSystemOptions options, IDeviceInfo device)
        => Update(options, device);

    internal void Update(XuiDesignSystemOptions options, IDeviceInfo device)
    {
        IsDark = device.ColorScheme == ColorScheme.Dark;

        var primaryHue = options.PrimaryHue;
        var chroma = options.Chroma;

        // Derive secondary and tertiary hues from the harmony if not explicitly set.
        var (secondaryHue, tertiaryHue) = DeriveHues(primaryHue, options.Harmony,
            options.SecondaryHue, options.TertiaryHue);

        var neutralHue = options.NeutralHue ?? primaryHue;

        // Build tonal ramps.
        var primaryRamp   = new OklchRamp(primaryHue, chroma);
        var secondaryRamp = new OklchRamp(secondaryHue, chroma);
        var accentRamp    = new OklchRamp(tertiaryHue, chroma);
        var warningRamp   = new OklchRamp(WarningHue, WarningChroma);
        var errorRamp     = new OklchRamp(ErrorHue, ErrorChroma);

        // Update the retained groups before notifying consumers; no palette objects are replaced.
        var primaryLightness = nfloat.Clamp(options.PrimaryLightness ?? (IsDark ? 0.75f : 0.56f), 0, 1);
        var primary = primaryRamp[primaryLightness];
        // Choose black or white by actual luminance, including very light/dark picker shades.
        Primary.Update(primaryRamp, IsDark, primaryLightness, ColorGroup.ContrastingTone(primary),
            IsDark ? 0.30f : 0.90f, IsDark ? 0.90f : 0.10f);
        Secondary.Update(secondaryRamp, IsDark);
        Tertiary.Update(accentRamp, IsDark);
        Warning.Update(warningRamp, IsDark);
        Error.Update(errorRamp, IsDark);

        // Application and Surface: neutral by default, optionally tinted by NeutralStyle.
        var pureNeutralRamp = new OklchRamp(neutralHue, 0);
        var secondaryTintRamp = new OklchRamp(secondaryHue, NeutralChroma);
        var tertiaryTintRamp = new OklchRamp(tertiaryHue, NeutralChroma);

        var appRamp = options.NeutralStyle switch
        {
            NeutralStyle.SecondaryApp => secondaryTintRamp,
            NeutralStyle.TertiaryApp  => tertiaryTintRamp,
            _                         => pureNeutralRamp,
        };

        var surfaceRamp = options.NeutralStyle switch
        {
            NeutralStyle.SecondarySurface => secondaryTintRamp,
            NeutralStyle.TertiarySurface  => tertiaryTintRamp,
            _                             => pureNeutralRamp,
        };

        UpdateNeutralGroup(Application, appRamp, IsDark, application: true);
        UpdateNeutralGroup(Surface, surfaceRamp, IsDark, application: false);
        UpdateNeutralInteractiveGroup(Neutral, pureNeutralRamp, IsDark);

        // Outlines from pure neutral ramp at mid-tones.
        Outline        = IsDark ? pureNeutralRamp[0.60f] : pureNeutralRamp[0.50f];
        OutlineVariant = IsDark ? pureNeutralRamp[0.30f] : pureNeutralRamp[0.80f];

        FocusRing = Tertiary.Background;
    }

    /// <inheritdoc/>
    public ColorGroup Application { get; } = new();

    /// <inheritdoc/>
    public ColorGroup Surface { get; } = new();

    /// <inheritdoc/>
    public ColorGroup Neutral { get; } = new();

    /// <inheritdoc/>
    public Color Outline { get; private set; }

    /// <inheritdoc/>
    public Color OutlineVariant { get; private set; }

    /// <inheritdoc/>
    public ColorGroup Primary { get; } = new();

    /// <inheritdoc/>
    public ColorGroup Secondary { get; } = new();

    /// <inheritdoc/>
    public ColorGroup Tertiary { get; } = new();

    /// <inheritdoc/>
    public ColorGroup Warning { get; } = new();

    /// <inheritdoc/>
    public ColorGroup Error { get; } = new();

    /// <inheritdoc/>
    public Color FocusRing { get; private set; }

    /// <inheritdoc/>
    public bool IsDark { get; private set; }

    /// <inheritdoc/>
    public OklchRamp GetTonalRamp(nfloat hueDegrees, nfloat chroma)
        => new OklchRamp(hueDegrees, chroma);

    private static (nfloat secondary, nfloat tertiary) DeriveHues(
        nfloat primary, ColorHarmony harmony, nfloat? secondaryOverride, nfloat? tertiaryOverride)
    {
        nfloat secondary, tertiary;

        switch (harmony)
        {
            case ColorHarmony.Complementary:
                secondary = primary + 180;
                tertiary = primary + 180;
                break;
            case ColorHarmony.Analogous:
                secondary = primary + 30;
                tertiary = primary + 60;
                break;
            case ColorHarmony.SplitComplementary:
                secondary = primary + 150;
                tertiary = primary + 210;
                break;
            case ColorHarmony.Triadic:
                secondary = primary + 120;
                tertiary = primary + 240;
                break;
            case ColorHarmony.Tetradic:
                secondary = primary + 90;
                tertiary = primary + 180;
                break;
            default:
                secondary = primary + 150;
                tertiary = primary + 210;
                break;
        }

        secondary = secondaryOverride ?? secondary;
        tertiary = tertiaryOverride ?? tertiary;

        return (Normalize(secondary), Normalize(tertiary));
    }

    private static void UpdateNeutralGroup(ColorGroup group, OklchRamp ramp, bool isDark, bool application)
    {
        var bg = isDark ? (application ? 0.14f : 0.23f) : (application ? 0.94f : 0.995f);
        group.Update(ramp, isDark, bg, isDark ? 0.90f : 0.10f,
            isDark ? (application ? 0.23f : 0.34f) : (application ? 0.995f : 0.88f),
            isDark ? (application ? 0.90f : 0.80f) : (application ? 0.10f : 0.30f));
    }

    private static void UpdateNeutralInteractiveGroup(ColorGroup group, OklchRamp ramp, bool isDark) =>
        group.Update(ramp, isDark, isDark ? 0.25f : 0.88f, isDark ? 0.90f : 0.15f,
            isDark ? 0.20f : 0.92f, isDark ? 0.85f : 0.25f);

    private static nfloat Normalize(nfloat hue)
    {
        hue %= 360;
        if (hue < 0) hue += 360;
        return hue;
    }
}
