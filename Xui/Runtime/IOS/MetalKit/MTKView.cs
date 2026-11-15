using System.Runtime.InteropServices;
using Xui.Core.Math2D;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Runtime.IOS;

internal static partial class MetalKit
{
    private static readonly nint Lib = NativeLibrary.Load("/System/Library/Frameworks/MetalKit.framework/MetalKit");

    internal class MTKView : UIKit.UIView
    {
        internal static new readonly Class Class = new Class(Lib, "MTKView")
            .Extend("XuiMetalSurfaceView").AddMethod("drawRect:", DrawRect).Register();
        private static readonly Sel DrawSel = new("draw"), DrawableSel = new("currentDrawable"),
            DepthSel = new("depthStencilTexture"), SizeSel = new("setDrawableSize:");

        internal MTKView(nint device) : base(Init(Class.Alloc(), new Sel("initWithFrame:device:"), default, device))
        {
            ObjC.objc_msgSend(this, new Sel("setPaused:"), true);
            ObjC.objc_msgSend(this, new Sel("setEnableSetNeedsDisplay:"), false);
            ObjC.objc_msgSend(this, new Sel("setAutoResizeDrawable:"), false);
            ObjC.objc_msgSend(this, new Sel("setUserInteractionEnabled:"), false);
            ObjC.objc_msgSend(this, new Sel("setColorPixelFormat:"), (nuint)80); // BGRA8Unorm
            ObjC.objc_msgSend(this, new Sel("setDepthStencilPixelFormat:"), (nuint)252); // Depth32Float
            ObjC.objc_msgSend(this, new Sel("setFramebufferOnly:"), true);
        }

        internal nint CurrentDrawable => objc_msgSend_retIntPtr(this, DrawableSel);
        internal nint DepthTexture => objc_msgSend_retIntPtr(this, DepthSel);
        internal void SetDrawableSize(Size pixels) => UIKit.CompositionNative.SetSize(this, SizeSel, new CGSize(pixels.Width, pixels.Height));
        internal void Draw() => ObjC.objc_msgSend(this, DrawSel);
        protected virtual void DrawFrame() { }
        private static void DrawRect(nint self, nint sel, CGRect rect) => Marshalling.Get<MTKView>(self).DrawFrame();
    }

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static partial nint Init(nint obj, nint sel, CGRect frame, nint device);
}
