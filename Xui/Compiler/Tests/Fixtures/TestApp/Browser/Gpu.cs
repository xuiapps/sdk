using System;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;

namespace TestAppCanvas;

/// <summary>Copies borrowed managed uploads into a bounded linear scratch frame before the host consumes them.</summary>
public sealed class BrowserGpuCommandList(int handle) : IGpuCommandList
{
    public unsafe void SetVertexBuffer<T>(ReadOnlySpan<T> vertices) where T : unmanaged
    {
        // GPU uploads consume the bytes synchronously. No pointer escapes this frame.
        T* data = stackalloc T[vertices.Length];
        vertices.CopyTo(new Span<T>(data, vertices.Length));
        SetVertexBuffer(data, new GpuVertexBufferDesc { Stride = sizeof(T), VertexCount = vertices.Length });
    }
    public unsafe void SetConstantBuffer<T>(in T value) where T : unmanaged
    {
        T* data = stackalloc T[1];
        new Span<T>(data, 1)[0] = value;
        SetConstantBuffer(data, sizeof(T));
    }
    public unsafe void SetVertexBuffer(void* data, GpuVertexBufferDesc desc)
    {
        if (desc.Stride<=0 || desc.VertexCount<0 || desc.VertexCount>int.MaxValue/desc.Stride)
            throw new ArgumentOutOfRangeException(nameof(desc));
        NativeGpu.Invoke("Vertices", handle, (int)data, desc.Stride * desc.VertexCount, desc.Stride, desc.VertexCount);
    }
    public unsafe void SetConstantBuffer(void* data, int sizeInBytes) => NativeGpu.Invoke("Constants", handle, (int)data, sizeInBytes);
    public void BeginRenderPass(IGpuRenderTarget target, Color4 clearColor)
    {
        NativeGpu.Invoke("Begin", handle, ((BrowserGpuTarget)target).Handle);
        NativeGpu.Invoke("Clear", handle, BitConverter.SingleToInt32Bits(clearColor.R), BitConverter.SingleToInt32Bits(clearColor.G),
            BitConverter.SingleToInt32Bits(clearColor.B), BitConverter.SingleToInt32Bits(clearColor.A));
    }
    public void SetPipeline(GpuPipelineDesc pipeline)
    {
        NativeGpu.Invoke("Pipeline", handle, ((BrowserVertexShader)pipeline.VertexShader!).Handle, ((BrowserFragmentShader)pipeline.FragmentShader!).Handle,
            (pipeline.DepthTestEnabled ? 1 : 0) | (pipeline.DepthWriteEnabled ? 2 : 0) | (pipeline.PremultipliedAlpha ? 4 : 0), (int)pipeline.CullMode);
        if (pipeline.VertexComponents is { } components)
            for (int i=0;i<components.Length;i++) NativeGpu.Invoke("Attribute",handle,i,components[i]);
        else { NativeGpu.Invoke("Attribute",handle,0,3);NativeGpu.Invoke("Attribute",handle,1,4); }
    }
    public void Draw(int vertexCount) => NativeGpu.Invoke("Draw", handle, vertexCount);
    public void EndRenderPass() => NativeGpu.Invoke("End", handle);
    public void Commit() => NativeGpu.Invoke("Commit", handle);
    public void Dispose() => NativeGpu.Invoke("Dispose", handle);
}

public static class NativeGpu
{
    public static int Invoke(string operation, int handle, int a = 0, int b = 0, int c = 0, int d = 0, string text = "") =>
        throw new InvalidOperationException("Browser GPU import required.");
}


public sealed class BrowserGpuDevice : IGpuDevice
{
    public readonly int Handle = NativeGpu.Invoke("Device",0);
    public string BackendName => "WebGL 2";
    public bool IsDisposed { get; private set; }
    public IGpuVertexShader CompileVertexShader(string source,string entryPoint) =>
        new BrowserVertexShader(NativeGpu.Invoke("VertexShader",Handle,text:source));
    public IGpuFragmentShader CompileFragmentShader(string source,string entryPoint) =>
        new BrowserFragmentShader(NativeGpu.Invoke("FragmentShader",Handle,text:source));
    public IGpuRenderTarget CreateRenderTarget(int width,int height) => new BrowserGpuTarget(this,width,height);
    public IGpuCommandList CreateCommandList() => new BrowserGpuCommandList(NativeGpu.Invoke("Commands",Handle));
    public void Dispose() { if(IsDisposed)return;IsDisposed=true;NativeGpu.Invoke("Dispose",Handle); }
}
public sealed class BrowserGpuTarget : IGpuRenderTarget
{
    public int Handle { get; private set; }
    public int Width { get; }
    public int Height { get; }
    public BrowserGpuTarget(BrowserGpuDevice device,int width,int height)
    { Width=width;Height=height;Handle=NativeGpu.Invoke("Target",device.Handle,width,height); }
    public IGpuTexture Texture => throw new NotSupportedException("Browser presentation targets do not expose textures.");
    public void ReadbackPixelsBgra(byte[] destination) => throw new NotSupportedException("GPU readback is not connected yet.");
    public void Dispose() { if(Handle==0)return;NativeGpu.Invoke("Dispose",Handle);Handle=0; }
}
public sealed class BrowserVertexShader(int handle) : IGpuVertexShader
{
    public int Handle { get; private set; } = handle;
    public string Name => "main";
    public void Dispose() { if(Handle==0)return;NativeGpu.Invoke("Dispose",Handle);Handle=0; }
}
public sealed class BrowserFragmentShader(int handle) : IGpuFragmentShader
{
    public int Handle { get; private set; } = handle;
    public string Name => "main";
    public void Dispose() { if(Handle==0)return;NativeGpu.Invoke("Dispose",Handle);Handle=0; }
}
public sealed class BrowserShaderBackend : Xui.GPU.Backends.IShaderBackend
{
    public string Name => "GLSL";
    public string GenerateCode(Xui.GPU.IR.IrShaderModule module) => throw new NotSupportedException("Shaders are generated at build time.");
}
public sealed class BrowserGpuServices : IServiceProvider, IDisposable
{
    public readonly BrowserGpuDevice Device = new();
    private readonly BrowserShaderBackend backend = new();
    public object GetService(Type type) => type==typeof(IGpuDevice) ? Device : type==typeof(Xui.GPU.Backends.IShaderBackend) ? backend : null;
    public void Dispose() => Device.Dispose();
}
