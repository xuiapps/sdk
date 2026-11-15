using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.CoreFoundation;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    /// <summary>Owned iOS 17+ native text loupe. Dispose on the UI thread when the gesture ends.</summary>
    public partial struct UITextLoupeSessionPtr : IDisposable
    {
        private static readonly Sel BeginSel = new("beginLoupeSessionAtPoint:fromSelectionWidgetView:inView:");
        private static readonly Sel MoveSel = new("moveToPoint:withCaretRect:trackingCaret:");
        private static readonly Sel InvalidateSel = new("invalidate");
        // objc_getClass returns zero on older iOS; don't construct a throwing Class wrapper.
        private static readonly nint NativeClass = objc_getClass("UITextLoupeSession");
        private nint handle;

        public static bool IsSupported => NativeClass != 0;
        public readonly bool IsActive => handle != 0;

        public static UITextLoupeSessionPtr Begin(CGPoint point, UIView view)
        {
            if (!IsSupported) return default;
            var session = BeginNative(NativeClass, BeginSel, point, 0, view);
            // The factory returns an autoreleased session. Own one retain across frames.
            return new UITextLoupeSessionPtr { handle = session == 0 ? 0 : CFRetain(session) };
        }

        public readonly void Move(CGPoint point, CGRect caret)
        {
            if (handle != 0) MoveNative(handle, MoveSel, point, caret, true);
        }

        public void Dispose()
        {
            var session = handle;
            handle = 0;
            if (session == 0) return;
            objc_msgSend(session, InvalidateSel);
            CFRelease(session);
        }

        [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
        private static partial nint BeginNative(nint receiver, nint selector, CGPoint point, nint widget, nint view);

        [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
        private static partial void MoveNative(nint receiver, nint selector, CGPoint point, CGRect caret,
            [MarshalAs(UnmanagedType.I1)] bool trackingCaret);
    }
}
