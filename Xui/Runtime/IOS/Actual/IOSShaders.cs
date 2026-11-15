using System;
using NativeMetal = global::Xui.Runtime.IOS.Metal;

namespace Xui.GPU.Hardware.Metal;

/// <summary>
/// Metal compiled vertex shader.
/// </summary>
internal sealed class IOSVertexShader : IGpuVertexShader
{
    private readonly nint _library;  // id<MTLLibrary>
    private readonly nint _function; // id<MTLFunction>
    private bool _disposed;

    public string Name { get; }
    internal nint Function => _function;

    internal IOSVertexShader(string name, nint library, string functionName)
    {
        Name = name;
        _library = library;
        _function = NativeMetal.Library_NewFunctionWithName(library, functionName);

        if (_function == 0)
            throw new InvalidOperationException(
                $"Could not find vertex function '{functionName}' in compiled Metal library.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMetal.Release(_function);
        NativeMetal.Release(_library);
    }
}

/// <summary>
/// Metal compiled fragment shader.
/// </summary>
internal sealed class IOSFragmentShader : IGpuFragmentShader
{
    private readonly nint _library;  // id<MTLLibrary>
    private readonly nint _function; // id<MTLFunction>
    private bool _disposed;

    public string Name { get; }
    internal nint Function => _function;

    internal IOSFragmentShader(string name, nint library, string functionName)
    {
        Name = name;
        _library = library;
        _function = NativeMetal.Library_NewFunctionWithName(library, functionName);

        if (_function == 0)
            throw new InvalidOperationException(
                $"Could not find fragment function '{functionName}' in compiled Metal library.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMetal.Release(_function);
        NativeMetal.Release(_library);
    }
}

/// <summary>
/// Metal render target backed by a texture.
/// </summary>
internal interface IIOSRenderTarget
{
    nint NativeTexture { get; }
    nint NativeDepthTexture { get; }
    nint Drawable { get; }
    void MarkPresented();
}

internal sealed class IOSRenderTarget : IGpuRenderTarget, IIOSRenderTarget
{
    private readonly nint _texture;       // id<MTLTexture> — color
    private readonly nint _depthTexture;  // id<MTLTexture> — depth (Depth32Float)
    private readonly nint _device;        // id<MTLDevice>
    private bool _disposed;

    public int Width { get; }
    public int Height { get; }
    public IGpuTexture Texture => new IOSTexture(_texture, Width, Height);
    public nint NativeTexture => _texture;
    public nint NativeDepthTexture => _depthTexture;

    public nint Drawable => 0;
    public void MarkPresented() { }

    internal IOSRenderTarget(int width, int height, nint device)
    {
        Width = width;
        Height = height;
        _device = device;

        var desc = NativeMetal.CreateTexture2DDescriptor(width, height);
        _texture = NativeMetal.Device_NewTexture(device, desc);
        NativeMetal.Release(desc);

        if (_texture == 0)
            throw new InvalidOperationException("Failed to create Metal render target texture.");

        _depthTexture = NativeMetal.CreateDepthTexture(device, width, height);
        if (_depthTexture == 0)
            throw new InvalidOperationException("Failed to create Metal depth texture.");
    }

    /// <inheritdoc/>
    public unsafe void ReadbackPixelsBgra(byte[] destination)
    {
        if (destination == null || destination.Length < Width * Height * 4)
            throw new ArgumentException($"Destination must be at least {Width * Height * 4} bytes.", nameof(destination));

        // Read pixels from the texture into the CPU buffer
        fixed (byte* pDst = destination)
        {
            var region = new NativeMetal.MTLRegion
            {
                OriginX = 0, OriginY = 0, OriginZ = 0,
                SizeX = (ulong)Width, SizeY = (ulong)Height, SizeZ = 1
            };
            NativeMetal.Texture_GetBytes(
                _texture,
                (nint)pDst,
                (nuint)(Width * 4),  // bytes per row
                region,
                0);  // mipmap level 0
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMetal.Release(_texture);
        NativeMetal.Release(_depthTexture);
    }
}

/// <summary>
/// Thin wrapper implementing IGpuTexture for Metal textures.
/// </summary>
internal sealed class IOSTexture : IGpuTexture
{
    public int Width { get; }
    public int Height { get; }

    internal IOSTexture(nint texture, int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Dispose()
    {
        // Lifetime is managed by IOSRenderTarget
    }
}
