using System;
using Xui.Core.Math2D;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Cached GPU-resident image data: a D3D11 texture and its D2D1 bitmap wrapper.
/// Owned and keyed by <see cref="DirectXImageFactory"/>. On device-lost recovery
/// <see cref="Update"/> replaces the GPU objects in-place so all referring
/// <see cref="DirectXImage"/> handles remain valid without reloading.
/// </summary>
internal sealed class DirectXImageResource : IDisposable
{
    internal D2D1.Bitmap1 D2D1Bitmap;
    internal D3D11.Texture2D? Texture2D;

    public uint Width { get; }
    public uint Height { get; }
    public Size Size => new Size(Width, Height);

    internal DirectXImageResource(D3D11.Texture2D? texture2D, D2D1.Bitmap1 d2d1Bitmap, uint width, uint height)
    {
        this.Texture2D = texture2D;
        this.D2D1Bitmap = d2d1Bitmap;
        this.Width = width;
        this.Height = height;
    }

    internal void Update(D2D1.Bitmap1 newBitmap, D3D11.Texture2D? newTexture)
    {
        this.D2D1Bitmap.Dispose();
        this.Texture2D?.Dispose();
        this.D2D1Bitmap = newBitmap;
        this.Texture2D = newTexture;
    }

    public void Dispose()
    {
        this.D2D1Bitmap.Dispose();
        this.Texture2D?.Dispose();
    }
}
