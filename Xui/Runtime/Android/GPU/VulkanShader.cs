using Xui.GPU.Hardware;
namespace Xui.Runtime.Android.GPU;

internal abstract class VulkanShader(VulkanDevice device, ulong handle, string name) : IGpuShader
{
    internal VulkanDevice Device { get; } = device;
    internal ulong Handle { get; private set; } = handle;
    public string Name { get; } = name;
    public void Dispose()
    {
        if(Handle==0)return;
        if(!Device.IsDisposed)VulkanNative.DestroyShader(Device.Handle,Handle);
        Handle=0;Device.Released(this);
    }
}
