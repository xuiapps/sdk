using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class WIC
{
    /// <summary>
    /// Wraps <c>IWICBitmapFrameDecode</c> — a single decoded image frame.
    /// Inherits <c>IWICBitmapSource</c> methods (GetSize, CopyPixels) at vtable [3] and [7].
    /// </summary>
    public unsafe class BitmapFrameDecode : COM.Unknown
    {
        // IID_IWICBitmapFrameDecode {3b16811b-6a43-4ec9-a813-3d930c13b940}
        public static new readonly Guid IID =
            new Guid(0x3b16811b, 0x6a43, 0x4ec9, 0xa8, 0x13, 0x3d, 0x93, 0x0c, 0x13, 0xb9, 0x40);

        public BitmapFrameDecode(void* ptr) : base(ptr) { }

        /// <summary>
        /// Returns the pixel dimensions of the frame.
        /// vtable [3] — IWICBitmapSource::GetSize
        /// </summary>
        public void GetSize(out uint width, out uint height)
        {
            uint w, h;
            Marshal.ThrowExceptionForHR(
                ((delegate* unmanaged[MemberFunction]<void*, uint*, uint*, int>)this[3])(this, &w, &h));
            width = w;
            height = h;
        }

        /// <summary>
        /// Copies decoded pixels into <paramref name="buffer"/> (full image, no rect clipping).
        /// vtable [7] — IWICBitmapSource::CopyPixels
        /// </summary>
        public void CopyPixels(uint stride, uint bufferSize, byte* buffer)
        {
            Marshal.ThrowExceptionForHR(
                ((delegate* unmanaged[MemberFunction]<void*, void*, uint, uint, byte*, int>)this[7])(
                    this, null, stride, bufferSize, buffer));
        }
    }
}
