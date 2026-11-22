using Xui.GPU.Shaders;
using Xui.GPU.Shaders.Attributes;
using Xui.GPU.Shaders.Intrinsics;
using Xui.GPU.Shaders.Types;

namespace Xui.Apps.TestApp.Pages.FloorplanGpu;

[ShaderProgram]
public partial struct FloorplanShader
{
    public struct Vertex
    {
        [Location(0)] public Float4 PositionUv;
        [Location(1)] public Float4 Detail;
    }
    public struct Varyings
    {
        [BuiltIn(BuiltIn.Position)] public Float4 Position;
        [Location(0)] public Float4 Texture;
    }
    public struct Bindings
    {
        public Float4 Camera;
        public Float4 Fade;
        public Texture2D<Color4> Atlas;
        public Sampler Sampler;
    }
    [VertexShader]
    public struct VertexStage : IVertexShader<Vertex, Varyings, Bindings>
    {
        public Varyings Execute(Vertex input, in Bindings bindings) => new Varyings
        {
            Position = new Float4(input.PositionUv.X * bindings.Camera.X + bindings.Camera.Z + input.Detail.Y * bindings.Fade.Z,
                input.PositionUv.Y * bindings.Camera.Y + bindings.Camera.W + input.Detail.Z * bindings.Fade.W, F32.Zero, F32.One),
            Texture = new Float4(input.PositionUv.Z, input.PositionUv.W,
                input.Detail.X + bindings.Fade.Y, bindings.Fade.X * input.Detail.W),
        };
    }
    [FragmentShader]
    public struct FragmentStage : IFragmentShader<Varyings, FragmentOutput, Bindings>
    {
        public FragmentOutput Execute(Varyings input, in Bindings bindings)
        {
            var color = Shader.SampleLevel(bindings.Atlas, bindings.Sampler,
                new Float2(input.Texture.X, input.Texture.Y), input.Texture.Z);
            return new FragmentOutput { Color = color * input.Texture.W };
        }
    }
}
