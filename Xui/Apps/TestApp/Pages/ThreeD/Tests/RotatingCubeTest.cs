using Xui.GPU.Shaders;
using Xui.GPU.Shaders.Types;
using Xui.GPU.Software;
using CubeMesh = Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest.CubeMesh;

namespace Xui.Apps.TestApp.Pages.ThreeD.Tests;

/// <summary>
/// A 300x300 3D surface showing a rotating cube rendered via the software pipeline.
/// Uses the same shaders and mesh as GpuHardwareCubeTest but runs entirely on the CPU.
/// Features click-and-drag rotation interaction.
/// </summary>
public class RotatingCubeTest : CubeSurface
{
    private unsafe Framebuffer? framebuffer;

    protected override string BackendLabel => "Software · developer preview";

    protected override unsafe bool RenderPixels(byte[] pixels)
    {
        framebuffer ??= new Framebuffer(300, 300, withDepthBuffer: true);
        RenderCube(framebuffer.Value);
        ConvertToBgra(framebuffer.Value, pixels);
        return true;
    }

    protected override unsafe void OnDeactivate()
    {
        framebuffer?.Dispose(); framebuffer = null;
        base.OnDeactivate();
    }

    private unsafe void RenderCube(Framebuffer fb)
    {
        fb.ClearColor(0xFF808080);
        fb.ClearDepth(1.0f);

        var vertices = CubeMesh.CreateVertices();
        var mvp = ComputeMvpMatrix();

        var renderContext = new RenderContext(fb);
        renderContext.DepthTestEnabled = true;

        fixed (CubeMesh.Vertex* pVerts = vertices)
        {
            var vertexSource = new VertexSource<CubeMesh.Vertex>(pVerts, vertices.Length);
            var vs = new CubeVertexShader();
            var fs = new CubeFragmentShader();
            renderContext.Draw(vertexSource, vs, fs, new CubeBindings { MVP = mvp });
        }
    }

    private Float4x4 ComputeMvpMatrix()
    {
        float rotX = (float)rotationX;
        float rotY = (float)rotationY;

        const float cubeScale = 0.6f;
        var scaleMatrix = new Float4x4(
            new Float4(new F32(cubeScale), F32.Zero, F32.Zero, F32.Zero),
            new Float4(F32.Zero, new F32(cubeScale), F32.Zero, F32.Zero),
            new Float4(F32.Zero, F32.Zero, new F32(cubeScale), F32.Zero),
            new Float4(F32.Zero, F32.Zero, F32.Zero, F32.One)
        );

        var modelMatrix = Float4x4.CreateRotationY(new F32(rotY)) *
                         Float4x4.CreateRotationX(new F32(rotX * 0.5f)) *
                         scaleMatrix;

        var viewMatrix = Float4x4.CreateLookAt(
            new Float3(F32.Zero, F32.Zero, new F32(5.0f)),
            new Float3(F32.Zero, F32.Zero, F32.Zero),
            new Float3(F32.Zero, F32.One, F32.Zero)
        );

        var projectionMatrix = Float4x4.CreatePerspective(
            new F32(MathF.PI / 4.0f),
            new F32(1.0f),
            new F32(0.1f),
            new F32(100.0f)
        );

        return projectionMatrix * viewMatrix * modelMatrix;
    }

    private static unsafe void ConvertToBgra(Framebuffer fb, byte[] bgra)
    {
        uint* src = fb.ColorData;
        int count = fb.Width * fb.Height;
        for (int i = 0; i < count; i++)
        {
            uint px = src[i];
            bgra[i * 4 + 0] = (byte)(px >> 8);   // B
            bgra[i * 4 + 1] = (byte)(px >> 16);  // G
            bgra[i * 4 + 2] = (byte)(px >> 24);  // R
            bgra[i * 4 + 3] = (byte)(px);        // A
        }
    }
}

// Software shader implementations using the same vertex/bindings types as the GPU path.

file struct CubeVaryings
{
    public Float4 Position;
    public Color4 Color;
}

file struct CubeBindings
{
    public Float4x4 MVP;
}

file readonly struct CubeVertexShader : IVertexShader<CubeMesh.Vertex, CubeVaryings, CubeBindings>
{
    public CubeVaryings Execute(CubeMesh.Vertex input, in CubeBindings bindings)
    {
        var pos4 = new Float4(input.Position, F32.One);
        return new CubeVaryings
        {
            Position = bindings.MVP * pos4,
            Color = input.Color
        };
    }
}

file readonly struct CubeFragmentShader : IFragmentShader<CubeVaryings, FragmentOutput, CubeBindings>
{
    public FragmentOutput Execute(CubeVaryings input, in CubeBindings bindings)
    {
        return new FragmentOutput { Color = input.Color };
    }
}
