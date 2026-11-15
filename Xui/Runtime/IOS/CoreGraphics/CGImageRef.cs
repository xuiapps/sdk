using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.IOS;

public static partial class CoreGraphics
{
    public ref partial struct CGImageRef : IDisposable
    {
        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGImageRelease(nint image);

        [LibraryImport(CoreGraphicsLib)]
        internal static partial nuint CGImageGetWidth(nint image);

        [LibraryImport(CoreGraphicsLib)]
        internal static partial nuint CGImageGetHeight(nint image);

        [LibraryImport(CoreGraphicsLib)]
        private static partial nint CGImageCreateWithImageInRect(nint image, CGRect rect);

        [LibraryImport(CoreGraphicsLib)]
        private static partial nint CGImageCreate(nuint width, nuint height, nuint bitsPerComponent, nuint bitsPerPixel,
            nuint bytesPerRow, nint colorSpace, CGBitmapInfo bitmapInfo, nint provider, nint decode,
            [MarshalAs(UnmanagedType.I1)] bool shouldInterpolate, int renderingIntent);

        public readonly nint Self;

        public CGImageRef(nint self)
        {
            if (self == 0)
                throw new ObjCException($"{nameof(CGImageRef)} instantiated with nil self.");
            Self = self;
        }

        public nuint Width => CGImageGetWidth(Self);
        public nuint Height => CGImageGetHeight(Self);

        public static CGImageRef CreateWithImageInRect(nint image, CGRect rect) => new(CGImageCreateWithImageInRect(image, rect));

        public static CGImageRef Create(nuint width, nuint height, nuint bitsPerComponent, nuint bitsPerPixel,
            nuint bytesPerRow, nint colorSpace, CGBitmapInfo bitmapInfo, nint provider, bool shouldInterpolate = true) =>
            new(CGImageCreate(width, height, bitsPerComponent, bitsPerPixel, bytesPerRow, colorSpace, bitmapInfo,
                provider, 0, shouldInterpolate, 0));

        public void Dispose()
        {
            if (Self != 0)
                CGImageRelease(Self);
        }

        public static implicit operator nint(CGImageRef value) => value.Self;
    }
}
