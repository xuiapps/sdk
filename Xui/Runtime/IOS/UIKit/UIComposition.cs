using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.ObjC;
namespace Xui.Runtime.IOS;
public static partial class UIKit
{
    internal static partial class CompositionNative
    {
    [LibraryImport(LibObjCLib,EntryPoint="objc_msgSend")] internal static partial void SetRect(nint obj,nint sel,CGRect rect);
    [LibraryImport(LibObjCLib,EntryPoint="objc_msgSend")] internal static partial void SetPoint(nint obj,nint sel,CGPoint point);
    [LibraryImport(LibObjCLib,EntryPoint="objc_msgSend")] internal static partial void SetSize(nint obj,nint sel,CGSize size);
    [LibraryImport(LibObjCLib,EntryPoint="objc_msgSend")] internal static partial void SetMatrix(nint obj,nint sel,CGAffineTransform matrix);
    [LibraryImport(LibObjCLib,EntryPoint="objc_msgSend")] internal static partial void SendFloat(nint obj,nint sel,float value);
    [LibraryImport(LibObjCLib,EntryPoint="objc_msgSend")] internal static partial nint SendObjectFloat(nint obj,nint sel,NFloat value);
    }
}
