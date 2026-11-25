using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xui.Middleware.DevTools.IO;

/// <summary>Newline-delimited JSON-RPC over a duplex stream or separate stdin/stdout streams.</summary>
public sealed class JsonRpcConnection : IDisposable
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
    private readonly StreamReader reader;
    private readonly StreamWriter writer;
    private readonly SemaphoreSlim writeLock = new(1, 1);

    public JsonRpcConnection(Stream stream) : this(stream, stream) { }

    public JsonRpcConnection(Stream input, Stream output, bool leaveOpen = false)
    {
        reader = new StreamReader(input, Encoding.UTF8, leaveOpen: leaveOpen);
        writer = new StreamWriter(output, new UTF8Encoding(false), leaveOpen: leaveOpen);
    }

    internal ValueTask<string?> ReadLineAsync(CancellationToken ct) => reader.ReadLineAsync(ct);

    /// <summary>Reads a request; malformed JSON throws instead of being mistaken for EOF.</summary>
    public async Task<RpcRequest?> ReadAsync(CancellationToken ct = default)
    {
        var line = await ReadLineAsync(ct).ConfigureAwait(false);
        return line is null ? null : JsonSerializer.Deserialize<RpcRequest>(line, Options);
    }

    public Task SendResponseAsync(RpcResponse response, CancellationToken ct = default) => SendAsync(response, ct);
    public Task SendNotificationAsync(RpcNotification notification, CancellationToken ct = default) => SendAsync(notification, ct);

    internal async Task SendAsync(object message, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(message, Options);
        await writeLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await writer.WriteLineAsync(json.AsMemory(), ct).ConfigureAwait(false);
            await writer.FlushAsync(ct).ConfigureAwait(false);
        }
        finally { writeLock.Release(); }
    }

    public void Dispose()
    {
        reader.Dispose();
        writer.Dispose();
        writeLock.Dispose();
    }
}
