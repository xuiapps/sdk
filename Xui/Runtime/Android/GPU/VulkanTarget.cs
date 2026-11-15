using Xui.GPU.Hardware;
namespace Xui.Runtime.Android.GPU;

/// <summary>Borrowed only during the content callback. The surface owns its native swapchain.</summary>
internal sealed class VulkanTarget(VulkanDevice device) : IGpuRenderTarget
{
    internal VulkanDevice Device { get; } = device;
    internal nint Handle { get; set; }
    internal bool Borrowed { get; set; }
    internal bool Presented { get; set; }
    public int Width { get; internal set; }
    public int Height { get; internal set; }
    public IGpuTexture Texture => throw new NotSupportedException("Presentation images are owned by the swapchain.");
    public void ReadbackPixelsBgra(byte[] destination) => throw new NotSupportedException("Native Vulkan presentation has no CPU readback.");
    public void Dispose() => throw new InvalidOperationException("The presentation target is borrowed.");
}
