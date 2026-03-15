using System;

namespace Xui.Runtime.MacOS;

public static partial class CoreFoundation
{
    public ref struct CFStringRef : IDisposable
    {
        public readonly nint Self;

        public CFStringRef(nint self)
        {
            if (self == 0)
            {
                throw new ObjCException($"{nameof(CFStringRef)} instantiated with nil self.");
            }

            this.Self = self;
        }

        public CFStringRef(string? str) : this(CFString(str))
        {
        }

        public CFStringRef(ReadOnlySpan<char> str) : this(CFString(str))
        {
        }

        internal static string? Marshal(nint cfStringRef) =>
            System.Runtime.InteropServices.Marshal.PtrToStringUTF8(
                ObjC.objc_msgSend_retIntPtr(cfStringRef, UTF8StringSel));

        public void Dispose()
        {
            if (this.Self != 0)
            {
                CFRelease(this.Self);
            }
        }

        public static implicit operator nint(CFStringRef cfStringRef) => cfStringRef.Self;

        public static implicit operator string?(CFStringRef cfStringRef) => Marshal(cfStringRef);
    }
}