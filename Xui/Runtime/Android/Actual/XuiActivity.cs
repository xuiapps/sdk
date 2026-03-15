using System;
using System.Runtime.InteropServices;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Xui.Core.Abstract.Events;

namespace Xui.Runtime.Android.Actual;

public class XuiActivity : global::Android.App.Activity, Xui.Core.Actual.IWindow, Choreographer.IFrameCallback
{
    public static XuiActivity? Instance { get; private set; }

    protected AndroidDrawingContext Context = new AndroidDrawingContext();

    protected XuiRoot? root;
    protected XuiUI? ui;

    private TimeSpan previous = TimeSpan.Zero;
    private TimeSpan next = TimeSpan.Zero;

    public new string Title { get; set; } = "";

    public Xui.Core.Abstract.IWindow? Abstract { get; internal set; }
    public bool RequireKeyboard { get; set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        if (Instance != null)
        {
            throw new Exception($"There may by only one instance of {nameof(XuiActivity)}.");
        }

        Instance = this;

        base.OnCreate(savedInstanceState);

        var windowFlags = global::Android.Views.WindowManagerFlags.LayoutNoLimits;
        this.Window!.SetFlags(windowFlags, windowFlags);

        this.root = new XuiRoot(this);
        this.ui = new XuiUI(this);
        this.root.AddView(this.ui);
        this.SetContentView(this.root);

        Choreographer.Instance!.PostFrameCallback(this);

        // Xui.Apps.BlankApp.App.Main(new string[0]);
    }

    public void Invalidate()
    {
        this.ui!.Invalidate();
    }

    public void Show()
    {
    }

    object? IServiceProvider.GetService(Type serviceType) => null;

    public override bool OnTouchEvent(MotionEvent? e)
    {
        if (e != null)
        {
            var pointersCount = e.PointerCount;
            Span<Touch> touches = stackalloc Touch[pointersCount];

            var density = this.Resources?.DisplayMetrics?.Density ?? 1;

            for (var i = 0; i < pointersCount; i++)
            {
                touches[i].Position = (e.GetX(i) / density, e.GetY(i) / density);
                touches[i].Radius = e.GetSize(i);
                touches[i].Index = e.GetPointerId(i);
                // TODO: Maybe pressure = radius?
                // touches[i].Radius = e.GetPressure(i);
                // touches[i].Phase = e.getAction
            }

            if (pointersCount == 1)
            {
                // TODO: Capture phases per index in a dictionary?
                // So we can refer them in batch pointer updates...
                var action = e.Action;
                switch(action)
                {
                    case MotionEventActions.Down:
                        touches[0].Phase = TouchPhase.Start;
                        break;
                    case MotionEventActions.Up:
                        touches[0].Phase = TouchPhase.End;
                        break;
                    default:
                        touches[0].Phase = TouchPhase.Move;
                        break;
                }
            }

            var touchEventRef = new TouchEventRef(touches);
            this.Abstract!.OnTouch(ref touchEventRef);
        }

        return base.OnTouchEvent(e);
    }

    internal void OnDraw(Core.Math2D.Vector size, Canvas canvas)
    {
        this.Context.Canvas = canvas;

        var density = this.Resources?.DisplayMetrics?.Density ?? 1;
        RenderEventRef render = new RenderEventRef()
        {
            Frame = new FrameEventRef(this.previous, this.next),
            Rect = new Core.Math2D.Rect(0, 0, size.X / density, size.Y / density)
        };
        canvas.Scale(density, density);

        this.Abstract!.Render(ref render);
    }

    public void DoFrame(long frameTimeNanos)
    {
        Choreographer.Instance!.PostFrameCallback(this);

        previous = TimeSpan.FromSeconds(frameTimeNanos / 1000000000.0);
        // TODO: Assumed 60 fps, but it will vary per device...
        next = previous + TimeSpan.FromSeconds(1 / 60.0);

        var animationFrameRef = new FrameEventRef(previous, next);
        this.Abstract!.OnAnimationFrame(ref animationFrameRef);
    }
}
