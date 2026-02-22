using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using static Xui.Runtime.Windows.D2D1;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Owns the Windows Imaging Component factory and the GPU bitmap cache.
/// Decoupled from the draw-command context so it can be shared across
/// multiple rendering pipelines (2D, future 3D).
/// </summary>
public sealed class DirectXBitmapContext : IBitmapContext, IDisposable
{
    private readonly WIC.ImagingFactory wicFactory;
    private readonly D3D11.Device d3d11Device;
    private readonly DeviceContext d2d1DeviceContext;
    private readonly Dictionary<string, DirectXBitmap> cache = new();

    public DirectXBitmapContext(D3D11.Device d3d11Device, DeviceContext d2d1DeviceContext)
    {
        this.d3d11Device = d3d11Device;
        this.d3d11Device.AddRef();

        this.d2d1DeviceContext = d2d1DeviceContext;
        this.d2d1DeviceContext.AddRef();

        this.wicFactory = WIC.CreateImagingFactory();
    }

    unsafe Core.Canvas.Bitmap IBitmapContext.LoadBitmap(string uri)
    {
        if (this.cache.TryGetValue(uri, out var cached))
            return cached;

        using var decoder = this.wicFactory.CreateDecoderFromFilename(uri);
        using var frame   = decoder.GetFrame(0);
        using var conv    = this.wicFactory.CreateFormatConverter();

        // Premultiplied bytes
        conv.Initialize(frame, WIC.PixelFormats.Pbgra32);
        conv.GetSize(out uint w, out uint h);

        uint stride  = w * 4;
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
                Usage          = 0, // D3D11_USAGE_DEFAULT
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

            var result = new DirectXBitmap(texture, d2dBitmap, w, h);
            this.cache[uri] = result;
            return result;
        }
        finally
        {
            NativeMemory.Free(pixels);
        }
    }

    public void Dispose()
    {
        foreach (var bitmap in this.cache.Values)
            bitmap.Dispose();
        this.cache.Clear();

        this.wicFactory.Dispose();
        this.d2d1DeviceContext.Dispose();
        this.d3d11Device.Dispose();
    }
}
