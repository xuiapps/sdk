using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.UIKit;
using static Xui.Runtime.IOS.CoreFoundation;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.UITextInputValues;

namespace Xui.Runtime.IOS.Actual;

/// <summary>UIKit text input backed by the focused Xui editor, with native selection interaction.</summary>
public sealed unsafe class IOSSoftKeyboard : UIView
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint ThreeArgs(nint self, nint sel, nint a, nint b, nint c);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate CGRect RectArg(nint self, nint sel, nint arg);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint PointArg(nint self, nint sel, CGPoint point);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint PointRangeArg(nint self, nint sel, CGPoint point, nint range);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void MarkedArg(nint self, nint sel, nint text, NativeRange range);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool InteractionPoint(nint self, nint sel, nint interaction, CGPoint point);
    [StructLayout(LayoutKind.Sequential)] private struct NativeRange { public nuint Location, Length; }

    public static new readonly Class Class = BuildClass();
    private static Class BuildClass() => UIView.Class.Extend("XUIIOSSoftKeyboard")
        .AddProtocol(UITextInput)
        .AddProtocol(new Protocol("UITextInteractionDelegate"))
        .AddMethod("canBecomeFirstResponder", (IdSel_Bool)CanBecome, "B@:")
        .AddMethod("nextResponder", (IdSel_Id)NextResponderStatic, "@@:")
        .AddMethod("textInputView", (IdSel_Id)InputView, "@@:")
        .AddMethod("keyboardType", (IdSel_Id)KeyboardType, "q@:")
        .AddMethod("keyboardAppearance", (IdSel_Id)Zero, "q@:")
        .AddMethod("returnKeyType", (IdSel_Id)ReturnKeyType, "q@:")
        .AddMethod("textContentType", (IdSel_Id)ContentType, "@@:")
        .AddMethod("autocapitalizationType", (IdSel_Id)Capitalization, "q@:")
        .AddMethod("autocorrectionType", (IdSel_Id)Correction, "q@:")
        .AddMethod("spellCheckingType", (IdSel_Id)Correction, "q@:")
        .AddMethod("smartQuotesType", (IdSel_Id)Correction, "q@:")
        .AddMethod("smartDashesType", (IdSel_Id)Correction, "q@:")
        .AddMethod("inputAccessoryView", (IdSel_Id)AccessoryView, "@@:")
        .AddMethod("previousInput:", (IdSelId_Void)PreviousInput, "v@:@")
        .AddMethod("nextInput:", (IdSelId_Void)NextInput, "v@:@")
        .AddMethod("doneInput:", (IdSelId_Void)DoneInput, "v@:@")
        .AddMethod("isSecureTextEntry", (IdSel_Bool)Secure, "B@:")
        .AddMethod("hasText", (IdSel_Bool)HasText, "B@:")
        .AddMethod("insertText:", (IdSelId_Void)Insert, "v@:@")
        .AddMethod("deleteBackward", (IdSel_Void)Delete, "v@:")
        .AddMethod("textInRange:", (IdSelId_Id)TextInRange, "@@:@")
        .AddMethod("replaceRange:withText:", (IdSelIdId_Void)Replace, "v@:@@")
        .AddMethod("selectedTextRange", (IdSel_Id)Selected, "@@:")
        .AddMethod("setSelectedTextRange:", (IdSelId_Void)SetSelected, "v@:@")
        .AddMethod("markedTextRange", (IdSel_Id)Marked, "@@:")
        .AddMethod("markedTextStyle", (IdSel_Id)MarkedStyle, "@@:")
        .AddMethod("setMarkedTextStyle:", (IdSelId_Void)SetMarkedStyle, "v@:@")
        .AddMethod("setMarkedText:selectedRange:", (MarkedArg)SetMarked, "v@:@{_NSRange=QQ}")
        .AddMethod("unmarkText", (IdSel_Void)Unmark, "v@:")
        .AddMethod("beginningOfDocument", (IdSel_Id)Beginning, "@@:")
        .AddMethod("endOfDocument", (IdSel_Id)End, "@@:")
        .AddMethod("textRangeFromPosition:toPosition:", (IdSelIdId_Id)MakeRange, "@@:@@")
        .AddMethod("positionFromPosition:offset:", (IdSelIdId_Id)OffsetPosition, "@@:@q")
        .AddMethod("positionFromPosition:inDirection:offset:", (ThreeArgs)DirectionPosition, "@@:@qq")
        .AddMethod("comparePosition:toPosition:", (IdSelIdId_Id)Compare, "q@:@@")
        .AddMethod("offsetFromPosition:toPosition:", (IdSelIdId_Id)Offset, "q@:@@")
        .AddMethod("inputDelegate", (IdSel_Id)GetDelegate, "@@:")
        .AddMethod("setInputDelegate:", (IdSelId_Void)SetDelegate, "v@:@")
        .AddMethod("tokenizer", (IdSel_Id)Tokenizer, "@@:")
        .AddMethod("positionWithinRange:farthestInDirection:", (IdSelIdId_Id)Farthest, "@@:@q")
        .AddMethod("characterRangeByExtendingPosition:inDirection:", (IdSelIdId_Id)Extend, "@@:@q")
        .AddMethod("baseWritingDirectionForPosition:inDirection:", (IdSelIdId_Id)WritingDirection, "q@:@q")
        .AddMethod("setBaseWritingDirection:forRange:", (IdSelIdId_Void)SetWritingDirection, "v@:q@")
        .AddMethod("firstRectForRange:", (RectArg)RangeRect, RectEncoding + "@:@")
        .AddMethod("caretRectForPosition:", (RectArg)CaretRect, RectEncoding + "@:@")
        .AddMethod("selectionRectsForRange:", (IdSelId_Id)SelectionRects, "@@:@")
        .AddMethod("closestPositionToPoint:", (PointArg)Closest, "@@:" + PointEncoding)
        .AddMethod("closestPositionToPoint:withinRange:", (PointRangeArg)ClosestInRange, "@@:" + PointEncoding + "@")
        .AddMethod("characterRangeAtPoint:", (PointArg)CharacterAtPoint, "@@:" + PointEncoding)
        .AddMethod("interactionShouldBegin:atPoint:", (InteractionPoint)InteractionShouldBegin, "B@:@" + PointEncoding)
        .Register();

    private static readonly Sel TextWillSel = new("textWillChange:");
    private static readonly Sel TextDidSel = new("textDidChange:");
    private static readonly Sel SelectionWillSel = new("selectionWillChange:");
    private static readonly Sel SelectionDidSel = new("selectionDidChange:");
    private static readonly Sel ConvertPointSel = new("convertPoint:fromView:");
    private static readonly Sel SetInputSel = new("setTextInput:");
    private static readonly Sel SetDelegateSel = new("setDelegate:");
    private static readonly Sel InteractionsSel = new("interactions");
    private static readonly Sel CountSel = new("count");
    private static readonly Sel ObjectAtIndexSel = new("objectAtIndex:");
    private static readonly Sel IsKindOfClassSel = new("isKindOfClass:");
    private static readonly Sel TextInputSel = new("textInput");
    private static readonly Sel ActivatedSel = new("setActivated:");
    private static readonly Sel SelectionUpdateSel = new("setNeedsSelectionUpdate");
    private static readonly Sel LayoutSelectionSel = new("layoutManagedSubviews");
    private static readonly Sel HandleViewsSel = new("handleViews");
    private static readonly Sel HiddenSel = new("isHidden"), AlphaSel = new("alpha"), SuperviewSel = new("superview");
    private static readonly Sel IsDescendantSel = new("isDescendantOfView:");
    private static readonly Sel IsFirstResponderSel = new("isFirstResponder");
    private static readonly Sel AddInteractionSel = new("addInteraction:");
    private static readonly Sel RemoveInteractionSel = new("removeInteraction:");
    private static readonly Sel ArraySel = new("array");
    private static readonly Sel ArrayObjectSel = new("arrayWithObject:");
    private static readonly Class ArrayClass = new(Foundation.Lib, "NSArray");
    private static long nextSession;
    private long session;
    private nint interaction;
    private nint selectionDisplay;
    private UITextInputAccessory? accessory;
    private Rect displayedStart, displayedEnd;
    private IOSTextTokenizer? tokenizer;
    private nint markedStyle;
    private readonly nint delegateSlot = (nint)NativeMemory.AllocZeroed((nuint)sizeof(nint));
    private bool nativeEdit;
    private bool interactionAttached;
    private bool suppressInteraction;
    private bool disposed;
    private long focusTouchTime;
    private nint selectionTouch;
    private IOSTextSelectionDrag selectionDrag;

    public IOSSoftKeyboard(IOSWindow window) : base(Class.New())
    {
        Window = window;
        // A view responder gives UIKit a window and native focus lifecycle. The
        // canvas remains the textInputView and hosts all selection gestures/UI.
        ObjC.objc_msgSend(this, new Sel("setUserInteractionEnabled:"), false);
        ObjC.objc_msgSend(window.RootView, new Sel("addSubview:"), (nint)this);
    }
    internal IOSWindow Window { get; }
    public nint NextResponder { get; set; }
    internal ITextEditingClient? Client { get; private set; }
    private TextEditingState? Editor => Client?.Editor;

    internal void SetClient(ITextEditingClient? client)
    {
        if (ReferenceEquals(Client, client)) return;
        Notify(TextEditingChange.Text | TextEditingChange.Selection, true);
        if (Client is { } previous)
        {
            previous.Editor.UnmarkText();
            previous.Editor.Changing -= Changing;
            previous.Editor.Changed -= Changed;
            previous.NativeSelection = false;
        }
        DetachInteraction();
        Client = client;
        accessory?.Update(client?.CanPerformInputAction(TextInputAction.Previous) == true, client is not null);
        session = Interlocked.Increment(ref nextSession);
        if (client is null)
        {
            Notify(TextEditingChange.Text | TextEditingChange.Selection, false);
            return;
        }
        // Keep the Core selection presentation on systems without the public
        // selection-display API. Range editing/keyboard input still work there.
        if (objc_getClass("UITextSelectionDisplayInteraction") == 0)
        {
            client.Editor.Changing += Changing;
            client.Editor.Changed += Changed;
            Notify(TextEditingChange.Text | TextEditingChange.Selection, false);
            return;
        }
        if (interaction == 0)
        {
            var cls = new Class(UIKit.Lib, "UITextInteraction");
            interaction = objc_msgSend_retIntPtr(cls, new Sel("textInteractionForMode:"), (nint)0);
            CFRetain(interaction);
            ObjC.objc_msgSend(interaction, SetInputSel, (nint)this);
            ObjC.objc_msgSend(interaction, SetDelegateSel, (nint)this);
        }
        if (objc_msgSend_retBool(this, IsFirstResponderSel)) ActivateInteraction();
        client.Editor.Changing += Changing;
        client.Editor.Changed += Changed;
        Notify(TextEditingChange.Text | TextEditingChange.Selection, false);
    }

    internal void ActivateInteraction()
    {
        if (Client is null || interaction == 0 || interactionAttached) return;
        // Install the editable interaction after becoming first responder. Its
        // activation is otherwise missed when textInput is a separate responder.
        ObjC.objc_msgSend(Window.RootView, AddInteractionSel, interaction);
        interactionAttached = true;
        // UITextInteraction owns this public display component. Explicitly activate
        // it for our separate responder and update it after canvas-side edits/layout.
        nint displays = objc_msgSend_retIntPtr(Window.RootView, InteractionsSel);
        nint displayClass = objc_getClass("UITextSelectionDisplayInteraction");
        nuint count = objc_msgSend_retNUInt(displays, CountSel);
        for (nuint i = 0; i < count; i++)
        {
            nint item = objc_msgSend_retIntPtr(displays, ObjectAtIndexSel, (nint)i);
            if (objc_msgSend_retBool(item, IsKindOfClassSel, displayClass)
                && objc_msgSend_retIntPtr(item, TextInputSel) == (nint)this)
            {
                selectionDisplay = item;
                CFRetain(item);
                break;
            }
        }
        Client.NativeSelection = selectionDisplay != 0;
        UpdateSelectionDisplay();
    }

    private void UpdateSelectionDisplay()
    {
        if (selectionDisplay == 0) return;
        ObjC.objc_msgSend(selectionDisplay, ActivatedSel, true);
        ObjC.objc_msgSend(selectionDisplay, SelectionUpdateSel);
        ObjC.objc_msgSend(selectionDisplay, LayoutSelectionSel);
    }

    internal void RefreshSelectionGeometry()
    {
        if (Client is null || selectionDisplay == 0) return;
        var range = Client.Editor.Selection;
        var start = Client.GetCaretRect(range.Start, Window.TextMeasureContext);
        var end = Client.GetCaretRect(range.End, Window.TextMeasureContext);
        if (start == displayedStart && end == displayedEnd) return;
        displayedStart = start; displayedEnd = end;
        UpdateSelectionDisplay();
    }

    // Preserve select-all through the initial tap. A hold can still start native
    // precision selection; the next contact immediately enables ordinary caret taps.
    internal void BeginTouches() { suppressInteraction = false; focusTouchTime = 0; }
    internal void FocusedByTouch() => focusTouchTime = Stopwatch.GetTimestamp();
    internal void CancelTouchSelection()
    {
        suppressInteraction = true;
        DetachInteraction(); // Cancels active recognizers if navigation steals capture.
    }
    internal void EndTouches()
    {
        if (!interactionAttached && Client is not null && interaction != 0)
        {
            ActivateInteraction();
        }
    }

    internal bool OwnsHandle(Point point) => HandleAtWindowPoint(point) != 0;

    internal void BeginHandleTouch(nint touch, nint target, Point point)
    {
        if (selectionTouch != 0 || !TryGetHandles(out nint leading, out nint trailing)) return;
        // Prefer UIKit's actual hit target when the expanded endpoint areas overlap.
        nint handle = IsHandleTarget(target, leading) ? leading
            : IsHandleTarget(target, trailing) ? trailing : HandleAtWindowPoint(point);
        if (handle == 0) return;
        selectionTouch = touch;
        selectionDrag.Begin(Client!.Editor.Selection, handle == leading);
    }

    internal void EndHandleTouch(nint touch)
    {
        if (selectionTouch != touch) return;
        selectionTouch = 0;
        selectionDrag.End();
    }

    private static bool IsHandleTarget(nint target, nint handle) => target != 0 && HandleVisible(handle)
        && (target == handle || objc_msgSend_retBool(target, IsDescendantSel, handle));

    internal nint HandleAtInputPoint(CGPoint point) => HandleAtWindowPoint(WindowPoint(point));

    private nint HandleAtWindowPoint(Point point)
    {
        if (!TryGetHandles(out nint leading, out nint trailing)) return 0;
        // Keep UIKit's actual handle objects in its selection display. The loupe
        // uses system-only shape behavior that a UIView protocol wrapper lacks.
        // Only expand touch routing; neither native geometry nor ownership changes.
        var range = Client!.Editor.Selection;
        var start = Client.GetCaretRect(range.Start, Window.TextMeasureContext);
        var end = Client.GetCaretRect(range.End, Window.TextMeasureContext);
        return IOSTextSelectionHitTest.Hit(point, start, end, HandleVisible(leading), HandleVisible(trailing)) switch
        {
            0 => leading,
            1 => trailing,
            _ => 0,
        };
    }

    private bool TryGetHandles(out nint leading, out nint trailing)
    {
        leading = trailing = 0;
        if (Client is null || !Client.NativeSelection || selectionDisplay == 0
            || Client.Editor.Selection.Length == 0) return false;
        nint handles = objc_msgSend_retIntPtr(selectionDisplay, HandleViewsSel);
        if (handles == 0 || objc_msgSend_retNUInt(handles, CountSel) != 2) return false;
        leading = objc_msgSend_retIntPtr(handles, ObjectAtIndexSel, (nint)0);
        trailing = objc_msgSend_retIntPtr(handles, ObjectAtIndexSel, (nint)1);
        return true;
    }

    private static bool HandleVisible(nint handle) => handle != 0
        && objc_msgSend_retIntPtr(handle, SuperviewSel) != 0
        && !objc_msgSend_retBool(handle, HiddenSel) && objc_msgSend_retNFloat(handle, AlphaSel) > 0.01;

    private void DetachInteraction()
    {
        EndHandleTouch(selectionTouch);
        if (!interactionAttached) return;
        if (selectionDisplay != 0)
        {
            ObjC.objc_msgSend(selectionDisplay, ActivatedSel, false);
            // Cancel pending selection updates before the responder can be disposed.
            ObjC.objc_msgSend(NSObject.Class, new Sel("cancelPreviousPerformRequestsWithTarget:"), selectionDisplay);
            CFRelease(selectionDisplay); selectionDisplay = 0;
        }
        ObjC.objc_msgSend(Window.RootView, RemoveInteractionSel, interaction);
        interactionAttached = false;
    }

    private void Changing(TextEditingChange change)
    {
        if ((change & TextEditingChange.Text) != 0) EndHandleTouch(selectionTouch);
        Notify(change, true);
    }
    private void Changed(TextEditingChange change)
    {
        Notify(change, false);
        UpdateSelectionDisplay();
    }
    private void Notify(TextEditingChange change, bool before)
    {
        if (nativeEdit) return; // UIKit already owns this transaction.
        nint inputDelegate = objc_loadWeakRetained(delegateSlot);
        if (inputDelegate == 0) return;
        try
        {
            if ((change & TextEditingChange.Text) != 0)
                ObjC.objc_msgSend(inputDelegate, before ? TextWillSel : TextDidSel, (nint)this);
            if ((change & TextEditingChange.Selection) != 0)
                ObjC.objc_msgSend(inputDelegate, before ? SelectionWillSel : SelectionDidSel, (nint)this);
        }
        finally { CFRelease(inputDelegate); }
    }

    private static IOSSoftKeyboard K(nint self) => Marshalling.Get<IOSSoftKeyboard>(self);
    private bool ReadPosition(nint value, out int index)
    {
        if (TryPosition(value, session, out index) && Editor is { } editor && index >= 0 && index <= editor.Text.Length) return true;
        index = 0; return false;
    }
    private bool ReadRange(nint value, out TextRange range)
    {
        range = default;
        if (!TryRange(value, session, out int start, out int end) || Editor is not { } editor
            || start < 0 || end < start || end > editor.Text.Length) return false;
        range = new(start, end); return true;
    }
    internal nint PositionAt(int index) => Position(session, index);
    internal nint DocumentRange() => Range(session, 0, Editor?.Text.Length ?? 0);
    internal bool TryIndex(nint position, out int index) => ReadPosition(position, out index);
    internal int Length => Editor?.Text.Length ?? 0;
    private nint NativeRangeOf(TextRange range) => Range(session, range.Start, range.End);
    private Point WindowPoint(CGPoint point) => ConvertPoint(Window, ConvertPointSel, point, Window.RootView);
    private CGRect NativeCaret(int index)
    {
        if (Client is null) return default;
        var rect = Client.GetCaretRect(index, Window.TextMeasureContext);
        CGPoint origin = ConvertPoint(Window.RootView, ConvertPointSel, new(rect.X, rect.Y), Window);
        return new(origin.X, origin.Y, rect.Width, rect.Height);
    }
    private int Hit(CGPoint point) => Client?.GetTextPosition(WindowPoint(point), Window.TextMeasureContext) ?? 0;

    private static bool CanBecome(nint self, nint sel) => K(self).Client is not null;
    private static nint NextResponderStatic(nint self, nint sel) => K(self).NextResponder;
    private static nint InputView(nint self, nint sel) => K(self).Window.RootView;
    private static nint Zero(nint self, nint sel) => 0;
    private static nint KeyboardType(nint self, nint sel) => UITextInputTraits.KeyboardType(K(self).Client?.InputOptions ?? default);
    private static nint ReturnKeyType(nint self, nint sel) => UITextInputTraits.ReturnKeyType(K(self).Client?.InputOptions ?? default);
    private static nint ContentType(nint self, nint sel) => UITextInputTraits.ContentType(K(self).Client);
    private static nint Capitalization(nint self, nint sel) => UITextInputTraits.Capitalization(K(self).Client);
    private static nint Correction(nint self, nint sel) => UITextInputTraits.PlainText(K(self).Client) ? 0 : 1;
    private static nint AccessoryView(nint self, nint sel)
    {
        var k = K(self);
        if (k.Client is null) return 0;
        k.accessory ??= new(self, new Sel("previousInput:"), new Sel("nextInput:"), new Sel("doneInput:"));
        k.accessory.Update(k.Client.CanPerformInputAction(TextInputAction.Previous), true);
        return k.accessory;
    }
    private static void PreviousInput(nint self, nint sel, nint sender) => K(self).Client?.PerformInputAction(TextInputAction.Previous);
    private static void NextInput(nint self, nint sel, nint sender) => K(self).Client?.PerformInputAction(TextInputAction.Next);
    private static void DoneInput(nint self, nint sel, nint sender) => K(self).Client?.PerformInputAction(TextInputAction.Done);
    private static bool Secure(nint self, nint sel) => K(self).Client?.IsPassword ?? false;
    private static bool HasText(nint self, nint sel) => K(self).Client?.HasText ?? false;
    private static nint Beginning(nint self, nint sel) => K(self).Editor is null ? 0 : K(self).PositionAt(0);
    private static nint End(nint self, nint sel) => K(self).Editor is { } e ? K(self).PositionAt(e.Text.Length) : 0;
    private static nint Selected(nint self, nint sel) => K(self).Editor is { } e ? K(self).NativeRangeOf(e.Selection) : 0;
    private static nint Marked(nint self, nint sel) => K(self).Editor?.MarkedRange is { } r ? K(self).NativeRangeOf(r) : 0;
    private static nint MarkedStyle(nint self, nint sel) => K(self).markedStyle;
    private static void SetMarkedStyle(nint self, nint sel, nint value)
    {
        var k = K(self);
        if (value != 0) value = objc_msgSend_retIntPtr(value, new Sel("copy"));
        if (k.markedStyle != 0) CFRelease(k.markedStyle);
        k.markedStyle = value;
    }
    private static void Insert(nint self, nint sel, nint text)
    {
        var k = K(self);
        var value = CFStringRef.Marshal(text) ?? string.Empty;
        if (value is "\n" or "\r" or "\r\n")
        {
            // Return is an action for single-line editors, not a filtered character.
            // Run outside nativeEdit: focus changes must notify the input delegate.
            k.Client?.PerformInputAction(TextInputAction.Default);
            return;
        }
        // Some locales put a comma on the decimal keypad. Honor a field that
        // explicitly accepts only a dot, without rewriting pasted numbers.
        if (value == "," && k.Client?.InputOptions.Keyboard == TextInputKeyboard.Decimal
            && k.Editor?.InputFilter is { } filter && !filter(',') && filter('.'))
            value = ".";
        k.nativeEdit = true;
        try { k.Editor?.Insert(value); }
        finally { k.nativeEdit = false; }
    }
    private static void Delete(nint self, nint sel)
    {
        var k = K(self); k.nativeEdit = true;
        try { k.Editor?.DeleteBackward(); }
        finally { k.nativeEdit = false; }
    }
    private static void SetSelected(nint self, nint sel, nint range)
    {
        var k = K(self);
        if (!k.ReadRange(range, out var r)) return;
        if (k.focusTouchTime != 0 && Stopwatch.GetElapsedTime(k.focusTouchTime).TotalMilliseconds < 350) return;
        k.nativeEdit = true;
        try { k.Editor!.Selection = k.selectionDrag.ConstrainSelection(k.Editor, r); }
        finally { k.nativeEdit = false; }
    }
    private static nint TextInRange(nint self, nint sel, nint range)
    {
        var k = K(self);
        if (!k.ReadRange(range, out var r)) return 0;
        // UITextInput must see real text even for secure entry; UIKit enforces its secure traits.
        return objc_msgSend_retIntPtr(CoreFoundation.CFString(k.Editor!.Text.AsSpan(r.Start, r.Length)), AutoreleaseSel);
    }
    private static void Replace(nint self, nint sel, nint range, nint text)
    {
        var k = K(self);
        if (!k.ReadRange(range, out var r)) return;
        k.nativeEdit = true;
        try { k.Editor!.Replace(r, CFStringRef.Marshal(text) ?? string.Empty); }
        finally { k.nativeEdit = false; }
    }
    private static void SetMarked(nint self, nint sel, nint text, NativeRange range)
    {
        var k = K(self); var value = text == 0 ? string.Empty : CFStringRef.Marshal(text) ?? string.Empty;
        int start = (int)Math.Min(range.Location, (nuint)value.Length);
        int end = start + (int)Math.Min(range.Length, (nuint)(value.Length - start));
        k.nativeEdit = true;
        try { k.Editor?.SetMarkedText(value, new(start, end)); }
        finally { k.nativeEdit = false; }
    }
    private static void Unmark(nint self, nint sel)
    {
        var k = K(self); k.nativeEdit = true;
        try { k.Editor?.UnmarkText(); }
        finally { k.nativeEdit = false; }
    }
    private static nint MakeRange(nint self, nint sel, nint a, nint b)
    {
        var k = K(self);
        return k.ReadPosition(a, out int start) && k.ReadPosition(b, out int end)
            ? Range(k.session, Math.Min(start, end), Math.Max(start, end)) : 0;
    }
    private static nint OffsetPosition(nint self, nint sel, nint position, nint offset)
    {
        var k = K(self);
        if (!k.ReadPosition(position, out int index) || offset < -index || offset > k.Length - index) return 0;
        // UITextInput offsets are UTF-16, not grapheme counts. Selection normalizes boundaries.
        return k.PositionAt(index + (int)offset);
    }
    private static nint DirectionPosition(nint self, nint sel, nint position, nint direction, nint offset)
    {
        var k = K(self);
        if (!k.ReadPosition(position, out int index) || offset < 0) return 0;
        if (offset == 0) return k.PositionAt(index);
        if (direction is 4 or 5) return k.PositionAt(direction == 4 ? 0 : k.Length);
        if (direction is not (2 or 3)) return 0;
        long remaining = Math.Min((long)offset, (long)k.Length + 1);
        while (remaining-- > 0)
        {
            int next = direction == 3 ? k.Editor!.Previous(index) : k.Editor!.Next(index);
            if (next == index) return 0;
            index = next;
        }
        return k.PositionAt(index);
    }
    private static nint Compare(nint self, nint sel, nint a, nint b)
    {
        var k = K(self); return k.ReadPosition(a, out int x) && k.ReadPosition(b, out int y) ? Math.Sign(x - y) : 0;
    }
    private static nint Offset(nint self, nint sel, nint a, nint b)
    {
        var k = K(self); return k.ReadPosition(a, out int x) && k.ReadPosition(b, out int y) ? y - x : 0;
    }
    private static nint GetDelegate(nint self, nint sel)
    {
        nint value = objc_loadWeakRetained(K(self).delegateSlot);
        return value == 0 ? 0 : objc_msgSend_retIntPtr(value, AutoreleaseSel);
    }
    private static void SetDelegate(nint self, nint sel, nint value) => objc_storeWeak(K(self).delegateSlot, value);
    private static nint Tokenizer(nint self, nint sel)
    {
        var k = K(self); return k.tokenizer ??= new IOSTextTokenizer(k);
    }
    private static nint Farthest(nint self, nint sel, nint range, nint direction)
    {
        var k = K(self);
        return k.ReadRange(range, out var r) ? k.PositionAt(direction is 3 or 4 ? r.Start : r.End) : 0;
    }
    private static nint Extend(nint self, nint sel, nint position, nint direction)
    {
        var k = K(self);
        if (!k.ReadPosition(position, out int index)) return 0;
        return direction is 3 or 4 ? Range(k.session, k.Editor!.Previous(index), index)
            : Range(k.session, index, k.Editor!.Next(index));
    }
    private static nint WritingDirection(nint self, nint sel, nint position, nint direction) => 0;
    private static void SetWritingDirection(nint self, nint sel, nint direction, nint range) { } // Fixed LTR, matching canvas layout.
    private static CGRect CaretRect(nint self, nint sel, nint position)
    {
        var k = K(self); return k.ReadPosition(position, out int index) ? k.NativeCaret(index) : default;
    }
    private static CGRect RangeRect(nint self, nint sel, nint range)
    {
        var k = K(self);
        if (!k.ReadRange(range, out var r)) return default;
        var start = k.NativeCaret(r.Start); var end = k.NativeCaret(r.End);
        return new(start.Origin.X, start.Origin.Y, end.Origin.X - start.Origin.X, start.Size.Height);
    }
    private static nint SelectionRects(nint self, nint sel, nint range)
    {
        var k = K(self);
        return k.ReadRange(range, out var r) && r.Length > 0
            ? objc_msgSend_retIntPtr(ArrayClass, ArrayObjectSel, SelectionRect(RangeRect(self, sel, range)))
            : objc_msgSend_retIntPtr(ArrayClass, ArraySel);
    }
    private static nint Closest(nint self, nint sel, CGPoint point)
    {
        var k = K(self); return k.Editor is null ? 0
            : k.PositionAt(k.selectionDrag.ConstrainPosition(k.Editor, k.Hit(point)));
    }
    private static nint ClosestInRange(nint self, nint sel, CGPoint point, nint range)
    {
        var k = K(self);
        if (!k.ReadRange(range, out var r)) return 0;
        int position = k.selectionDrag.ConstrainPosition(k.Editor!, Math.Clamp(k.Hit(point), r.Start, r.End));
        return position >= r.Start && position <= r.End ? k.PositionAt(position) : 0;
    }
    private static nint CharacterAtPoint(nint self, nint sel, CGPoint point)
    {
        var k = K(self); if (k.Editor is null) return 0;
        int index = k.Hit(point); return Range(k.session, index, k.Editor.Next(index));
    }
    private static bool InteractionShouldBegin(nint self, nint sel, nint interaction, CGPoint point)
    {
        var k = K(self);
        if (k.suppressInteraction || k.Client is null) return false;
        var position = k.WindowPoint(point);
        return k.Client.InputBounds.Contains(position) || k.OwnsHandle(position);
    }

    internal void Close()
    {
        if (disposed) return;
        SetClient(null);
        disposed = true;
        if (interaction != 0)
        {
            ObjC.objc_msgSend(interaction, SetInputSel, (nint)0);
            ObjC.objc_msgSend(interaction, SetDelegateSel, (nint)0);
            CFRelease(interaction); interaction = 0;
        }
        tokenizer?.Dispose(); tokenizer = null;
        accessory?.Dispose(); accessory = null;
        if (markedStyle != 0) { CFRelease(markedStyle); markedStyle = 0; }
        objc_storeWeak(delegateSlot, 0);
        ObjC.objc_msgSend(this, new Sel("removeFromSuperview"));
        Dispose();
        objc_destroyWeak(delegateSlot);
        NativeMemory.Free((void*)delegateSlot);
    }

    [DllImport(LibObjCLib)] private static extern nint objc_storeWeak(nint slot, nint value);
    [DllImport(LibObjCLib)] private static extern nint objc_loadWeakRetained(nint slot);
    [DllImport(LibObjCLib)] private static extern void objc_destroyWeak(nint slot);
    [DllImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static extern CGPoint ConvertPoint(nint view, nint selector, CGPoint point, nint fromView);
}
