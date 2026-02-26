using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.Windows.Win32.Types;
using static Xui.Runtime.Windows.Win32.User32.Types;

namespace Xui.Runtime.Windows.Win32;

public static partial class User32
{
    public const string User32Lib = "user32.dll";

    public static readonly nint Lib = NativeLibrary.Load(User32Lib);

    [LibraryImport(User32Lib)]
    public static partial nint GetSysColorBrush(int nIndex);

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerclassexw
    /// </summary>
    [LibraryImport(User32Lib, EntryPoint="RegisterClassExW")]
    public static partial ushort RegisterClassEx(WNDCLASSEXW p);

    [LibraryImport(User32Lib, EntryPoint="CreateWindowExW")]
    public static partial nint CreateWindowEx(uint dwExStyle, ushort atom, [MarshalAs(UnmanagedType.LPUTF8Str)] string lpWindowName, uint dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, nint hMenu, nint hInstance, nint lpParam);
    
    [LibraryImport(User32Lib)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetProcessDpiAwarenessContext(nint dpiContext);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-translatemessage</summary>
    [LibraryImport(User32Lib)]
    public static partial int TranslateMessage(ref MSG msg);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-dispatchmessagew</summary>
    [LibraryImport(User32Lib, EntryPoint="DispatchMessageW")]
    public static partial nint DispatchMessage(ref MSG msg);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-dispatchmessagew</summary>
    [LibraryImport(User32Lib, EntryPoint="GetMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetMessage(ref MSG msg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-dispatchmessagew</summary>
    [LibraryImport(User32Lib, EntryPoint="PeekMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PeekMessage(ref MSG msg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-postmessagew</summary>
    [LibraryImport(User32Lib, EntryPoint="PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessage(HWND hWnd, WindowMessage Msg, WPARAM wParam, LPARAM lParam);

    [LibraryImport(User32Lib)]
    public static partial nuint SetTimer(HWND hWnd, nuint nIDEvent, uint uElapse, nint lpTimerFunc);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessagew</summary>
    [LibraryImport(User32Lib, EntryPoint = "SendMessageW")]
    public static partial nint SendMessage(HWND hWnd, uint Msg, nuint wParam, nint lParam);

    [LibraryImport(User32Lib)]
    public static unsafe partial BOOL SetLayeredWindowAttributes(HWND hWnd, COLORREF crKey, byte bAlpha, LayeredWindowAttribute dwFlags);

    [LibraryImport(User32Lib)]
    public static partial uint GetDpiForSystem();

    [DllImport(User32Lib, SetLastError = true)]
    public static extern BOOL TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

    [LibraryImport(User32Lib, EntryPoint = "LoadCursorW", SetLastError = true)]
    public static partial nint LoadCursor(nint hInstance, int lpCursorName);

    [LibraryImport(User32Lib)]
    public static partial nint SetCursor(nint hCursor);

    [LibraryImport(User32Lib)]
    public static partial short GetKeyState(int nVirtKey);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-postthreadmessagew</summary>
    [LibraryImport(User32Lib, EntryPoint = "PostThreadMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostThreadMessage(uint idThread, uint Msg, nuint wParam, nint lParam);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-msgwaitformultipleobjectsex</summary>
    [LibraryImport(User32Lib, SetLastError = true)]
    public static partial uint MsgWaitForMultipleObjectsEx(
        uint nCount,
        nint[]? pHandles,
        uint dwMilliseconds,
        uint dwWakeMask,
        uint dwFlags);

    [LibraryImport(User32Lib)]
    public static partial int GetSystemMetricsForDpi(SystemMetric nIndex, uint dpi);

    [LibraryImport(User32Lib, EntryPoint = "GetSystemMetrics")]
    public static partial int GetSystemMetrics(SystemMetric nIndex);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-postquitmessage</summary>
    [LibraryImport(User32Lib)]
    public static partial void PostQuitMessage(int nExitCode);

    /// <summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroywindow</summary>
    [LibraryImport(User32Lib)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyWindow(HWND hWnd);
}