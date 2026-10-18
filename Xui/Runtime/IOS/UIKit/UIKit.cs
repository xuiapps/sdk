using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    public const string UIKitLib = "/System/Library/Frameworks/UIKit.framework/UIKit";

    public static readonly nint Lib;

    public static readonly Protocol UIApplicationDelegate;

    public static readonly Protocol UITextInput;
    public static readonly Protocol UIKeyInput;
    public static readonly Protocol UITextInputTraits;

    public static readonly Protocol UITextInputDelegate;

    static UIKit()
    {
        Lib = NativeLibrary.Load(UIKitLib);

        UIApplicationDelegate = new Protocol("UIApplicationDelegate");
        UITextInput = new Protocol("UITextInput");
        UIKeyInput = new Protocol("UIKeyInput");
        UITextInputTraits = new Protocol("UITextInputTraits");
        UITextInputDelegate = new Protocol("UITextInputDelegate");
    }

    [LibraryImport(UIKitLib)]
    public static partial int UIApplicationMain(int argc, nint argv, nint principalClassName, nint delegateClassName);

    [LibraryImport(UIKitLib)]
    public static partial nint NSSelectorFromString(nint selector);

    [LibraryImport(UIKitLib)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UIAccessibilityIsReduceMotionEnabled();

    [LibraryImport(UIKitLib)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UIAccessibilityDarkerSystemColorsEnabled();

    [LibraryImport(UIKitLib, EntryPoint="UIGraphicsGetCurrentContext")]
    public static partial nint UIGraphicsGetCurrentContext();
}
