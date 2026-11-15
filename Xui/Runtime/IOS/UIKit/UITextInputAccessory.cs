using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.CoreFoundation;

namespace Xui.Runtime.IOS;

/// <summary>Native keyboard accessory; the toolbar owns its items, which target the text responder.</summary>
internal sealed class UITextInputAccessory : UIKit.UIView
{
    private static readonly Class ToolbarClass = new(UIKit.Lib, "UIToolbar");
    private static readonly Class ItemClass = new(UIKit.Lib, "UIBarButtonItem");
    private static readonly Class ImageClass = new(UIKit.Lib, "UIImage");
    private static readonly Sel ImageInitSel = new("initWithImage:style:target:action:");
    private static readonly Sel SystemImageSel = new("systemImageNamed:");
    private static readonly Sel SystemInitSel = new("initWithBarButtonSystemItem:target:action:");
    private static readonly Sel EnabledSel = new("setEnabled:");
    private static readonly Sel HideBackgroundSel = new("setHidesSharedBackground:");
    private static readonly Sel AutoresizingSel = new("setAutoresizingMask:");
    private readonly nint previous, next;

    internal UITextInputAccessory(nint target, nint previousAction, nint nextAction, nint doneAction)
        : base()
    {
        Frame = new CoreGraphics.CGRect(0, 0, 0, 44);
        ObjC.objc_msgSend(this, AutoresizingSel, (nuint)2); // Flexible width

        // A single edge-to-edge material hosts the whole accessory. The toolbar
        // stays transparent; its items must not create separate floating capsules.
        var blur = ObjC.objc_msgSend_retIntPtr(objc_getClass("UIBlurEffect"), new Sel("effectWithStyle:"), (nint)8);
        var effect = ObjC.objc_msgSend_retIntPtr(ObjC.objc_msgSend_retIntPtr(objc_getClass("UIVisualEffectView"), new Sel("alloc")),
            new Sel("initWithEffect:"), blur);
        var toolbar = ToolbarClass.New();
        var appearance = ObjC.objc_msgSend_retIntPtr(objc_getClass("UIToolbarAppearance"), new Sel("new"));
        ObjC.objc_msgSend(appearance, new Sel("configureWithTransparentBackground"));
        ObjC.objc_msgSend(toolbar, new Sel("setStandardAppearance:"), appearance);
        ObjC.objc_msgSend(toolbar, new Sel("setCompactAppearance:"), appearance);
        CFRelease(appearance);
        UIKit.CompositionNative.SetRect(effect, SetFrameSel, Frame);
        UIKit.CompositionNative.SetRect(toolbar, SetFrameSel, Frame);
        ObjC.objc_msgSend(effect, AutoresizingSel, (nuint)18); // Flexible width and height
        ObjC.objc_msgSend(toolbar, AutoresizingSel, (nuint)18);
        ObjC.objc_msgSend(this, new Sel("addSubview:"), effect);
        var contentView = ObjC.objc_msgSend_retIntPtr(effect, new Sel("contentView"));
        ObjC.objc_msgSend(contentView, new Sel("addSubview:"), toolbar);
        previous = CreateSymbol("chevron.up", "Previous field", target, previousAction);
        next = CreateSymbol("chevron.down", "Next field", target, nextAction);
        nint space = InitSystem(ItemClass.Alloc(), SystemInitSel, 5, 0, 0);
        nint done = CreateSymbol("checkmark", "Done", target, doneAction);
        using var items = new CFMutableArrayRef();
        items.Add(previous); items.Add(next); items.Add(space); items.Add(done);
        ObjC.objc_msgSend(toolbar, new Sel("setItems:"), (nint)items);
        CFRelease(previous); CFRelease(next); CFRelease(space); CFRelease(done);
        // The view hierarchy owns the effect and toolbar, and the toolbar owns its items.
        CFRelease(toolbar); CFRelease(effect);
    }

    internal void Update(bool canPrevious, bool canNext)
    {
        ObjC.objc_msgSend(previous, EnabledSel, canPrevious);
        ObjC.objc_msgSend(next, EnabledSel, canNext);
    }

    private static nint CreateSymbol(string symbol, string label, nint target, nint action)
    {
        using var name = new CFStringRef(symbol);
        using var accessibilityLabel = new CFStringRef(label);
        // systemImageNamed returns a borrowed image; the bar item retains it.
        var image = ObjC.objc_msgSend_retIntPtr(ImageClass, SystemImageSel, name);
        nint item = InitImage(ItemClass.Alloc(), ImageInitSel, image, 0, target, action);
        ObjC.objc_msgSend(item, new Sel("setWidth:"), (NFloat)44);
        ObjC.objc_msgSend(item, new Sel("setAccessibilityLabel:"), (nint)accessibilityLabel);
        ObjC.objc_msgSend(item, new Sel("setAccessibilityIdentifier:"), (nint)accessibilityLabel);
        if (ObjC.objc_msgSend_retBool(item, new Sel("respondsToSelector:"), HideBackgroundSel))
            ObjC.objc_msgSend(item, HideBackgroundSel, true);
        return item;
    }

    [DllImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static extern nint InitImage(nint obj, nint sel, nint image, nint style, nint target, nint action);
    [DllImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static extern nint InitSystem(nint obj, nint sel, nint item, nint target, nint action);
}
