using System.Runtime.CompilerServices;
using Xui.GPU.Shaders;
using Xui.GPU.Shaders.Types;
using Xui.GPU.Software;
using Xui.Runtime.Software;

namespace Xui.Tests.Component.GPU;

/// <summary>
/// Generates the color picker's reusable white/black mask through the software GPU,
/// using a full-screen quad and vertex/fragment shaders like ColorWheelSnapshotTest.
/// The shipped PNG is the reference; failures leave a generated PNG for review.
/// </summary>
public class ColorTriangleSnapshotTest
{
    private const int Size = 256;

    [Fact]
    public unsafe void ColorTriangle_MatchesAppAsset()
    {
        using var framebuffer = new Framebuffer(Size, Size);
        var context = new RenderContext(framebuffer)
        {
            CullMode = CullMode.None,
            DepthTestEnabled = false,
            // Store straight alpha, rather than blending the mask into the target.
            BlendEnabled = false,
        };
        context.ClearColor(new Color4(F32.Zero, F32.Zero, F32.Zero, F32.Zero));

        var vertices = new MaskVertex[]
        {
            new() { Position = new(-1, -1), UV = new(0, 0) },
            new() { Position = new( 1, -1), UV = new(1, 0) },
            new() { Position = new( 1,  1), UV = new(1, 1) },
            new() { Position = new(-1, -1), UV = new(0, 0) },
            new() { Position = new( 1,  1), UV = new(1, 1) },
            new() { Position = new(-1,  1), UV = new(0, 1) },
        };
        fixed (MaskVertex* pointer = vertices)
        {
            context.Draw(new VertexSource<MaskVertex>(pointer, vertices.Length),
                new MaskVS(), new MaskFS(), new NoBindings());
        }

        var pixels = new byte[Size * Size * 4];
        for (var i = 0; i < Size * Size; i++)
        {
            var pixel = framebuffer.ColorData[i];
            pixels[i * 4] = (byte)(pixel >> 24);
            pixels[i * 4 + 1] = (byte)(pixel >> 16);
            pixels[i * 4 + 2] = (byte)(pixel >> 8);
            pixels[i * 4 + 3] = (byte)pixel;
        }

        AssertAppAsset(pixels);
    }

    private static void AssertAppAsset(byte[] pixels, [CallerFilePath] string filePath = "")
    {
        var directory = Path.GetDirectoryName(filePath)!;
        var snapshotDirectory = Path.Combine(directory, "Snapshots");
        Directory.CreateDirectory(snapshotDirectory);
        var actualPath = Path.Combine(snapshotDirectory, "ColorTriangle.Actual.png");
        var assetPath = Path.GetFullPath(Path.Combine(directory, "../../../Apps/TestApp/Assets/ColorTriangle.png"));
        PngEncoder.SaveRGBA(actualPath, pixels, Size, Size);

        Assert.True(File.Exists(assetPath),
            $"Missing triangle asset. Review '{actualPath}', then copy it to '{assetPath}'.");
        var expected = PngDecoder.LoadRGBA(assetPath, out var width, out var height);
        Assert.Equal(Size, width);
        Assert.Equal(Size, height);

        // Match the color-wheel snapshot tolerance for floating-point rasterization
        // and conversion of shader values to 8-bit channels.
        var differences = 0;
        for (var i = 0; i < pixels.Length; i++)
            if (Math.Abs(pixels[i] - expected[i]) > 2) differences++;
        Assert.True(differences == 0,
            $"Triangle mask differs in {differences} channels. Review '{actualPath}' before updating '{assetPath}'.");
        File.Delete(actualPath);
    }

    private struct MaskVertex
    {
        public Float2 Position;
        public Float2 UV;
    }

    private struct MaskVarying
    {
        public Float4 Position;
        public Float2 UV;
    }

    private struct NoBindings { }

    private readonly struct MaskVS : IVertexShader<MaskVertex, MaskVarying, NoBindings>
    {
        public MaskVarying Execute(MaskVertex input, in NoBindings bindings) => new()
        {
            Position = new Float4(input.Position, F32.Zero, F32.One),
            UV = input.UV,
        };
    }

    private readonly struct MaskFS : IFragmentShader<MaskVarying, FragmentOutput, NoBindings>
    {
        public FragmentOutput Execute(MaskVarying input, in NoBindings bindings)
        {
            // Rasterization samples pixel centers. Map the first and last texels
            // exactly to the triangle's endpoints, including a transparent hue tip.
            float x = Math.Clamp((input.UV.X * Size - 0.5f) / (Size - 1), 0, 1);
            float y = Math.Clamp((input.UV.Y * Size - 0.5f) / (Size - 1), 0, 1);

            // Triangle vertices: white (0,0), black (0,1), hue (1,0.5).
            // Over a solid hue, alpha=white+black and gray=white/alpha gives
            // white*White + black*Black + hue*Hue via ordinary source-over.
            float alpha = 1 - x;
            float white = Math.Clamp((1 - x) / 2 - (y - 0.5f), 0, alpha);
            float gray = alpha > 0 ? white / alpha : 0;

            // Extend the mask beyond the triangle. The canvas clips its outline,
            // avoiding transparent fringes when the image is filtered at the edges.
            return new FragmentOutput { Color = new Color4(gray, gray, gray, alpha) };
        }
    }
}
