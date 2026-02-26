using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xui.Core.Canvas;
using static Xui.Runtime.Windows.D2D1;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Window-level image factory: decodes images via WIC, uploads to a D3D11 texture,
/// wraps as a D2D1 Bitmap1, and caches results by path.
/// On device-lost recovery, call <see cref="Rehydrate"/> to re-upload all cached images
/// transparently — callers hold the same <see cref="DirectXImage"/> references throughout.
/// </summary>
internal sealed class DirectXImageFactory : IImageFactory, IDisposable
{
    private readonly WIC.ImagingFactory wicFactory;
    private D3D11.Device d3d11Device;
    private DeviceContext d2d1DeviceContext;
    private readonly Dictionary<string, DirectXImage> cache = new();

    internal DirectXImageFactory(D3D11.Device d3d11Device, DeviceContext d2d1DeviceContext)
    {
        this.d3d11Device = d3d11Device;
        this.d3d11Device.AddRef();

        this.d2d1DeviceContext = d2d1DeviceContext;
        this.d2d1DeviceContext.AddRef();

        this.wicFactory = WIC.CreateImagingFactory();
    }

    public IImage? Load(string path)
    {
        if (this.cache.TryGetValue(path, out var cached))
            return cached;

        return this.Decode(path);
    }

    public Task<IImage?> LoadAsync(string path) =>
        Task.Run(() => this.Load(path));

    /// <summary>
    /// Re-uploads all cached images to the new device after a device-lost recovery.
    /// Existing <see cref="DirectXImage"/> references remain valid.
    /// </summary>
    internal void Rehydrate(D3D11.Device newD3D11Device, DeviceContext newD2D1DeviceContext)
    {
        this.d3d11Device.Dispose();
        this.d2d1DeviceContext.Dispose();

        this.d3d11Device = newD3D11Device;
        this.d3d11Device.AddRef();

        this.d2d1DeviceContext = newD2D1DeviceContext;
        this.d2d1DeviceContext.AddRef();

        foreach (var (path, image) in this.cache)
        {
            string resolved = System.IO.Path.IsPathRooted(path)
                ? path
                : System.IO.Path.Combine(AppContext.BaseDirectory, path);
            try
            {
                var (newBitmap, newTexture, _, _) = this.Upload(resolved);
                image.Update(newBitmap, newTexture);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DirectXImageFactory: rehydrate failed for '{path}': {ex.Message}");
            }
        }
    }

    private DirectXImage? Decode(string path)
    {
        string resolved = System.IO.Path.IsPathRooted(path)
            ? path
            : System.IO.Path.Combine(AppContext.BaseDirectory, path);

        try
        {
            var (d2dBitmap, texture, w, h) = this.Upload(resolved);
            var result = new DirectXImage(texture, d2dBitmap, w, h);
            this.cache[path] = result;
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DirectXImageFactory: failed to load '{resolved}': {ex.Message}");
            return null;
        }
    }

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

            var sub = new D3D11.SubresourceData
            {
                pSysMem     = pixels,
                SysMemPitch = stride,
            };

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
        foreach (var image in this.cache.Values)
            image.Dispose();
        this.cache.Clear();

        this.wicFactory.Dispose();
        this.d2d1DeviceContext.Dispose();
        this.d3d11Device.Dispose();
    }
}
