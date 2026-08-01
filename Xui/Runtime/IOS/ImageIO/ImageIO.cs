using System.Runtime.InteropServices;

namespace Xui.Runtime.IOS;

public static partial class ImageIO
{
    public const string ImageIOLib = "/System/Library/Frameworks/ImageIO.framework/ImageIO";
    public static readonly nint Lib = NativeLibrary.Load(ImageIOLib);
}
