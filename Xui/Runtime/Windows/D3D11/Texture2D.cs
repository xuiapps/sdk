using System;

namespace Xui.Runtime.Windows;

public static partial class D3D11
{
    /// <summary>
    /// Wraps <c>ID3D11Texture2D</c>.
    /// After creation, query <see cref="DXGI.Surface.IID"/> via
    /// <see cref="COM.Unknown.QueryInterface"/> to obtain an <c>IDXGISurface</c>
    /// usable with <c>ID2D1DeviceContext::CreateBitmapFromDxgiSurface</c>.
    /// </summary>
    public unsafe class Texture2D : COM.Unknown
    {
        // IID_ID3D11Texture2D {6f15aaf2-d208-4e89-9ab4-489535d34f9c}
        public static new readonly Guid IID =
            new Guid(0x6f15aaf2, 0xd208, 0x4e89, 0x9a, 0xb4, 0x48, 0x95, 0x35, 0xd3, 0x4f, 0x9c);

        public Texture2D(void* ptr) : base(ptr) { }
    }
}
