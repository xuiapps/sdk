using System.Collections.Concurrent;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Middleware.DevTools;
using Xui.Runtime.Software.Actual;

var runtime = new HostRuntime();
var services = new Services(new DevToolsPlatform(runtime));
return new ProbeApplication(services).Run();

sealed class Services(IRuntime runtime) : IServiceProvider
{
    public object? GetService(Type type) => type == typeof(IRuntime) ? runtime : null;
}

sealed class ProbeApplication(IServiceProvider services) : Application(services)
{
    public override void Start()
    {
        Console.WriteLine("Application logging goes to stderr.");
        Console.Error.WriteLine("PROFILE_ENV=" + Environment.GetEnvironmentVariable("XUI_PROBE_ENV"));
        new ProbeWindow(Context).Show();
    }
}

sealed class ProbeWindow(IServiceProvider services) : Window(services)
{
    private bool allocate;
    private bool failNextFrame;
    private bool failRender;
    private bool cancelSeen;
    public override void OnMouseDown(ref MouseDownEventRef e) { allocate = e.Position.X > 0; failNextFrame = e.Position.X < 0; }
    public override void OnMouseUp(ref MouseUpEventRef e) { }
    public override void OnTouch(ref TouchEventRef e)
    {
        // A cancel must reach the app as Cancel, never as End.
        foreach (var touch in e.Touches) if (touch.Phase == TouchPhase.Cancel) cancelSeen = true;
    }
    public override void OnAnimationFrame(ref FrameEventRef e)
    {
        failRender = failNextFrame;
        failNextFrame = false;
        if (allocate) GC.KeepAlive(new byte[256]);
    }
    public override void Render(ref RenderEventRef e)
    {
        if (RootView.Frame.Width == 0) RootView.Arrange(e.Rect, e.Context);
        if (failRender) throw new InvalidOperationException("probe-render-failure");
        if (allocate) GC.KeepAlive(new byte[2048]);
        if (cancelSeen) GC.KeepAlive(new byte[512]);
    }
}

sealed class HostRuntime : IRuntime, IRunLoop, IDispatcher
{
    private readonly BlockingCollection<Action> queue = new();
    private Application? app;
    private bool quit;
    public IDispatcher MainDispatcher => this;
    public IRunLoop CreateRunloop(Application applicationAbstract) { app = applicationAbstract; return this; }
    public void Post(Action action) => queue.Add(action);
    public int Run()
    {
        app!.Start();
        while (!quit) queue.Take()();
        app.OnExit();
        queue.Dispose();
        return 0;
    }
    public void Quit() => quit = true;
    public Xui.Core.Actual.IWindow CreateWindow(Xui.Core.Abstract.IWindow window, IServiceProvider services) => new HostWindow(this, window, services);

    private sealed class HostWindow(HostRuntime runtime, Xui.Core.Abstract.IWindow window, IServiceProvider services) : Xui.Core.Actual.IWindow
    {
        private bool invalidated;
        public Xui.Core.Abstract.IWindow Abstract => window;
        public IServiceProvider NextServiceProvider => services;
        public string Title { get; set; } = "Probe";
        public bool RequireKeyboard { get; set; }
        public object? GetService(Type type) => services.GetService(type);
        public void Show() => Invalidate();
        public void Invalidate()
        {
            if (invalidated) return;
            invalidated = true;
            runtime.Post(() =>
            {
                invalidated = false;
                using var context = new SvgDrawingContext((320, 240), Stream.Null);
                window.DisplayArea = (0, 0, 320, 240);
                var render = new RenderEventRef(window.DisplayArea, new(TimeSpan.Zero, TimeSpan.Zero), context);
                window.Render(ref render);
            });
        }
    }
}
