using System.Runtime.CompilerServices;
using Xui.DevKit.UI.Design;
using Xui.GPU.Shaders;
using Xui.GPU.Shaders.Types;
using Xui.GPU.Software;
using Xui.Runtime.Software;

namespace Xui.Tests.Component.GPU;

/// <summary>
/// Renders an OKLCH color wheel to a 512×512 PNG using the software GPU pipeline.
/// Two triangles form a full-screen quad; the fragment shader computes hue via atan2
/// and converts from OKLCH to sRGB.
/// </summary>
public class ColorWheelSnapshotTest
{
    const int Size = 512;

    [Fact]
    public unsafe void ColorWheel_MatchesSnapshot()
    {
        using var fb = new Framebuffer(Size, Size);
        var ctx = new RenderContext(fb);
        ctx.CullMode = CullMode.None;

        ctx.ClearColor(new Color4(new F32(0.15f), new F32(0.15f), new F32(0.15f), F32.One));

        // Full-screen quad: two triangles, UVs in [-1, 1]
        var vertices = new WheelVertex[]
        {
            new() { Position = new Float2(new F32(-1f), new F32(-1f)), UV = new Float2(new F32(-1f), new F32(-1f)) },
            new() { Position = new Float2(new F32( 1f), new F32(-1f)), UV = new Float2(new F32( 1f), new F32(-1f)) },
            new() { Position = new Float2(new F32( 1f), new F32( 1f)), UV = new Float2(new F32( 1f), new F32( 1f)) },

            new() { Position = new Float2(new F32(-1f), new F32(-1f)), UV = new Float2(new F32(-1f), new F32(-1f)) },
            new() { Position = new Float2(new F32( 1f), new F32( 1f)), UV = new Float2(new F32( 1f), new F32( 1f)) },
            new() { Position = new Float2(new F32(-1f), new F32( 1f)), UV = new Float2(new F32(-1f), new F32( 1f)) },
        };

        fixed (WheelVertex* ptr = vertices)
        {
            var source = new VertexSource<WheelVertex>(ptr, vertices.Length);
            ctx.Draw(source, new WheelVS(), new WheelFS(), new NoBindings());
        }

        AssertSnapshot(fb, "ColorWheel");
    }

    #region Snapshot comparison

    static void AssertSnapshot(Framebuffer fb, string name, [CallerFilePath] string filePath = "")
    {
        var snapshotDir = Path.Combine(Path.GetDirectoryName(filePath)!, "Snapshots");
        Directory.CreateDirectory(snapshotDir);

        var referencePath = Path.Combine(snapshotDir, $"{name}.png");
        var actualPath = Path.Combine(snapshotDir, $"{name}.Actual.png");

        var actualPixels = FramebufferToRgba(fb);

        PngEncoder.SaveRGBA(actualPath, actualPixels, fb.Width, fb.Height);

        if (!File.Exists(referencePath))
        {
            PngEncoder.SaveRGBA(referencePath, actualPixels, fb.Width, fb.Height);
            Assert.Fail($"Reference snapshot '{name}.png' did not exist and was generated. " +
                        "Verify it visually and re-run the test.");
            return;
        }

        var referencePixels = PngDecoder.LoadRGBA(referencePath, out int refW, out int refH);

        Assert.Equal(fb.Width, refW);
        Assert.Equal(fb.Height, refH);

        int diffCount = 0;
        int tolerance = 2;
        for (int i = 0; i < actualPixels.Length; i++)
        {
            if (Math.Abs(actualPixels[i] - referencePixels[i]) > tolerance)
                diffCount++;
        }

        if (diffCount > 0)
        {
            double diffPercent = 100.0 * diffCount / (fb.Width * fb.Height * 4);
            Assert.Fail($"Snapshot '{name}' differs in {diffCount} bytes ({diffPercent:F2}%). " +
                        $"Actual saved to '{actualPath}'. Delete reference and re-run to update.");
        }

        File.Delete(actualPath);
    }

    static unsafe byte[] FramebufferToRgba(Framebuffer fb)
    {
        var rgba = new byte[fb.Width * fb.Height * 4];
        uint* src = fb.ColorData;

        for (int i = 0; i < fb.Width * fb.Height; i++)
        {
            uint px = src[i];
            rgba[i * 4 + 0] = (byte)(px >> 24);
            rgba[i * 4 + 1] = (byte)(px >> 16);
            rgba[i * 4 + 2] = (byte)(px >> 8);
            rgba[i * 4 + 3] = (byte)(px);
        }

        return rgba;
    }

    #endregion

    #region Shader types

    struct WheelVertex
    {
        public Float2 Position;
        public Float2 UV;
    }

    struct WheelVarying
    {
        public Float4 Position;
        public Float2 UV;
    }

    struct NoBindings { }

    readonly struct WheelVS : IVertexShader<WheelVertex, WheelVarying, NoBindings>
    {
        public WheelVarying Execute(WheelVertex input, in NoBindings bindings) => new()
        {
            Position = new Float4(input.Position, F32.Zero, F32.One),
            UV = input.UV,
        };
    }

    /// <summary>
    /// Fragment shader that computes an OKLCH color wheel.
    /// UV is in [-1, 1]; atan2(y, x) gives hue, distance from center gives chroma,
    /// lightness is fixed at 0.70 for a vibrant result.
    /// Outside the unit circle, render dark background.
    /// </summary>
    readonly struct WheelFS : IFragmentShader<WheelVarying, FragmentOutput, NoBindings>
    {
        public FragmentOutput Execute(WheelVarying input, in NoBindings bindings)
        {
            float x = input.UV.X;
            float y = input.UV.Y;
            float dist = MathF.Sqrt(x * x + y * y);

            // Outside the circle — dark background
            if (dist > 1.0f)
                return new FragmentOutput { Color = new Color4(new F32(0.15f), new F32(0.15f), new F32(0.15f), F32.One) };

            // Hue from angle (0–360)
            float hue = MathF.Atan2(y, x) * (180f / MathF.PI);
            if (hue < 0) hue += 360f;

            // Chroma scales with distance from center
            float lightness = 0.70f;
            float maxChroma = MaxSrgbChroma(lightness, hue);
            float chroma = dist * maxChroma;

            // OKLCH → sRGB (inline for shader)
            float hRad = hue * (MathF.PI / 180f);
            float labA = chroma * MathF.Cos(hRad);
            float labB = chroma * MathF.Sin(hRad);

            float l_ = lightness + 0.3963377774f * labA + 0.2158037573f * labB;
            float m_ = lightness - 0.1055613458f * labA - 0.0638541728f * labB;
            float s_ = lightness - 0.0894841775f * labA - 1.2914855480f * labB;

            float l = l_ * l_ * l_;
            float m = m_ * m_ * m_;
            float s = s_ * s_ * s_;

            float lr = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
            float lg = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
            float lb = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

            float r = LinearToSrgb(lr);
            float g = LinearToSrgb(lg);
            float b = LinearToSrgb(lb);

            return new FragmentOutput
            {
                Color = new Color4(new F32(r), new F32(g), new F32(b), F32.One)
            };
        }

        static float LinearToSrgb(float c)
        {
            c = Math.Clamp(c, 0f, 1f);
            float s = c <= 0.0031308f
                ? c * 12.92f
                : 1.055f * MathF.Pow(c, 1f / 2.4f) - 0.055f;
            return Math.Clamp(s, 0f, 1f);
        }

        static float MaxSrgbChroma(float lightness, float hueDegrees)
        {
            if (lightness <= 0f || lightness >= 1f)
                return 0f;

            float lo = 0f;
            float hi = 0.5f;

            for (int i = 0; i < 16; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (IsInGamut(lightness, mid, hueDegrees))
                    lo = mid;
                else
                    hi = mid;
            }
            return lo;
        }

        static bool IsInGamut(float lightness, float chroma, float hueDegrees)
        {
            float hRad = hueDegrees * (MathF.PI / 180f);
            float labA = chroma * MathF.Cos(hRad);
            float labB = chroma * MathF.Sin(hRad);

            float l_ = lightness + 0.3963377774f * labA + 0.2158037573f * labB;
            float m_ = lightness - 0.1055613458f * labA - 0.0638541728f * labB;
            float s_ = lightness - 0.0894841775f * labA - 1.2914855480f * labB;

            float l = l_ * l_ * l_;
            float m = m_ * m_ * m_;
            float s = s_ * s_ * s_;

            float lr = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
            float lg = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
            float lb = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

            const float eps = -0.001f;
            const float max = 1.001f;
            return lr >= eps && lr <= max && lg >= eps && lg <= max && lb >= eps && lb <= max;
        }
    }

    #endregion
}
