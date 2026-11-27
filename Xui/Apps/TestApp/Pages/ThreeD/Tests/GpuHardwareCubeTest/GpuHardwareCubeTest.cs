using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Composition.GPU;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;

namespace Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest;

/// <summary>
/// A 300x300 3D surface showing a rotating cube rendered via hardware GPU acceleration.
/// Uses Direct3D 11 on Windows, Metal on macOS/iOS, and Vulkan on Android.
/// Features click-and-drag rotation interaction.
/// </summary>
public class GpuHardwareCubeTestView : CubeSurface, IGpuSurfaceContent
{
    private long nativeVersion = -1;
    private readonly object compositionKey = new(), labelKey = new();
    private readonly CubeMesh.Vertex[] vertices = CubeMesh.CreateVertices();

    protected override void RenderCore(IContext context)
    {
        using var group = context.Group(new GroupOptions
        { Key = compositionKey, Bounds = Frame, ViewBacking = true });
        if (context.GpuSurface(new GroupOptions
        { Key = this, Bounds = Frame, HasUpdates = IsLive && nativeVersion != SceneVersion }, out var failure))
        {
            EnsureDevice();
            using var overlay = context.Group(new GroupOptions
            { Key = labelKey, Bounds = Frame, ViewBacking = true });
            context.SetStroke(Colors.Black);
            context.StrokeRect(Frame);
            context.SetFill(new Color(0x000000CC));
            context.SetFont(new Font(11, "Inter"));
            context.TextBaseline = TextBaseline.Top;
            context.TextAlign = TextAlign.Left;
            context.FillText(_nativeLabel, (Frame.X + 6, Frame.Y + 6));
        }
        else if (failure == GpuSurfaceFailure.NotSupported)
        {
            context.SetFill(new Color(0x000000CC));
            context.SetFont(new Font(14, "Inter"));
            context.TextBaseline = TextBaseline.Middle;
            context.TextAlign = TextAlign.Center;
            context.FillText("Not supported on this device", Frame.Center);
        }
        else base.RenderCore(context); // Explicit portable path for SVG capture and unsupported clips.
    }

    void IGpuSurfaceContent.Render(IGpuRenderTarget target)
    {
        if (!EnsureDevice()) throw new InvalidOperationException("GPU presentation requires a GPU device.");
        RenderFrame(target);
        nativeVersion = SceneVersion;
        NativeSceneRendered();
    }

    // GPU resources
    private IGpuDevice? _gpuDevice;
    private IGpuCommandList? _commands;
    private GpuPipelineDesc? _pipeline;
    private IGpuRenderTarget? _renderTarget;
    private IGpuVertexShader? _vertexShader;
    private IGpuFragmentShader? _fragmentShader;
    private string _nativeLabel = "GPU · native surface";
    private string _backendName = "Hardware rendering unavailable on this platform.";

    protected override string BackendLabel => _backendName;

    private bool EnsureDevice()
    {
        if (_vertexShader is not null) return true;
        _gpuDevice = GetService(typeof(IGpuDevice)) as IGpuDevice;
        var backend = GetService(typeof(Xui.GPU.Backends.IShaderBackend)) as Xui.GPU.Backends.IShaderBackend;
        if (_gpuDevice is null || backend is null) return false;
        _backendName = $"GPU: {_gpuDevice.BackendName}";
        _nativeLabel = $"{_backendName} · native surface";
        var source = CubeShader.GenerateShaderSource(backend);
        _vertexShader = _gpuDevice.CompileVertexShader(source, CubeShader.GetVertexEntry(backend.Name));
        _fragmentShader = _gpuDevice.CompileFragmentShader(source, CubeShader.GetFragmentEntry(backend.Name));
        _pipeline = new GpuPipelineDesc
        {
            VertexShader = _vertexShader, FragmentShader = _fragmentShader,
            DepthTestEnabled = true, DepthWriteEnabled = true,
        };
        return true;
    }

    protected override bool RenderPixels(byte[] pixels)
    {
        if (!EnsureDevice()) return false;
        _renderTarget ??= _gpuDevice!.CreateRenderTarget(300, 300);
        RenderFrame(_renderTarget);
        _renderTarget.ReadbackPixelsBgra(pixels);
        return true;
    }

    protected override void OnDeactivate()
    {
        _commands?.Dispose(); _commands = null; _pipeline = null;
        _renderTarget?.Dispose(); _renderTarget = null;
        _vertexShader?.Dispose(); _vertexShader = null;
        _fragmentShader?.Dispose(); _fragmentShader = null;
        base.OnDeactivate();
    }

    private void RenderFrame(IGpuRenderTarget target)
    {
        var cmd = _commands ??= _gpuDevice!.CreateCommandList();

        cmd.BeginRenderPass(target, new Color4(0.502f, 0.502f, 0.502f, 1.0f));

        cmd.SetPipeline(_pipeline!);

        var mvp = ComputeMvpMatrix((float)target.Width / target.Height);

        cmd.SetVertexBuffer<CubeMesh.Vertex>(vertices);
        cmd.SetConstantBuffer(in mvp);
        cmd.Draw(vertices.Length);

        cmd.EndRenderPass();
        cmd.Commit();
    }

    private Float4x4 ComputeMvpMatrix(float aspectRatio)
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

        var modelMatrix = Float4x4.CreateRotationX(new F32(rotX)) *
                         Float4x4.CreateRotationY(new F32(rotY)) *
                         scaleMatrix;

        var viewMatrix = Float4x4.CreateLookAt(
            new Float3(F32.Zero, F32.Zero, new F32(5.0f)),
            new Float3(F32.Zero, F32.Zero, F32.Zero),
            new Float3(F32.Zero, F32.One, F32.Zero)
        );

        var projectionMatrix = Float4x4.CreatePerspective(
            new F32(MathF.PI / 4.0f),
            new F32(aspectRatio),
            new F32(0.1f),
            new F32(100.0f)
        );

        return projectionMatrix * viewMatrix * modelMatrix;
    }
}
