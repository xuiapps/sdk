using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.CoreFoundation;

namespace Xui.Runtime.IOS;

/// <summary>Native keyboard accessory; the toolbar owns its items, which target the text responder.</summary>
internal sealed class UITextInputAccessory : UIKit.UIView
{
    private static readonly Class ToolbarClass = new(UIKit.Lib, "UIToolbar");
    private static readonly Class ItemClass = new(UIKit.Lib, "UIBarButtonItem");
    private static readonly Sel TitleInitSel = new("initWithTitle:style:target:action:");
    private static readonly Sel SystemInitSel = new("initWithBarButtonSystemItem:target:action:");
    private static readonly Sel EnabledSel = new("setEnabled:");
    private readonly nint previous, next;

    internal UITextInputAccessory(nint target, nint previousAction, nint nextAction, nint doneAction)
        : base(ToolbarClass.New())
    {
        Frame = new CoreGraphics.CGRect(0, 0, 0, 44);
        ObjC.objc_msgSend(this, new Sel("setAutoresizingMask:"), (nuint)2); // Flexible width
        previous = CreateArrow("↑", "Previous field", target, previousAction);
        next = CreateArrow("↓", "Next field", target, nextAction);
        nint space = InitSystem(ItemClass.Alloc(), SystemInitSel, 5, 0, 0);
        nint done = InitSystem(ItemClass.Alloc(), SystemInitSel, 0, target, doneAction);
        using var items = new CFMutableArrayRef();
        items.Add(previous); items.Add(next); items.Add(space); items.Add(done);
        ObjC.objc_msgSend(this, new Sel("setItems:"), (nint)items);
        CFRelease(previous); CFRelease(next); CFRelease(space); CFRelease(done);
    }

    internal void Update(bool canPrevious, bool canNext)
    {
        ObjC.objc_msgSend(previous, EnabledSel, canPrevious);
        ObjC.objc_msgSend(next, EnabledSel, canNext);
    }

    private static nint CreateArrow(string title, string label, nint target, nint action)
    {
        using var text = new CFStringRef(title);
        using var accessibilityLabel = new CFStringRef(label);
        nint item = InitTitle(ItemClass.Alloc(), TitleInitSel, text, 0, target, action);
        ObjC.objc_msgSend(item, new Sel("setWidth:"), (NFloat)44);
        ObjC.objc_msgSend(item, new Sel("setAccessibilityLabel:"), (nint)accessibilityLabel);
        return item;
    }

    [DllImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static extern nint InitTitle(nint obj, nint sel, nint title, nint style, nint target, nint action);
    [DllImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static extern nint InitSystem(nint obj, nint sel, nint item, nint target, nint action);
}
