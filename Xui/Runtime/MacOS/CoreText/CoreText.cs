using System.Runtime.InteropServices;

namespace Xui.Runtime.MacOS;

public static partial class CoreText
{
    public const string CoreTextLib = "/System/Library/Frameworks/CoreText.framework/CoreText";

    public static readonly nint Lib;

    static CoreText()
    {
        Lib = NativeLibrary.Load(CoreTextLib);
    }

    [LibraryImport(CoreTextLib)]
    public static partial nint NSSelectorFromString(nint selector);

    [LibraryImport(CoreTextLib)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool CTFontManagerRegisterFontsForURL(nint fontUrl, uint scope, out nint error);
}
