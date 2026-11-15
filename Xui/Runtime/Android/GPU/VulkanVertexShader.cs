using Xui.GPU.Hardware;
namespace Xui.Runtime.Android.GPU;
internal sealed class VulkanVertexShader(VulkanDevice d,ulong h,string n) : VulkanShader(d,h,n), IGpuVertexShader;
