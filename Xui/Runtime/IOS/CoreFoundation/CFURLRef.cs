using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.IOS;

public static partial class CoreFoundation
{
    public ref partial struct CFURLRef : IDisposable
    {
        private const int kCFURLPOSIXPathStyle = 0;

        [LibraryImport(CoreFoundationLib)]
        private static partial nint CFURLCreateWithFileSystemPath(
            nint allocator,
            nint filePath,
            int pathStyle,
            [MarshalAs(UnmanagedType.I1)] bool isDirectory);

        public readonly nint Self;

        public CFURLRef(nint self)
        {
            if (self == 0)
                throw new ObjCException($"{nameof(CFURLRef)} instantiated with nil self.");
            Self = self;
        }

        public static CFURLRef CreateWithFileSystemPath(nint path) =>
            new(CFURLCreateWithFileSystemPath(0, path, kCFURLPOSIXPathStyle, false));

        public void Dispose()
        {
            if (Self != 0)
                CFRelease(Self);
        }

        public static implicit operator nint(CFURLRef value) => value.Self;
    }
}
