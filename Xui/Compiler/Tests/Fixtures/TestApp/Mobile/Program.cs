using System;
using System.Runtime.InteropServices;
using TestAppCanvas;
using Xui.Apps.TestApp.Pages.Canvas.Tests;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Middleware.Emulator;

// Lifecycle bridge for the compiler milestone, not a replacement MobileTestAppShell.
public sealed class MobileSession
{
    public readonly CanvasContext Context = new();
    public readonly CompilerPhoneHost Phone = new();
    public readonly InteractiveFillRect View = new();
    public int Frames, PointerEvents, Animations;
    public int ActivePointer = -1;
    public long PreviousTicks;
    public bool ResetAnimationClock;
}

public sealed class InteractiveFillRect : FillRectTest
{
    public double MarkerX, MarkerY, Age = 1;
    private bool held;
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;
        MarkerX = (double)e.State.Position.X; MarkerY = (double)e.State.Position.Y;
        held = e.Type == PointerEventType.Down || e.Type == PointerEventType.Move;
        Age = 0;
        InvalidateRender();
        if (!held) RequestAnimationFrame();
    }
    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        if (held) return;
        Age += (current.Ticks - previous.Ticks) * 0.0000001d;
        InvalidateRender();
        if (Age < 1) RequestAnimationFrame();
    }
    protected override void RenderCore(IContext context)
    {
        base.RenderCore(context);
        if (Age >= 1) return;
        context.GlobalAlpha = (NFloat)(1 - Age);
        context.SetFill(new Color(0x6633DDFF));
        context.BeginPath();
        context.RoundRect(new Rect((NFloat)MarkerX - 20, (NFloat)MarkerY - 20, 40, 40), 20);
        context.Fill();
    }
}

public static class Program
{
    public static int Main() => 0;
    public static MobileSession Create() => new();
    public static void Frame(MobileSession session, double width, double height, long ticks)
    {
        if (width <= 48 || height <= 48) return;
        var view = session.View; var context = session.Context;
        if (NeedsFrame(session) != 0)
        {
            view.Animate(new TimeSpan(session.ResetAnimationClock ? ticks : session.PreviousTicks), new TimeSpan(ticks));
            session.Animations++;
        }
        session.PreviousTicks = ticks;
        session.ResetAnimationClock = false;
        session.Phone.Begin(context, width, height);
        var desired = view.Measure((393, 759), context);
        view.Arrange((0, 59, 393, 759), context, desired);
        view.Render(context);
        session.Phone.End(context);
        session.Frames++;
    }
    public static int NeedsFrame(MobileSession session) => (session.View.Flags & View.ViewFlags.Animated) != 0 ? 1 : 0;
    public static int Pointer(MobileSession session, int kind, int id, double x, double y, double pressure)
    {
        if (session.Phone.ScreenScale <= 0) return 0;
        x = (x - session.Phone.ScreenX) / session.Phone.ScreenScale;
        y = (y - session.Phone.ScreenY) / session.Phone.ScreenScale;
        if (kind == 2)
        {
            if (session.ActivePointer != -1 || x < 0 || x >= 393 || y < 59 || y >= 818) return 0;
            session.ActivePointer = id;
        }
        else if (session.ActivePointer != id) return 0;
        var state = new PointerState(new Point((NFloat)x, (NFloat)y), (1, 1), (NFloat)pressure, 0, (0, 0), 0, 0, 0,
            PointerType.Touch, PointerButton.Left, kind == 2 || kind == 3 ? PointerButtons.Left : PointerButtons.None);
        var e = new PointerEventRef((PointerEventType)kind, id, 0, true, state, default, default, session.Context);
        session.View.OnPointerEvent(ref e, EventPhase.Bubble);
        session.PointerEvents++;
        if (kind == 4 || kind == 5) { session.ActivePointer = -1; session.ResetAnimationClock = true; }
        return 1;
    }
    public static double ScreenX(MobileSession session) => session.Phone.ScreenX;
    public static double ScreenY(MobileSession session) => session.Phone.ScreenY;
    public static double ScreenScale(MobileSession session) => session.Phone.ScreenScale;
    public static double MarkerX(MobileSession session) => session.View.MarkerX;
    public static double MarkerY(MobileSession session) => session.View.MarkerY;
    public static int Frames(MobileSession session) => session.Frames;
    public static int PointerEvents(MobileSession session) => session.PointerEvents;
    public static int Animations(MobileSession session) => session.Animations;
}
