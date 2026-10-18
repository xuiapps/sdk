using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS.Actual;

// UIKit supplies word/sentence tokenization. The editor supplies its single-line
// layout and maps visual directions to storage directions for the base tokenizer.
internal sealed class IOSTextTokenizer : NSObject
{
    private static readonly Class BaseClass = new(UIKit.Lib, "UITextInputStringTokenizer");
    private static new readonly Class Class = BaseClass.Extend("XUITextInputTokenizer")
        .AddMethod("rangeEnclosingPosition:withGranularity:inDirection:", (Query)Range, "@@:@qq")
        .AddMethod("isPosition:atBoundary:inDirection:", (BoolQuery)AtBoundary, "B@:@qq")
        .AddMethod("positionFromPosition:toBoundary:inDirection:", (Query)ToBoundary, "@@:@qq")
        .AddMethod("isPosition:withinTextUnit:inDirection:", (BoolQuery)Within, "B@:@qq")
        .Register();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint Query(nint self, nint sel, nint position, nint granularity, nint direction);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool BoolQuery(nint self, nint sel, nint position, nint granularity, nint direction);
    private readonly IOSSoftKeyboard keyboard;
    internal IOSTextTokenizer(IOSSoftKeyboard keyboard)
        : base(objc_msgSend_retIntPtr(Class.Alloc(), new Sel("initWithTextInput:"), (nint)keyboard)) { this.keyboard = keyboard; }
    private static bool Backward(nint direction) => direction is 1 or 3 or 4;
    private static nint StorageDirection(nint direction) => Backward(direction) ? 1 : 0;
    private static nint Range(nint self, nint sel, nint position, nint granularity, nint direction)
    {
        var k = Marshalling.Get<IOSTextTokenizer>(self).keyboard;
        if (!k.TryIndex(position, out _)) return 0;
        if (granularity == 4) return k.DocumentRange();
        var super = new Super(self, BaseClass);
        return SendSuper(ref super, sel, position, granularity, StorageDirection(direction));
    }
    private static nint ToBoundary(nint self, nint sel, nint position, nint granularity, nint direction)
    {
        var k = Marshalling.Get<IOSTextTokenizer>(self).keyboard;
        if (!k.TryIndex(position, out _)) return 0;
        if (granularity == 4) return k.PositionAt(Backward(direction) ? 0 : k.Length);
        var super = new Super(self, BaseClass);
        return SendSuper(ref super, sel, position, granularity, StorageDirection(direction));
    }
    private static bool AtBoundary(nint self, nint sel, nint position, nint granularity, nint direction)
    {
        var k = Marshalling.Get<IOSTextTokenizer>(self).keyboard;
        if (!k.TryIndex(position, out int index)) return false;
        if (granularity == 4) return index == (Backward(direction) ? 0 : k.Length);
        var super = new Super(self, BaseClass);
        return SendSuperBool(ref super, sel, position, granularity, StorageDirection(direction));
    }
    private static bool Within(nint self, nint sel, nint position, nint granularity, nint direction)
    {
        var k = Marshalling.Get<IOSTextTokenizer>(self).keyboard;
        if (!k.TryIndex(position, out int index)) return false;
        if (granularity == 4) return Backward(direction) ? index > 0 : index < k.Length;
        var super = new Super(self, BaseClass);
        return SendSuperBool(ref super, sel, position, granularity, StorageDirection(direction));
    }
    [DllImport(LibObjCLib, EntryPoint = "objc_msgSendSuper")]
    private static extern nint SendSuper(ref Super super, nint sel, nint position, nint granularity, nint direction);
    [DllImport(LibObjCLib, EntryPoint = "objc_msgSendSuper")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SendSuperBool(ref Super super, nint sel, nint position, nint granularity, nint direction);
}
