using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class WIC
{
    /// <summary>
    /// Wraps <c>IWICImagingFactory</c> — the entry point for all WIC operations.
    /// </summary>
    public unsafe class ImagingFactory : COM.Unknown
    {
        // IID_IWICImagingFactory {ec5ec8a9-c395-4314-9c77-54d7a935ff70}
        public static new readonly Guid IID =
            new Guid(0xec5ec8a9, 0xc395, 0x4314, 0x9c, 0x77, 0x54, 0xd7, 0xa9, 0x35, 0xff, 0x70);

        public ImagingFactory(void* ptr) : base(ptr) { }

        /// <summary>
        /// Opens and decodes an image file.
        /// vtable [3] — CreateDecoderFromFilename
        /// </summary>
        public BitmapDecoder CreateDecoderFromFilename(string path)
        {
            void* ppv;
            fixed (char* pathPtr = path)
            {
                // GENERIC_READ = 0x80000000, WICDecodeMetadataCacheOnDemand = 0
                Marshal.ThrowExceptionForHR(
                    ((delegate* unmanaged[MemberFunction]<void*, char*, void*, uint, uint, void**, int>)this[3])(
                        this, pathPtr, null, 0x80000000u, 0u, &ppv));
            }
            return new BitmapDecoder(ppv);
        }

        /// <summary>
        /// Creates a format converter for pixel-format conversion.
        /// vtable [10] — CreateFormatConverter
        /// </summary>
        public FormatConverter CreateFormatConverter()
        {
            void* ppv;
            Marshal.ThrowExceptionForHR(
                ((delegate* unmanaged[MemberFunction]<void*, void**, int>)this[10])(this, &ppv));
            return new FormatConverter(ppv);
        }
    }
}
