using System.IO.Pipes;

namespace Xui.Middleware.DevTools.IO;

/// <summary>Serves the same protocol as stdio, for existing DevTools/MCP clients.</summary>
public sealed class PipeServer(string pipeName, IDevToolsHandler handler) : IDisposable
{
    private readonly CancellationTokenSource cancellation = new();
    private Task? serverTask;

    public void Start() => serverTask = Task.Run(AcceptAsync);
    public void Stop() => cancellation.Cancel();

    private async Task AcceptAsync()
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await pipe.WaitForConnectionAsync(cancellation.Token).ConfigureAwait(false);
                using var connection = new JsonRpcConnection(pipe, pipe, leaveOpen: true);
                await new RpcServer(handler).ServeAsync(connection, cancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { break; }
            catch (IOException) { /* A client disconnected; accept the next client. */ }
        }
    }

    public void Dispose()
    {
        Stop();
        // No UI-thread wait: a command may itself be awaiting the dispatcher.
        if (serverTask is { } task)
            _ = task.ContinueWith(_ => cancellation.Dispose(), TaskScheduler.Default);
        else cancellation.Dispose();
    }
}

/// <summary>Implemented by <see cref="Xui.Middleware.DevTools.Actual.DevToolsWindow"/>.</summary>
public interface IDevToolsHandler
{
    Task<InspectResult> HandleInspect();
    Task<ScreenshotResult> HandleScreenshot();
    Task HandleTap(TapParams p);
    Task HandlePointer(PointerParams p);
    Task HandleClick(ClickParams p);
    Task HandleMouseMove(MouseMoveParams p);
    Task HandleInvalidate();
    Task HandleIdentify(IdentifyParams p);
    Task<FrameResult> HandleFrame(FrameParams p);
    Task<ProfileResult> HandleProfileReset();
    Task<ProfileResult> HandleProfileRead();
}
