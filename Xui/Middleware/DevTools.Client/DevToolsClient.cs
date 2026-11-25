using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xui.Middleware.DevTools.Client;

/// <summary>Named-pipe JSON-RPC client for the Xui DevTools protocol.</summary>
public sealed class DevToolsClient : IDisposable
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly NamedPipeClientStream? pipe;
    private readonly SemaphoreSlim commandLock = new(1, 1);
    private bool faulted;
    private StreamWriter? writer;
    private StreamReader? reader;
    private int nextId = 1;

    public DevToolsClient(string pipeName)
        => this.pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

    /// <summary>Connects to the redirected streams of an already started app process.</summary>
    public DevToolsClient(StreamReader output, StreamWriter input)
    {
        reader = output;
        writer = input;
        writer.AutoFlush = true;
    }

    /// <param name="clientName">
    /// Optional display name sent to the app immediately after connecting (e.g. "Claude" or
    /// "E2E Automation"). When provided, the app shows an overlay label on the next frame.
    /// </param>
    public async Task ConnectAsync(string? clientName = null)
    {
        if (pipe is null) throw new InvalidOperationException("Stdio is already connected.");
        await pipe.ConnectAsync(10_000);
        writer = new StreamWriter(pipe, new UTF8Encoding(false), bufferSize: 1024, leaveOpen: true) { AutoFlush = true };
        reader = new StreamReader(pipe, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);

        if (clientName != null)
            await SendAsync("app.identify", new { label = clientName });
    }

    /// <summary>Sends a JSON-RPC request and returns the result element, or null on void responses.</summary>
    public async Task<object?> SendAsync(string method, object? @params = null, CancellationToken cancellationToken = default)
    {
        await commandLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (writer == null || reader == null || faulted)
                throw new InvalidOperationException("Not connected, or an earlier request lost synchronization.");
            var id = nextId++;
            var json = JsonSerializer.Serialize(new { jsonrpc = "2.0", method, id, @params }, JsonOptions);
            string? line;
            try
            {
                await writer.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);
                line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (line is null) throw new IOException("DevTools connection closed.");
            }
            catch { faulted = true; throw; }

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            if (!root.TryGetProperty("id", out var responseId) || !responseId.TryGetInt32(out var number) || number != id)
            {
                faulted = true;
                throw new IOException("Unexpected JSON-RPC response id.");
            }
            if (root.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
                throw new InvalidOperationException($"RPC error {error.GetProperty("code").GetInt32()}: {error.GetProperty("message").GetString()}");
            return root.TryGetProperty("result", out var result) && result.ValueKind != JsonValueKind.Null
                ? result.Clone() : null;
        }
        finally { commandLock.Release(); }
    }

    public void Dispose()
    {
        faulted = true;
        pipe?.Dispose();
    }
}
