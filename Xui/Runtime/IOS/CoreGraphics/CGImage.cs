using System;

namespace Xui.Runtime.IOS;

public static partial class CoreGraphics
{
    /// <summary>Long-lived CoreGraphics image owned by an iOS image resource.</summary>
    public sealed class CGImage : IDisposable
    {
        private nint pointer;

        public CGImage(nint pointer)
        {
            if (pointer == 0)
                throw new ObjCException($"{nameof(CGImage)} instantiated with nil pointer.");
            this.pointer = pointer;
        }

        public uint Width => (uint)CGImageRef.CGImageGetWidth(pointer);
        public uint Height => (uint)CGImageRef.CGImageGetHeight(pointer);

        public static implicit operator nint(CGImage? image) => image?.pointer ?? 0;

        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }

        ~CGImage() => Release();

        private void Release()
        {
            if (pointer != 0)
            {
                CGImageRef.CGImageRelease(pointer);
                pointer = 0;
            }
        }
    }
}
