using System;

namespace Xui.Runtime.IOS;

public static partial class CoreGraphics
{
    [Flags]
    public enum CGBitmapInfo : uint
    {
        AlphaPremultipliedFirst = 2,
        ByteOrder32Little = 2 << 12,
    }
}
