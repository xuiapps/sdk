using Xui.GPU.Hardware;
namespace Xui.Runtime.Android.GPU;
internal sealed class VulkanFragmentShader(VulkanDevice d,ulong h,string n) : VulkanShader(d,h,n), IGpuFragmentShader;
