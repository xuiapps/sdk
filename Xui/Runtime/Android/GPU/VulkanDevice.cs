using System.Security.Cryptography;
using System.Text;
using Xui.GPU.Hardware;
namespace Xui.Runtime.Android.GPU;

/// <summary>Native Vulkan device. Shaders are compiled to SPIR-V by the Android build.</summary>
internal sealed class VulkanDevice : IGpuDevice
{
    internal nint Handle { get; private set; }
    private readonly nint assets;
    private readonly int owner = Environment.CurrentManagedThreadId;
    private bool disposed, attempted;
    private readonly HashSet<VulkanShader> shaders = [];
    internal void Released(VulkanShader shader) => shaders.Remove(shader);
    public string BackendName => "Vulkan";
    public bool IsDisposed => disposed;
    internal VulkanDevice(nint host) { assets=VulkanNative.Assets(host); }
    internal bool IsSupported { get { Verify(); if(!attempted){attempted=true;Handle=VulkanNative.Create();}return Handle!=0; } }
    internal void Verify()
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(Environment.CurrentManagedThreadId!=owner)throw new InvalidOperationException("Vulkan calls require the window UI thread.");
    }
    internal static string ShaderAsset(string source,string entry) =>
        "xui-shaders/"+Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(source)))+"."+entry+".spv";
    private ulong Load(string source,string entry)
    {
        Verify();if(!IsSupported)throw VulkanNative.Failure();
        var shader=VulkanNative.Shader(Handle,assets,ShaderAsset(source,entry));
        return shader!=0?shader:throw VulkanNative.Failure();
    }
    public IGpuVertexShader CompileVertexShader(string source,string entryPoint)
    {
        if(entryPoint!="VSMain")throw new NotSupportedException("Vulkan vertex entry must be VSMain.");
        var shader=new VulkanVertexShader(this,Load(source,entryPoint),entryPoint);shaders.Add(shader);return shader;
    }
    public IGpuFragmentShader CompileFragmentShader(string source,string entryPoint)
    {
        if(entryPoint!="PSMain")throw new NotSupportedException("Vulkan fragment entry must be PSMain.");
        var shader=new VulkanFragmentShader(this,Load(source,entryPoint),entryPoint);shaders.Add(shader);return shader;
    }
    public IGpuRenderTarget CreateRenderTarget(int width,int height) => throw new NotSupportedException("Android Vulkan currently supports native presentation surfaces only; no CPU readback target.");
    public IGpuCommandList CreateCommandList() { Verify();return new VulkanCommandList(this); }
    public void Dispose()
    {
        if(disposed)return;Verify();foreach(var shader in shaders.ToArray())shader.Dispose();shaders.Clear();
        if(Handle!=0)VulkanNative.Destroy(Handle);Handle=0;disposed=true;
    }
}
