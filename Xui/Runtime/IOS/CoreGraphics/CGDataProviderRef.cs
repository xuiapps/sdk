using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.IOS;

public static partial class CoreGraphics
{
    public ref partial struct CGDataProviderRef : IDisposable
    {
        [LibraryImport(CoreGraphicsLib)]
        private static partial nint CGDataProviderCreateWithData(nint info, nint data, nuint size, nint releaseCallback);

        [LibraryImport(CoreGraphicsLib)]
        private static partial void CGDataProviderRelease(nint provider);

        public readonly nint Self;

        public CGDataProviderRef(nint self)
        {
            if (self == 0)
                throw new ObjCException($"{nameof(CGDataProviderRef)} instantiated with nil self.");
            Self = self;
        }

        public static CGDataProviderRef CreateWithData(nint data, nuint size, nint releaseCallback = 0) =>
            new(CGDataProviderCreateWithData(0, data, size, releaseCallback));

        public void Dispose()
        {
            if (Self != 0)
                CGDataProviderRelease(Self);
        }

        public static implicit operator nint(CGDataProviderRef value) => value.Self;
    }
}
