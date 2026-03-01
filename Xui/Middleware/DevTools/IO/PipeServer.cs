using System.IO.Pipes;
using System.Text.Json;

namespace Xui.Middleware.DevTools.IO;

/// <summary>
/// Accepts named-pipe connections and dispatches JSON-RPC commands to <see cref="IDevToolsHandler"/>.
/// Runs a background thread; accepts one client connection at a time.
/// </summary>
public sealed class PipeServer
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _pipeName;
    private readonly IDevToolsHandler _handler;
    private CancellationTokenSource? _cts;

    public PipeServer(string pipeName, IDevToolsHandler handler)
    {
        _pipeName = pipeName;
        _handler = handler;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        Thread t = new(() => AcceptLoop(ct)) { IsBackground = true, Name = "DevTools.PipeServer" };
        t.Start();
    }

    public void Stop() => _cts?.Cancel();

    private void AcceptLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut,
                    maxNumberOfServerInstances: 1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                pipe.WaitForConnection();  // blocks until a client connects

                using var conn = new JsonRpcConnection(pipe);
                ServeClient(conn, ct).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) { break; }
            catch { /* client disconnected; loop to accept next */ }
        }
    }

    private async Task ServeClient(JsonRpcConnection conn, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var req = await conn.ReadAsync(ct).ConfigureAwait(false);
            if (req is null) break;

            RpcResponse response;
            try
            {
                object? result = req.Method switch
                {
                    Methods.UiInspect     => await _handler.HandleInspect().ConfigureAwait(false),
                    Methods.UiScreenshot  => await _handler.HandleScreenshot().ConfigureAwait(false),
                    Methods.InputTap      => await HandleTap(req, _handler).ConfigureAwait(false),
                    Methods.InputPointer  => await HandlePointer(req, _handler).ConfigureAwait(false),
                    Methods.InputClick    => await HandleClick(req, _handler).ConfigureAwait(false),
                    Methods.AppInvalidate => await HandleInvalidate(_handler).ConfigureAwait(false),
                    Methods.AppIdentify   => await HandleIdentify(req, _handler).ConfigureAwait(false),
                    _                     => null,
                };
                response = new RpcResponse(req.Id, result);
            }
            catch (Exception ex)
            {
                response = new RpcResponse(req.Id, Error: new RpcError(-32000, ex.Message));
            }

            await conn.SendResponseAsync(response, ct).ConfigureAwait(false);
        }
    }

    private static async Task<object?> HandleTap(RpcRequest req, IDevToolsHandler handler)
    {
        var p = req.Params?.Deserialize<TapParams>(_opts) ?? new TapParams(0, 0);
        await handler.HandleTap(p).ConfigureAwait(false);
        return null;
    }

    private static async Task<object?> HandlePointer(RpcRequest req, IDevToolsHandler handler)
    {
        var p = req.Params?.Deserialize<PointerParams>(_opts) ?? new PointerParams("start", 0, 0);
        await handler.HandlePointer(p).ConfigureAwait(false);
        return null;
    }

    private static async Task<object?> HandleClick(RpcRequest req, IDevToolsHandler handler)
    {
        var p = req.Params?.Deserialize<ClickParams>(_opts) ?? new ClickParams(0, 0);
        await handler.HandleClick(p).ConfigureAwait(false);
        return null;
    }

    private static async Task<object?> HandleInvalidate(IDevToolsHandler handler)
    {
        await handler.HandleInvalidate().ConfigureAwait(false);
        return null;
    }

    private static async Task<object?> HandleIdentify(RpcRequest req, IDevToolsHandler handler)
    {
        var p = req.Params?.Deserialize<IdentifyParams>(_opts) ?? new IdentifyParams("");
        await handler.HandleIdentify(p).ConfigureAwait(false);
        return null;
    }
}

/// <summary>Implemented by <see cref="Xui.Middleware.DevTools.Actual.DevToolsWindow"/>.</summary>
public interface IDevToolsHandler
{
    Task<InspectResult>    HandleInspect();
    Task<ScreenshotResult> HandleScreenshot();
    Task                   HandleTap(TapParams p);
    Task                   HandlePointer(PointerParams p);
    Task                   HandleClick(ClickParams p);
    Task                   HandleInvalidate();
    Task                   HandleIdentify(IdentifyParams p);
}
