using Xui.Core.Canvas;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// A gamut-aware tonal ramp that maps t in [0, 1] to a color at lightness t.
/// At each lightness, chroma is clamped to the sRGB gamut boundary for the hue,
/// ensuring every output color is representable without clipping.
/// </summary>
public readonly struct OklchRamp
{
    /// <summary>Hue angle in degrees (0–360).</summary>
    public nfloat Hue { get; init; }

    /// <summary>Requested target chroma (clamped to sRGB gamut at each lightness).</summary>
    public nfloat TargetChroma { get; init; }

    /// <summary>
    /// Creates a gamut-aware tonal ramp for the given hue and target chroma.
    /// </summary>
    /// <param name="hueDegrees">Hue angle in degrees (0–360).</param>
    /// <param name="targetChroma">Desired chroma level (will be clamped to sRGB gamut at each lightness).</param>
    public OklchRamp(nfloat hueDegrees, nfloat targetChroma)
    {
        Hue = hueDegrees;
        TargetChroma = targetChroma;
    }

    /// <summary>
    /// Evaluates the ramp at position t in [0, 1], returning an sRGB Color.
    /// t maps to lightness; chroma is gamut-clamped.
    /// </summary>
    public Color this[nfloat t]
    {
        get
        {
            var maxC = Oklch.MaxSrgbChroma(t, Hue);
            return new Oklch(t, nfloat.Min(TargetChroma, maxC), Hue).ToColor();
        }
    }
}
