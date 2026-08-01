using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.CoreFoundation;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Runtime.IOS;

public static partial class ImageIO
{
    public ref partial struct CGImageSourceRef : IDisposable
    {
        [LibraryImport(ImageIOLib)]
        private static partial nint CGImageSourceCreateWithURL(nint url, nint options);

        [LibraryImport(ImageIOLib)]
        private static partial nint CGImageSourceCreateImageAtIndex(nint source, nuint index, nint options);

        public readonly nint Self;

        public CGImageSourceRef(nint self)
        {
            if (self == 0)
                throw new ObjCException($"{nameof(CGImageSourceRef)} instantiated with nil self.");
            Self = self;
        }

        public static CGImageSourceRef CreateWithURL(nint url) => new(CGImageSourceCreateWithURL(url, 0));
        public CGImageRef CreateImageAtIndex(nuint index = 0) => new(CGImageSourceCreateImageAtIndex(Self, index, 0));

        public void Dispose()
        {
            if (Self != 0)
                CFRelease(Self);
        }

        public static implicit operator nint(CGImageSourceRef value) => value.Self;
    }
}
