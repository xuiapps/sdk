using System;

namespace Xui.Runtime.Windows;

public static partial class WIC
{
    /// <summary>
    /// Well-known WIC pixel format GUIDs.
    /// </summary>
    public static class PixelFormats
    {
        /// <summary>
        /// 32-bit premultiplied BGRA — the target format for all image decoding in this pipeline.
        /// GUID_WICPixelFormat32bppPBGRA {6fddc324-4e03-4bfe-b185-3d77768dc910}
        /// </summary>
        public static readonly Guid Pbgra32 =
            new Guid(0x6fddc324, 0x4e03, 0x4bfe, 0xb1, 0x85, 0x3d, 0x77, 0x76, 0x8d, 0xc9, 0x10);
    }
}
