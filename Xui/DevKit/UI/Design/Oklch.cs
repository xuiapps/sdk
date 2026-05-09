using Xui.Core.Canvas;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// OKLCH perceptual color space (Oklab-based Lightness, Chroma, Hue).
/// Interpolation in OKLCH produces vivid, perceptually uniform color transitions.
/// See: https://bottosson.github.io/posts/oklab/
/// </summary>
public readonly struct Oklch
{
    /// <summary>Perceptual lightness (0.0 = black, 1.0 = white).</summary>
    public nfloat L { get; init; }

    /// <summary>Colorfulness (0.0 = neutral gray; typical max ~0.37 for sRGB gamut).</summary>
    public nfloat C { get; init; }

    /// <summary>Hue angle in degrees (0–360).</summary>
    public nfloat H { get; init; }

    /// <summary>
    /// Creates a new OKLCH color value.
    /// </summary>
    public Oklch(nfloat l, nfloat c, nfloat h)
    {
        L = l;
        C = c;
        H = h;
    }

    /// <summary>
    /// Converts an sRGB <see cref="Color"/> to OKLCH.
    /// </summary>
    public Oklch(Color color)
    {
        // sRGB → linear sRGB
        var lr = SrgbToLinear(color.Red);
        var lg = SrgbToLinear(color.Green);
        var lb = SrgbToLinear(color.Blue);

        // linear sRGB → Oklab (using the M1 and M2 matrices)
        var l_ = (nfloat)Math.Cbrt((double)(0.4122214708f * lr + 0.5363325363f * lg + 0.0514459929f * lb));
        var m_ = (nfloat)Math.Cbrt((double)(0.2119034982f * lr + 0.6806995451f * lg + 0.1073969566f * lb));
        var s_ = (nfloat)Math.Cbrt((double)(0.0883024619f * lr + 0.2817188376f * lg + 0.6299787005f * lb));

        var labL = 0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_;
        var labA = 1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_;
        var labB = 0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_;

        // Oklab → OKLCH
        L = labL;
        C = (nfloat)Math.Sqrt((double)(labA * labA + labB * labB));
        H = C > 0.0001f
            ? NormalizeHue((nfloat)(Math.Atan2((double)labB, (double)labA) * (180.0 / Math.PI)))
            : 0;
    }

    /// <summary>
    /// Converts this OKLCH value back to an sRGB <see cref="Color"/>.
    /// </summary>
    public Color ToColor()
    {
        // OKLCH → Oklab
        var hRad = (double)H * (Math.PI / 180.0);
        var labA = C * (nfloat)Math.Cos(hRad);
        var labB = C * (nfloat)Math.Sin(hRad);

        // Oklab → LMS (cube roots)
        var l_ = L + 0.3963377774f * labA + 0.2158037573f * labB;
        var m_ = L - 0.1055613458f * labA - 0.0638541728f * labB;
        var s_ = L - 0.0894841775f * labA - 1.2914855480f * labB;

        // Undo cube root
        var l = l_ * l_ * l_;
        var m = m_ * m_ * m_;
        var s = s_ * s_ * s_;

        // LMS → linear sRGB
        var lr = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
        var lg = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
        var lb = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

        return new Color(
            LinearToSrgb(lr),
            LinearToSrgb(lg),
            LinearToSrgb(lb),
            1
        );
    }

    /// <summary>
    /// Returns the maximum chroma representable in sRGB for the given lightness and hue.
    /// Uses bisection search: for a candidate chroma, convert OKLCH to sRGB and check
    /// whether all channels are in [0, 1].
    /// </summary>
    public static nfloat MaxSrgbChroma(nfloat lightness, nfloat hueDegrees)
    {
        if (lightness <= 0 || lightness >= 1)
            return 0;

        nfloat lo = 0;
        nfloat hi = (nfloat)0.5;

        for (int i = 0; i < 20; i++)
        {
            var mid = (lo + hi) / 2;
            var test = new Oklch(lightness, mid, hueDegrees).ToColor();
            if (IsInGamut(test))
                lo = mid;
            else
                hi = mid;
        }

        return lo;
    }

    /// <summary>
    /// Converts this OKLCH value to an sRGB <see cref="Color"/>.
    /// </summary>
    public static implicit operator Color(Oklch oklch) => oklch.ToColor();

    /// <summary>
    /// Converts an sRGB <see cref="Color"/> to OKLCH.
    /// </summary>
    public static implicit operator Oklch(Color color) => new Oklch(color);

    private static bool IsInGamut(Color c)
    {
        nfloat eps = (nfloat)(-0.001);
        nfloat max = (nfloat)1.001;
        return c.Red >= eps && c.Red <= max
            && c.Green >= eps && c.Green <= max
            && c.Blue >= eps && c.Blue <= max;
    }

    private static nfloat SrgbToLinear(nfloat c)
    {
        return c <= 0.04045f
            ? c / 12.92f
            : (nfloat)Math.Pow((double)((c + 0.055f) / 1.055f), 2.4);
    }

    private static nfloat LinearToSrgb(nfloat c)
    {
        if (nfloat.IsNaN(c) || nfloat.IsInfinity(c)) return 0;
        c = nfloat.Clamp(c, 0, 1);
        var s = c <= 0.0031308f
            ? c * 12.92f
            : 1.055f * (nfloat)Math.Pow((double)c, 1.0 / 2.4) - 0.055f;
        return nfloat.Clamp(s, 0, 1);
    }

    private static nfloat NormalizeHue(nfloat h)
    {
        h %= 360;
        if (h < 0) h += 360;
        return h;
    }
}
