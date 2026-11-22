using System;
using NativeMetal = global::Xui.Runtime.IOS.Metal;

namespace Xui.GPU.Hardware.Metal;

/// <summary>
/// Apple Metal hardware GPU device implementation.
/// Provides hardware-accelerated shader compilation and rendering on macOS and iOS.
/// </summary>
/// <remarks>
/// This backend uses the Metal framework for rendering and the Metal shader compiler
/// for MSL shader compilation. Supports offscreen readback targets and native MTKView presentation targets.
/// </remarks>
public sealed partial class IOSGpuDevice : IGpuResourceDevice
{
    private readonly nint _device;  // id<MTLDevice>
    private readonly nint _commandQueue;  // id<MTLCommandQueue>
    private bool _disposed;

    /// <inheritdoc/>
    public string BackendName => "Metal";

    internal nint NativeDevice => _device;

    /// <inheritdoc/>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Creates a new Metal GPU device using the system default device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no Metal device is available.</exception>
    public IOSGpuDevice()
    {
        _device = NativeMetal.MTLCreateSystemDefaultDevice();
        if (_device == 0)
            throw new InvalidOperationException("Failed to create Metal device. Metal may not be supported on this system.");

        _commandQueue = NativeMetal.Device_NewCommandQueue(_device);
        if (_commandQueue == 0)
            throw new InvalidOperationException("Failed to create Metal command queue.");
    }

    /// <inheritdoc/>
    public IGpuVertexShader CompileVertexShader(string source, string entryPoint)
    {
        using var pool = new NativeMetal.AutoreleaseScope();
        ObjectDisposedException.ThrowIf(_disposed, this);
        var library = CompileLibrary(source);
        return new IOSVertexShader(entryPoint, library, entryPoint);
    }

    /// <inheritdoc/>
    public IGpuFragmentShader CompileFragmentShader(string source, string entryPoint)
    {
        using var pool = new NativeMetal.AutoreleaseScope();
        ObjectDisposedException.ThrowIf(_disposed, this);
        var library = CompileLibrary(source);
        return new IOSFragmentShader(entryPoint, library, entryPoint);
    }

    /// <inheritdoc/>
    public IGpuRenderTarget CreateRenderTarget(int width, int height)
    {
        using var pool = new NativeMetal.AutoreleaseScope();
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new IOSRenderTarget(width, height, _device);
    }

    /// <inheritdoc/>
    public IGpuCommandList CreateCommandList()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new IOSCommandList(_device, _commandQueue);
    }

    private nint CompileLibrary(string mslSource)
    {
        var sourceStr = NativeMetal.NSStringFromString(mslSource);
        var library = NativeMetal.Device_NewLibraryWithSource(_device, sourceStr, 0, out nint error);

        if (library == 0 || error != 0)
        {
            var errorDesc = NativeMetal.GetErrorDescription(error);
            throw new InvalidOperationException($"MSL shader compilation failed:\n{errorDesc}");
        }

        return library;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        NativeMetal.Release(_commandQueue);
        NativeMetal.Release(_device);
    }
}
