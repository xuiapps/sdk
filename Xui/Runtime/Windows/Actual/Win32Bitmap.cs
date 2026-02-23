using System;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Platform-specific bitmap wrapping a <c>ID2D1Bitmap1</c>.
/// Optionally owns a <c>ID3D11Texture2D</c> when created via the D3D11 upload path;
/// null when created via <c>ID2D1DeviceContext::CreateBitmapFromWicBitmap</c>.
/// </summary>
public sealed class Win32Bitmap : Xui.Core.Canvas.Bitmap, IImage, IDisposable
{
    internal readonly D3D11.Texture2D? Texture2D;
    internal readonly D2D1.Bitmap1 D2D1Bitmap;

    public override uint Width { get; }
    public override uint Height { get; }

    Size IImage.Size => new Size(Width, Height);

    public Win32Bitmap(D3D11.Texture2D? texture2D, D2D1.Bitmap1 d2d1Bitmap, uint width, uint height)
    {
        this.Texture2D = texture2D;
        this.D2D1Bitmap = d2d1Bitmap;
        this.Width = width;
        this.Height = height;
    }

    public void Dispose()
    {
        this.D2D1Bitmap.Dispose();
        this.Texture2D?.Dispose();
    }
}
