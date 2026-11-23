using System.Runtime.InteropServices;
using Xui.Core.Math2D;
using Xui.GPU.Backends;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;
using Xui.GPU.Software;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.FloorplanGpu;

/// <summary>Uploads immutable resources once; each frame changes only camera/fade uniforms.</summary>
public sealed class FloorplanGpuRenderer(FloorplanGpuModel model) : IDisposable
{
    private IGpuResourceDevice? device;
    private IGpuResourceCommandList? commands;
    private IGpuTexture? texture;
    private IGpuVertexShader? vertex;
    private IGpuFragmentShader? fragment;
    private GpuPipelineDesc? pipeline;
    private readonly IGpuVertexBuffer?[] buffers = new IGpuVertexBuffer[4];

    /// <summary>Completes every upload and an offscreen draw of each mip and layer before presentation.</summary>
    public unsafe void PrepareAndWarm(IGpuResourceDevice nextDevice, IShaderBackend backend)
    {
        Prepare(nextDevice, backend);
        using var target = nextDevice.CreateRenderTarget(64, 64);
        FloorplanShader.Vertex[] quad =
        [
            new() { PositionUv = new(0, 0, 0, 0) }, new() { PositionUv = new(1, 0, 1, 0) },
            new() { PositionUv = new(1, 1, 1, 1) }, new() { PositionUv = new(0, 0, 0, 0) },
            new() { PositionUv = new(1, 1, 1, 1) }, new() { PositionUv = new(0, 1, 0, 1) },
        ];
        for (int i = 0; i < quad.Length; i++) quad[i].Detail = new(0, 0, 0, 1);
        using var buffer = nextDevice.CreateVertexBuffer(MemoryMarshal.AsBytes(quad.AsSpan()), 32);
        commands!.BeginRenderPass(target, Color4.White);
        commands.SetPipeline(pipeline!);
        commands.SetTexture(0, texture!, Sampler.LinearClamp);
        commands.SetVertexBuffer(buffer);
        for (int mip = 0; mip < model.Mips.Length; mip++)
        {
            var uniforms = new UniformData { Camera = new(2, -2, -1, 1), Fade = new(1, mip, 0, 0) };
            commands.SetConstantBuffer(&uniforms, sizeof(UniformData));
            commands.Draw(6);
        }
        commands.EndRenderPass(); commands.Commit();
        var transition = new FloorplanLayerTransition();
        for (int level = 0; level < model.Layers.Length; level++)
        {
            transition.Request(level);
            transition.Advance((NFloat).45); // Exercise both draws used during the first zoom transition.
            RenderNative(target, (0, 0, 1000, 760), AffineTransform.Translate((0, -120)), transition);
            transition.Advance((NFloat).15);
        }
        // Offscreen Commit waits for completion. No pending GPU work or first-use pipeline
        // creation remains when this renderer is handed to the UI thread.
    }

    public void Prepare(IGpuResourceDevice nextDevice, IShaderBackend backend)
    {
        if (ReferenceEquals(device, nextDevice) && pipeline is not null) return;
        Dispose(); device = nextDevice;
        try
        {
            var source = FloorplanShader.GenerateShaderSource(backend);
            vertex = device.CompileVertexShader(source, FloorplanShader.GetVertexEntry(backend.Name));
            fragment = device.CompileFragmentShader(source, FloorplanShader.GetFragmentEntry(backend.Name));
            texture = device.CreateSampledTexture(model.Width, model.Height, model.Mips);
            for (int i = 0; i < 4; i++)
                if (model.Layers[i].Length > 0) buffers[i] = device.CreateVertexBuffer(MemoryMarshal.AsBytes(model.Layers[i].AsSpan()), 32);
            commands = (IGpuResourceCommandList)device.CreateCommandList();
            pipeline = new() { VertexShader = vertex, FragmentShader = fragment, VertexComponents = [4, 4],
                PremultipliedAlpha = true, DepthTestEnabled = false, DepthWriteEnabled = false, CullMode = GpuCullMode.None };
        }
        catch { Dispose(); throw; }
    }

    public unsafe void RenderNative(IGpuRenderTarget target, Rect viewport, AffineTransform camera, FloorplanLayerTransition transition)
    {
        if (commands is null || pipeline is null || texture is null) throw new InvalidOperationException("Prepare GPU resources first.");
        commands.BeginRenderPass(target, Color4.White);
        commands.SetPipeline(pipeline);
        commands.SetTexture(0, texture, Sampler.LinearClamp);
        // Incoming below outgoing: first reveal the new layer, then remove the old top layer.
        if (transition.Active) Draw(transition.Incoming, transition.IncomingOpacity);
        Draw(transition.Current, transition.CurrentOpacity);
        commands.EndRenderPass(); commands.Commit();

        void Draw(int level, NFloat opacity)
        {
            if (opacity <= 0 || buffers[level] is not { } buffer) return;
            var bindings = Uniforms(viewport, camera, target.Width, target.Height, opacity);
            commands.SetVertexBuffer(buffer);
            commands.SetConstantBuffer(&bindings, sizeof(UniformData));
            commands.Draw(buffer.VertexCount);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UniformData { public Float4 Camera, Fade; }
    private static UniformData Uniforms(Rect viewport, AffineTransform camera, int pixelWidth, int pixelHeight, NFloat opacity) => new()
    {
        Camera = new((float)(2 * camera.A / viewport.Width), (float)(-2 * camera.D / viewport.Height),
            (float)(2 * (camera.Tx - viewport.X) / viewport.Width - 1), (float)(1 - 2 * (camera.Ty - viewport.Y) / viewport.Height)),
        Fade = new((float)opacity, (float)-NFloat.Log2(camera.A * pixelWidth / viewport.Width), 2f / pixelWidth, -2f / pixelHeight),
    };

    public unsafe void RenderSoftware(Framebuffer framebuffer, Rect viewport, AffineTransform camera, FloorplanLayerTransition transition)
    {
        framebuffer.ClearColor(0xFFFFFFFF);
        var context = new RenderContext(framebuffer) { Viewport = new Viewport(0, 0, framebuffer.Width, framebuffer.Height, yUp: true), DepthTestEnabled = false, BlendEnabled = true, PremultipliedAlpha = true, CullMode = CullMode.None };
        if (transition.Active) Draw(transition.Incoming, transition.IncomingOpacity);
        Draw(transition.Current, transition.CurrentOpacity);
        void Draw(int level, NFloat opacity)
        {
            if (opacity <= 0) return;
            var uniforms = Uniforms(viewport, camera, framebuffer.Width, framebuffer.Height, opacity);
            var bindings = new FloorplanShader.Bindings { Camera = uniforms.Camera, Fade = uniforms.Fade,
                Atlas = model.SoftwareTexture, Sampler = Sampler.LinearClamp };
            fixed (FloorplanShader.Vertex* data = model.Layers[level])
                context.Draw(new VertexSource<FloorplanShader.Vertex>(data, model.Layers[level].Length),
                    new FloorplanShader.VertexStage(), new FloorplanShader.FragmentStage(), bindings);
        }
    }

    public void Dispose()
    {
        commands?.Dispose(); commands = null;
        for (int i = 0; i < buffers.Length; i++) { buffers[i]?.Dispose(); buffers[i] = null; }
        texture?.Dispose(); texture = null;
        vertex?.Dispose(); vertex = null; fragment?.Dispose(); fragment = null;
        pipeline = null; device = null;
    }
}
