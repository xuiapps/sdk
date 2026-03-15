using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Abstract.IWindow.IDesktopStyle;
using static Xui.Runtime.MacOS.AppKit;
using static Xui.Runtime.MacOS.AppKit.NSEventRef;
using static Xui.Runtime.MacOS.CoreAnimation;
using static Xui.Runtime.MacOS.CoreFoundation;
using static Xui.Runtime.MacOS.Foundation;
using static Xui.Runtime.MacOS.ObjC;

namespace Xui.Runtime.MacOS.Actual;

public partial class MacOSWindow : NSWindow, Xui.Core.Actual.IWindow
{
    public static readonly Sel CloseSel = new Sel("close");

    public static Sel AnimationFrameSel = new Sel("animationFrame:");

    public bool RequireKeyboard { get; set; }

    protected static unsafe new readonly Class Class = NSWindow.Class
        .Extend("XUIMacOSWindow")
        .AddMethod("sendEvent:", SendEvent)
        .AddMethod("animationFrame:", AnimationFrame)
        .AddMethod("close", Close)
        .Register();

    protected static void SendEvent(nint self, nint sel, nint e) =>
        Marshalling.Get<MacOSWindow>(self)
            .SendEvent(sel, new NSEventRef(e));

    public static void AnimationFrame(nint self, nint sel, nint caDisplayLink) =>
        Marshalling.Get<MacOSWindow>(self).AnimationFrame(caDisplayLink);

    protected static void Close(nint self, nint sel) =>
        Marshalling.Get<MacOSWindow>(self)
            .Close();

    private CADisplayLink displayLink;
    private TimeSpan previousFrameTime;
    private TimeSpan nextFrameTime;
    private bool pendingInvalidate;

    // Custom resize mechanism
    private WindowHitTestEventRef.WindowArea _activeResizeEdge = WindowHitTestEventRef.WindowArea.Default;
    private Point _resizeStartPoint;
    private NSRect _resizeStartFrame;

    // Keyboard modifier tracking (for FlagsChanged)
    private nuint _lastModifierFlags;

    // The root view — always the flipped drawing view, regardless of content view hierarchy.
    private MacOSWindowRootView rootView = null!;

    // Per-window drawing context (owns Path2D, text measure, paint state)
    private readonly MacOSDrawingContext drawingContext = new MacOSDrawingContext();

    // Text measurement context (used for hit-testing cursor position on mouse click)
    private MacOSTextMeasureContext? _textMeasureContext;

    public ITextMeasureContext? TextMeasureContext =>
        _textMeasureContext ??= new MacOSTextMeasureContext();

    // Image pipeline (decodes via ImageIO, caches CGImageRef by URI)
    private MacOSImageFactory? _imageFactory;

    private MacOSImageFactory ImageFactory =>
        _imageFactory ??= new MacOSImageFactory();

    // Active popups owned by this window
    private readonly List<MacOSPopup> activePopups = new();

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IContext)) return drawingContext.Bind();
        if (serviceType == typeof(ITextMeasureContext)) return TextMeasureContext;
        if (serviceType == typeof(IImage)) return ImageFactory.CreateImage();
        if (serviceType == typeof(IDeviceInfo)) return MacOSDeviceInfo.Instance;
        if (serviceType == typeof(Xui.Core.UI.IPopup)) return CreatePopup();
        return null;
    }

    private MacOSPopup CreatePopup()
    {
        var popup = new MacOSPopup(this);
        activePopups.Add(popup);
        popup.Closed += () => activePopups.Remove(popup);
        return popup;
    }

    /// <summary>
    /// Dismisses all active popups owned by this window.
    /// </summary>
    internal void DismissPopups()
    {
        // Copy to avoid modifying during iteration
        var popups = activePopups.ToArray();
        foreach (var p in popups)
            p.Close();
    }

    /// <summary>
    /// Checks if a mouse-down at the given screen point should dismiss any popups.
    /// Returns true if any popup was dismissed.
    /// </summary>
    private bool TryDismissPopupsOnMouseDown(NSPoint screenPoint)
    {
        if (activePopups.Count == 0) return false;

        bool dismissed = false;
        var popups = activePopups.ToArray();
        foreach (var p in popups)
        {
            if (p.TryDismissOnMouseDown(screenPoint))
                dismissed = true;
        }
        return dismissed;
    }

    public static nint InitWithAbstract(Xui.Core.Abstract.IWindow @abstract)
    {
        NSWindowStyleMask mask =
            NSWindowStyleMask.Titled |
            NSWindowStyleMask.Closable |
            NSWindowStyleMask.Miniaturizable |
            NSWindowStyleMask.Resizable;

        Rect rect = new Rect(200, 200, 600, 400);

        if (@abstract is Xui.Core.Abstract.IWindow.IDesktopStyle dws)
        {
            if (dws.Backdrop is WindowBackdrop.Mica or WindowBackdrop.Acrylic or WindowBackdrop.Chromeless)
            {
                mask =
                    NSWindowStyleMask.Titled |
                    NSWindowStyleMask.Closable |
                    NSWindowStyleMask.Miniaturizable |
                    NSWindowStyleMask.Resizable |
                    NSWindowStyleMask.FullSizeContentView |
                    NSWindowStyleMask.Borderless;
            }

            if (dws.StartupSize.HasValue)
            {
                rect.Size = dws.StartupSize.Value;
            }
        }

        nint windowIntPtr = InitWithContentRectStyleMaskBackingDefer(
            Class.Alloc(),
            rect: rect,
            nswindowstylemask: mask,
            nsbackingstoretype: NSBackingStoreType.Buffered,
            defer: false
        );

        return windowIntPtr;
    }

    public MacOSWindow(Xui.Core.Abstract.IWindow @abstract) : base(InitWithAbstract(@abstract))
    {
        rootView = new MacOSWindowRootView(this);

        this.Abstract = @abstract;
        this.Title = "";
        this.Delegate = new MacOSWindowDelegate(this);

        this.IsReleasedWhenClosed = false;
        this.AcceptsMouseMovedEvents = true;

        if (@abstract is Xui.Core.Abstract.IWindow.IDesktopStyle dws)
        {
            this.Level = dws.Level switch
            {
                DesktopWindowLevel.Normal    => NSWindowLevel.Normal,
                DesktopWindowLevel.Floating  => NSWindowLevel.Floating,
                DesktopWindowLevel.StatusBar => NSWindowLevel.StatusBar,
                DesktopWindowLevel.Modal     => NSWindowLevel.ModalPanel,
                _                            => NSWindowLevel.Normal
            };

            if (dws.Backdrop == WindowBackdrop.Chromeless)
            {
                using var transparent = new NSColorRef(0, 0, 0, 0);
                this.BackgroundColor = transparent;
                this.TitleVisibility = NSWindowTitleVisibility.Hidden;
                this.TitlebarAppearsTransparent = true;
                // this.HideTitleButtons();

                this.StyleMask |= NSWindowStyleMask.FullSizeContentView;
                this.Toolbar = new NSToolbar
                {
                    ShowsBaselineSeparator = false,
                    Visible = true
                };
                this.ToolbarStyle = NSWindowToolbarStyle.Unified;
                this.ContentView = rootView;
            }
            else if (dws.Backdrop is WindowBackdrop.Mica or WindowBackdrop.Acrylic)
            {
                using var transparent = new NSColorRef(0, 0, 0, 0);
                this.BackgroundColor = transparent;

                var vev = new NSVisualEffectView
                {
                    Material = NSVisualEffectMaterial.UnderWindowBackground,
                    BlendingMode = NSVisualEffectBlendingMode.BehindWindow,
                    State = NSVisualEffectState.Active
                };
                rootView.AutoresizingMask =
                    NSAutoresizingMaskOptions.WidthSizable |
                    NSAutoresizingMaskOptions.HeightSizable;
                vev.AddSubview(rootView);
                this.ContentView = vev;
                this.TitleVisibility = NSWindowTitleVisibility.Hidden;
                this.TitlebarAppearsTransparent = true;
                this.Toolbar = new NSToolbar
                {
                    ShowsBaselineSeparator = false,
                    Visible = true
                };
                this.ToolbarStyle = NSWindowToolbarStyle.Unified;
            }
            else
            {
                this.ContentView = rootView;
            }
        }
        else
        {
            this.ContentView = rootView;
        }

        this.displayLink = CADisplayLink.DisplayLink(this, AnimationFrameSel);
        this.displayLink.AddToRunLoopForMode(NSRunLoop.MainRunLoop, NSRunLoop.Mode.Common);
    }

    protected internal Xui.Core.Abstract.IWindow Abstract { get; }

    string Xui.Core.Actual.IWindow.Title
    {
        get => this.Title!;
        set => this.Title = value;
    }

    void Xui.Core.Actual.IWindow.Show()
    {
        this.MakeKeyAndOrderFront();
        this.PositionSystemButtons();
    }

    protected void SendEvent(nint sel, NSEventRef e)
    {
        var type = e.Type;
        // if (type == NSEventType.AppKitDefined)
        // {
        //     System.Diagnostics.Debug.WriteLine("XuiWindow sendEvent " + type + " " + e.Subtype);
        // }
        // else
        // {
        //     System.Diagnostics.Debug.WriteLine("XuiWindow sendEvent " + type);
        // }

        if (type == NSEventType.AppKitDefined)
        {
        }
        else if (type == NSEventType.MouseEntered)
        {
            var rect = this.Rect;
            WindowHitTestEventRef eventRef = new()
            {
                Area = WindowHitTestEventRef.WindowArea.Default,
                Point = e.LocationInWindow,
                Window = rect
            };
            this.Abstract.WindowHitTest(ref eventRef);
        }
        else if (type == NSEventType.KeyDown)
        {
            VirtualKey key = MacOSKeyMap.ToVirtualKey(e.KeyCode);
            bool shift = (e.ModifierFlags & MacOSKeyMap.ShiftFlag) != 0;
            bool isRepeat = e.IsARepeat;

            var keyEvent = new KeyEventRef { Key = key, Shift = shift, IsRepeat = isRepeat };
            this.Abstract.OnKeyDown(ref keyEvent);

            // Dispatch printable characters via OnChar (>= space, excludes control chars)
            var characters = e.Characters;
            if (characters is { Length: > 0 } && characters[0] >= ' ')
            {
                var charEvent = new KeyEventRef { Character = characters[0], Shift = shift, IsRepeat = isRepeat };
                this.Abstract.OnChar(ref charEvent);
            }
        }
        else if (type == NSEventType.KeyUp)
        {
            // No OnKeyUp in the abstract interface; super is still called below.
        }
        else if (type == NSEventType.FlagsChanged)
        {
            var flags = e.ModifierFlags;
            var diff = flags ^ _lastModifierFlags;
            _lastModifierFlags = flags;

            // Dispatch OnKeyDown for each modifier that just became pressed.
            DispatchModifierIfPressed(diff, flags, MacOSKeyMap.ShiftFlag,   VirtualKey.Shift);
            DispatchModifierIfPressed(diff, flags, MacOSKeyMap.ControlFlag, VirtualKey.Control);
            DispatchModifierIfPressed(diff, flags, MacOSKeyMap.OptionFlag,  VirtualKey.Alt);
        }
        else if (
            type == NSEventType.MouseMoved ||
            type == NSEventType.LeftMouseDragged ||
            type == NSEventType.RightMouseDragged ||
            type == NSEventType.OtherMouseDragged)
        {
            var rect = this.Rect;
            var position = rootView.ConvertPointFromView(e.LocationInWindow, null);

            WindowHitTestEventRef eventRef = new()
            {
                Area = WindowHitTestEventRef.WindowArea.Default,
                Point = position,
                Window = new Rect(0, 0, rect.Size.width, rect.Size.height)
            };

            // Custom frame window chrome resizing.
            if (type == NSEventType.LeftMouseDragged && _activeResizeEdge != WindowHitTestEventRef.WindowArea.Default)
            {
                var delta = position - _resizeStartPoint;
                switch (_activeResizeEdge)
                {
                    case WindowHitTestEventRef.WindowArea.BorderTopRight:
                    case WindowHitTestEventRef.WindowArea.BorderRight:
                    case WindowHitTestEventRef.WindowArea.BorderBottomRight:
                        _resizeStartFrame.Size.width += delta.X;
                        _resizeStartPoint.X += delta.X;
                        break;
                    case WindowHitTestEventRef.WindowArea.BorderTopLeft:
                    case WindowHitTestEventRef.WindowArea.BorderLeft:
                    case WindowHitTestEventRef.WindowArea.BorderBottomLeft:
                        _resizeStartFrame.Size.width -= delta.X;
                        _resizeStartFrame.Origin.x += delta.X;
                        break;
                }

                switch (_activeResizeEdge)
                {
                    case WindowHitTestEventRef.WindowArea.BorderTopLeft:
                    case WindowHitTestEventRef.WindowArea.BorderTop:
                    case WindowHitTestEventRef.WindowArea.BorderTopRight:
                        _resizeStartFrame.Size.height -= delta.Y;
                        break;
                    case WindowHitTestEventRef.WindowArea.BorderBottomLeft:
                    case WindowHitTestEventRef.WindowArea.BorderBottom:
                    case WindowHitTestEventRef.WindowArea.BorderBottomRight:
                        _resizeStartFrame.Origin.y -= delta.Y;
                        _resizeStartFrame.Size.height += delta.Y;
                        _resizeStartPoint.Y += delta.Y;
                        break;
                }

                ApplyNSCursor(_activeResizeEdge);
                SetFrame(_resizeStartFrame, true);

                eventRef.Area = _activeResizeEdge;
            }
            else
            {
                this.Abstract.WindowHitTest(ref eventRef);

                var evRef = new MouseMoveEventRef()
                {
                    Position = position
                };
                this.Abstract.OnMouseMove(ref evRef);
            }

            if (eventRef.Area != WindowHitTestEventRef.WindowArea.Default)
            {
                ApplyNSCursor(eventRef.Area);
            }

            // Debug.WriteLine("XuiWindow sendEvent " + type + " " + point.x + " : " + point.y);
        }
        else if (type == NSEventType.ScrollWheel)
        {
            var evRef = new ScrollWheelEventRef()
            {
                Delta = e.ScrollingDelta
            };
            this.Abstract.OnScrollWheel(ref evRef);
        }
        else if (type == NSEventType.LeftMouseDown || type == NSEventType.RightMouseDown || type == NSEventType.OtherMouseDown)
        {
            // Dismiss popups on click-outside
            if (activePopups.Count > 0)
            {
                var windowRect = new NSRect
                {
                    Origin = e.LocationInWindow,
                    Size = new NSSize { width = 0, height = 0 }
                };
                var screenPoint = this.ConvertRectToScreen(windowRect).Origin;
                this.TryDismissPopupsOnMouseDown(screenPoint);
            }

            var rect = this.Rect;
            var position = rootView.ConvertPointFromView(e.LocationInWindow, null);

            WindowHitTestEventRef eventRef = new()
            {
                Area = WindowHitTestEventRef.WindowArea.Default,
                Point = position,
                Window = new Rect(0, 0, rect.Size.width, rect.Size.height)
            };
            this.Abstract.WindowHitTest(ref eventRef);

            var evRef = new MouseDownEventRef()
            {
                Position = position
            };
            switch (type)
            {
                case NSEventType.LeftMouseDown:
                    evRef.Button = MouseButton.Left;
                    break;
                case NSEventType.RightMouseDown:
                    evRef.Button = MouseButton.Right;
                    break;
                case NSEventType.OtherMouseDown:
                    evRef.Button = MouseButton.Other;
                    break;
            }

            if (type == NSEventType.LeftMouseDown &&
                (eventRef.Area is
                    WindowHitTestEventRef.WindowArea.BorderTop or
                    WindowHitTestEventRef.WindowArea.BorderBottom or
                    WindowHitTestEventRef.WindowArea.BorderLeft or
                    WindowHitTestEventRef.WindowArea.BorderRight or
                    WindowHitTestEventRef.WindowArea.BorderTopLeft or
                    WindowHitTestEventRef.WindowArea.BorderTopRight or
                    WindowHitTestEventRef.WindowArea.BorderBottomLeft or
                    WindowHitTestEventRef.WindowArea.BorderBottomRight))
            {
                // Disable resizable as we will handle it in custom code...
                this.StyleMask &= ~NSWindowStyleMask.Resizable;

                _activeResizeEdge = eventRef.Area;
                _resizeStartPoint = position;
                _resizeStartFrame = this.Rect;
            }
            else if (type == NSEventType.LeftMouseDown &&
                eventRef.Area is WindowHitTestEventRef.WindowArea.Title)
            {
                // TODO: If user clicked on the window chrome buttons - don't perform drag to allow close/miniaturize etc.

                this.OrderFrontRegardless();
                this.PerformDrag(e);

                // TODO: If use clicked on the window border - return here to prevent default resize behavior
                // return;
            }
            else
            {
                this.Abstract.OnMouseDown(ref evRef);
            }
        }
        else if (type == NSEventType.LeftMouseUp || type == NSEventType.RightMouseUp || type == NSEventType.OtherMouseUp)
        {
            // When custom resize is performed, and mouse is released - return resizable...
            this.StyleMask |= NSWindowStyleMask.Resizable;

            var rect = this.Rect;
            var position = rootView.ConvertPointFromView(e.LocationInWindow, null);

            WindowHitTestEventRef eventRef = new()
            {
                Area = WindowHitTestEventRef.WindowArea.Default,
                Point = position,
                Window = new Rect(0, 0, rect.Size.width, rect.Size.height)
            };
            this.Abstract.WindowHitTest(ref eventRef);

            if (e.Type == NSEventType.LeftMouseUp &&
                e.ClickCount == 2 &&
                eventRef.Area == WindowHitTestEventRef.WindowArea.Title &&
                this.Abstract is Xui.Core.Abstract.IWindow.IDesktopStyle dws && dws.Backdrop == WindowBackdrop.Chromeless)
            {
                this.PerformZoom();
            }

            var evRef = new MouseUpEventRef()
            {
                Position = position
            };
            switch (type)
            {
                case NSEventType.LeftMouseDown:
                    evRef.Button = MouseButton.Left;
                    break;
                case NSEventType.RightMouseDown:
                    evRef.Button = MouseButton.Right;
                    break;
                case NSEventType.OtherMouseDown:
                    evRef.Button = MouseButton.Other;
                    break;
            }

            _activeResizeEdge = WindowHitTestEventRef.WindowArea.Default;

            this.Abstract.OnMouseUp(ref evRef);
        }
        else if (type == NSEventType.Pressure)
        {
            // Debug.WriteLine("Pressure: " + e.Pressure);
        }
        else if (type == NSEventRef.NSEventType.AppKitDefined)
        {
            var subtype = e.Subtype;
            // Debug.WriteLine("XuiWindow sendEvent " + type + " " + subtype);
        }
        else
        {
            // Debug.WriteLine("XuiWindow sendEvent " + type);
        }

        // Note you don't have to call super, and if you don't native UI wont receive events... even paint..
        Super super = new Super(this, NSWindow.Class);
        ObjC.objc_msgSendSuper(ref super, sel, e);

        if (this.Abstract is Xui.Core.Abstract.IWindow.IDesktopStyle dwsc && dwsc.Backdrop == WindowBackdrop.Chromeless)
        {
            // this.HideTitleButtons();
        }
    }

    private void DispatchModifierIfPressed(nuint diff, nuint current, nuint flag, VirtualKey key)
    {
        if ((diff & flag) != 0 && (current & flag) != 0)
        {
            var e = new KeyEventRef { Key = key };
            this.Abstract.OnKeyDown(ref e);
        }
    }

    private static void ApplyNSCursor(WindowHitTestEventRef.WindowArea area)
    {
        switch (area)
        {
            case WindowHitTestEventRef.WindowArea.BorderTopLeft:
            case WindowHitTestEventRef.WindowArea.BorderBottomRight:
                NSCursor._WindowResizeNorthWestSouthEastCursor.Set();
                break;
            case WindowHitTestEventRef.WindowArea.BorderTopRight:
            case WindowHitTestEventRef.WindowArea.BorderBottomLeft:
                NSCursor._WindowResizeNorthEastSouthWestCursor.Set();
                break;
            case WindowHitTestEventRef.WindowArea.BorderTop:
            case WindowHitTestEventRef.WindowArea.BorderBottom:
                NSCursor._WindowResizeNorthSouthCursor.Set();
                break;
            case WindowHitTestEventRef.WindowArea.BorderLeft:
            case WindowHitTestEventRef.WindowArea.BorderRight:
                NSCursor._WindowResizeEastWestCursor.Set();
                break;
            default:
                NSCursor.ArrowCursor.Set();
                break;
        }
    }

    protected void Close()
    {
        this.DismissPopups();
        this.Abstract.Closed();
        Super super = new Super(this, NSWindow.Class);
        objc_msgSendSuper(ref super, CloseSel);
    }

    internal bool Closing() => this.Abstract.Closing();

    private void AnimationFrame(nint caDisplayLink)
    {
        this.previousFrameTime = this.displayLink.Timestamp;
        this.nextFrameTime = this.displayLink.TargetTimestamp;
        var animationFrame = new FrameEventRef(this.previousFrameTime, this.nextFrameTime);
        this.Abstract.OnAnimationFrame(ref animationFrame);

        if (this.pendingInvalidate)
        {
            this.pendingInvalidate = false;
            rootView.NeedsDisplay = true;
        }
    }

    public void Invalidate()
    {
        this.pendingInvalidate = true;
        rootView.NeedsDisplay = true;
    }

    internal void Render(NSRect rect)
    {
        var contentFrame = rootView.Frame;
        var area = new Rect(0, 0, contentFrame.Size.width, contentFrame.Size.height);
        this.Abstract.DisplayArea = area;
        this.Abstract.SafeArea = area;

        FrameEventRef frame = new(this.previousFrameTime, this.nextFrameTime);
        RenderEventRef render = new(rect, frame);

        this.pendingInvalidate = false;
        var ctx = this.drawingContext.Bind();
        MacOSPlatform.DisplayContextStack.Push(ctx);
        try
        {
            this.Abstract.Render(ref render);
        }
        finally
        {
            MacOSPlatform.DisplayContextStack.Pop();
        }
    }
}
