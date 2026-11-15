using Xui.Core.Abstract;
using Xui.Core.Actual;

namespace Xui.Runtime.Android.Actual;

/// <summary>Android runtime independent of Microsoft Android bindings and MAUI.</summary>
public sealed class AndroidPlatform : IRuntime
{
    private sealed class Dispatcher : IDispatcher { public void Post(Action callback) => AndroidWindow.Pending.Enqueue(callback); }
    private sealed class RunLoop(Application app) : IRunLoop { public int Run() { app.Start(); return 0; } public void Quit() => AndroidWindow.Current?.Close(); }
    public IDispatcher MainDispatcher { get; } = new Dispatcher();
    public IRunLoop CreateRunloop(Application applicationAbstract) => new RunLoop(applicationAbstract);
    public Xui.Core.Actual.IWindow CreateWindow(Xui.Core.Abstract.IWindow windowAbstract, IServiceProvider applicationServices)
    {
        var window = AndroidWindow.Current ?? throw new InvalidOperationException("Create AndroidWindow from the native Activity before starting Xui.");
        window.Attach(windowAbstract, applicationServices); return window;
    }
}
