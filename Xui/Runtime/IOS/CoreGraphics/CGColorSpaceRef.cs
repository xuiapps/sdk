using System.Runtime.InteropServices;

namespace Xui.Runtime.IOS;

public static partial class CoreGraphics
{
    public ref partial struct CGColorSpaceRef
    {
        [LibraryImport(CoreGraphicsLib)]
        private static partial nint CGColorSpaceCreateDeviceRGB();

        [LibraryImport(CoreGraphicsLib)]
        private static partial void CGColorSpaceRelease(nint self);

        public readonly nint Self;

        public static CGColorSpaceRef CreateDeviceRGB() => new CGColorSpaceRef(CGColorSpaceCreateDeviceRGB());

        public CGColorSpaceRef(nint self)
        {
            if (self == 0)
            {
                throw new ObjCException($"{nameof(CGColorRef)} instantiated with nil self.");
            }

            this.Self = self;
        }

        public void Dispose()
        {
            if (this.Self != 0)
            {
                CGColorSpaceRelease(this.Self);
            }
        }

        public static implicit operator nint(CGColorSpaceRef cfColorRef) => cfColorRef.Self;
    }
}
