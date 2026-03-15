using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Runtime.Windows.Win32.Types;
using static Xui.Runtime.Windows.Win32.User32;
using static Xui.Runtime.Windows.Win32.User32.Types;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Win32 popup implementation using a borderless, non-activating owned window.
/// The popup auto-dismisses when the user clicks outside it.
/// </summary>
internal sealed class Win32Popup : IPopup
{
    private static ushort popupClassAtom;
    private static WNDPROC? popupWndProcDelegate;

    private readonly Win32Window parentWindow;
    private HWND popupHwnd;
    private View? content;

    public bool IsVisible => popupHwnd.value != 0;
    public event Action? Closed;

    public Win32Popup(Win32Window parentWindow)
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

        // Convert anchor rect from DIP client coordinates to screen pixels.
        var dpiScale = parentWindow.Hwnd.DPIScale;

        var clientTopLeft = new POINT
        {
            X = (int)(anchorRect.X * dpiScale),
            Y = (int)(anchorRect.Y * dpiScale)
        };
        ClientToScreen(parentWindow.Hwnd, ref clientTopLeft);

        var clientBottomRight = new POINT
        {
            X = (int)((anchorRect.X + anchorRect.Width) * dpiScale),
            Y = (int)((anchorRect.Y + anchorRect.Height) * dpiScale)
        };
        ClientToScreen(parentWindow.Hwnd, ref clientBottomRight);

        int anchorScreenX = clientTopLeft.X;
        int anchorScreenY = clientTopLeft.Y;
        int anchorScreenW = clientBottomRight.X - clientTopLeft.X;
        int anchorScreenH = clientBottomRight.Y - clientTopLeft.Y;

        int popupW = (int)(popupSize.Width * dpiScale);
        int popupH = (int)(popupSize.Height * dpiScale);

        // Position popup based on placement
        int popupX, popupY;
        switch (placement)
        {
            case PopupPlacement.Above:
                popupX = anchorScreenX;
                popupY = anchorScreenY - popupH;
                break;
            case PopupPlacement.Right:
                popupX = anchorScreenX + anchorScreenW;
                popupY = anchorScreenY;
                break;
            case PopupPlacement.Left:
                popupX = anchorScreenX - popupW;
                popupY = anchorScreenY;
                break;
            case PopupPlacement.Below:
            default:
                popupX = anchorScreenX;
                popupY = anchorScreenY + anchorScreenH;
                break;
        }

        EnsureWindowClassRegistered();

        popupHwnd = CreateWindowEx(
            dwExStyle: (uint)(ExtendedWindowStyles.WS_EX_TOOLWINDOW |
                              ExtendedWindowStyles.WS_EX_NOACTIVATE |
                              ExtendedWindowStyles.WS_EX_NOREDIRECTIONBITMAP),
            atom: popupClassAtom,
            lpWindowName: "",
            dwStyle: (uint)(WindowStyles.WS_POPUP | WindowStyles.WS_CLIPSIBLINGS),
            X: popupX, Y: popupY,
            nWidth: popupW, nHeight: popupH,
            hWndParent: parentWindow.Hwnd,
            hMenu: 0, hInstance: 0, lpParam: 0);

        // TODO: Initialize rendering pipeline (DirectXContext or D2D render target)
        // for the popup HWND so content can be rendered via View.Update(LayoutGuide).

        // Show without activating
        HWND.ShowWindow(popupHwnd, 8); // SW_SHOWNA = 8
    }

    public void Close()
    {
        if (popupHwnd.value == 0) return;

        DestroyWindow(popupHwnd);
        popupHwnd = default;
        content = null;

        Closed?.Invoke();
    }

    /// <summary>
    /// Called by the parent window's WM_LBUTTONDOWN handler to check if a
    /// mouse-down should dismiss this popup. Returns true if the popup was dismissed.
    /// </summary>
    internal bool TryDismissOnMouseDown(POINT screenPoint)
    {
        if (popupHwnd.value == 0) return false;

        HWND.GetWindowRect(popupHwnd, out var rect);
        if (screenPoint.X < rect.Left ||
            screenPoint.X > rect.Right ||
            screenPoint.Y < rect.Top ||
            screenPoint.Y > rect.Bottom)
        {
            Close();
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        Close();
    }

    private static void EnsureWindowClassRegistered()
    {
        if (popupClassAtom != 0) return;

        popupWndProcDelegate = PopupWndProc;
        GCHandle.Alloc(popupWndProcDelegate);
        nint wndProc = Marshal.GetFunctionPointerForDelegate(popupWndProcDelegate);

        nint className = Marshal.StringToHGlobalUni("XuiPopup");
        var wc = new WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
            styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW,
            lpfnWndProc = wndProc,
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = 0,
            hIcon = 0,
            hCursor = 0,
            hbrBackground = GetSysColorBrush((int)WindowColor.COLOR_WINDOW),
            lpszMenuName = 0,
            lpszClassName = className,
            hIconSm = 0
        };
        Marshal.FreeHGlobal(className);

        popupClassAtom = RegisterClassEx(wc);
    }

    private static int PopupWndProc(HWND hWnd, WindowMessage uMsg, WPARAM wParam, LPARAM lParam)
    {
        // TODO: Handle WM_PAINT for rendering popup content.
        return HWND.DefWindowProc(hWnd, uMsg, wParam, lParam);
    }
}
