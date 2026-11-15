using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Runtime.IOS;

// Native, autoreleased value objects. No managed wrappers or entries in Marshalling:
// UIKit may retain a position/range across callbacks without rooting managed objects.
internal static unsafe class UITextInputValues
{
    internal const string RectEncoding = "{CGRect={CGPoint=dd}{CGSize=dd}}";
    internal const string PointEncoding = "{CGPoint=dd}";
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate CGRect RectGetter(nint self, nint sel);
    [StructLayout(LayoutKind.Sequential)]
    internal struct Value
    {
        public long Session;
        public int Start, End;
        public CGRect Rect;
    }

    private static readonly Sel Autorelease = new("autorelease");
    private static readonly Sel Retain = new("retain");
    private static readonly Class PositionClass = BuildPosition();
    private static readonly Class RangeClass = BuildRange();
    private static readonly Class SelectionClass = BuildSelection();
    private static readonly nint PositionOffset = Offset(PositionClass);
    private static readonly nint RangeOffset = Offset(RangeClass);
    private static readonly nint SelectionOffset = Offset(SelectionClass);

    private static Class BuildPosition() => new Class(UIKit.Lib, "UITextPosition").Extend("XUITextPosition")
        .AddValueStorage("xuiValue", 64)
        .AddMethod("copyWithZone:", (IdSelId_Id)Copy, "@@:^v")
        .Register();
    private static Class BuildRange() => new Class(UIKit.Lib, "UITextRange").Extend("XUITextRange")
        .AddValueStorage("xuiValue", 64)
        .AddMethod("start", (IdSel_Id)Start, "@@:")
        .AddMethod("end", (IdSel_Id)End, "@@:")
        .AddMethod("isEmpty", (IdSel_Bool)Empty, "B@:")
        .AddMethod("copyWithZone:", (IdSelId_Id)Copy, "@@:^v")
        .Register();
    private static Class BuildSelection() => new Class(UIKit.Lib, "UITextSelectionRect").Extend("XUITextSelectionRect")
        .AddValueStorage("xuiValue", 64)
        .AddMethod("rect", (RectGetter)Rect, RectEncoding + "@:")
        .AddMethod("writingDirection", (IdSel_Id)Direction, "q@:")
        .AddMethod("containsStart", (IdSel_Bool)Yes, "B@:")
        .AddMethod("containsEnd", (IdSel_Bool)Yes, "B@:")
        .AddMethod("isVertical", (IdSel_Bool)No, "B@:")
        .Register();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate nint IdSelId_Id(nint self, nint sel, nint arg);
    private static nint Copy(nint self, nint sel, nint zone) => objc_msgSend_retIntPtr(self, Retain);
    private static nint Start(nint self, nint sel) { var v = Read(self, RangeOffset); return Position(v.Session, v.Start); }
    private static nint End(nint self, nint sel) { var v = Read(self, RangeOffset); return Position(v.Session, v.End); }
    private static bool Empty(nint self, nint sel) { var v = Read(self, RangeOffset); return v.Start == v.End; }
    private static CGRect Rect(nint self, nint sel) => Read(self, SelectionOffset).Rect;
    private static nint Direction(nint self, nint sel) => 0; // Single-line LTR editor.
    private static bool Yes(nint self, nint sel) => true;
    private static bool No(nint self, nint sel) => false;

    internal static nint Position(long session, int index) => Create(PositionClass, PositionOffset, new() { Session = session, Start = index, End = index });
    internal static nint Range(long session, int start, int end) => Create(RangeClass, RangeOffset, new() { Session = session, Start = start, End = end });
    internal static nint SelectionRect(CGRect rect) => Create(SelectionClass, SelectionOffset, new() { Rect = rect });
    internal static bool TryPosition(nint obj, long session, out int index)
    {
        index = 0;
        if (obj == 0 || object_getClass(obj) != PositionClass.Id) return false;
        var v = Read(obj, PositionOffset);
        index = v.Start;
        return v.Session == session;
    }
    internal static bool TryRange(nint obj, long session, out int start, out int end)
    {
        start = end = 0;
        if (obj == 0 || object_getClass(obj) != RangeClass.Id) return false;
        var v = Read(obj, RangeOffset);
        start = v.Start; end = v.End;
        return v.Session == session;
    }
    private static Value Read(nint obj, nint offset) => *(Value*)(obj + offset);
    private static nint Create(Class cls, nint offset, Value value)
    {
        nint obj = cls.New();
        *(Value*)(obj + offset) = value;
        return objc_msgSend_retIntPtr(obj, Autorelease);
    }
    private static nint Offset(Class cls) => ivar_getOffset(class_getInstanceVariable(cls, "xuiValue"));
    [DllImport(LibObjCLib)] private static extern nint class_getInstanceVariable(nint cls, string name);
    [DllImport(LibObjCLib)] private static extern nint ivar_getOffset(nint ivar);
}
