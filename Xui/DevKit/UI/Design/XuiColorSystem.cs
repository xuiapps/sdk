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
        var neutralRamp   = new OklchRamp(neutralHue, NeutralChroma);

        // Build semantic groups.
        Primary   = ColorGroup.FromRamp(primaryRamp, IsDark);
        Secondary = ColorGroup.FromRamp(secondaryRamp, IsDark);
        Tertiary    = ColorGroup.FromRamp(accentRamp, IsDark);
        Warning   = ColorGroup.FromRamp(warningRamp, IsDark);
        Error     = ColorGroup.FromRamp(errorRamp, IsDark);

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

        Application = BuildNeutralGroup(appRamp, IsDark, application: true);
        Surface     = BuildNeutralGroup(surfaceRamp, IsDark, application: false);
        Neutral     = BuildNeutralInteractiveGroup(pureNeutralRamp, IsDark);

        // Outlines from pure neutral ramp at mid-tones.
        Outline        = IsDark ? pureNeutralRamp[0.60f] : pureNeutralRamp[0.50f];
        OutlineVariant = IsDark ? pureNeutralRamp[0.30f] : pureNeutralRamp[0.80f];

        FocusRing = Tertiary.Background;
    }

    /// <inheritdoc/>
    public ColorGroup Application { get; }

    /// <inheritdoc/>
    public ColorGroup Surface { get; }

    /// <inheritdoc/>
    public ColorGroup Neutral { get; }

    /// <inheritdoc/>
    public Color Outline { get; }

    /// <inheritdoc/>
    public Color OutlineVariant { get; }

    /// <inheritdoc/>
    public ColorGroup Primary { get; }

    /// <inheritdoc/>
    public ColorGroup Secondary { get; }

    /// <inheritdoc/>
    public ColorGroup Tertiary { get; }

    /// <inheritdoc/>
    public ColorGroup Warning { get; }

    /// <inheritdoc/>
    public ColorGroup Error { get; }

    /// <inheritdoc/>
    public Color FocusRing { get; }

    /// <inheritdoc/>
    public bool IsDark { get; }

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

    private static ColorGroup BuildNeutralGroup(OklchRamp ramp, bool isDark, bool application)
    {
        var bgL = isDark ? (application ? 0.06f : 0.12f) : (application ? 0.99f : 0.98f);

        if (isDark)
        {
            return new ColorGroup
            {
                Background  = ramp[bgL],
                Foreground  = ramp[0.90f],
                Container   = ramp[application ? 0.12f : 0.30f],
                OnContainer = ramp[application ? 0.90f : 0.80f],
                Ramp        = ramp,
                BackgroundLightness = bgL,
            };
        }

        return new ColorGroup
        {
            Background  = ramp[bgL],
            Foreground  = ramp[0.10f],
            Container   = ramp[application ? 0.98f : 0.90f],
            OnContainer = ramp[application ? 0.10f : 0.30f],
            Ramp        = ramp,
            BackgroundLightness = bgL,
        };
    }

    /// <summary>
    /// Builds a low-contrast interactive group — sits between Surface background and mid-tone.
    /// Think GitHub gray buttons, VS Code toolbar buttons, quiet controls.
    /// </summary>
    private static ColorGroup BuildNeutralInteractiveGroup(OklchRamp ramp, bool isDark)
    {
        if (isDark)
        {
            return new ColorGroup
            {
                Background  = ramp[0.25f],
                Foreground  = ramp[0.90f],
                Container   = ramp[0.20f],
                OnContainer = ramp[0.85f],
                Ramp        = ramp,
                BackgroundLightness = 0.25f,
            };
        }

        return new ColorGroup
        {
            Background  = ramp[0.88f],
            Foreground  = ramp[0.15f],
            Container   = ramp[0.92f],
            OnContainer = ramp[0.25f],
            Ramp        = ramp,
            BackgroundLightness = 0.88f,
        };
    }

    private static nfloat Normalize(nfloat hue)
    {
        hue %= 360;
        if (hue < 0) hue += 360;
        return hue;
    }
}
