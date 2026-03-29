using System;
using System.Runtime.InteropServices;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Runtime.Windows.Win32.Types;
using static Xui.Runtime.Windows.Win32.User32;
using static Xui.Runtime.Windows.Win32.User32.Types;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Win32 native popup window — borderless, non-activating, owned by the parent HWND.
/// Reserved for future use: native-level popups that must extend outside the app window
/// (e.g. system-level dropdowns, tooltips anchored to the taskbar).
/// The cross-platform in-window overlay is handled by <c>PopupOverlay</c> in Xui.Core.
/// </summary>
internal sealed class Win32Popup : IPopup
{
    private static ushort popupClassAtom;
    private static WNDPROC? popupWndProcDelegate;

    private readonly Win32Window parentWindow;
    private HWND popupHwnd;

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

        var popupSize = size ?? new Size(anchorRect.Width, 120);
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

        // TODO: Initialize rendering pipeline (DirectXContext) for this HWND
        // so View content can be rendered into a native floating window that
        // can extend beyond the parent window bounds.

        HWND.ShowWindow(popupHwnd, 8); // SW_SHOWNA
    }

    public void Close()
    {
        if (popupHwnd.value == 0) return;

        DestroyWindow(popupHwnd);
        popupHwnd = default;

        Closed?.Invoke();
    }

    public void Dispose() => Close();

    private static void EnsureWindowClassRegistered()
    {
        if (popupClassAtom != 0) return;

        popupWndProcDelegate = PopupWndProc;
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

        popupClassAtom = RegisterClassEx(wc);
        Marshal.FreeHGlobal(className);
    }

    private static int PopupWndProc(HWND hWnd, WindowMessage uMsg, WPARAM wParam, LPARAM lParam)
        => HWND.DefWindowProc(hWnd, uMsg, wParam, lParam);
}
