using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xui.Middleware.DevTools.IO;

/// <summary>Sequential JSON-RPC dispatch shared by named pipes and stdio.</summary>
public sealed class RpcServer(IDevToolsHandler handler, Action? quit = null)
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public async Task ServeAsync(JsonRpcConnection connection, CancellationToken ct = default)
    {
        while (await connection.ReadLineAsync(ct).ConfigureAwait(false) is { } line)
        {
            JsonElement? id = null;
            bool notification = false;
            bool exit = false;
            object response;
            try
            {
                using var document = JsonDocument.Parse(line);
                var request = document.RootElement;
                if (request.ValueKind != JsonValueKind.Object)
                    throw new RpcException(-32600, "Expected a request object.");
                if (request.TryGetProperty("id", out var requestId))
                {
                    if (requestId.ValueKind is not (JsonValueKind.String or JsonValueKind.Number or JsonValueKind.Null))
                        throw new RpcException(-32600, "Invalid request id.");
                    id = requestId.Clone();
                }
                if (!request.TryGetProperty("method", out var method) || method.ValueKind != JsonValueKind.String ||
                    (request.TryGetProperty("jsonrpc", out var version) &&
                     (version.ValueKind != JsonValueKind.String || version.GetString() != "2.0")))
                    throw new RpcException(-32600, "Invalid JSON-RPC request.");
                notification = !request.TryGetProperty("id", out _);
                JsonElement? parameters = request.TryGetProperty("params", out var p) ? p : null;
                if (parameters is { ValueKind: not (JsonValueKind.Object or JsonValueKind.Array) })
                    throw new RpcException(-32602, "Expected structured params.");
                object? result = null;
                switch (method.GetString())
                {
                    case Methods.UiInspect: result = await handler.HandleInspect().WaitAsync(ct); break;
                    case Methods.UiScreenshot: result = await handler.HandleScreenshot().WaitAsync(ct); break;
                    case Methods.InputTap: await handler.HandleTap(Parse<TapParams>(parameters)).WaitAsync(ct); break;
                    case Methods.InputClick: await handler.HandleClick(Parse<ClickParams>(parameters)).WaitAsync(ct); break;
                    case Methods.InputPointer: await handler.HandlePointer(Parse<PointerParams>(parameters)).WaitAsync(ct); break;
                    case Methods.InputMouseMove: await handler.HandleMouseMove(Parse<MouseMoveParams>(parameters)).WaitAsync(ct); break;
                    case Methods.AppIdentify: await handler.HandleIdentify(Parse<IdentifyParams>(parameters)).WaitAsync(ct); break;
                    case Methods.AppInvalidate: await handler.HandleInvalidate().WaitAsync(ct); break;
                    case Methods.AppFrame:
                        result = await handler.HandleFrame(parameters is null ? new() : Parse<FrameParams>(parameters)).WaitAsync(ct);
                        break;
                    case Methods.ProfileReset: result = await handler.HandleProfileReset().WaitAsync(ct); break;
                    case Methods.ProfileRead: result = await handler.HandleProfileRead().WaitAsync(ct); break;
                    case Methods.AppQuit when quit is not null: exit = true; break;
                    default: throw new RpcException(-32601, "Method not found.");
                }
                response = new SuccessReply(id, result);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (JsonException) { response = new ErrorReply(null, new(-32700, "Parse error.")); }
            catch (RpcException ex) { response = new ErrorReply(id, new(ex.Code, ex.Message)); }
            catch (ArgumentException ex) { response = new ErrorReply(id, new(-32602, ex.Message)); }
            catch (Exception ex) { response = new ErrorReply(id, new(-32000, ex.Message)); }

            if (!notification) await connection.SendAsync(response, ct).ConfigureAwait(false);
            if (exit) { quit!(); return; }
        }
    }

    private static T Parse<T>(JsonElement? parameters)
    {
        try
        {
            if (parameters is not { ValueKind: JsonValueKind.Object })
                throw new RpcException(-32602, "Expected named parameters.");
            return parameters.Value.Deserialize<T>(Options) ?? throw new RpcException(-32602, "Missing parameters.");
        }
        catch (JsonException ex) { throw new RpcException(-32602, ex.Message); }
    }

    private sealed class RpcException(int code, string message) : Exception(message)
    {
        public int Code { get; } = code;
    }

    private sealed record SuccessReply(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] JsonElement? Id,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] object? Result)
    {
        public string Jsonrpc => "2.0";
    }

    private sealed record ErrorReply(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] JsonElement? Id, RpcError Error)
    {
        public string Jsonrpc => "2.0";
    }
}
