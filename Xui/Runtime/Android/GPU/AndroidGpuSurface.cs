using Xui.Core.Canvas.Composition.GPU;
namespace Xui.Runtime.Android.GPU;

internal sealed class AndroidGpuSurface(VulkanDevice device,Action invalidate) : ICompositionGpuSurface
{
    private readonly VulkanTarget target = new(device);
    private IGpuSurfaceContent? content;
    private bool pending,disposed;
    public bool HasContent { get; private set; }
    public void RequestRender(IGpuSurfaceContent value) { content=value;pending=true; }
    // A retained TextureView can lose its displayed buffer while the Activity is
    // stopped without a SurfaceTexture destruction/creation callback.
    internal void Resume() { if(!disposed) {pending=true;HasContent=false;} }
    internal void Bind(nint window,int width,int height)
    {
        if(disposed)return;
        // TextureView notifies only on creation, size change, and destruction.
        // A new surface invalidates its previous swapchain, never a managed image.
        if(target.Handle!=0)VulkanNative.DestroyTarget(target.Handle);
        target.Handle=0;HasContent=false;pending=true;
        target.Width=width;target.Height=height;
        if(window!=0)
        {
            target.Handle=VulkanNative.Target(device.Handle,window);
            if(target.Handle==0)throw VulkanNative.Failure();
            invalidate();
        }
    }
    internal void Present()
    {
        if(!pending||content is null||target.Handle==0)return;
        target.Borrowed=true;target.Presented=false;
        try
        {
            content.Render(target);
            HasContent=target.Presented;pending=!HasContent;
            if(pending)invalidate();
        }
        finally {target.Borrowed=false;}
    }
    public void Dispose()
    {
        if(disposed)return;disposed=true;
        if(target.Handle!=0)VulkanNative.DestroyTarget(target.Handle);target.Handle=0;content=null;
    }
}
