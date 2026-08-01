using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using static Xui.Runtime.MacOS.CoreFoundation;
using static Xui.Runtime.MacOS.CoreGraphics;
using static Xui.Runtime.MacOS.ImageIO;

namespace Xui.Runtime.MacOS.Actual;

/// <summary>
/// Window-level image catalog: decodes via ImageIO, stores as CGImageRef, caches by URI.
/// Implements <see cref="IImagePipeline"/> so the service chain can vend <see cref="IImage"/>
/// handles to views without exposing platform types.
/// The CGImageRef is drawable by CoreGraphics today and the resource struct is designed
/// to accommodate an MTLTexture handle for Metal rendering in the future.
/// </summary>
internal sealed class MacOSImageFactory : IImagePipeline, IDisposable
{
    private readonly Dictionary<string, MacOSImageResource> cache = new();

    // ── IImagePipeline ───────────────────────────────────────────────────────

    public IImage CreateImage() => new MacOSImage(this);

    // ── Internal catalog ─────────────────────────────────────────────────────

    internal MacOSImageResource? GetOrLoad(string uri)
    {
        if (cache.TryGetValue(uri, out var cached))
            return cached;

        string resolved = Resolve(uri);
        try
        {
            var resource = Decode(resolved);
            cache[uri] = resource;
            return resource;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MacOSImageFactory: failed to load '{resolved}': {ex.Message}");
            return null;
        }
    }

    internal Task<MacOSImageResource?> GetOrLoadAsync(string uri) =>
        Task.Run(() => GetOrLoad(uri));

    /// <summary>
    /// Uploads raw BGRA32 pixel data to a new CGImage.
    /// Creates a CGDataProvider backed by a pinned managed array copy,
    /// then wraps it in a CGImage for CoreGraphics rendering.
    /// </summary>
    internal unsafe MacOSImageResource UploadFromPixels(int width, int height, ReadOnlySpan<byte> bgra32Data)
    {
        uint stride = (uint)(width * 4);
        uint dataSize = (uint)(height * stride);

        // Copy the pixel data to a pinned managed array that will stay alive
        // for the lifetime of the CGImage. The data provider will reference this buffer.
        var pixelBuffer = new byte[dataSize];
        bgra32Data.CopyTo(pixelBuffer);

        // Pin the buffer and create a data provider
        var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
        nint dataPtr = handle.AddrOfPinnedObject();

        // Create a data provider with a release callback that unpins the buffer
        using var provider = CGDataProviderRef.CreateWithData(
            dataPtr,
            dataSize,
            releaseCallbackPtr);

        // Create RGB colorspace
        using var colorspace = CGColorSpaceRef.CreateDeviceRGB();

        // Create CGImage from the data provider
        // BGRA format with premultiplied alpha
        using var cgImageRef = CGImageRef.Create(
            width: (nuint)width,
            height: (nuint)height,
            bitsPerComponent: 8,
            bitsPerPixel: 32,
            bytesPerRow: stride,
            colorspace: colorspace,
            bitmapInfo: CGBitmapInfo.ByteOrder32Little | CGBitmapInfo.AlphaPremultipliedFirst,
            provider: provider,
            shouldInterpolate: true);

        // Transfer ownership of the CGImage (retain it so it survives the using block)
        nint retained = CFRetain(cgImageRef);
        
        // Store the GCHandle using data pointer as key for proper cleanup
        // The entry will be removed when the CGDataProvider is released by CoreGraphics
        lock (releaseCallbackHandles)
        {
            releaseCallbackHandles[dataPtr] = handle;
        }

        return new MacOSImageResource(new CoreGraphics.CGImage(retained));
    }

    // Callback delegate for CGDataProvider to unpin the buffer when the provider is destroyed
    private delegate void DataProviderReleaseCallbackDelegate(nint info, nint data, nuint size);
    private static readonly DataProviderReleaseCallbackDelegate DataProviderReleaseCallback = OnDataProviderRelease;
    private static readonly nint releaseCallbackPtr = Marshal.GetFunctionPointerForDelegate(DataProviderReleaseCallback);
    private static readonly Dictionary<nint, GCHandle> releaseCallbackHandles = new();

    private static void OnDataProviderRelease(nint info, nint data, nuint size)
    {
        // The data pointer is the address of the pinned buffer
        // Find and free the corresponding GCHandle
        // This is called by CoreGraphics when the CGDataProvider is destroyed,
        // which happens when the last CGImage referencing it is released
        lock (releaseCallbackHandles)
        {
            if (releaseCallbackHandles.TryGetValue(data, out var handle))
            {
                handle.Free();
                releaseCallbackHandles.Remove(data);
            }
        }
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static string Resolve(string uri)
    {
        if (AssetUri.TryGetRelativePath(uri, out var assetPath))
            return System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", assetPath);

        return System.IO.Path.IsPathRooted(uri)
            ? uri
            : System.IO.Path.Combine(AppContext.BaseDirectory, uri);
    }

    private static MacOSImageResource Decode(string resolvedPath)
    {
        using var pathString = new CFStringRef(resolvedPath);
        using var url = CFURLRef.CreateWithFileSystemPath(pathString);
        using var source = CGImageSourceRef.CreateWithURL(url);
        using var cgImageRef = source.CreateImageAtIndex(0);

        // Transfer ownership of the CGImageRef out of the ref-struct without releasing.
        // MacOSImageResource takes ownership and will call CGImageRelease on Dispose.
        nint retained = CFRetain(cgImageRef);
        return new MacOSImageResource(new CoreGraphics.CGImage(retained));
    }

    public void Dispose()
    {
        foreach (var resource in cache.Values)
            resource.Dispose();
        cache.Clear();
    }
}
