using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class WIC
{
    /// <summary>
    /// Wraps <c>IWICFormatConverter</c> — converts a <see cref="BitmapFrameDecode"/> to a
    /// target pixel format (typically <see cref="PixelFormats.Pbgra32"/>).
    /// Inherits <c>IWICBitmapSource</c> methods at the same vtable slots as
    /// <see cref="BitmapFrameDecode"/>.
    /// </summary>
    public unsafe class FormatConverter : COM.Unknown
    {
        // IID_IWICFormatConverter {00000301-a8f2-4877-ba0a-fd2b6645fb94}
        public static new readonly Guid IID =
            new Guid(0x00000301, 0xa8f2, 0x4877, 0xba, 0x0a, 0xfd, 0x2b, 0x66, 0x45, 0xfb, 0x94);

        public FormatConverter(void* ptr) : base(ptr) { }

        /// <summary>
        /// Configures the converter to produce pixels in <paramref name="dstFormat"/>.
        /// vtable [8] — IWICFormatConverter::Initialize
        /// </summary>
        public void Initialize(BitmapFrameDecode source, in Guid dstFormat)
        {
            fixed (Guid* fmtPtr = &dstFormat)
            {
                // dither=0 (None), palette=null, alphaThreshold=0.0, paletteTranslate=0 (Custom)
                Marshal.ThrowExceptionForHR(
                    ((delegate* unmanaged[MemberFunction]<void*, void*, Guid*, uint, void*, double, uint, int>)this[8])(
                        this, source, fmtPtr, 0u, null, 0.0, 0u));
            }
        }

        /// <summary>
        /// Returns the pixel dimensions after conversion.
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
        /// Copies converted pixels into <paramref name="buffer"/> (full image, no rect clipping).
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
