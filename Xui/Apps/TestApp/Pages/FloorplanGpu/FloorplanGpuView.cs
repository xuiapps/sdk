using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.GPU;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.GPU.Backends;
using Xui.GPU.Hardware;
using Xui.GPU.Software;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.FloorplanGpu;

public sealed class FloorplanGpuView : FloorplanCameraView, IGpuSurfaceContent
{
    public FloorplanGpuModel Model { get; }
    public FloorplanLayerTransition Transition { get; } = new();
    private readonly FloorplanGpuRenderer renderer;
    private readonly object clipKey = new(), labelsKey = new();
    private IGpuResourceDevice? device;
    private IShaderBackend? backend;
    private IImage? image;
    private IGpuRenderTarget? readbackTarget;
    private byte[]? pixels;
    private bool hasFrame;

    public FloorplanGpuView(FloorplanGpuModel model) : this(model, null) { }
    internal FloorplanGpuView(FloorplanGpuModel model, FloorplanGpuRenderer? preparedRenderer)
    { Model = model; renderer = preparedRenderer ?? new(model); Id = "FloorplanGpu.Map"; }

    protected override void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
    {
        base.AnimateCore(previousTime, currentTime);
        if (IsNavigating) return;
        Transition.Request(DetailLevel);
        if (Transition.Advance((NFloat)(currentTime - previousTime).TotalSeconds)) InvalidateRender();
        if (Transition.Active) RequestAnimationFrame();
    }

    protected override void RenderCore(IContext context)
    {
        if (!HasCamera) return;
        if (!IsNavigating) Transition.Request(DetailLevel);
        if (Transition.Active && !IsNavigating) RequestAnimationFrame();
        device ??= GetService(typeof(IGpuDevice)) as IGpuResourceDevice;
        backend ??= GetService(typeof(IShaderBackend)) as IShaderBackend;
        using var clip = context.Group(new GroupOptions { Key = clipKey, Bounds = Frame, ViewBacking = true, ClipToBounds = true });
        if (!clip.NeedsRedraw) return;
        if (device is not null && backend is not null && context.GpuSurface(new GroupOptions
            { Key = this, Bounds = Frame, HasUpdates = !hasFrame || !IsNavigating }))
        {
            using var labels = context.Group(new GroupOptions { Key = labelsKey, Bounds = Frame, ViewBacking = true, HasUpdates = !IsNavigating });
            if (labels.NeedsRedraw) DrawLabels(context);
        }
        else
        {
            RenderPortable();
            if (image is not null) context.DrawImage(image, Frame);
            DrawLabels(context);
        }
    }

    void IGpuSurfaceContent.Render(IGpuRenderTarget target)
    {
        renderer.Prepare(device!, backend!);
        renderer.RenderNative(target, Frame, CameraTransform, Transition);
        hasFrame = true;
    }

    private unsafe void RenderPortable()
    {
        if (hasFrame && IsNavigating) return;
        int width = Math.Max(1, (int)NFloat.Ceiling(Frame.Width));
        int height = Math.Max(1, (int)NFloat.Ceiling(Frame.Height));
        if (pixels?.Length != width * height * 4) pixels = new byte[width * height * 4];
        if (device is not null && backend is not null)
        {
            renderer.Prepare(device, backend);
            if (readbackTarget is null || readbackTarget.Width != width || readbackTarget.Height != height)
            { readbackTarget?.Dispose(); readbackTarget = device.CreateRenderTarget(width, height); }
            renderer.RenderNative(readbackTarget, Frame, CameraTransform, Transition);
            readbackTarget.ReadbackPixelsBgra(pixels);
        }
        else
        {
            using var framebuffer = new Framebuffer(width, height, withDepthBuffer: false);
            renderer.RenderSoftware(framebuffer, Frame, CameraTransform, Transition);
            for (int i = 0; i < width * height; i++)
            {
                uint p = framebuffer.ColorData[i];
                pixels[i * 4] = (byte)(p >> 8); pixels[i * 4 + 1] = (byte)(p >> 16);
                pixels[i * 4 + 2] = (byte)(p >> 24); pixels[i * 4 + 3] = (byte)p;
            }
        }
        image ??= GetService(typeof(IImage)) as IImage;
        image?.LoadPixels(width, height, pixels);
        hasFrame = true;
    }

    private void DrawLabels(IContext context)
    {
        context.Save(); context.Transform(CameraTransform);
        if (Transition.Active) Layer(Transition.Incoming, Transition.IncomingOpacity);
        Layer(Transition.Current, Transition.CurrentOpacity);
        context.Restore();
        void Layer(int level, NFloat opacity)
        {
            if (opacity <= 0) return;
            var visible = VisibleWorld;
            foreach (var label in Model.Labels[level])
            {
                var bounds = label.Bounds.Expand(8);
                if (bounds.Right < visible.X || bounds.X > visible.Right || bounds.Bottom < visible.Y || bounds.Y > visible.Bottom) continue;
                context.Save(); context.Transform(label.Transform); context.GlobalAlpha = opacity;
                context.SetFill(label.Color); context.SetFont(label.Font); context.TextAlign = label.Align;
                context.TextBaseline = TextBaseline.Alphabetic; context.FillText(label.Text, label.Position);
                context.Restore();
            }
        }
    }

    protected override void OnDeactivate()
    {
        renderer.Dispose(); readbackTarget?.Dispose(); readbackTarget = null;
        device = null; backend = null; hasFrame = false;
        if (image is IDisposable disposable) disposable.Dispose(); image = null; pixels = null;
        base.OnDeactivate();
    }
}
