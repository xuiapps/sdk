using System.Runtime.InteropServices;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;

namespace Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest;

/// <summary>
/// A 300x300 3D surface showing a rotating cube rendered via hardware GPU acceleration.
/// Uses Direct3D 11 on Windows, Metal on macOS/iOS.
/// Features click-and-drag rotation interaction.
/// </summary>
public class GpuHardwareCubeTestView : CubeSurface
{
    // GPU resources
    private IGpuDevice? _gpuDevice;
    private IGpuRenderTarget? _renderTarget;
    private IGpuVertexShader? _vertexShader;
    private IGpuFragmentShader? _fragmentShader;
    private string _backendName = "Hardware rendering unavailable on this platform.";

    protected override string BackendLabel => _backendName;

    protected override bool RenderPixels(byte[] pixels)
    {
        if (_renderTarget is null)
        {
            _gpuDevice = GetService(typeof(IGpuDevice)) as IGpuDevice;
            var backend = GetService(typeof(Xui.GPU.Backends.IShaderBackend)) as Xui.GPU.Backends.IShaderBackend;
            if (_gpuDevice is null || backend is null) return false;
            _backendName = $"GPU: {_gpuDevice.BackendName}";
            var source = CubeShader.GenerateShaderSource(backend);
            _vertexShader = _gpuDevice.CompileVertexShader(source, CubeShader.GetVertexEntry(backend.Name));
            _fragmentShader = _gpuDevice.CompileFragmentShader(source, CubeShader.GetFragmentEntry(backend.Name));
            _renderTarget = _gpuDevice.CreateRenderTarget(300, 300);
        }
        RenderFrame(pixels);
        return true;
    }

    protected override void OnDeactivate()
    {
        _renderTarget?.Dispose(); _renderTarget = null;
        _vertexShader?.Dispose(); _vertexShader = null;
        _fragmentShader?.Dispose(); _fragmentShader = null;
        base.OnDeactivate();
    }

    private unsafe void RenderFrame(byte[] pixels)
    {
        using var cmd = _gpuDevice!.CreateCommandList();

        cmd.BeginRenderPass(_renderTarget!, new Color4(0.502f, 0.502f, 0.502f, 1.0f));

        cmd.SetPipeline(new GpuPipelineDesc
        {
            VertexShader = _vertexShader,
            FragmentShader = _fragmentShader,
            DepthTestEnabled = true,
            DepthWriteEnabled = true,
        });

        var vertices = CubeMesh.CreateVertices();
        var mvp = ComputeMvpMatrix();

        fixed (CubeMesh.Vertex* pVerts = vertices)
        {
            cmd.SetVertexBuffer(pVerts, new GpuVertexBufferDesc
            {
                Stride = Marshal.SizeOf<CubeMesh.Vertex>(),
                VertexCount = vertices.Length
            });
            cmd.SetConstantBuffer(&mvp, sizeof(Float4x4));
            cmd.Draw(vertices.Length);
        }

        cmd.EndRenderPass();
        cmd.Commit();

        _renderTarget!.ReadbackPixelsBgra(pixels);
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
}
