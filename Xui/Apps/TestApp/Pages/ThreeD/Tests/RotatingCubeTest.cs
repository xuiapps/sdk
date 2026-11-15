using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.GPU.Shaders;
using Xui.GPU.Shaders.Types;
using Xui.GPU.Software;
using static Xui.Core.Canvas.Colors;
using CubeMesh = Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest.CubeMesh;

namespace Xui.Apps.TestApp.Pages.ThreeD.Tests;

/// <summary>
/// A 300x300 3D surface showing a rotating cube rendered via the software pipeline.
/// Uses the same shaders and mesh as GpuHardwareCubeTest but runs entirely on the CPU.
/// Features click-and-drag rotation interaction.
/// </summary>
public class RotatingCubeTest : View
{
    private NFloat rotationX = 0.3f;
    private NFloat rotationY = 0.3f;
    private Point? dragStart = null;
    private NFloat dragStartRotX = 0;
    private NFloat dragStartRotY = 0;

    private unsafe Framebuffer? framebuffer = null;
    private IImage? platformImage = null;
    private byte[]? bgraBuffer = null;
    private bool isInitialized = false;

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

    protected override unsafe void RenderCore(IContext context)
    {
        if (!isInitialized || framebuffer == null)
        {
            framebuffer?.Dispose();
            framebuffer = new Framebuffer(300, 300, withDepthBuffer: true);
            bgraBuffer = new byte[300 * 300 * 4];
            platformImage = this.GetService<IImage>();
            isInitialized = true;
        }

        RenderCube(framebuffer.Value);
        ConvertToBgra(framebuffer.Value, bgraBuffer!);

        platformImage?.LoadPixels(300, 300, bgraBuffer);
        var destRect = new Rect(this.Frame.X, this.Frame.Y, 300, 300);
        if (platformImage != null)
            context.DrawImage(platformImage, destRect);

        context.SetStroke(Black);
        context.StrokeRect(destRect);

        // Draw label
        context.SetFill(new Color(0x00, 0x00, 0x00, 0xCC));
        context.SetFont(new Font(11, "Inter"));
        context.FillText("Software", new Point(this.Frame.X + 6, this.Frame.Y + 16));
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
