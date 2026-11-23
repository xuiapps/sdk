using Xui.GPU.Shaders;
using Xui.GPU.Shaders.Intrinsics;
using Xui.GPU.Shaders.Types;
using Xui.GPU.Software;

namespace Xui.Core.Tests.GPU;

public class TextureSamplingTests
{
    [Fact]
    public void BilinearAndTrilinearSamplingUsePremultipliedColorsAndClampAtEdges()
    {
        var image = new SampledTexture(2, 2,
        [
            [255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 0, 0, 0, 0],
            [64, 64, 64, 191],
        ]);
        var texture = new Texture2D<Color4>(image);
        var center = Shader.SampleLevel(texture, Sampler.LinearClamp, new Float2(.5f, .5f), 0);
        Assert.Equal(.25f, (float)center.R, 5); Assert.Equal(.75f, (float)center.A, 5);
        var between = Shader.SampleLevel(texture, Sampler.LinearClamp, new Float2(0, 0), .5f);
        Assert.Equal((1 + 64 / 255f) / 2, (float)between.R, 5);
        Assert.Equal(64 / 510f, (float)between.G, 5);
        Assert.Equal(1f, (float)Shader.Sample(texture, Sampler.LinearClamp, new Float2(-1, -1)).R);
        Assert.Equal(191 / 255f, (float)Shader.SampleLevel(texture, Sampler.LinearClamp, new Float2(1, 1), 100).A, 5);
    }

    [Fact]
    public void InvalidMipDataIsRejectedBeforeSampling()
    {
        Assert.Throws<ArgumentException>(() => new SampledTexture(2, 2, [new byte[15]]));
        Assert.Throws<ArgumentException>(() => new SampledTexture(1, 1, [new byte[4], new byte[4]]));
    }

    [Fact]
    public unsafe void TranslucentQuadCoversItsSharedDiagonalExactlyOnce()
    {
        using var fb = new Framebuffer(8, 8, false);
        var context = new RenderContext(fb) { DepthTestEnabled = false, BlendEnabled = true, PremultipliedAlpha = true, CullMode = CullMode.None };
        Float4[] points = [new(-1, 1, 0, 1), new(1, 1, 0, 1), new(1, -1, 0, 1), new(-1, 1, 0, 1), new(1, -1, 0, 1), new(-1, -1, 0, 1)];
        fixed (Float4* data = points) context.Draw(new VertexSource<Float4>(data, points.Length), new Vertex(), new Fragment(), 0);
        for (int i = 0; i < 64; i++) Assert.Equal(0x7F00007Fu, fb.ColorData[i]);
    }
    private struct Vertex : IVertexShader<Float4, Float4, int>
    { public Float4 Execute(Float4 input, in int bindings) => input; }
    private struct Fragment : IFragmentShader<Float4, FragmentOutput, int>
    { public FragmentOutput Execute(Float4 input, in int bindings) => new() { Color = new(.5f, 0, 0, .5f) }; }
}
