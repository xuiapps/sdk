using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest;

/// <summary>
/// A 300x300 3D surface showing a rotating cube rendered via hardware GPU acceleration.
/// Uses Direct3D 11 on Windows, Metal on macOS/iOS.
/// Features click-and-drag rotation interaction.
/// </summary>
public class GpuHardwareCubeTestView : View
{
    private NFloat rotationX = 0.3f;
    private NFloat rotationY = 0.3f;
    private Point? dragStart = null;
    private NFloat dragStartRotX = 0;
    private NFloat dragStartRotY = 0;

    // GPU resources
    private IGpuDevice? _gpuDevice;
    private IGpuRenderTarget? _renderTarget;
    private IGpuVertexShader? _vertexShader;
    private IGpuFragmentShader? _fragmentShader;
    private string _backendName = "None";

    // Display resources
    private IImage? _platformImage;
    private byte[]? _bgraBuffer;
    private bool _initialized = false;

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            this.CapturePointer(e.PointerId);
            dragStart = e.State.Position;
            dragStartRotX = rotationX;
            dragStartRotY = rotationY;
        }
        else if (e.Type == PointerEventType.Move && dragStart != null)
        {
            var delta = e.State.Position - dragStart.Value;
            rotationY = dragStartRotY + delta.X * 0.01f;
            rotationX = dragStartRotX + delta.Y * 0.01f;
            this.InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Up)
        {
            this.ReleasePointer(e.PointerId);
            dragStart = null;
        }

        base.OnPointerEvent(ref e, phase);
    }

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        return new Size(300, 300);
    }

    protected override void RenderCore(IContext context)
    {
        if (!_initialized)
        {
            _bgraBuffer = new byte[300 * 300 * 4];
            _platformImage = this.GetService<IImage>();
            InitializeGpu();
            _initialized = true;
        }

        RenderFrame();

        // Upload pixels to platform image and draw
        _platformImage?.LoadPixels(300, 300, _bgraBuffer!);
        var destRect = new Rect(this.Frame.X, this.Frame.Y, 300, 300);
        if (_platformImage != null)
            context.DrawImage(_platformImage, destRect);

        // Draw border
        context.SetStroke(Black);
        context.StrokeRect(destRect);

        // Draw backend label
        context.SetFill(new Color(0x00, 0x00, 0x00, 0xCC));
        context.SetFont(new Font(11, "Inter"));
        context.FillText(
            $"GPU: {_backendName}",
            new Point(this.Frame.X + 6, this.Frame.Y + 16));
    }

    private void InitializeGpu()
    {
        _gpuDevice = this.GetRequiredService<IGpuDevice>();
        var shaderBackend = this.GetRequiredService<Xui.GPU.Backends.IShaderBackend>();

        _backendName = _gpuDevice.BackendName;

        var shaderSource = CubeShader.GenerateShaderSource(shaderBackend);
        _vertexShader = _gpuDevice.CompileVertexShader(shaderSource, CubeShader.GetVertexEntry(shaderBackend.Name));
        _fragmentShader = _gpuDevice.CompileFragmentShader(shaderSource, CubeShader.GetFragmentEntry(shaderBackend.Name));
        _renderTarget = _gpuDevice.CreateRenderTarget(300, 300);
    }

    private unsafe void RenderFrame()
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

        _renderTarget!.ReadbackPixelsBgra(_bgraBuffer!);
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
