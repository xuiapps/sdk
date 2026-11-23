using Xui.GPU.Shaders.Types;

namespace Xui.GPU.Hardware;

/// <summary>Optional device capability for sampled RGBA textures and immutable vertex buffers.</summary>
public interface IGpuResourceDevice : IGpuDevice
{
    /// <summary>Uploads premultiplied RGBA8 mip levels, largest first, with tightly packed rows.</summary>
    IGpuTexture CreateSampledTexture(int width, int height, IReadOnlyList<byte[]> mipLevels);
    /// <summary>Uploads immutable vertices once. The caller owns the returned buffer.</summary>
    IGpuVertexBuffer CreateVertexBuffer(ReadOnlySpan<byte> vertices, int stride);
}

/// <summary>An owned immutable vertex buffer.</summary>
public interface IGpuVertexBuffer : IDisposable
{
    /// <summary>Number of vertices in the buffer.</summary>
    int VertexCount { get; }
}

/// <summary>Resource bindings supported by devices implementing <see cref="IGpuResourceDevice"/>.</summary>
public interface IGpuResourceCommandList : IGpuCommandList
{
    /// <summary>Binds an immutable vertex buffer without uploading it again.</summary>
    void SetVertexBuffer(IGpuVertexBuffer buffer);
    /// <summary>Binds a sampled texture to a fragment texture and sampler slot; mip filtering is linear.</summary>
    void SetTexture(int slot, IGpuTexture texture, Sampler sampler);
}
