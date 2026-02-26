using System;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Platform-specific image wrapping a <c>ID2D1Bitmap1</c> and an optional <c>ID3D11Texture2D</c>.
/// Acquired through <see cref="DirectXImageFactory"/>. The factory may update the internal
/// D2D1 bitmap in-place on device reconnect — callers hold the same reference throughout.
/// </summary>
internal sealed class DirectXImage : IImage, IDisposable
{
    internal D3D11.Texture2D? Texture2D;
    internal D2D1.Bitmap1 D2D1Bitmap;

    public uint Width { get; }
    public uint Height { get; }
    public Size Size => new Size(Width, Height);

    internal DirectXImage(D3D11.Texture2D? texture2D, D2D1.Bitmap1 d2d1Bitmap, uint width, uint height)
    {
        this.Texture2D = texture2D;
        this.D2D1Bitmap = d2d1Bitmap;
        this.Width = width;
        this.Height = height;
    }

    /// <summary>Replaces the GPU resources after a device-lost recovery.</summary>
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
