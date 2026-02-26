using System;

namespace Xui.Runtime.Windows;

public static partial class D2D1
{
    /// <summary>
    /// Wraps <c>ID2D1BitmapBrush</c> — a brush that paints with a tiled or clamped bitmap.
    /// Obtained via <c>ID2D1RenderTarget::CreateBitmapBrush</c> (vtable [7]).
    /// For drawing, the brush is used through the <see cref="Brush.Ptr"/> handle.
    /// </summary>
    public unsafe class BitmapBrush : Brush
    {
        // IID_ID2D1BitmapBrush {2cd906aa-12e2-11dc-9fed-001143a055f9}
        public static new readonly Guid IID =
            new Guid(0x2cd906aa, 0x12e2, 0x11dc, 0x9f, 0xed, 0x00, 0x11, 0x43, 0xa0, 0x55, 0xf9);

        public BitmapBrush(void* ptr) : base(ptr) { }
    }
}
