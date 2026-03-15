using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;

namespace Xui.Runtime.Browser.Actual;

public partial class BrowserWindow : Xui.Core.Actual.IWindow
{
    [JSImport("Xui.Runtime.Browser.Actual.BrowserWindow.setTitle", "main.js")]
    internal static partial void SetTitle(string title);

    [JSExport]
    internal static void OnAnimationFrame(double width, double height, double timestamp, double pixelRatio) => Instance?.SendAnimationFrameEvent(width, height, timestamp, pixelRatio);

    [JSExport]
    internal static void OnMouseMove(double x, double y) => Instance?.SendMouseMoveEvent(x, y);

    [JSExport]
    internal static void OnWheel(double x, double y, double deltaX, double deltaY) => Instance?.SendWheelEvent(x, y, deltaX, deltaY);

    [JSExport]
    internal static void OnTouchStart(int[] id, double[] x, double[] y, double[] force) => Instance?.SendTouchStart(id, x, y, force);

    [JSExport]
    internal static void OnTouchMove(int[] id, double[] x, double[] y, double[] force) => Instance?.SendTouchMove(id, x, y, force);

    [JSExport]
    internal static void OnTouchEnd(int[] id, double[] x, double[] y, double[] force) => Instance?.SendTouchEnd(id, x, y, force);

    public static BrowserWindow? Instance { get; internal set; }

    private Xui.Core.Abstract.IWindow Abstract;

    private string _title = "";
    private bool invalidated = false;

    public BrowserWindow(Xui.Core.Abstract.IWindow windowAbstract)
    {
        this.Abstract = windowAbstract;
    }

    public string Title
    {
        get => _title;
        set
        {
            if (this._title != value)
            {
                this._title = value;
                SetTitle(this._title);
            }
        }
    }

    public bool RequireKeyboard { get; set; }

    public void Invalidate()
    {
        this.invalidated = true;
        // Call "draw" on next animation frame...
    }

    public void Show()
    {
    }

    public object? GetService(Type serviceType) => null;

    private void SendAnimationFrameEvent(double width, double height, double timestamp, double pixelRatio)
    {
        var previous = TimeSpan.FromMilliseconds(timestamp);
        // TODO: Imagine 60fps in all browsers, probably find an API or track FPS.
        var next = previous + TimeSpan.FromSeconds(1.0 / 60.0);
        FrameEventRef frameEventRef = new FrameEventRef(previous, next);
        this.Abstract.OnAnimationFrame(ref frameEventRef);

        // If invalidated = draw...
        if (this.invalidated)
        {
            this.invalidated = false;

            Rect rect = new Rect(0, 0, (NFloat)width, (NFloat)height);
            RenderEventRef renderEventRef = new RenderEventRef(rect, frameEventRef);

            BrowserDrawingContext.CanvasReset();
            BrowserDrawingContext.CanvasScale(pixelRatio, pixelRatio);
            this.Abstract.Render(ref renderEventRef);
        }
    }

    private void SendMouseMoveEvent(double x, double y)
    {
        MouseMoveEventRef mouseMoveEventRef = new MouseMoveEventRef()
        {
            Position = new Core.Math2D.Point((NFloat)x, (NFloat)y)
        };
        this.Abstract.OnMouseMove(ref mouseMoveEventRef);
    }

    private void SendWheelEvent(double x, double y, double deltaX, double deltaY)
    {
        ScrollWheelEventRef scrollWheelEventRef = new ScrollWheelEventRef()
        {
            Delta = new Vector((NFloat)deltaX, (NFloat)deltaY)
        };
        this.Abstract.OnScrollWheel(ref scrollWheelEventRef);
    }

    private void SendTouchStart(int[] id, double[] x, double[] y, double[] force)
    {
        Span<Touch> touches = stackalloc Touch[id.Length];
        for (int i = 0; i < id.Length; i++)
        {
            touches[i] = new Touch
            {
                Index = id[i],
                Position = new Point((NFloat)x[i], (NFloat)y[i]),
                Phase = TouchPhase.Start,
            };
        }
        TouchEventRef touchEventRef = new TouchEventRef(touches);
        this.Abstract.OnTouch(ref touchEventRef);
    }

    private void SendTouchMove(int[] id, double[] x, double[] y, double[] force)
    {
        Span<Touch> touches = stackalloc Touch[id.Length];
        for (int i = 0; i < id.Length; i++)
        {
            touches[i] = new Touch
            {
                Index = id[i],
                Position = new Point((NFloat)x[i], (NFloat)y[i]),
                Phase = TouchPhase.Move,
            };
        }
        TouchEventRef touchEventRef = new TouchEventRef(touches);
        this.Abstract.OnTouch(ref touchEventRef);
    }

    private void SendTouchEnd(int[] id, double[] x, double[] y, double[] force)
    {
        Span<Touch> touches = stackalloc Touch[id.Length];
        for (int i = 0; i < id.Length; i++)
        {
            touches[i] = new Touch
            {
                Index = id[i],
                Position = new Point((NFloat)x[i], (NFloat)y[i]),
                Phase = TouchPhase.End,
            };
        }

        TouchEventRef touchEventRef = new TouchEventRef(touches);
        this.Abstract.OnTouch(ref touchEventRef);
    }
}
