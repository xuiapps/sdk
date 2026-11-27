using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;

namespace Xui.Runtime.Browser.Actual;

internal static partial class BrowserGpuInterop
{
    // Keep these values synchronized with webgl.js.
    internal enum Op { Device, Dispose, VertexShader, FragmentShader, Commands, Target, Begin, Clear,
        Pipeline, Attribute, Draw, End, Commit, Buffer, Texture, BindBuffer, BindTexture }
    internal enum UploadOp { Vertices, Constants, ImmutableVertices, Mip, Readback }
    [JSImport("gpuInvoke", "xui-browser")]
    internal static partial int Invoke(int op, int handle, int a = 0, int b = 0, int c = 0, int d = 0, string text = "");
    internal static int Call(Op op, int handle, int a = 0, int b = 0, int c = 0, int d = 0, string text = "") => Invoke((int)op, handle, a, b, c, d, text);
    [JSImport("gpuUpload", "xui-browser")]
    internal static partial void Upload(int op, int handle, [JSMarshalAs<JSType.MemoryView>] Span<byte> data, int a = 0, int b = 0);
    internal static Span<byte> Borrow(ReadOnlySpan<byte> data) => MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(data), data.Length);
}

/// <summary>WebGL 2 hardware device using the shared GLSL shader backend.</summary>
public sealed class BrowserGpuDevice : IGpuResourceDevice
{
    internal int Handle { get; private set; }
    public string BackendName => "WebGL 2";
    public bool IsDisposed => Handle == 0;
    private BrowserGpuDevice(int handle) => Handle = handle;
    internal static BrowserGpuDevice? TryCreate()
    {
        var handle = BrowserGpuInterop.Call(BrowserGpuInterop.Op.Device, 0);
        return handle == 0 ? null : new(handle);
    }
    private int Create(BrowserGpuInterop.Op op, int a = 0, int b = 0, int c = 0, string text = "")
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        return BrowserGpuInterop.Call(op, Handle, a, b, c, text: text);
    }
    public IGpuVertexShader CompileVertexShader(string source, string entryPoint)
    {
        if (entryPoint != "main") throw new NotSupportedException("GLSL entry point must be main.");
        return new BrowserVertexShader(this, Create(BrowserGpuInterop.Op.VertexShader, text: source));
    }
    public IGpuFragmentShader CompileFragmentShader(string source, string entryPoint)
    {
        if (entryPoint != "main") throw new NotSupportedException("GLSL entry point must be main.");
        return new BrowserFragmentShader(this, Create(BrowserGpuInterop.Op.FragmentShader, text: source));
    }
    public IGpuRenderTarget CreateRenderTarget(int width, int height) =>
        new BrowserGpuTarget(this, Create(BrowserGpuInterop.Op.Target, width, height), width, height);
    public IGpuCommandList CreateCommandList() => new BrowserGpuCommandList(this, Create(BrowserGpuInterop.Op.Commands));
    public IGpuVertexBuffer CreateVertexBuffer(ReadOnlySpan<byte> vertices, int stride)
    {
        if (stride <= 0 || vertices.Length % stride != 0) throw new ArgumentException("Vertex data must contain whole vertices.");
        var handle = Create(BrowserGpuInterop.Op.Buffer, stride, vertices.Length / stride);
        try { BrowserGpuInterop.Upload((int)BrowserGpuInterop.UploadOp.ImmutableVertices, handle, BrowserGpuInterop.Borrow(vertices)); }
        catch { BrowserGpuInterop.Call(BrowserGpuInterop.Op.Dispose, handle); throw; }
        return new BrowserGpuVertexBuffer(this, handle, vertices.Length / stride);
    }
    public IGpuTexture CreateSampledTexture(int width, int height, IReadOnlyList<byte[]> mipLevels)
    {
        GpuTextureValidation.Validate(width, height, mipLevels);
        var handle = Create(BrowserGpuInterop.Op.Texture, width, height, mipLevels.Count);
        try
        {
            for (var level = 0; level < mipLevels.Count; level++)
                BrowserGpuInterop.Upload((int)BrowserGpuInterop.UploadOp.Mip, handle, mipLevels[level], level);
        }
        catch { BrowserGpuInterop.Call(BrowserGpuInterop.Op.Dispose, handle); throw; }
        return new BrowserGpuTexture(this, handle, width, height);
    }
    public void Dispose()
    {
        if (IsDisposed) return;
        BrowserGpuInterop.Call(BrowserGpuInterop.Op.Dispose, Handle);
        Handle = 0;
    }
}

internal abstract class BrowserGpuResource(BrowserGpuDevice device, int handle) : IDisposable
{
    internal BrowserGpuDevice Device { get; } = device;
    internal int Handle { get; private set; } = handle;
    internal int Require(BrowserGpuDevice device)
    {
        ObjectDisposedException.ThrowIf(Handle == 0 || Device.IsDisposed, this);
        if (!ReferenceEquals(device, Device)) throw new ArgumentException("GPU resource belongs to another device.");
        return Handle;
    }
    public void Dispose()
    {
        if (Handle == 0) return;
        if (!Device.IsDisposed) BrowserGpuInterop.Call(BrowserGpuInterop.Op.Dispose, Handle);
        Handle = 0;
    }
}
internal sealed class BrowserVertexShader(BrowserGpuDevice device, int handle) : BrowserGpuResource(device, handle), IGpuVertexShader
{ public string Name => "main"; }
internal sealed class BrowserFragmentShader(BrowserGpuDevice device, int handle) : BrowserGpuResource(device, handle), IGpuFragmentShader
{ public string Name => "main"; }
internal sealed class BrowserGpuVertexBuffer(BrowserGpuDevice device, int handle, int count) : BrowserGpuResource(device, handle), IGpuVertexBuffer
{ public int VertexCount { get; } = count; }
internal sealed class BrowserGpuTexture(BrowserGpuDevice device, int handle, int width, int height) : BrowserGpuResource(device, handle), IGpuTexture
{ public int Width { get; } = width; public int Height { get; } = height; }
internal sealed class BrowserGpuTarget(BrowserGpuDevice device, int handle, int width, int height) : BrowserGpuResource(device, handle), IGpuRenderTarget
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    public IGpuTexture Texture => throw new NotSupportedException("Browser composition targets are not exposed as sampled textures.");
    public void ReadbackPixelsBgra(byte[] destination)
    {
        if (destination.Length != checked(Width * Height * 4)) throw new ArgumentException("Readback requires width * height * 4 bytes.");
        BrowserGpuInterop.Upload((int)BrowserGpuInterop.UploadOp.Readback, Require(Device), destination);
    }
}
internal sealed class BrowserGpuCommandList(BrowserGpuDevice device, int handle) : BrowserGpuResource(device, handle), IGpuResourceCommandList
{
    private int Call(BrowserGpuInterop.Op op, int a = 0, int b = 0, int c = 0, int d = 0) =>
        BrowserGpuInterop.Call(op, Require(Device), a, b, c, d);
    private int Resource(object resource) => resource is BrowserGpuResource r ? r.Require(Device) : throw new ArgumentException("GPU resource belongs to another runtime.");
    public void BeginRenderPass(IGpuRenderTarget target, Color4 clearColor)
    {
        Call(BrowserGpuInterop.Op.Begin, Resource(target));
        Call(BrowserGpuInterop.Op.Clear, BitConverter.SingleToInt32Bits(clearColor.R), BitConverter.SingleToInt32Bits(clearColor.G),
            BitConverter.SingleToInt32Bits(clearColor.B), BitConverter.SingleToInt32Bits(clearColor.A));
    }
    public void SetPipeline(GpuPipelineDesc pipeline)
    {
        Call(BrowserGpuInterop.Op.Pipeline, Resource(pipeline.VertexShader!), Resource(pipeline.FragmentShader!),
            (pipeline.DepthTestEnabled ? 1 : 0) | (pipeline.DepthWriteEnabled ? 2 : 0) | (pipeline.PremultipliedAlpha ? 4 : 0), (int)pipeline.CullMode);
        if (pipeline.VertexComponents is { } components)
            for (var i = 0; i < components.Length; i++) Call(BrowserGpuInterop.Op.Attribute, i, components[i]);
        else { Call(BrowserGpuInterop.Op.Attribute, 0, 3); Call(BrowserGpuInterop.Op.Attribute, 1, 4); }
    }
    public unsafe void SetVertexBuffer(void* data, GpuVertexBufferDesc desc)
    {
        if (desc.Stride <= 0 || desc.VertexCount < 0) throw new ArgumentOutOfRangeException(nameof(desc));
        BrowserGpuInterop.Upload((int)BrowserGpuInterop.UploadOp.Vertices, Require(Device), new Span<byte>(data, checked(desc.Stride * desc.VertexCount)), desc.Stride, desc.VertexCount);
    }
    public unsafe void SetConstantBuffer(void* data, int sizeInBytes) =>
        BrowserGpuInterop.Upload((int)BrowserGpuInterop.UploadOp.Constants, Require(Device), new Span<byte>(data, sizeInBytes));
    public void SetVertexBuffer(IGpuVertexBuffer buffer) => Call(BrowserGpuInterop.Op.BindBuffer, Resource(buffer));
    public void SetTexture(int slot, IGpuTexture texture, Sampler sampler)
    {
        if (sampler.AddressU == AddressMode.ClampToBorder || sampler.AddressV == AddressMode.ClampToBorder)
            throw new NotSupportedException("WebGL 2 does not support border-color sampling.");
        if (!Enum.IsDefined(sampler.MinFilter) || !Enum.IsDefined(sampler.MagFilter) || !Enum.IsDefined(sampler.AddressU) || !Enum.IsDefined(sampler.AddressV))
            throw new ArgumentException("Unsupported sampler.", nameof(sampler));
        Call(BrowserGpuInterop.Op.BindTexture, slot, Resource(texture), (int)sampler.MinFilter | (int)sampler.MagFilter << 4,
            (int)sampler.AddressU | (int)sampler.AddressV << 4);
    }
    public void Draw(int vertexCount) => Call(BrowserGpuInterop.Op.Draw, vertexCount);
    public void EndRenderPass() => Call(BrowserGpuInterop.Op.End);
    public void Commit() => Call(BrowserGpuInterop.Op.Commit);
}
