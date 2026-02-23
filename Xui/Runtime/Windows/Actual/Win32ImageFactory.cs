using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using static Xui.Runtime.Windows.D2D1;
using static Xui.Runtime.Windows.DXGI;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Window-level image factory: decodes images via WIC, uploads to a D3D11 texture,
/// wraps as a D2D1 Bitmap1, and caches results by path.
/// </summary>
public sealed class Win32ImageFactory : IImageFactory, IDisposable
{
    private readonly D3D11.Device d3d11Device;
    private readonly D2D1.DeviceContext d2d1DeviceContext;
    private readonly Dictionary<string, Win32Bitmap> cache = new();

    public event Action? Invalidated;

    public Win32ImageFactory(D3D11.Device d3d11Device, D2D1.DeviceContext d2d1DeviceContext)
    {
        this.d3d11Device = d3d11Device;
        this.d3d11Device.AddRef();

        this.d2d1DeviceContext = d2d1DeviceContext;
        this.d2d1DeviceContext.AddRef();
    }

    public unsafe IImage? Load(string path)
    {
        if (this.cache.TryGetValue(path, out var cached))
            return cached;

        // Resolve relative paths against the application's output directory.
        string resolvedPath = System.IO.Path.IsPathRooted(path)
            ? path
            : System.IO.Path.Combine(AppContext.BaseDirectory, path);

        try
        {
            using var wicFactory = WIC.CreateImagingFactory();
            using var decoder = wicFactory.CreateDecoderFromFilename(resolvedPath);
            using var frame = decoder.GetFrame(0);
            using var conv = wicFactory.CreateFormatConverter();

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
                    Width = w,
                    Height = h,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = DXGI.Format.B8G8R8A8_UNORM,
                    SampleDesc = new DXGI.SampleDesc { Count = 1, Quality = 0 },
                    Usage = 0,
                    BindFlags = (uint)D3D11.BindFlags.ShaderResource,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                };

                var sub = new D3D11.SubresourceData
                {
                    pSysMem = pixels,
                    SysMemPitch = stride,
                };

                var texture = this.d3d11Device.CreateTexture2D(desc, sub);

                void* surfacePtr = texture.QueryInterface(in DXGI.Surface.IID);
                using var surface = new DXGI.Surface(surfacePtr);

                var bitmapProps = new BitmapProperties1
                {
                    PixelFormat = new PixelFormat
                    {
                        Format = DXGI.Format.B8G8R8A8_UNORM,
                        AlphaMode = D2D1.AlphaMode.Premultiplied,
                    },
                    BitmapOptions = BitmapOptions.None,
                    DpiX = 96.0f,
                    DpiY = 96.0f,
                };

                var d2dBitmap = this.d2d1DeviceContext.CreateBitmapFromDxgiSurface(surface, bitmapProps);

                var result = new Win32Bitmap(texture, d2dBitmap, w, h);
                this.cache[path] = result;
                return result;
            }
            finally
            {
                NativeMemory.Free(pixels);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Win32ImageFactory: failed to load '{resolvedPath}': {ex.Message}");
            return null;
        }
    }

    /// <summary>Disposes all cached bitmaps (call on device-lost before recreating the device).</summary>
    public void InvalidateAll()
    {
        foreach (var bitmap in this.cache.Values)
            bitmap.Dispose();
        this.cache.Clear();
        this.Invalidated?.Invoke();
    }

    public void Dispose()
    {
        this.InvalidateAll();
        this.d3d11Device.Dispose();
        this.d2d1DeviceContext.Dispose();
    }
}
