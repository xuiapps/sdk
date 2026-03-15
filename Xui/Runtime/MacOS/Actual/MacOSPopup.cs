using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Runtime.MacOS.AppKit;
using static Xui.Runtime.MacOS.Foundation;
using static Xui.Runtime.MacOS.ObjC;

namespace Xui.Runtime.MacOS.Actual;

/// <summary>
/// macOS popup implementation using a borderless child NSWindow.
/// The popup window is non-activating (does not steal focus from the parent)
/// and auto-dismisses when the user clicks outside it.
/// </summary>
internal sealed class MacOSPopup : IPopup
{
    private readonly MacOSWindow parentWindow;
    private NSWindow? popupWindow;
    private PopupRootView? rootView;
    private View? content;
    private readonly MacOSDrawingContext drawingContext = new();

    public bool IsVisible => popupWindow != null;
    public event Action? Closed;

    public MacOSPopup(MacOSWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }

    public void Show(View content, Rect anchorRect, PopupPlacement placement, Size? size, PopupEffect effect)
    {
        if (IsVisible)
            Close();

        this.content = content;

        // Determine popup size
        var popupSize = size ?? new Size(anchorRect.Width, 120);

        // Convert anchor rect from the parent's flipped content coordinates to screen coordinates.
        // macOS uses bottom-left origin, but our rootView is flipped, so we need to
        // convert through the window's coordinate system.
        var contentFrame = parentWindow.ContentView?.Frame ?? new NSRect();
        var contentHeight = contentFrame.Size.height;

        // anchorRect is in the flipped content-view coordinate space (origin = top-left).
        // NSWindow.convertRectToScreen expects window coordinates (origin = bottom-left of window).
        // The root view is flipped, so flipped-Y = contentHeight - y - height.
        var windowRect = new NSRect
        {
            Origin = new NSPoint
            {
                x = (NFloat)anchorRect.X,
                y = contentHeight - (NFloat)anchorRect.Y - (NFloat)anchorRect.Height
            },
            Size = new NSSize
            {
                width = (NFloat)anchorRect.Width,
                height = (NFloat)anchorRect.Height
            }
        };

        var screenRect = parentWindow.ConvertRectToScreen(windowRect);

        // Position popup based on placement
        NFloat popupX, popupY;
        switch (placement)
        {
            case PopupPlacement.Above:
                popupX = screenRect.Origin.x;
                popupY = screenRect.Origin.y + screenRect.Size.height;
                break;
            case PopupPlacement.Right:
                popupX = screenRect.Origin.x + screenRect.Size.width;
                popupY = screenRect.Origin.y;
                break;
            case PopupPlacement.Left:
                popupX = screenRect.Origin.x - (NFloat)popupSize.Width;
                popupY = screenRect.Origin.y;
                break;
            case PopupPlacement.Below:
            default:
                popupX = screenRect.Origin.x;
                popupY = screenRect.Origin.y - (NFloat)popupSize.Height;
                break;
        }

        var popupFrame = new NSRect
        {
            Origin = new NSPoint { x = popupX, y = popupY },
            Size = new NSSize { width = (NFloat)popupSize.Width, height = (NFloat)popupSize.Height }
        };

        // Create borderless, non-activating popup window via the NSWindow constructor
        popupWindow = new NSWindow(popupFrame);
        popupWindow.IsReleasedWhenClosed = false;
        popupWindow.HasShadow = true;
        popupWindow.Level = NSWindowLevel.Floating;

        // Create root view for rendering content
        rootView = new PopupRootView(this);

        // TODO: Work on liquid glass effect
        using var bgColor = new NSColorRef(1f, 1f, 1f, 1f);
        popupWindow.BackgroundColor = bgColor;
        popupWindow.ContentView = rootView;

        // Attach as child window (moves with parent, stays above)
        parentWindow.AddChildWindow(popupWindow);
        popupWindow.OrderFrontRegardless();

        // Trigger initial render
        rootView.NeedsDisplay = true;
    }

    public void Close()
    {
        if (popupWindow == null) return;

        parentWindow.RemoveChildWindow(popupWindow);
        popupWindow.OrderOut();
        popupWindow = null;
        rootView = null;
        content = null;

        Closed?.Invoke();
    }

    /// <summary>
    /// Called by the parent window's SendEvent to check if a mouse-down
    /// should dismiss this popup. Returns true if the popup was dismissed.
    /// </summary>
    internal bool TryDismissOnMouseDown(NSPoint screenPoint)
    {
        if (popupWindow == null) return false;

        var frame = popupWindow.Rect;
        if (screenPoint.x < frame.Origin.x ||
            screenPoint.x > frame.Origin.x + frame.Size.width ||
            screenPoint.y < frame.Origin.y ||
            screenPoint.y > frame.Origin.y + frame.Size.height)
        {
            Close();
            return true;
        }
        return false;
    }

    internal void Render(NSRect dirtyRect)
    {
        if (content == null || popupWindow == null) return;

        var frame = rootView!.Frame;
        var rect = new Rect(0, 0, frame.Size.width, frame.Size.height);

        var drawCtx = drawingContext.Bind();
        IContext ctx = drawCtx;
        MacOSPlatform.DisplayContextStack.Push(drawCtx);
        try
        {
            // White background
            ctx.SetFill(new Color(0xFF, 0xFF, 0xFF, 0xFF));
            ctx.FillRect(rect);

            // Full layout + render pass via the view's Update method
            content.Update(new LayoutGuide()
            {
                Anchor = rect.TopLeft,
                Pass =
                    LayoutGuide.LayoutPass.Measure |
                    LayoutGuide.LayoutPass.Arrange |
                    LayoutGuide.LayoutPass.Render,
                AvailableSize = rect.Size,
                MeasureContext = ctx,
                XAlign = LayoutGuide.Align.Start,
                YAlign = LayoutGuide.Align.Start,
                XSize = LayoutGuide.SizeTo.Exact,
                YSize = LayoutGuide.SizeTo.Exact,
                RenderContext = ctx,
            });
        }
        finally
        {
            MacOSPlatform.DisplayContextStack.Pop();
        }
    }

    public void Dispose()
    {
        Close();
    }

    /// <summary>
    /// Root NSView for the popup window. Flipped coordinate system, delegates drawing to the popup.
    /// </summary>
    private sealed class PopupRootView : NSView
    {
        private static new readonly Class Class = NSView.Class
            .Extend("XUIMacOSPopupRootView")
            .AddMethod("drawRect:", DrawRect)
            .Register();

        public static void DrawRect(nint self, nint sel, NSRect rect) =>
            Marshalling.Get<PopupRootView>(self).DrawRect(rect);

        private readonly MacOSPopup popup;

        public PopupRootView(MacOSPopup popup) : base(Class.New())
        {
            this.popup = popup;
            this.Flipped = true;
        }

        private void DrawRect(NSRect rect) => this.popup.Render(rect);
    }

    /// <summary>
    /// Creates a borderless, non-activating NSWindow for popup use.
    /// </summary>
    private sealed class NSWindow : AppKit.NSWindow
    {
        public NSWindow(NSRect frame) : base(
            InitWithContentRectStyleMaskBackingDefer(
                AppKit.NSWindow.Class.Alloc(),
                rect: frame,
                nswindowstylemask: NSWindowStyleMask.Borderless,
                nsbackingstoretype: NSBackingStoreType.Buffered,
                defer: false))
        {
        }
    }
}
