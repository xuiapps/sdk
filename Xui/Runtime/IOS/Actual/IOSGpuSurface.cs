using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Composition.GPU;
using Xui.Core.Math2D;
using Xui.GPU.Hardware;
using Xui.GPU.Hardware.Metal;
using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS.Actual;

/// <summary>A window-scheduled MTKView. Drawables never cross the native draw callback.</summary>
internal sealed class IOSGpuSurface : MetalKit.MTKView, ICompositionGpuSurface
{
    private readonly Action invalidate;
    private readonly IOSDrawableTarget target = new();
    private IGpuSurfaceContent? content;
    private ExceptionDispatchInfo? failure;
    private bool pending;
    private int width, height;
    public bool HasContent { get; private set; }

    internal IOSGpuSurface(IOSGpuDevice device, Action invalidate) : base(device.NativeDevice) => this.invalidate = invalidate;

    public new void Dispose()
    {
        if ((nint)this == 0) return;
        ObjC.objc_msgSend(this, new Sel("removeFromSuperview"));
        content=null; pending=false;
        base.Dispose();
    }

    public void RequestRender(IGpuSurfaceContent content) { this.content = content; pending = true; }

    internal void Present(Rect bounds, NFloat scale)
    {
        // The containing composition body already supplies the group's coordinate system.
        Frame = bounds;
        int w = Math.Max(1, checked((int)NFloat.Ceiling(bounds.Width * scale)));
        int h = Math.Max(1, checked((int)NFloat.Ceiling(bounds.Height * scale)));
        if (width != w || height != h)
        {
            width = w; height = h;
            SetDrawableSize((w, h));
            pending = true; HasContent = false;
        }
        if (!pending || content is null || bounds.Width <= 0 || bounds.Height <= 0) return;
        Draw(); // MTKView acquires/releases its drawable around DrawFrame, with no independent timer.
        var error = failure; failure = null;
        error?.Throw();
    }

    protected override void DrawFrame()
    {
        if (!pending || content is null) return;
        try
        {
            var drawable = CurrentDrawable;
            var depth = DepthTexture;
            if (drawable == 0 || depth == 0) { invalidate(); return; }
            target.Bind(drawable, depth, width, height);
            content.Render(target);
            if (!target.Presented) throw new InvalidOperationException("GPU surface content must commit its render commands.");
            HasContent = true; pending = false;
        }
        catch (Exception error) { failure = ExceptionDispatchInfo.Capture(error); }
        finally { target.Unbind(); }
    }

    /// <summary>Borrowed only inside DrawFrame; the Metal command buffer retains submitted resources.</summary>
    private sealed class IOSDrawableTarget : IGpuRenderTarget, IIOSRenderTarget
    {
        private static readonly Sel TextureSel = new("texture");
        public int Width { get; private set; }
        public int Height { get; private set; }
        public nint NativeTexture { get; private set; }
        public nint NativeDepthTexture { get; private set; }
        public nint Drawable { get; private set; }
        public bool Presented { get; private set; }
        public IGpuTexture Texture => throw new NotSupportedException("Presentation textures are framebuffer-only.");
        public void Bind(nint drawable, nint depth, int width, int height)
        {
            Drawable=drawable; NativeDepthTexture=depth; Width=width; Height=height; Presented=false;
            NativeTexture=objc_msgSend_retIntPtr(drawable, TextureSel);
        }
        public void Unbind() { Drawable=NativeTexture=NativeDepthTexture=0; }
        public void MarkPresented() => Presented=true;
        public void ReadbackPixelsBgra(byte[] destination) => throw new NotSupportedException("Native presentation has no CPU readback. Use an offscreen target for capture.");
        public void Dispose() => throw new InvalidOperationException("The presentation target is borrowed from MTKView.");
    }
}
