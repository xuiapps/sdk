using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using static Xui.Core.Abstract.IWindow.IDesktopStyle;
using static Xui.Runtime.MacOS.AppKit;
using static Xui.Runtime.MacOS.AppKit.NSEventRef;
using static Xui.Runtime.MacOS.Foundation;
using static Xui.Runtime.MacOS.ObjC;

namespace Xui.Runtime.MacOS.Actual;

public partial class MacOSWindow
{
    protected static void SendEvent(nint self, nint sel, nint e) =>
        Marshalling.Get<MacOSWindow>(self)
            .SendEvent(sel, new NSEventRef(e));

    protected void SendEvent(nint sel, NSEventRef e)
    {
        _suppressNativeEventDispatch = false;

        try
        {
            switch (e.Type)
            {
                case NSEventType.AppKitDefined:
                    this.HandleAppKitDefined(e);
                    return;
                case NSEventType.MouseEntered:
                    this.HandleMouseEntered(e);
                    return;
                case NSEventType.KeyDown:
                    this.HandleKeyDown(e);
                    return;
                case NSEventType.KeyUp:
                    this.HandleKeyUp(e);
                    return;
                case NSEventType.FlagsChanged:
                    this.HandleFlagsChanged(e);
                    return;
                case NSEventType.MouseMoved:
                case NSEventType.LeftMouseDragged:
                case NSEventType.RightMouseDragged:
                case NSEventType.OtherMouseDragged:
                    this.HandleMouseMovedOrDragged(e);
                    return;
                case NSEventType.ScrollWheel:
                    this.HandleScrollWheel(e);
                    return;
                case NSEventType.LeftMouseDown:
                case NSEventType.RightMouseDown:
                case NSEventType.OtherMouseDown:
                    this.HandleMouseDown(e);
                    return;
                case NSEventType.LeftMouseUp:
                case NSEventType.RightMouseUp:
                case NSEventType.OtherMouseUp:
                    this.HandleMouseUp(e);
                    return;
                case NSEventType.Pressure:
                    this.HandlePressure(e);
                    return;
                default:
                    this.HandleUnknownEvent(e);
                    return;
            }
        }
        finally
        {
            // Native UI, including painting, does not receive events without this.
            if (!_suppressNativeEventDispatch)
            {
                Super super = new(this, NSWindow.Class);
                objc_msgSendSuper(ref super, sel, e);
            }

            this.style.OnEventDispatched(this);
        }
    }

    private void HandleAppKitDefined(NSEventRef e)
    {
        // Debug.WriteLine("XuiWindow sendEvent " + e.Type + " " + e.Subtype);
    }

    private void HandleMouseEntered(NSEventRef e)
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

    private void HandleKeyDown(NSEventRef e)
    {
        VirtualKey key = MacOSKeyMap.ToVirtualKey(e.KeyCode);
        bool shift = (e.ModifierFlags & MacOSKeyMap.ShiftFlag) != 0;
        bool isRepeat = e.IsARepeat;

        var keyEvent = new KeyEventRef { Key = key, Shift = shift, IsRepeat = isRepeat };
        this.Abstract.OnKeyDown(ref keyEvent);

        var characters = e.Characters;
        if (!keyEvent.Handled && characters is { Length: > 0 } && characters[0] >= ' ')
        {
            var charEvent = new KeyEventRef { Character = characters[0], Shift = shift, IsRepeat = isRepeat };
            this.Abstract.OnChar(ref charEvent);
        }
    }

    private void HandleKeyUp(NSEventRef e)
    {
        // There is no OnKeyUp in the abstract interface.
    }

    private void HandleFlagsChanged(NSEventRef e)
    {
        var flags = e.ModifierFlags;
        var diff = flags ^ _lastModifierFlags;
        _lastModifierFlags = flags;

        DispatchModifierIfPressed(diff, flags, MacOSKeyMap.ShiftFlag, VirtualKey.Shift);
        DispatchModifierIfPressed(diff, flags, MacOSKeyMap.ControlFlag, VirtualKey.Control);
        DispatchModifierIfPressed(diff, flags, MacOSKeyMap.OptionFlag, VirtualKey.Alt);
    }

    private void HandleMouseMovedOrDragged(NSEventRef e)
    {
        var rect = this.Rect;
        var position = rootView.ConvertPointFromView(e.LocationInWindow, null);
        WindowHitTestEventRef eventRef = new()
        {
            Area = WindowHitTestEventRef.WindowArea.Default,
            Point = position,
            Window = new Rect(0, 0, rect.Size.width, rect.Size.height)
        };

        if (e.Type == NSEventType.LeftMouseDragged && _activeResizeEdge != WindowHitTestEventRef.WindowArea.Default)
        {
            if (this.style is BorderlessStyle)
                _suppressNativeEventDispatch = true;

            this.ResizeFromDrag(position);
            eventRef.Area = _activeResizeEdge;
        }
        else
        {
            this.Abstract.WindowHitTest(ref eventRef);
            var moveEvent = new MouseMoveEventRef { Position = position };
            this.Abstract.OnMouseMove(ref moveEvent);
        }

        if (eventRef.Area != WindowHitTestEventRef.WindowArea.Default)
            ApplyNSCursor(eventRef.Area);
    }

    private void ResizeFromDrag(Point position)
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
        this.SetFrame(_resizeStartFrame, true);
    }

    private void HandleScrollWheel(NSEventRef e)
    {
        var scrollEvent = new ScrollWheelEventRef { Delta = e.ScrollingDelta };
        this.Abstract.OnScrollWheel(ref scrollEvent);
    }

    private void HandleMouseDown(NSEventRef e)
    {
        this.DismissPopupsOnMouseDown(e);

        var rect = this.Rect;
        var position = rootView.ConvertPointFromView(e.LocationInWindow, null);
        WindowHitTestEventRef eventRef = new()
        {
            Area = WindowHitTestEventRef.WindowArea.Default,
            Point = position,
            Window = new Rect(0, 0, rect.Size.width, rect.Size.height)
        };
        this.Abstract.WindowHitTest(ref eventRef);

        // A Borderless window owns its transparent and resize areas. Forwarding
        // either mouse-down to a titled AppKit window would let the hidden
        // native frame start a second drag/resize alongside Xui's own one.
        if (this.style is BorderlessStyle &&
            (eventRef.Area == WindowHitTestEventRef.WindowArea.Transparent || IsResizeArea(eventRef.Area)))
        {
            _suppressNativeEventDispatch = true;

            if (eventRef.Area == WindowHitTestEventRef.WindowArea.Transparent)
                return;
        }

        var mouseEvent = new MouseDownEventRef { Position = position, Button = GetMouseButton(e.Type) };
        if (e.Type == NSEventType.LeftMouseDown && IsResizeArea(eventRef.Area))
        {
            _activeResizeEdge = eventRef.Area;
            _resizeStartPoint = position;
            _resizeStartFrame = this.Rect;
        }
        else if (e.Type == NSEventType.LeftMouseDown && eventRef.Area == WindowHitTestEventRef.WindowArea.Title)
        {
            if (e.ClickCount == 2 && this.ShouldSoftwareTitleBarZoom)
                this.PerformZoom();
            else
            {
                this.OrderFrontRegardless();
                this.PerformDrag(e);
            }
        }
        else
        {
            this.Abstract.OnMouseDown(ref mouseEvent);
        }
    }

    private void DismissPopupsOnMouseDown(NSEventRef e)
    {
        if (activePopups.Count == 0)
            return;

        var windowRect = new NSRect
        {
            Origin = e.LocationInWindow,
            Size = new NSSize { width = 0, height = 0 }
        };
        this.TryDismissPopupsOnMouseDown(this.ConvertRectToScreen(windowRect).Origin);
    }

    private void HandleMouseUp(NSEventRef e)
    {
        var wasCustomBorderlessResize =
            this.style is BorderlessStyle &&
            _activeResizeEdge != WindowHitTestEventRef.WindowArea.Default;
        if (wasCustomBorderlessResize)
            _suppressNativeEventDispatch = true;

        var rect = this.Rect;
        var position = rootView.ConvertPointFromView(e.LocationInWindow, null);
        WindowHitTestEventRef eventRef = new()
        {
            Area = WindowHitTestEventRef.WindowArea.Default,
            Point = position,
            Window = new Rect(0, 0, rect.Size.width, rect.Size.height)
        };
        this.Abstract.WindowHitTest(ref eventRef);

        var mouseEvent = new MouseUpEventRef { Position = position, Button = GetMouseUpButton(e.Type) };
        _activeResizeEdge = WindowHitTestEventRef.WindowArea.Default;
        this.Abstract.OnMouseUp(ref mouseEvent);
    }

    private void HandlePressure(NSEventRef e)
    {
        // Debug.WriteLine("Pressure: " + e.Pressure);
    }

    private void HandleUnknownEvent(NSEventRef e)
    {
        // Debug.WriteLine("XuiWindow sendEvent " + e.Type);
    }

    private void DispatchModifierIfPressed(nuint diff, nuint current, nuint flag, VirtualKey key)
    {
        if ((diff & flag) != 0 && (current & flag) != 0)
        {
            var keyEvent = new KeyEventRef { Key = key };
            this.Abstract.OnKeyDown(ref keyEvent);
        }
    }

    private bool ShouldSoftwareTitleBarZoom => this.style.ShouldSoftwareTitleBarZoom(this);

    private static MouseButton GetMouseButton(NSEventType type) => type switch
    {
        NSEventType.LeftMouseDown or NSEventType.LeftMouseUp => MouseButton.Left,
        NSEventType.RightMouseDown or NSEventType.RightMouseUp => MouseButton.Right,
        _ => MouseButton.Other
    };

    // Preserves the original dispatch mapping exactly. The currently matched
    // events are mouse-up values, so this falls back to MouseButton's default.
    private static MouseButton GetMouseUpButton(NSEventType type) => type switch
    {
        NSEventType.LeftMouseDown => MouseButton.Left,
        NSEventType.RightMouseDown => MouseButton.Right,
        NSEventType.OtherMouseDown => MouseButton.Other,
        _ => default
    };

    private static bool IsResizeArea(WindowHitTestEventRef.WindowArea area) => area is
        WindowHitTestEventRef.WindowArea.BorderTop or
        WindowHitTestEventRef.WindowArea.BorderBottom or
        WindowHitTestEventRef.WindowArea.BorderLeft or
        WindowHitTestEventRef.WindowArea.BorderRight or
        WindowHitTestEventRef.WindowArea.BorderTopLeft or
        WindowHitTestEventRef.WindowArea.BorderTopRight or
        WindowHitTestEventRef.WindowArea.BorderBottomLeft or
        WindowHitTestEventRef.WindowArea.BorderBottomRight;

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
}
