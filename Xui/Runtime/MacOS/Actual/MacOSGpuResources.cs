using System;
using System.Collections.Generic;
using Xui.GPU.Shaders.Types;
using NativeMetal = global::Xui.Runtime.MacOS.Metal;

namespace Xui.GPU.Hardware.Metal;

public sealed partial class MacOSGpuDevice
{
    /// <inheritdoc/>
    public unsafe IGpuTexture CreateSampledTexture(int width, int height, IReadOnlyList<byte[]> mipLevels)
    {
        using var pool = new NativeMetal.AutoreleaseScope();
        ObjectDisposedException.ThrowIf(_disposed, this);
        GpuTextureValidation.Validate(width, height, mipLevels);
        var descriptor = NativeMetal.CreateSampledTextureDescriptor(width, height, mipLevels.Count);
        nint texture;
        try { texture = NativeMetal.Device_NewTexture(_device, descriptor); }
        finally { NativeMetal.Release(descriptor); }
        if (texture == 0) throw new InvalidOperationException("Failed to allocate sampled texture.");
        try
        {
            for (int level = 0; level < mipLevels.Count; level++)
            {
                int w = Math.Max(1, width >> level), h = Math.Max(1, height >> level);
                fixed (byte* data = mipLevels[level]) NativeMetal.Texture_Upload(texture, (nint)data, w, h, level);
            }
            return new MacOSSampledTexture(_device, texture, width, height);
        }
        catch { NativeMetal.Release(texture); throw; }
    }

    /// <inheritdoc/>
    public unsafe IGpuVertexBuffer CreateVertexBuffer(ReadOnlySpan<byte> vertices, int stride)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (stride <= 0 || vertices.IsEmpty || vertices.Length % stride != 0) throw new ArgumentException("Invalid vertex buffer.");
        fixed (byte* data = vertices)
        {
            var buffer = NativeMetal.Device_NewBufferWithBytes(_device, (nint)data, (nuint)vertices.Length, 0);
            if (buffer == 0) throw new InvalidOperationException("Failed to allocate vertex buffer.");
            return new MacOSVertexBuffer(_device, buffer, vertices.Length / stride);
        }
    }
}

internal sealed class MacOSSampledTexture(nint device, nint handle, int width, int height) : IGpuTexture
{
    internal readonly nint Device = device;
    internal nint Handle = handle;
    public int Width => width;
    public int Height => height;
    public void Dispose() { if (Handle != 0) NativeMetal.Release(Handle); Handle = 0; }
}

internal sealed class MacOSVertexBuffer(nint device, nint handle, int count) : IGpuVertexBuffer
{
    internal readonly nint Device = device;
    internal nint Handle = handle;
    public int VertexCount => count;
    public void Dispose() { if (Handle != 0) NativeMetal.Release(Handle); Handle = 0; }
}

internal sealed unsafe partial class MacOSCommandList
{
    private readonly Dictionary<int, nint> _samplers = new();
    private bool _blend;
    private int[]? _components;

    public void SetVertexBuffer(IGpuVertexBuffer buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (buffer is not MacOSVertexBuffer native || native.Device != _device || native.Handle == 0)
            throw new ArgumentException("Vertex buffer must be live and owned by this device.", nameof(buffer));
        NativeMetal.Encoder_SetVertexBuffer(_encoder, native.Handle, 0, 0);
    }

    public void SetTexture(int slot, IGpuTexture texture, Sampler sampler)
    {
        using var pool = new NativeMetal.AutoreleaseScope();
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (slot < 0 || slot > 15) throw new ArgumentOutOfRangeException(nameof(slot));
        if (texture is not MacOSSampledTexture native || native.Device != _device || native.Handle == 0)
            throw new ArgumentException("Texture must be live and owned by this device.", nameof(texture));
        int key = (int)sampler.MinFilter | ((int)sampler.MagFilter << 2) | ((int)sampler.AddressU << 4) | ((int)sampler.AddressV << 8);
        if (!_samplers.TryGetValue(key, out var state))
        {
            state = NativeMetal.CreateSampler(_device, sampler);
            if (state == 0) throw new InvalidOperationException("Failed to allocate sampler.");
            _samplers.Add(key, state);
        }
        NativeMetal.Encoder_SetTexture(_encoder, native.Handle, state, (nuint)slot);
    }
}
