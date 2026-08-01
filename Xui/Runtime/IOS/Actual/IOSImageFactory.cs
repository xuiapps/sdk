using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using static Xui.Runtime.IOS.CoreFoundation;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.ImageIO;

namespace Xui.Runtime.IOS.Actual;

/// <summary>
/// Per-window image catalog. Decodes application assets with ImageIO, retains CGImages
/// for CoreGraphics drawing, and caches them by URI.
/// </summary>
internal sealed class IOSImageFactory : IImagePipeline, IDisposable
{
    private readonly Dictionary<string, IOSImageResource> cache = new();

    public IImage CreateImage() => new IOSImage(this);

    internal IOSImageResource? GetOrLoad(string uri)
    {
        if (cache.TryGetValue(uri, out var cached))
            return cached;

        var resolved = Resolve(uri);
        try
        {
            var resource = Decode(resolved);
            cache.Add(uri, resource);
            return resource;
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"IOSImageFactory: failed to load '{resolved}': {exception.Message}");
            return null;
        }
    }

    internal Task<IOSImageResource?> GetOrLoadAsync(string uri) => Task.Run(() => GetOrLoad(uri));

    internal unsafe IOSImageResource UploadFromPixels(int width, int height, ReadOnlySpan<byte> bgra32Data)
    {
        var stride = checked((nuint)(width * 4));
        var dataSize = checked((nuint)(height * (int)stride));
        var pixelBuffer = bgra32Data.ToArray();
        var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
        var data = handle.AddrOfPinnedObject();

        try
        {
            using var provider = CGDataProviderRef.CreateWithData(data, dataSize, releaseCallbackPointer);
            using var colorSpace = CGColorSpaceRef.CreateDeviceRGB();
            using var image = CGImageRef.Create((nuint)width, (nuint)height, 8, 32, stride, colorSpace,
                CGBitmapInfo.ByteOrder32Little | CGBitmapInfo.AlphaPremultipliedFirst, provider);

            lock (releaseHandles)
                releaseHandles.Add(data, handle);

            return new IOSImageResource(new CGImage(CFRetain(image)));
        }
        catch
        {
            handle.Free();
            throw;
        }
    }

    private delegate void DataProviderReleaseCallback(nint info, nint data, nuint size);
    private static readonly DataProviderReleaseCallback releaseCallback = OnDataProviderRelease;
    private static readonly nint releaseCallbackPointer = Marshal.GetFunctionPointerForDelegate(releaseCallback);
    private static readonly Dictionary<nint, GCHandle> releaseHandles = new();

    private static void OnDataProviderRelease(nint info, nint data, nuint size)
    {
        lock (releaseHandles)
        {
            if (releaseHandles.Remove(data, out var handle))
                handle.Free();
        }
    }

    private static string Resolve(string uri)
    {
        if (AssetUri.TryGetRelativePath(uri, out var assetPath))
            return Path.Combine(AppContext.BaseDirectory, "Assets", assetPath);

        return Path.IsPathRooted(uri) ? uri : Path.Combine(AppContext.BaseDirectory, uri);
    }

    private static IOSImageResource Decode(string path)
    {
        using var pathString = new CFStringRef(path);
        using var url = CFURLRef.CreateWithFileSystemPath(pathString);
        using var source = CGImageSourceRef.CreateWithURL(url);
        using var image = source.CreateImageAtIndex();
        return new IOSImageResource(new CGImage(CFRetain(image)));
    }

    public void Dispose()
    {
        foreach (var resource in cache.Values)
            resource.Dispose();
        cache.Clear();
    }
}
