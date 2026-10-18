using System.Diagnostics;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.UIKit;
using Xui.Core.Math2D;

namespace Xui.Runtime.IOS.Actual;

public class IOSWindowRootView : UIView
{
    private static readonly Sel SafeAreaInsetsDidChangeSel = new Sel("safeAreaInsetsDidChange");
    private static readonly Sel TraitCollectionDidChangeSel = new Sel("traitCollectionDidChange:");
    private static readonly Sel LayoutSubviewsSel = new Sel("layoutSubviews");
    // UIKit does not expose physical display radii through the public UIScreen API.
    // This selector is present on rounded-corner iPhones and iPads; retain zero as
    // the fallback for square displays or system versions where it is unavailable.
    private static readonly Sel DisplayCornerRadiusSel = new Sel("_displayCornerRadius");
    private static readonly Sel RespondsToSelectorSel = new Sel("respondsToSelector:");

    public static new readonly Class Class = UIView.Class
        .Extend("XUIIOSWindowRootView")
        .AddMethod("drawRect:", DrawRect)
        .AddMethod("safeAreaInsetsDidChange", SafeAreaInsetsDidChange)
        .AddMethod("traitCollectionDidChange:", TraitCollectionDidChange)
        .AddMethod("layoutSubviews", LayoutSubviews)
        .AddMethod("hitTest:withEvent:", (HitCallback)HitTest, "@@:{CGPoint=dd}@")
        .Register();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint HitCallback(nint self, nint sel, CGPoint point, nint evt);

    private static nint HitTest(nint self, nint sel, CGPoint point, nint evt)
    {
        var super = new Super(self, UIView.Class);
        nint hit = HitTestSuper(ref super, sel, point, evt);
        // Respect UIKit subviews (menus, real handles, etc.) and the root's own
        // visibility/bounds. Route extra canvas padding to the original handle.
        if (hit != self) return hit;
        nint handle = Marshalling.Get<IOSWindowRootView>(self).Window.HitTestSelectionHandle(point);
        return handle != 0 ? handle : hit;
    }

    [DllImport(LibObjCLib, EntryPoint = "objc_msgSendSuper")]
    private static extern nint HitTestSuper(ref Super super, nint sel, CGPoint point, nint evt);

    public static void DrawRect(nint self, nint sel, CGRect rect) =>
        Marshalling.Get<IOSWindowRootView>(self).DrawRect(rect);
    
    public static void SafeAreaInsetsDidChange(nint self, nint sel) =>
        Marshalling.Get<IOSWindowRootView>(self).SafeAreaInsetsDidChange(sel);

    public static void TraitCollectionDidChange(nint self, nint sel, nint previousTraits) =>
        Marshalling.Get<IOSWindowRootView>(self).TraitCollectionDidChange(sel, previousTraits);

    public static void LayoutSubviews(nint self, nint sel) =>
        Marshalling.Get<IOSWindowRootView>(self).LayoutSubviews(sel);
    
    public IOSWindowRootView(IOSWindow window) : base(Class.New())
    {
        this.Window = window;
        this.MultipleTouchEnabled = true;
    }
    
    private bool? clearsContextBeforeDrawing;

    protected internal IOSWindow Window { get; }

    public new void SetNeedsDisplay()
    {
        UpdateDrawingCoverage();
        base.SetNeedsDisplay();
    }

    private void UpdateDrawingCoverage()
    {
        // UIKit clears before drawRect, so update this before scheduling the draw.
        var clears = this.Window.Abstract is not Xui.Core.Abstract.Window
            { RootView.Content: Xui.Core.UI.IFullScreenView { DrawsOpaqueFullScreen: true } };
        if (clearsContextBeforeDrawing != clears)
        {
            this.ClearsContextBeforeDrawing = clears;
            clearsContextBeforeDrawing = clears;
        }
    }

    private void DrawRect(CGRect rect) => this.Window.Render(rect);

    protected void SafeAreaInsetsDidChange(nint sel)
    {
        Super super = new Super(this, UIView.Class);
        objc_msgSendSuper(ref super, sel);

        UpdateGeometry();
    }

    protected void TraitCollectionDidChange(nint sel, nint previousTraits)
    {
        Super super = new Super(this, UIView.Class);
        objc_msgSendSuper(ref super, sel, previousTraits);
        Window.DeviceInfo.Refresh();
    }

    protected void LayoutSubviews(nint sel)
    {
        Super super = new Super(this, UIView.Class);
        objc_msgSendSuper(ref super, sel);
        UpdateGeometry();
        UpdateDrawingCoverage();
    }

    private void UpdateGeometry()
    {

        var frame = this.Frame;
        var safeInsets = this.SafeAreaInsets;

        this.Window.DisplayArea = frame;
        this.Window.SafeArea = new Rect(
            frame.Origin.X + safeInsets.Left,
            frame.Origin.Y + safeInsets.Top,
            frame.Size.Width - safeInsets.Left - safeInsets.Right,
            frame.Size.Height - safeInsets.Top - safeInsets.Bottom);
        var screen = Window.Screen;
        this.Window.ScreenCornerRadius = screen != 0 && objc_msgSend_retBool(screen, RespondsToSelectorSel, DisplayCornerRadiusSel)
            ? objc_msgSend_retNFloat(screen, DisplayCornerRadiusSel)
            : 0;
        this.Window.DeviceInfo.Refresh();
    }
}
