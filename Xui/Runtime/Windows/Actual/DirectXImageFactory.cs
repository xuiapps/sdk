using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using static Xui.Runtime.Windows.D2D1;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Window-level image catalog: decodes via WIC, uploads to D3D11/D2D1, caches by URI.
/// Implements <see cref="IImagePipeline"/> so the service chain can vend <see cref="IImage"/>
/// handles to views without exposing platform types.
/// On device-lost, call <see cref="Rehydrate"/> to re-upload all cached resources
/// transparently — existing <see cref="DirectXImage"/> handles remain valid.
/// </summary>
internal sealed class DirectXImageFactory : IImagePipeline, IDisposable
{
    private readonly WIC.ImagingFactory wicFactory;
    private D3D11.Device d3d11Device;
    private DeviceContext d2d1DeviceContext;
    private readonly Dictionary<string, DirectXImageResource> cache = new();

    internal DirectXImageFactory(D3D11.Device d3d11Device, DeviceContext d2d1DeviceContext)
    {
        this.d3d11Device = d3d11Device;
        this.d3d11Device.AddRef();

        this.d2d1DeviceContext = d2d1DeviceContext;
        this.d2d1DeviceContext.AddRef();

        this.wicFactory = WIC.CreateImagingFactory();
    }

    // ── IImagePipeline ───────────────────────────────────────────────────────

    public IImage CreateImage() => new DirectXImage(this);

    // ── Internal catalog ─────────────────────────────────────────────────────

    internal DirectXImageResource? GetOrLoad(string uri)
    {
        if (this.cache.TryGetValue(uri, out var cached))
            return cached;

        string resolved = Resolve(uri);
        try
        {
            var (bitmap, texture, w, h) = this.Upload(resolved);
            var resource = new DirectXImageResource(texture, bitmap, w, h);
            this.cache[uri] = resource;
            return resource;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DirectXImageFactory: failed to load '{resolved}': {ex.Message}");
            return null;
        }
    }

    internal Task<DirectXImageResource?> GetOrLoadAsync(string uri) =>
        Task.Run(() => GetOrLoad(uri));

    // ── Device-lost recovery ─────────────────────────────────────────────────

    /// <summary>
    /// Re-uploads all cached images to the new device after device-lost recovery.
    /// Existing <see cref="DirectXImage"/> handles remain valid — their resource objects
    /// are updated in-place.
    /// </summary>
    internal void Rehydrate(D3D11.Device newDevice, DeviceContext newContext)
    {
        this.d3d11Device.Dispose();
        this.d2d1DeviceContext.Dispose();

        this.d3d11Device = newDevice;
        this.d3d11Device.AddRef();

        this.d2d1DeviceContext = newContext;
        this.d2d1DeviceContext.AddRef();

        foreach (var (uri, resource) in this.cache)
        {
            try
            {
                var (newBitmap, newTexture, _, _) = this.Upload(Resolve(uri));
                resource.Update(newBitmap, newTexture);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectXImageFactory: rehydrate failed for '{uri}': {ex.Message}");
            }
        }
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static string Resolve(string uri) =>
        System.IO.Path.IsPathRooted(uri)
            ? uri
            : System.IO.Path.Combine(AppContext.BaseDirectory, uri);

    private unsafe (D2D1.Bitmap1 bitmap, D3D11.Texture2D? texture, uint w, uint h) Upload(string resolvedPath)
    {
        using var decoder = this.wicFactory.CreateDecoderFromFilename(resolvedPath);
        using var frame = decoder.GetFrame(0);
        using var conv = this.wicFactory.CreateFormatConverter();

        conv.Initialize(frame, WIC.PixelFormats.Pbgra32);
        conv.GetSize(out uint w, out uint h);

        uint stride = w * 4;
        uint bufSize = stride * h;

        byte* pixels = (byte*)NativeMemory.Alloc(bufSize);
        try
        {
            conv.CopyPixels(stride, bufSize, pixels);

            var desc = new D3D11.Texture2DDesc
            {
                Width          = w,
                Height         = h,
                MipLevels      = 1,
                ArraySize      = 1,
                Format         = DXGI.Format.B8G8R8A8_UNORM,
                SampleDesc     = new DXGI.SampleDesc { Count = 1, Quality = 0 },
                Usage          = 0,
                BindFlags      = (uint)D3D11.BindFlags.ShaderResource,
                CPUAccessFlags = 0,
                MiscFlags      = 0,
            };

            var sub = new D3D11.SubresourceData { pSysMem = pixels, SysMemPitch = stride };
            var texture = this.d3d11Device.CreateTexture2D(desc, sub);

            void* surfacePtr = texture.QueryInterface(in DXGI.Surface.IID);
            using var surface = new DXGI.Surface(surfacePtr);

            var bitmapProps = new BitmapProperties1
            {
                PixelFormat = new PixelFormat
                {
                    Format    = DXGI.Format.B8G8R8A8_UNORM,
                    AlphaMode = AlphaMode.Premultiplied,
                },
                BitmapOptions = BitmapOptions.None,
                DpiX = 96.0f,
                DpiY = 96.0f,
            };

            var d2dBitmap = this.d2d1DeviceContext.CreateBitmapFromDxgiSurface(surface, bitmapProps);
            return (d2dBitmap, texture, w, h);
        }
        finally
        {
            NativeMemory.Free(pixels);
        }
    }

    public void Dispose()
    {
        foreach (var resource in this.cache.Values)
            resource.Dispose();
        this.cache.Clear();

        this.wicFactory.Dispose();
        this.d2d1DeviceContext.Dispose();
        this.d3d11Device.Dispose();
    }
}
