using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class D2D1
{
    public unsafe class DeviceContext : RenderTarget
    {
        public static new readonly Guid IID = new Guid("e8f7fe7a-191c-466d-ad95-975678bda998");

        public DeviceContext(void* ptr) : base(ptr)
        {
        }

        /// <summary>
        /// Creates a <c>ID2D1Bitmap1</c> by letting D2D1 decode and upload a <c>IWICBitmapSource</c> directly.
        /// D2D1 handles format conversion internally — no manual pixel copy required.
        /// vtable [58] — ID2D1DeviceContext::CreateBitmapFromWicBitmap
        /// </summary>
        public Bitmap1 CreateBitmapFromWicBitmap(COM.Unknown wicBitmapSource)
        {
            void* ppv;
            Marshal.ThrowExceptionForHR(
                ((delegate* unmanaged[MemberFunction]<void*, void*, void*, void**, int>)this[58])
                (this, wicBitmapSource, null, &ppv));
            return new Bitmap1(ppv);
        }

        public Bitmap1 CreateBitmapFromDxgiSurface(DXGI.Surface surface, in BitmapProperties1 bitmapProperties1)
        {
            void* ppv;
            fixed(BitmapProperties1* bitmapProperties1Ptr = &bitmapProperties1)
            {
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, void*, BitmapProperties1*, void**, int> )this[62])(this, surface, bitmapProperties1Ptr, &ppv));
            }
            return new Bitmap1(ppv);
        }

        public void SetTarget(Bitmap1? d2dTargetBitmap) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void> )this[74])(this, d2dTargetBitmap);
    }
}