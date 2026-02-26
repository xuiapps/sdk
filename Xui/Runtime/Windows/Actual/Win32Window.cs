using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Debug;
using CoreRuntime = Xui.Core.Actual.Runtime;
using Xui.Core.Math2D;
using Xui.Runtime.Windows.Win32;
using static Xui.Core.Abstract.IWindow.IDesktopStyle;
using static Xui.Runtime.Windows.Win32.Types;
using static Xui.Runtime.Windows.Win32.User32;
using static Xui.Runtime.Windows.Win32.User32.Types;

namespace Xui.Runtime.Windows.Actual;

public partial class Win32Window : Xui.Core.Actual.IWindow
{
    public const uint WM_ANIMATION_FRAME_MSG = 0x0401;

    [ThreadStatic]
    private static Win32Window? constructedInstanceOnStack;

    private static Dictionary<HWND, Win32Window> HwndToWindow = new Dictionary<HWND, Win32Window>();

    private volatile bool invalid = true;

    public bool NeedsFrame => this.invalid;

    private TimeSpan previous;
    private TimeSpan next;

    private NFloat dpiScale = 1.0f;

    private NFloat invDpiScale = 1.0f;

    private bool trackingMouseLeave;

    /// <summary>
    /// Physical pixels hidden at the top of the client area when extended frame is active.
    /// Normal: ~1px (DWM border). Maximized: frameY + pad (off-screen).
    /// </summary>
    private NFloat extendedFrameTopOffset;

    internal NFloat ExtendedFrameTopOffset => this.extendedFrameTopOffset;

    public nint CompositionFrameHandle => this.Renderer.FrameLatencyHandle;

    private static int OnMessageStatic(HWND hWnd, WindowMessage uMsg, WPARAM wParam, LPARAM lParam)
    {
        Win32Window window;
        if (HwndToWindow.TryGetValue(hWnd, out var w))
        {
            window = w;
        }
        else if (constructedInstanceOnStack != null)
        {
            window = constructedInstanceOnStack;
            HwndToWindow[hWnd] = constructedInstanceOnStack;
            constructedInstanceOnStack.Hwnd = hWnd;
            constructedInstanceOnStack = null;
        }
        else
        {
            throw new Win32Exception("Unknown window for hWnd.");
        }

        return window.OnMessage(hWnd, uMsg, wParam, lParam);
    }

    public Win32Window(Xui.Core.Abstract.IWindow @abstract)
    {
        this.Abstract = @abstract;
        this.Title = "";

        nint hbrBackground = GetSysColorBrush((int)WindowColor.COLOR_WINDOW);

        WNDPROC wndProcDelegate = OnMessageStatic;
        GCHandle.Alloc(wndProcDelegate);
        nint wndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);

        nint lpszClassNamePtr = Marshal.StringToHGlobalUni("XuiWindow");
        var w = new WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
            styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW,
            lpfnWndProc = wndProc,
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = 0,
            hIcon = 0,
            hCursor = 0,
            hbrBackground = hbrBackground,
            lpszMenuName = 0,
            lpszClassName = lpszClassNamePtr,
            hIconSm = 0
        };
        Marshal.FreeHGlobal(lpszClassNamePtr);

        this.Renderer = new DirectXContext(this);

        ushort classAtom = RegisterClassEx(w);

        uint dwExStyle;
        uint dwStyle;

        int x = 100;
        int y = 100;
        int width = 800;
        int height = 600;

        DesktopWindowLevel level = DesktopWindowLevel.Normal;

        if (this.Abstract is Xui.Core.Abstract.IWindow.IDesktopStyle desktopStyle)
        {
            level = desktopStyle.Level;

            if (desktopStyle.StartupSize.HasValue)
            {
                width = (int)desktopStyle.StartupSize.Value.Width;
                height = (int)desktopStyle.StartupSize.Value.Height;
            }

            if (desktopStyle.Backdrop == WindowBackdrop.Chromeless)
            {
                dwExStyle = (uint)ExtendedWindowStyles.WS_EX_NOREDIRECTIONBITMAP;
                dwStyle = (uint)WindowStyles.WS_POPUP;
                hbrBackground = 0;
            }
            else if (desktopStyle.Backdrop == WindowBackdrop.Mica)
            {
                dwExStyle = (uint)ExtendedWindowStyles.WS_EX_NOREDIRECTIONBITMAP;
                dwStyle = (uint)WindowStyles.WS_OVERLAPPEDWINDOW | (uint)WindowStyles.WS_VISIBLE | (uint)WindowStyles.WS_CAPTION;
                hbrBackground = 0;
            }
            else
            {
                dwExStyle = 0;
                dwStyle = (uint)WindowStyles.WS_TILEDWINDOW;
                hbrBackground = GetSysColorBrush((int)WindowColor.COLOR_WINDOW);
            }
        }
        else
        {
            dwExStyle = 0;
            dwStyle = (uint)WindowStyles.WS_TILEDWINDOW;
            hbrBackground = GetSysColorBrush((int)WindowColor.COLOR_WINDOW);
        }

        // This sets a property for the whole process.
        // That's better be in its own platform abstraction like PlatformUIProcess.
        SetProcessDpiAwarenessContext((nint)DPIAwarenessContext.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

        width = (int)(width * PrimaryMonitorDPIScale);
        height = (int)(height * PrimaryMonitorDPIScale);

        constructedInstanceOnStack = this;
        this.Hwnd = CreateWindowEx(
            dwExStyle,
            classAtom,
            this.Title,
            dwStyle,
            x, y, width, height,
            hWndParent: 0,
            hMenu: 0,
            hInstance: 0,
            lpParam: 0);

        if (constructedInstanceOnStack == this)
        {
            constructedInstanceOnStack = null;
        }

        if (this.Abstract is Xui.Core.Abstract.IWindow.IDesktopStyle desktopStyle2)
        {
            if (desktopStyle2.Backdrop == WindowBackdrop.Mica)
            {
                int backdropType = Dwmapi.DWMSBT_MAINWINDOW;
                Dwmapi.DwmSetWindowAttribute(this.Hwnd, Dwmapi.DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));

                var margins = new Dwmapi.MARGINS { Left = 0, Right = 0, Top = -1, Bottom = 0 };
                Dwmapi.DwmExtendFrameIntoClientArea(this.Hwnd, ref margins);
            }
        }

        HwndToWindow[this.Hwnd] = this;

        SetLevel(this.Hwnd, level);

        // Make black color in layered window transparent
        SetLayeredWindowAttributes(this.Hwnd, new COLORREF(0), 255, LayeredWindowAttribute.LWA_COLORKEY);

        // Initialize GPU resources now so they are available during OnAttach.
        this.Renderer.EnsureInitialized(this.Hwnd);
    }

    public HWND Hwnd { get; private set; }

    protected internal Xui.Core.Abstract.IWindow Abstract { get; }

    public DirectXContext Renderer { get; }

    public string Title
    {
        get
        {
            this.Hwnd.GetWindowText(out var str, 2048);
            return str;
        }
        set => this.Hwnd.SetWindowText(value);
    }

    public bool RequireKeyboard { get; set; }

    private DirectWriteContext? textMeasureContext;

    public ITextMeasureContext? TextMeasureContext
    {
        get
        {
            if (this.textMeasureContext == null && this.Renderer.DWriteFactory is { } factory)
            {
                this.textMeasureContext = new DirectWriteContext(factory);
            }

            return this.textMeasureContext;
        }
    }

    public IBitmapContext? BitmapContext => this.Renderer.ImageContext;

    public int OnMessage(HWND hWnd, WindowMessage uMsg, WPARAM wParam, LPARAM lParam)
    {
        var msg = (WindowMessage)uMsg;

        switch (msg)
        {
            case WindowMessage.WM_WINDOWPOSCHANGED:
            {
                this.UpdateExtendedFrameTopOffset();

                var res = this.Hwnd.DefWindowProc(uMsg, wParam, lParam);
                this.Hwnd.GetClientRect(out var rc);
                uint clientW = (uint)(rc.Right - rc.Left);
                uint clientH = (uint)(rc.Bottom - rc.Top);
                if (clientW > 0 && clientH > 0)
                {
                    this.Renderer.ResizeBuffers(hWnd, clientW, clientH);
                    this.invalid = true;
                    this.Render();
                    this.invalid = true;
                    Dwmapi.DwmFlush();
                }
                return res;
            }

            case WindowMessage.WM_NCCALCSIZE:
            {
                if (this.Abstract is Xui.Core.Abstract.IWindow.IDesktopStyle desktopStyle && desktopStyle.ClientArea == WindowClientArea.Extended)
                {
                    this.Hwnd.DefWindowProc(uMsg, wParam, lParam);

                    if (wParam.Value == 0)
                    {
                        unsafe
                        {
                            var rc = (RECT*)lParam.Value;

                            uint dpi = this.Hwnd.DPI;
                            int caption = GetSystemMetricsForDpi(SystemMetric.SM_CYCAPTION, dpi);
                            int frameY  = GetSystemMetricsForDpi(SystemMetric.SM_CYSIZEFRAME, dpi);
                            int pad     = GetSystemMetricsForDpi(SystemMetric.SM_CXPADDEDBORDER, dpi);

                            rc->Top -= caption + frameY + pad;
                        }

                        return 0;
                    }
                    else
                    {
                        unsafe
                        {
                            var p = (NCCALCSIZE_PARAMS*)lParam.Value;

                            uint dpi = this.Hwnd.DPI;
                            int caption = GetSystemMetricsForDpi(SystemMetric.SM_CYCAPTION, dpi);
                            int frameY  = GetSystemMetricsForDpi(SystemMetric.SM_CYSIZEFRAME, dpi);
                            int pad     = GetSystemMetricsForDpi(SystemMetric.SM_CXPADDEDBORDER, dpi);

                            p->r1.Top -= caption + frameY + pad;
                        }

                        return 0;
                    }
                }

                break;
            }

            case WindowMessage.WM_DPICHANGED:
            {
                this.UpdateDpiScale();

                // lParam points to a RECT in *physical pixels* (suggested new bounds)
                unsafe
                {
                    RECT* suggested = (RECT*)lParam.Value;
                    HWND.SetWindowPos(
                        this.Hwnd,
                        0,
                        suggested->Left,
                        suggested->Top,
                        suggested->Right - suggested->Left,
                        suggested->Bottom - suggested->Top,
                        SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
                }

                var res = this.Hwnd.DefWindowProc(uMsg, wParam, lParam);

                unsafe
                {
                    var p = (User32.RECT*)lParam.Value;

                    // ((D2DComp)this.Renderer).ResizeBuffers(hWnd, (uint)(p->Right - p->Left), (uint)(p->Bottom - p->Top));
                    // this.invalid = true;
                    // this.Render();
                    // this.invalid = true; // Fixe bug - wont render after startup
                    // Dwmapi.DwmFlush();
                }

                return res;
            }
            case WindowMessage.WM_SIZE:
            {
                hWnd.GetClientRect(out var sizeRc);
                var clientW = sizeRc.Right - sizeRc.Left;
                var clientH = sizeRc.Bottom - sizeRc.Top;
                var dipRect = this.ToDip(new Rect(0, this.extendedFrameTopOffset, clientW, clientH - this.extendedFrameTopOffset));

                this.Abstract.SafeArea = dipRect;

                CoreRuntime.CurrentInstruments.Log(Scope.Rendering, LevelOfDetail.Essential,
                    $"WM_SIZE client=({clientW}, {clientH}) dpi={this.dpiScale:F2} dip=({dipRect.Width:F1}, {dipRect.Height:F1})");

                var res = this.Hwnd.DefWindowProc(uMsg, wParam, lParam);

                // ((D2DComp)this.Renderer).ResizeBuffers(hWnd, (uint)clientW, (uint)clientH);
                // this.invalid = true;
                // this.Render();
                // this.invalid = true; // Fixe bug - wont render after startup
                // Dwmapi.DwmFlush();

                return res;
            }

            case WindowMessage.WM_PAINT:
            {
                // Validate the update region; do not render here.
                // Rendering should be driven by compositor pacing (frame latency handle / run loop),
                // not by WM_PAINT.
                PAINTSTRUCT ps = new();
                hWnd.BeginPaint(ref ps);
                hWnd.EndPaint(ref ps);

                return 0;
            }

            case WindowMessage.WM_CREATE:
            {
                this.UpdateDpiScale();
                break;
            }

            case WindowMessage.WM_CLOSE:
            {
                if (!this.Abstract.Closing())
                    return 0;
                break;
            }

            case WindowMessage.WM_DESTROY:
            {
                Win32Platform.Instance.RemoveWindow(this);
                this.Abstract.Closed();
                break;
            }

            case WindowMessage.WM_NCHITTEST:
            {
                POINT win32ClientPoint = new POINT() { X = lParam.LoWord, Y = lParam.HiWord };
                this.Hwnd.ScreenToClient(ref win32ClientPoint);

                bool isExtendedFrame = this.Abstract is Xui.Core.Abstract.IWindow.IDesktopStyle desktopStyle
                    && desktopStyle.ClientArea == WindowClientArea.Extended;

                if (isExtendedFrame)
                {
                    // Let DWM handle the window buttons (minimize/maximize/close).
                    if (Dwmapi.DwmDefWindowProc(hWnd, uMsg, wParam.Value, lParam.Value, out var dwmResult))
                    {
                        return (int)dwmResult;
                    }
                }

                Point point = this.ToDip(new Point(win32ClientPoint.X, win32ClientPoint.Y));

                hWnd.GetClientRect(out var rc);
                Rect rect = this.ToDip(new Rect(0, 0, rc.Right - rc.Left, rc.Bottom - rc.Top));

                WindowHitTestEventRef eventRef = new WindowHitTestEventRef(point, rect);

                // For extended client area, set the default frame hit areas
                // (resize borders and caption) before the abstract window gets
                // a chance to override them.
                if (isExtendedFrame)
                {
                    uint dpi = this.Hwnd.DPI;
                    int frameX  = GetSystemMetricsForDpi(SystemMetric.SM_CXSIZEFRAME, dpi);
                    int frameY  = GetSystemMetricsForDpi(SystemMetric.SM_CYSIZEFRAME, dpi);
                    int caption = GetSystemMetricsForDpi(SystemMetric.SM_CYCAPTION, dpi);
                    int pad     = GetSystemMetricsForDpi(SystemMetric.SM_CXPADDEDBORDER, dpi);

                    int x = win32ClientPoint.X;
                    int y = win32ClientPoint.Y;
                    int clientW = rc.Right - rc.Left;

                    bool isTop    = y < frameY;
                    bool isLeft   = x < frameX + pad;
                    bool isRight  = x >= clientW - frameX - pad;

                    if (isTop && isLeft)
                        eventRef.Area = WindowHitTestEventRef.WindowArea.BorderTopLeft;
                    else if (isTop && isRight)
                        eventRef.Area = WindowHitTestEventRef.WindowArea.BorderTopRight;
                    else if (isTop)
                        eventRef.Area = WindowHitTestEventRef.WindowArea.BorderTop;
                    else if (y < caption + frameY + pad)
                    {
                        if (isLeft)
                            eventRef.Area = WindowHitTestEventRef.WindowArea.BorderLeft;
                        else if (isRight)
                            eventRef.Area = WindowHitTestEventRef.WindowArea.BorderRight;
                        else
                            eventRef.Area = WindowHitTestEventRef.WindowArea.Title;
                    }
                }

                this.Abstract.WindowHitTest(ref eventRef);

                var area = eventRef.Area;
                switch (area)
                {
                    case WindowHitTestEventRef.WindowArea.Title: return (int)HitTest.HTCAPTION;

                    case WindowHitTestEventRef.WindowArea.Transparent: return (int)HitTest.HTTRANSPARENT;
                    case WindowHitTestEventRef.WindowArea.Client: return (int)HitTest.HTCLIENT;

                    case WindowHitTestEventRef.WindowArea.BorderTopLeft: return (int)HitTest.HTTOPLEFT;
                    case WindowHitTestEventRef.WindowArea.BorderTop: return (int)HitTest.HTTOP;
                    case WindowHitTestEventRef.WindowArea.BorderTopRight: return (int)HitTest.HTTOPRIGHT;
                    case WindowHitTestEventRef.WindowArea.BorderRight: return (int)HitTest.HTRIGHT;
                    case WindowHitTestEventRef.WindowArea.BorderBottomRight: return (int)HitTest.HTBOTTOMRIGHT;
                    case WindowHitTestEventRef.WindowArea.BorderBottom: return (int)HitTest.HTBOTTOM;
                    case WindowHitTestEventRef.WindowArea.BorderBottomLeft: return (int)HitTest.HTBOTTOMLEFT;
                    case WindowHitTestEventRef.WindowArea.BorderLeft: return (int)HitTest.HTLEFT;

                    case WindowHitTestEventRef.WindowArea.Default:
                    default:
                        break;
                }

                break;
            }

            case WindowMessage.WM_MOUSEMOVE:
            {
                // Ensure we get WM_MOUSELEAVE so hover can clear when leaving the window.
                if (!this.trackingMouseLeave)
                {
                    TRACKMOUSEEVENT tme = new TRACKMOUSEEVENT
                    {
                        cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
                        dwFlags = TrackMouseEventFlags.TME_LEAVE,
                        hwndTrack = this.Hwnd,
                        dwHoverTime = 0
                    };

                    TrackMouseEvent(ref tme);
                    this.trackingMouseLeave = true;
                }

                var pos = this.GetMousePosDip(lParam);

                MouseMoveEventRef mouseMoveEventRef = new MouseMoveEventRef()
                {
                    Position = pos
                };

                this.OnMouseMove(mouseMoveEventRef);
                return 0;
            }

            case WindowMessage.WM_MOUSELEAVE:
            {
                this.trackingMouseLeave = false;
                return 0;
            }

            case WindowMessage.WM_LBUTTONDOWN:
            {
                // Capture so we continue to get mouse up even if the cursor leaves the window while pressed.
                this.Hwnd.CaptureMouse();

                var pos = this.GetMousePosDip(lParam);

                // If you want parity with macOS: do a hit test first.
                var hit = new WindowHitTestEventRef(pos, this.GetClientRectDip(hWnd));
                this.Abstract.WindowHitTest(ref hit);

                // For now: always forward to Xui unless you're doing custom chrome behaviors.
                this.RaiseMouseDown(MouseButton.Left, pos);
                return 0;
            }

            case WindowMessage.WM_LBUTTONUP:
            {
                HWND.ReleaseCapture();

                var pos = this.GetMousePosDip(lParam);

                var hit = new WindowHitTestEventRef(pos, this.GetClientRectDip(hWnd));
                this.Abstract.WindowHitTest(ref hit);

                this.RaiseMouseUp(MouseButton.Left, pos);
                return 0;
            }

            case WindowMessage.WM_RBUTTONDOWN:
            {
                this.Hwnd.CaptureMouse();

                var pos = this.GetMousePosDip(lParam);
                this.RaiseMouseDown(MouseButton.Right, pos);
                return 0;
            }

            case WindowMessage.WM_RBUTTONUP:
            {
                HWND.ReleaseCapture();

                var pos = this.GetMousePosDip(lParam);
                this.RaiseMouseUp(MouseButton.Right, pos);
                return 0;
            }

            case WindowMessage.WM_MBUTTONDOWN:
            {
                this.Hwnd.CaptureMouse();

                var pos = this.GetMousePosDip(lParam);
                this.RaiseMouseDown(MouseButton.Other, pos);
                return 0;
            }

            case WindowMessage.WM_MBUTTONUP:
            {
                HWND.ReleaseCapture();

                var pos = this.GetMousePosDip(lParam);
                this.RaiseMouseUp(MouseButton.Other, pos);
                return 0;
            }

            case WindowMessage.WM_MOUSEWHEEL:
                ScrollWheelEventRef scrollWheelEventRef = new ScrollWheelEventRef()
                {
                    Delta = wParam.WheelDelta
                };

                this.OnScrollWheel(scrollWheelEventRef);
                return 0;

            case WindowMessage.WM_KEYDOWN:
            case WindowMessage.WM_SYSKEYDOWN:
            {
                var key = (VirtualKey)(wParam.Value & 0xFFFF);
                var isRepeat = (lParam.Value & (1 << 30)) != 0;
                var shift = (User32.GetKeyState(0x10) & 0x8000) != 0;
                var e = new KeyEventRef { Key = key, IsRepeat = isRepeat, Shift = shift };
                this.Abstract.OnKeyDown(ref e);
                if (e.Handled)
                    return 0;
                break;
            }

            case WindowMessage.WM_CHAR:
            {
                var character = (char)(wParam.Value & 0xFFFF);
                var isRepeat = (lParam.Value & (1 << 30)) != 0;
                var shift = (User32.GetKeyState(0x10) & 0x8000) != 0;
                var e = new KeyEventRef { Character = character, IsRepeat = isRepeat, Shift = shift };
                this.Abstract.OnChar(ref e);
                if (e.Handled)
                    return 0;
                break;
            }

            case WindowMessage.WM_SETCURSOR:
            {
                // LOWORD(lParam) == hit-test code
                int hitTest = (short)lParam.LoWord;

                if (hitTest == (int)HitTest.HTCLIENT)
                {
                    var arrow = Win32.User32.LoadCursor(0, (int)Win32.User32.SystemCursor.Arrow);
                    Win32.User32.SetCursor(arrow);
                    return 1;
                }

                // Let Windows handle resize / caption / etc
                return this.Hwnd.DefWindowProc(uMsg, wParam, lParam);
            }
        }

        return this.Hwnd.DefWindowProc(uMsg, wParam, lParam);
    }

    private void OnMouseMove(MouseMoveEventRef mouseMoveEventRef) =>
        this.Abstract.OnMouseMove(ref mouseMoveEventRef);

    protected virtual void OnScrollWheel(ScrollWheelEventRef scrollWheelEventRef) =>
        this.Abstract.OnScrollWheel(ref scrollWheelEventRef);

    public void Show()
    {
        CoreRuntime.CurrentInstruments.Log(Scope.Application, LevelOfDetail.Essential,
            $"Win32Window.Show hwnd={this.Hwnd} dpiScale={this.dpiScale:F2}");
        this.Hwnd.ShowWindow();
        this.Hwnd.UpdateWindow();
    }

    public void Close() => DestroyWindow(this.Hwnd);

    public virtual void Invalidate() => this.invalid = true;

    public void OnAnimationFrame(ref FrameEventRef @event) =>
        this.Abstract.OnAnimationFrame(ref @event);

    public void Render()
    {
        if (this.invalid)
        {
            this.invalid = false;
            this.Renderer.Render();

            CoreRuntime.CurrentInstruments.Log(Scope.ViewState, LevelOfDetail.Info,
                $"Win32Window.Render completed, invalid={this.invalid}");
        }
        else
        {
            CoreRuntime.CurrentInstruments.Log(Scope.ViewState, LevelOfDetail.Diagnostic,
                $"Win32Window.Render SKIPPED (invalid=false)");
        }
    }

    internal void Render(RenderEventRef render)
    {
        // Console.WriteLine("Render()");
        this.Abstract.Render(ref render);
    }


    private static void SetLevel(HWND hwnd, DesktopWindowLevel level)
    {
        HWND insertAfter = level == DesktopWindowLevel.Normal
            ? HWND.HWND_NOTOPMOST
            : HWND.HWND_TOPMOST;

        HWND.SetWindowPos(
            hwnd,
            insertAfter,
            0, 0, 0, 0,
            SetWindowPosFlags.SWP_NOMOVE |
            SetWindowPosFlags.SWP_NOSIZE |
            SetWindowPosFlags.SWP_NOACTIVATE);
    }

    public static float PrimaryMonitorDPIScale => GetDpiForSystem() / 96f;

    private void UpdateDpiScale()
    {
        this.dpiScale = (NFloat)this.Hwnd.DPIScale;
        this.invDpiScale = (NFloat)1.0 / this.dpiScale;
    }

    private Point ToDip(Point p) => new Point(p.X * this.invDpiScale, p.Y * this.invDpiScale);

    private Rect ToDip(Rect r) => new Rect(
        r.X * this.invDpiScale,
        r.Y * this.invDpiScale,
        r.Width * this.invDpiScale,
        r.Height * this.invDpiScale);

    private Point GetMousePosDip(LPARAM lParam)
    {
        var pos = lParam.MousePosition * this.invDpiScale;
        return new Point(pos.X, pos.Y - this.extendedFrameTopOffset * this.invDpiScale);
    }

    private Rect GetClientRectDip(HWND hWnd)
    {
        hWnd.GetClientRect(out var rc);
        return this.ToDip(new Rect(0, this.extendedFrameTopOffset, rc.Right - rc.Left, rc.Bottom - rc.Top - this.extendedFrameTopOffset));
    }

    private void UpdateExtendedFrameTopOffset()
    {
        if (this.Abstract is Xui.Core.Abstract.IWindow.IDesktopStyle desktopStyle
            && desktopStyle.ClientArea == WindowClientArea.Extended)
        {
            uint dpi = this.Hwnd.DPI;
            if (this.Hwnd.IsMaximized)
            {
                int frameY = GetSystemMetricsForDpi(SystemMetric.SM_CYSIZEFRAME, dpi);
                int pad    = GetSystemMetricsForDpi(SystemMetric.SM_CXPADDEDBORDER, dpi);
                this.extendedFrameTopOffset = frameY + pad;
            }
            else
            {
                this.extendedFrameTopOffset = 1.0f * dpi / 96.0f;
            }
        }
        else
        {
            this.extendedFrameTopOffset = 0;
        }
    }

    private void RaiseMouseDown(MouseButton button, Point pos)
    {
        var evRef = new MouseDownEventRef()
        {
            Position = pos,
            Button = button
        };
        this.Abstract.OnMouseDown(ref evRef);
    }

    private void RaiseMouseUp(MouseButton button, Point pos)
    {
        var evRef = new MouseUpEventRef()
        {
            Position = pos,
            Button = button
        };
        this.Abstract.OnMouseUp(ref evRef);
    }
}
