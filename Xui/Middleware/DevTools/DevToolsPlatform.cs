using Xui.Core.Abstract;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Middleware.DevTools.Actual;
using Xui.Middleware.DevTools.IO;

namespace Xui.Middleware.DevTools;

/// <summary>
/// A runtime middleware layer that wraps any <see cref="IRuntime"/> to inject DevTools support.
///
/// In Debug builds, DevTools starts a named-pipe JSON-RPC server on application startup.
/// External tools (e.g. the Xui MCP server) can connect to inspect and interact with the running app.
/// The pipe name is deterministic: <c>xui-devtools-{processId}</c>.
/// When the pipe server is ready, the app prints <c>DEVTOOLS_READY:{pipeName}</c> to stdout.
/// </summary>
public class DevToolsPlatform : IRuntime
{
    internal readonly IRuntime Base;
    internal DevToolsWindow? Window;

    public DevToolsPlatform(IRuntime @base) => Base = @base;

    /// <inheritdoc/>
    public IContext DrawingContext => Base.DrawingContext;

    /// <inheritdoc/>
    public IDispatcher MainDispatcher => Base.MainDispatcher;

    /// <summary>
    /// Intercepts window creation to insert a <see cref="DevToolsWindow"/> between
    /// the abstract application window and the real platform window.
    /// </summary>
    public Xui.Core.Actual.IWindow CreateWindow(Xui.Core.Abstract.IWindow windowAbstract)
    {
        var dw = new DevToolsWindow(this);
        dw.Abstract = windowAbstract;
        var pw = Base.CreateWindow(dw);
        dw.Platform = pw;
        Window = dw;
        return dw;
    }

    /// <summary>
    /// Starts the DevTools pipe server, then delegates run loop creation to the base platform.
    /// Prints <c>DEVTOOLS_READY:{pipeName}</c> to stdout so external tools can connect.
    /// </summary>
    public IRunLoop CreateRunloop(Application applicationAbstract)
    {
        var pipeName = $"xui-devtools-{Environment.ProcessId}";
        var server = new PipeServer(pipeName, new DevToolsHandler(this));
        server.Start();
        Console.WriteLine($"DEVTOOLS_READY:{pipeName}");
        return Base.CreateRunloop(applicationAbstract);
    }
}

/// <summary>
/// Adapts <see cref="DevToolsWindow"/> to the <see cref="IDevToolsHandler"/> interface
/// expected by <see cref="PipeServer"/>. Lazily resolves the window so it's available
/// after the first window is created.
/// </summary>
file sealed class DevToolsHandler(DevToolsPlatform platform) : IDevToolsHandler
{
    private DevToolsWindow? Window => platform.Window;

    public Task<IO.InspectResult>    HandleInspect()      => Window?.HandleInspect()      ?? Task.FromResult(new IO.InspectResult(new IO.ViewNode("no-window", 0, 0, 0, 0, false, [])));
    public Task<IO.ScreenshotResult> HandleScreenshot()   => Window?.HandleScreenshot()   ?? Task.FromResult(new IO.ScreenshotResult("<svg/>"));
    public Task                      HandleTap(IO.TapParams p)          => Window?.HandleTap(p)      ?? Task.CompletedTask;
    public Task                      HandlePointer(IO.PointerParams p)  => Window?.HandlePointer(p)  ?? Task.CompletedTask;
    public Task                      HandleClick(IO.ClickParams p)      => Window?.HandleClick(p)    ?? Task.CompletedTask;
    public Task                      HandleInvalidate()                  => Window?.HandleInvalidate() ?? Task.CompletedTask;
    public Task                      HandleIdentify(IO.IdentifyParams p) => Window?.HandleIdentify(p)  ?? Task.CompletedTask;
}
