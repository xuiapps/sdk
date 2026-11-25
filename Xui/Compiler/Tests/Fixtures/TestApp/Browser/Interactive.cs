using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TestAppCanvas;
using Xui.Apps.TestApp;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Middleware.Emulator;

public sealed class BrowserSession
{
    public readonly CanvasContext Context = new();
    public readonly CompilerPhoneHost Phone = new();
    public readonly Window Window;
    public readonly BrowserPlatform Platform;
    public int Frames, PointerEvents, Animations;
    public int ActivePointer = -1;
    public long PreviousTicks;
    public BrowserSession()
    {
        Platform = new BrowserPlatform(Context);
        Window = new Window(Platform)
        {
            DisplayArea = (0, 0, 393, 852), SafeArea = (0, 59, 393, 759),
            Content = BrowserMenu.CreateRoot(new MobilePerformanceMetrics())
        };
    }
}

public static class Program
{
    [InlineArray(1)] private struct TouchBuffer { private Touch first; }
    public static int Main() => 0;
    public static BrowserSession Create() => new();
    public static void Frame(BrowserSession session, double width, double height, long ticks)
    {
        if (width <= 48 || height <= 48) return;
        session.Platform.Clock.Ticks = ticks;
        ((BrowserWindow)session.Window.Actual).Dirty = false;
        var frame = new FrameEventRef(new TimeSpan(session.Frames == 0 ? ticks : session.PreviousTicks), new TimeSpan(ticks));
        session.Window.OnAnimationFrame(ref frame);
        session.PreviousTicks = ticks;
        session.Animations++;
        session.Phone.Begin(session.Context, width, height);
        var root = session.Window.RootView;
        var desired = root.Measure((393, 852), session.Context);
        root.Arrange((0, 0, 393, 852), session.Context, desired);
        root.Render(session.Context);
        session.Phone.End(session.Context);
        session.Frames++;
    }
    public static int NeedsFrame(BrowserSession session) => ((BrowserWindow)session.Window.Actual).Dirty ||
        (session.Window.RootView.Flags & (View.ViewFlags.Animated | View.ViewFlags.DescendantAnimated)) != 0 ? 1 : 0;
    public static int Pointer(BrowserSession session, int kind, int id, double x, double y, double pressure)
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
        TouchBuffer touches = default;
        touches[0] = new Touch { Index=id, Position=((NFloat)x,(NFloat)y), Radius=1,
            Phase=kind == 2 ? TouchPhase.Start : kind == 3 ? TouchPhase.Move : kind == 4 ? TouchPhase.End : TouchPhase.Cancel };
        var touch = new TouchEventRef(touches);
        session.Window.OnTouch(ref touch);
        session.PointerEvents++;
        if (kind == 4 || kind == 5) session.ActivePointer = -1;
        return 1;
    }
    public static double ScreenX(BrowserSession session) => session.Phone.ScreenX;
    public static double ScreenY(BrowserSession session) => session.Phone.ScreenY;
    public static double ScreenScale(BrowserSession session) => session.Phone.ScreenScale;
    public static int Frames(BrowserSession session) => session.Frames;
    public static int PointerEvents(BrowserSession session) => session.PointerEvents;
    public static int Animations(BrowserSession session) => session.Animations;
    private static View Find(BrowserSession session,string id) => session.Window.RootView.FindViewById(id) ?? throw new InvalidOperationException("Missing view: " + id);
    public static double TargetX(BrowserSession session,string id) { var view=Find(session,id); return (double)view.LocalToGlobal(view.Frame.Center).X; }
    public static double TargetY(BrowserSession session,string id) { var view=Find(session,id); return (double)view.LocalToGlobal(view.Frame.Center).Y; }
    public static int HasView(BrowserSession session,string id) => session.Window.RootView.FindViewById(id) is null ? 0 : 1;
}
