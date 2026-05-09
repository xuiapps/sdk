using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using static Xui.Runtime.MacOS.ObjC;

namespace Xui.Runtime.MacOS;

public static partial class AppKit
{
    public ref partial struct NSColorRef
    {
        public static readonly Class Class = new Class(AppKit.Lib, "NSColor");

        private static Sel ColorWithCalibratedRedGreenBlueAlpha = new Sel("colorWithCalibratedRed:green:blue:alpha:");


        [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
        private static partial nint objc_msgSend_retIntPtr(nint obj, nint sel, NFloat red, NFloat green, NFloat blue, NFloat alpha);

        public readonly nint Self;

        public NSColorRef(nint self)
        {
            if (self == 0)
            {
                throw new ObjCException($"{nameof(NSColorRef)} instantiated with nil self.");
            }

            this.Self = self;
        }

        /// <summary>
        /// Creates an NSColor from an sRGB color. The autoreleased object returned by
        /// colorWithCalibratedRed:green:blue:alpha: is retained so that Dispose's
        /// CFRelease produces a balanced release.
        /// </summary>
        public NSColorRef(Color color)
            : this(CoreFoundation.CFRetain(
                objc_msgSend_retIntPtr(Class, ColorWithCalibratedRedGreenBlueAlpha, color.Red, color.Green, color.Blue, color.Alpha)))
        {
        }

        /// <summary>
        /// Creates an NSColor from explicit RGBA components. Retained for balanced ownership.
        /// </summary>
        public NSColorRef(NFloat red, NFloat green, NFloat blue, NFloat alpha)
            : this(CoreFoundation.CFRetain(
                objc_msgSend_retIntPtr(Class, ColorWithCalibratedRedGreenBlueAlpha, red, green, blue, alpha)))
        {
        }

        public void Dispose()
        {
            if (this.Self != 0)
            {
                CoreFoundation.CFRelease(this.Self);
            }
        }

        public static implicit operator nint(NSColorRef nsColorRef) => nsColorRef.Self;
    }
}