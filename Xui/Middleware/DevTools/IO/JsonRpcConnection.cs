using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xui.Middleware.DevTools.IO;

/// <summary>
/// Wraps a bidirectional stream with line-delimited JSON-RPC read/write.
/// Each message is a single JSON object terminated by a newline character.
/// </summary>
public sealed class JsonRpcConnection : IDisposable
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly Stream _stream;
    private readonly StreamReader _reader;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public JsonRpcConnection(Stream stream)
    {
        _stream = stream;
        _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
    }

    /// <summary>Reads the next JSON-RPC request line. Returns null on EOF or parse error.</summary>
    public async Task<RpcRequest?> ReadAsync(CancellationToken ct = default)
    {
        var line = await _reader.ReadLineAsync(ct).ConfigureAwait(false);
        if (line is null) return null;
        try { return JsonSerializer.Deserialize<RpcRequest>(line, _opts); }
        catch { return null; }
    }

    public async Task SendResponseAsync(RpcResponse response, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(response, _opts);
        await WriteLineAsync(json, ct).ConfigureAwait(false);
    }

    public async Task SendNotificationAsync(RpcNotification notification, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(notification, _opts);
        await WriteLineAsync(json, ct).ConfigureAwait(false);
    }

    private async Task WriteLineAsync(string json, CancellationToken ct)
    {
        await _writeLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var bytes = Encoding.UTF8.GetBytes(json + "\n");
            await _stream.WriteAsync(bytes, ct).ConfigureAwait(false);
            await _stream.FlushAsync(ct).ConfigureAwait(false);
        }
        finally { _writeLock.Release(); }
    }

    public void Dispose()
    {
        _reader.Dispose();
        _stream.Dispose();
    }
}
