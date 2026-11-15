using Xui.GPU.Shaders;
using Xui.GPU.Shaders.Attributes;
using Xui.GPU.Shaders.Types;

namespace Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest;

/// <summary>
/// Cube shader program defined as C# types.
/// The [ShaderProgram] attribute marks this for source generation — a Roslyn source generator
/// reads the Execute methods, builds the IR, and emits HLSL/MSL as compile-time constants.
/// The Android build compiles the generated HLSL into validated SPIR-V for Vulkan.
/// </summary>
[ShaderProgram]
public partial struct CubeShader
{
    /// <summary>Varyings passed from vertex to fragment stage.</summary>
    public struct Varyings
    {
        [BuiltIn(BuiltIn.Position)]
        public Float4 Position;

        [Location(0)]
        public Color4 Color;
    }

    /// <summary>Uniform bindings shared across shader stages.</summary>
    public struct Bindings
    {
        public Float4x4 MVP;
    }

    /// <summary>Vertex shader: transforms position by MVP and passes color through.</summary>
    [VertexShader]
    public struct VertexStage : IVertexShader<CubeMesh.Vertex, Varyings, Bindings>
    {
        public Varyings Execute(CubeMesh.Vertex input, in Bindings bindings)
        {
            var pos4 = new Float4(input.Position, F32.One);
            return new Varyings
            {
                Position = bindings.MVP * pos4,
                Color = input.Color,
            };
        }
    }

    /// <summary>Fragment shader: outputs interpolated vertex color.</summary>
    [FragmentShader]
    public struct FragmentStage : IFragmentShader<Varyings, FragmentOutput, Bindings>
    {
        public FragmentOutput Execute(Varyings input, in Bindings bindings)
        {
            return new FragmentOutput { Color = input.Color };
        }
    }
}
