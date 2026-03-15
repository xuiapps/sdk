using System.Runtime.InteropServices;
using static Xui.Runtime.MacOS.Foundation;
using static Xui.Runtime.MacOS.CoreFoundation;
using static Xui.Runtime.MacOS.ObjC;
using System;

namespace Xui.Runtime.MacOS;

public static partial class AppKit
{
    public partial class NSWindow : NSResponder
    {
        public static new readonly Class Class = new Class(Lib, "NSWindow");

        public static readonly Sel InitWithContentRectStyleMaskBackingDeferSel = new Sel("initWithContentRect:styleMask:backing:defer:");

        protected static readonly Prop.Bool ReleasedWhenClosedProp = new Prop.Bool("isReleasedWhenClosed", "setReleasedWhenClosed:");

        protected static readonly Prop.Bool AcceptsMouseMovedEventsProp = new Prop.Bool("acceptsMouseMovedEvents", "setAcceptsMouseMovedEvents:");

        private static readonly Sel SetAcceptsMouseMovedEventsSel = new Sel("setAcceptsMouseMovedEvents:");
        private static readonly Sel MakeKeyAndOrderFrontSel = new Sel("makeKeyAndOrderFront:");
        private static readonly Sel OrderFrontRegardlessSel = new Sel("orderFrontRegardless");

        public static readonly Prop.String TitleProp = new Prop.String("title", "setTitle:");

        public static readonly Sel FrameSel = new Sel("frame");

        private static readonly Sel ContentViewSel = new Sel("contentView");

        private static readonly Sel SetContentViewSel = new Sel("setContentView:");

        private static readonly Prop OpaqueProp = new Prop("isOpaque", "setOpaque:");

        private static readonly Prop BackgroundColorProp = new Prop("backgroundColor", "setBackgroundColor:");

        private static readonly Prop StyleMaskProp = new Prop("styleMask", "setStyleMask:");

        private static readonly Prop TitleVisibilityProp = new Prop("titleVisibility", "setTitleVisibility:");

        private static readonly Prop.NInt ToolbarProp = new Prop.NInt("toolbar", "setToolbar:");

        private static readonly Prop.NInt ToolbarStyleProp = new Prop.NInt("toolbarStyle", "setToolbarStyle:");

        private static readonly Prop.Bool TitlebarAppearsTransparentProp = new Prop.Bool("titlebarAppearsTransparent", "setTitlebarAppearsTransparent:");

        private static readonly Prop.NInt DelegateProp = new Prop.NInt("delegate", "setDelegate:");

        private static readonly Sel AddTitlebarAccessoryViewControllerSel = new Sel("addTitlebarAccessoryViewController:");

        public static readonly Sel PerformWindowDragWithEventSel = new Sel("performWindowDragWithEvent:");

        public static readonly Sel SetFrameDisplaySel = new Sel("setFrame:display:");

        public static readonly Sel PerformZoomSel = new Sel("performZoom:");

        private static readonly Sel SetContentMinSizeSel = new Sel("setContentMinSize:");

        private static readonly Prop.NInt LevelProp = new Prop.NInt("level", "setLevel:");
        private static readonly Sel StandardWindowButtonSel = new Sel("standardWindowButton:");

        private static readonly Sel AddChildWindowOrderedSel = new Sel("addChildWindow:ordered:");
        private static readonly Sel RemoveChildWindowSel = new Sel("removeChildWindow:");
        private static readonly Sel OrderOutSel = new Sel("orderOut:");
        private static readonly Sel ConvertRectToScreenSel = new Sel("convertRectToScreen:");
        private static readonly Prop.Bool HasShadowProp = new Prop.Bool("hasShadow", "setHasShadow:");

        public NSWindow(nint id) : base(id)
        {
        }

        public NSWindow(string title) : this(CreateNSWindowWithTitle(Class.Alloc(), title))
        {
        }

        public NSWindowLevel Level
        {
            get => (NSWindowLevel)LevelProp.Get(this);
            set => LevelProp.Set(this, (nint)value);
        }

        public bool IsReleasedWhenClosed
        {
            get => ReleasedWhenClosedProp.Get(this);
            set => ReleasedWhenClosedProp.Set(this, value);
        }

        public bool AcceptsMouseMovedEvents
        {
            get => AcceptsMouseMovedEventsProp.Get(this);
            set => AcceptsMouseMovedEventsProp.Set(this, value);
        }

        public NSView? ContentView
        {
            get => Marshalling.Get<NSView>(ObjC.objc_msgSend_retIntPtr(this, ContentViewSel));
            set => ObjC.objc_msgSend_retIntPtr(this, SetContentViewSel, value == null ? 0 : value);
        }
        
        public NSSize MinSize
        {
            set => objc_msgSend(this, SetContentMinSizeSel, value);
        }

        protected static nint CreateNSWindowWithTitle(nint self, string title)
        {
            nint nsWindowRef = InitWithContentRectStyleMaskBackingDefer(
                self,
                rect: new NSRect
                {
                    Origin = new NSPoint
                    {
                        x = 200,
                        y = 200
                    },
                    Size = new NSSize
                    {
                        width = 600,
                        height = 400
                    }
                },
                nswindowstylemask:
                    NSWindowStyleMask.Titled |
                    NSWindowStyleMask.Closable |
                    NSWindowStyleMask.Miniaturizable |
                    NSWindowStyleMask.Resizable,
                nsbackingstoretype:
                    NSBackingStoreType.Buffered,
                defer: false
            );

            ReleasedWhenClosedProp.Set(nsWindowRef, false);
            AcceptsMouseMovedEventsProp.Set(nsWindowRef, true);
            objc_msgSend_retIntPtr(nsWindowRef, MakeKeyAndOrderFrontSel, nint.Zero);

            if (title != null)
            {
                TitleProp.Set(nsWindowRef, title);
            }

            return nsWindowRef;
        }

        public string? Title
        {
            get => TitleProp.Get(this);
            set => TitleProp.Set(this, value);
        }

        public bool Opaque
        {
            get => objc_msgSend_retBool(this, OpaqueProp.GetSel);
            set => objc_msgSend(this, OpaqueProp.SetSel, value);
        }

        public NSRect Rect => ObjC.objc_msgSend_retNSRect(this, FrameSel);

        public NSColorRef BackgroundColor
        {
            get => new NSColorRef(ObjC.objc_msgSend_retIntPtr(this, BackgroundColorProp.GetSel));
            set => ObjC.objc_msgSend(this, BackgroundColorProp.SetSel, value);
        }

        public NSWindowStyleMask StyleMask
        {
            get => (NSWindowStyleMask)ObjC.objc_msgSend_retIntPtr(this, StyleMaskProp.GetSel);
            set => objc_msgSend(this, StyleMaskProp.SetSel, (nuint)value);
        }

        public NSWindowTitleVisibility TitleVisibility
        {
            get => (NSWindowTitleVisibility)ObjC.objc_msgSend_retIntPtr(this, TitleVisibilityProp.GetSel);
            set => objc_msgSend(this, TitleVisibilityProp.SetSel, (nint)value);
        }

        public bool TitlebarAppearsTransparent
        {
            get => TitlebarAppearsTransparentProp.Get(this);
            set => TitlebarAppearsTransparentProp.Set(this, value);
        }

        public NSToolbar Toolbar
        {
            get => throw new NotImplementedException("Need some sort of marshalling");
            set => ToolbarProp.Set(this, value);
        }

        public NSWindowToolbarStyle ToolbarStyle
        {
            get => (NSWindowToolbarStyle)ToolbarStyleProp.Get(this);
            set => ToolbarStyleProp.Set(this, (nint)value);
        }

        /// <summary>
        /// NOTE: The Delegate property is of type:
        /// @property(weak) id<NSWindowDelegate> delegate;
        /// </summary>
        public nint Delegate
        {
            get => throw new NotImplementedException();
            set => DelegateProp.Set(this, value);
        }

        public void PerformDrag(NSEventRef e) => objc_msgSend(this, PerformWindowDragWithEventSel, e);

        public void PerformZoom() => objc_msgSend(this, PerformZoomSel, IntPtr.Zero);

        /// <summary>
        /// Returns the raw native pointer to one of the standard window chrome buttons.
        /// The returned pointer is borrowed — do not retain or dispose it.
        /// </summary>
        public nint StandardWindowButton(NSWindowButton button) =>
            objc_msgSend_retIntPtr(this, StandardWindowButtonSel, (nint)button);

        public void SetFrame(NSRect frame, bool display) => objc_msgSend_frameDisplay(this, SetFrameDisplaySel, frame, display);

        public void MakeKeyAndOrderFront(nint sender = 0) => objc_msgSend_retIntPtr(this, MakeKeyAndOrderFrontSel, sender);

        public void OrderFrontRegardless() => objc_msgSend(this, OrderFrontRegardlessSel);

        public void AddTitlebarAccessoryViewController(NSTitlebarAccessoryViewController controller) => objc_msgSend(this, AddTitlebarAccessoryViewControllerSel, controller);

        /// <summary>
        /// Adds a child window that moves with the parent and stays above it.
        /// NSWindowOrderingMode: .above = 1
        /// </summary>
        public void AddChildWindow(NSWindow child, nint ordered = 1) =>
            objc_msgSend(this, AddChildWindowOrderedSel, child, ordered);

        /// <summary>Removes a previously added child window.</summary>
        public void RemoveChildWindow(NSWindow child) =>
            objc_msgSend(this, RemoveChildWindowSel, (nint)child);

        /// <summary>Hides the window without closing it.</summary>
        public void OrderOut(nint sender = 0) =>
            objc_msgSend(this, OrderOutSel, sender);

        /// <summary>
        /// Converts a rect from the window's coordinate space to screen coordinates.
        /// </summary>
        public NSRect ConvertRectToScreen(NSRect rect) =>
            ObjC.objc_msgSend_retNSRect(this, ConvertRectToScreenSel, rect);

        public bool HasShadow
        {
            get => HasShadowProp.Get(this);
            set => HasShadowProp.Set(this, value);
        }

        [LibraryImport(FoundationLib, EntryPoint = "objc_msgSend")]
        private static partial IntPtr objc_msgSend_retIntPtr(nint obj, nint sel, NSRect rect, nuint nswindowstylemask, nuint nsbackingstoretype, [MarshalAs(UnmanagedType.I1)] bool defer);

        [LibraryImport(CoreFoundationLib, EntryPoint = "objc_msgSend")]
        private static partial IntPtr objc_msgSend_retIntPtr(nint obj, nint sel, NSPoint point);

        [LibraryImport(CoreFoundationLib, EntryPoint = "objc_msgSend")]
        private static partial IntPtr objc_msgSend_retIntPtr(nint obj, nint sel, NSSize point);

        [LibraryImport(CoreFoundationLib, EntryPoint = "objc_msgSend")]
        private static partial IntPtr objc_msgSend_retIntPtr(nint obj, nint sel, [MarshalAs(UnmanagedType.I1)] bool b);

        [LibraryImport(CoreFoundationLib, EntryPoint = "objc_msgSend")]
        private static partial IntPtr objc_msgSend_retIntPtr(nint obj, nint sel, nint id1);

        [LibraryImport(AppKitLib, EntryPoint = "objc_msgSend")]
        private static partial void objc_msgSend_frameDisplay(nint obj, nint sel, NSRect frame, [MarshalAs(UnmanagedType.I1)] bool display);

        protected static nint InitWithContentRectStyleMaskBackingDefer(nint id, NSRect rect, NSWindowStyleMask nswindowstylemask, NSBackingStoreType nsbackingstoretype, bool defer)
            => objc_msgSend_retIntPtr(id, InitWithContentRectStyleMaskBackingDeferSel, rect, (nuint)nswindowstylemask, (nuint)nsbackingstoretype, defer);
    }
}