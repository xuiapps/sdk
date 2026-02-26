using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class WIC
{
    /// <summary>
    /// Wraps <c>IWICBitmapDecoder</c> — a container for one or more image frames.
    /// </summary>
    public unsafe class BitmapDecoder : COM.Unknown
    {
        // IID_IWICBitmapDecoder {9edde9e7-8dee-47ea-99df-e6faf2ed44bf}
        public static new readonly Guid IID =
            new Guid(0x9edde9e7, 0x8dee, 0x47ea, 0x99, 0xdf, 0xe6, 0xfa, 0xf2, 0xed, 0x44, 0xbf);

        public BitmapDecoder(void* ptr) : base(ptr) { }

        /// <summary>
        /// Returns the decoded frame at the given index (0 for most formats).
        /// vtable [13] — GetFrame
        /// </summary>
        public BitmapFrameDecode GetFrame(uint index = 0)
        {
            void* ppv;
            Marshal.ThrowExceptionForHR(
                ((delegate* unmanaged[MemberFunction]<void*, uint, void**, int>)this[13])(this, index, &ppv));
            return new BitmapFrameDecode(ppv);
        }
    }
}
