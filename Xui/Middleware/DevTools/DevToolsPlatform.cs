using Xui.Core.Abstract;
using Xui.Core.Actual;
using Xui.Middleware.DevTools.Actual;
using Xui.Middleware.DevTools.IO;

namespace Xui.Middleware.DevTools;

/// <summary>
/// A runtime middleware layer that wraps any <see cref="IRuntime"/> to inject DevTools support.
///
/// Debug apps can select stdio JSON-RPC with --enable-stdio-jsonrpc; otherwise a named-pipe server starts.
/// External tools (e.g. the Xui MCP server) can connect to inspect and interact with the running app.
/// The pipe name is deterministic: <c>xui-devtools-{processId}</c>.
/// When the pipe server is ready, the app prints <c>DEVTOOLS_READY:{pipeName}</c> to stdout.
/// </summary>
public class DevToolsPlatform : IRuntime
{
    internal readonly IRuntime Base;
    internal volatile DevToolsWindow? Window;

    /// <summary>True only in Debug when the explicit command-line switch is present.</summary>
    public bool StdioEnabled { get; }

    public DevToolsPlatform(IRuntime @base)
    {
        Base = @base;
#if DEBUG
        StdioEnabled = Environment.GetCommandLineArgs().Contains("--enable-stdio-jsonrpc", StringComparer.Ordinal);
#endif
    }

    /// <inheritdoc/>
    public IDispatcher MainDispatcher => Base.MainDispatcher;

    /// <summary>
    /// Intercepts window creation to insert a <see cref="DevToolsWindow"/> between
    /// the abstract application window and the real platform window.
    /// </summary>
    public Xui.Core.Actual.IWindow CreateWindow(Xui.Core.Abstract.IWindow windowAbstract, IServiceProvider applicationServices)
    {
        var dw = new DevToolsWindow(this, windowAbstract, applicationServices);
        var pw = Base.CreateWindow(dw, applicationServices);
        dw.AttachActual(pw);
        Window = dw;
        return dw;
    }

    /// <summary>
    /// Starts the DevTools pipe server, then delegates run loop creation to the base platform.
    /// Prints <c>DEVTOOLS_READY:{pipeName}</c> to stdout so external tools can connect.
    /// </summary>
    public IRunLoop CreateRunloop(Application applicationAbstract)
    {
        var runloop = Base.CreateRunloop(applicationAbstract);
        var handler = new DevToolsHandler(this);
        if (StdioEnabled)
        {
            // Capture the protocol stream before moving all application logging to stderr.
            var output = Console.OpenStandardOutput();
            Console.SetOut(Console.Error);
            return new ControlledRunLoop(runloop, this, handler, output);
        }
        var pipeName = $"xui-devtools-{Environment.ProcessId}";
        var server = new PipeServer(pipeName, handler);
        server.Start();
        Console.WriteLine($"DEVTOOLS_READY:{pipeName}");
        return new PipeRunLoop(runloop, server);
    }
    private sealed class PipeRunLoop(IRunLoop inner, PipeServer server) : IRunLoop
    {
        public int Run() { try { return inner.Run(); } finally { server.Dispose(); } }
        public void Quit() => inner.Quit();
    }

    private sealed class ControlledRunLoop(IRunLoop inner, DevToolsPlatform platform,
        IDevToolsHandler handler, Stream output) : IRunLoop
    {
        public int Run()
        {
            using var cancellation = new CancellationTokenSource();
            var token = cancellation.Token;
            _ = Task.Run(async () =>
            {
                var quitRequested = false;
                try
                {
                    using var connection = new JsonRpcConnection(Console.OpenStandardInput(), output, leaveOpen: true);
                    await new RpcServer(handler, () => { quitRequested = true; Quit(); }).ServeAsync(connection, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { }
                catch (Exception ex) { Console.Error.WriteLine($"Stdio control: {ex.Message}"); }
                finally { if (!quitRequested && !token.IsCancellationRequested) Quit(); }
            });
            try { return inner.Run(); }
            finally { cancellation.Cancel(); }
        }
        public void Quit() => platform.MainDispatcher.Post(inner.Quit);
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

    public Task<IO.InspectResult> HandleInspect() => Window?.HandleInspect() ?? Task.FromResult(new IO.InspectResult(new IO.ViewNode("no-window", 0, 0, 0, 0, 0, 0, false, null, null, [])));
    public Task<IO.ScreenshotResult> HandleScreenshot() => Window?.HandleScreenshot() ?? Task.FromResult(new IO.ScreenshotResult("<svg/>"));
    public Task HandleTap(IO.TapParams p) => RequireWindow().HandleTap(p);
    public Task HandlePointer(IO.PointerParams p) => RequireWindow().HandlePointer(p);
    public Task HandleClick(IO.ClickParams p) => RequireWindow().HandleClick(p);
    public Task HandleMouseMove(IO.MouseMoveParams p) => RequireWindow().HandleMouseMove(p);
    public Task HandleInvalidate() => Window?.HandleInvalidate() ?? Task.CompletedTask;
    private DevToolsWindow RequireWindow() => Window ?? throw new InvalidOperationException("Window is not ready.");
    public Task<FrameResult> HandleFrame(FrameParams p) => RequireWindow().HandleFrame(p);
    public Task<ProfileResult> HandleProfileReset() => RequireWindow().HandleProfileReset();
    public Task<ProfileResult> HandleProfileRead() => RequireWindow().HandleProfileRead();
    public Task HandleIdentify(IO.IdentifyParams p) => Window?.HandleIdentify(p) ?? Task.CompletedTask;
}
