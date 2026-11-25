using System.Diagnostics;
using System.Text.Json;

namespace Xui.Middleware.DevTools.Client;

/// <summary>Owns a directly launched app and its stdio connection, including failed startup cleanup.</summary>
public sealed class StdioApp : IAsyncDisposable
{
    private readonly Process process;
    private readonly DevToolsClient client;
    private readonly Queue<string> log = new();
    private readonly Task stderrDrain;
    private bool disposed;

    private StdioApp(Process process)
    {
        this.process = process;
        client = new(process.StandardOutput, process.StandardInput);
        stderrDrain = DrainAsync();
    }

    /// <summary>
    /// Launch a built executable (or DLL via dotnet). Environment overrides are applied only
    /// to this child, so a CLR profiler can be loaded before any application code executes.
    /// Build separately: build output must never share the JSON-RPC stdout stream.
    /// </summary>
    public static async Task<StdioApp> StartAsync(string path,
        IReadOnlyDictionary<string, string?>? environment = null,
        TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        path = Path.GetFullPath(path);
        var isDll = path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        var start = new ProcessStartInfo(isDll ? "dotnet" : path)
        {
            WorkingDirectory = Path.GetDirectoryName(path)!,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        if (isDll) start.ArgumentList.Add(path);
        start.ArgumentList.Add("--enable-stdio-jsonrpc");
        if (environment is not null)
            foreach (var (key, value) in environment) start.Environment[key] = value;
        var process = Process.Start(start) ?? throw new IOException("Could not start app.");
        var app = new StdioApp(process);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(timeout ?? TimeSpan.FromSeconds(30));
        try
        {
            while (true)
            {
                var root = await app.InspectAsync(deadline.Token);
                if (root is { Type: not "no-window", W: > 0, H: > 0 }) return app;
                await Task.Delay(25, deadline.Token);
            }
        }
        catch (Exception ex)
        {
            await app.DisposeAsync();
            throw new InvalidOperationException($"Stdio app startup failed: {ex.Message}\n{app.GetLog()}", ex);
        }
    }

    /// <summary>Every request has a deadline, including a native render that never arrives.</summary>
    public async Task<JsonElement?> SendAsync(string method, object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(30));
        return (JsonElement?)await client.SendAsync(method, parameters, deadline.Token);
    }

    public async Task<ViewNode?> InspectAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync("ui.inspect", cancellationToken: cancellationToken);
        return result?.Deserialize<InspectResult>(DevToolsClient.JsonOptions)?.Root;
    }

    public Task<JsonElement?> FrameAsync(int elapsedMilliseconds = 16, CancellationToken cancellationToken = default) =>
        SendAsync("app.frame", new { elapsedMilliseconds }, cancellationToken);

    public Task<JsonElement?> ResetProfileAsync(CancellationToken cancellationToken = default) =>
        SendAsync("profile.reset", cancellationToken: cancellationToken);

    public Task<JsonElement?> ReadProfileAsync(CancellationToken cancellationToken = default) =>
        SendAsync("profile.read", cancellationToken: cancellationToken);

    /// <summary>Formats exact allocation stacks with portable-PDB file, line and column locations.</summary>
    public async Task<string> ReadAllocationLogAsync(CancellationToken cancellationToken = default)
    {
        var profile = await ReadProfileAsync(cancellationToken);
        return AllocationStacks.Format(profile ?? throw new IOException("Missing allocation profile."));
    }

    public string GetLog() { lock (log) return string.Join(Environment.NewLine, log); }

    private async Task DrainAsync()
    {
        while (await process.StandardError.ReadLineAsync().ConfigureAwait(false) is { } line)
            lock (log)
            {
                log.Enqueue(line);
                if (log.Count > 200) log.Dequeue();
            }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        try
        {
            if (!process.HasExited)
            {
                // EOF requests graceful shutdown, even after a timed-out command.
                process.StandardInput.Close();
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                try { await process.WaitForExitAsync(deadline.Token).ConfigureAwait(false); }
                catch (OperationCanceledException)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync().ConfigureAwait(false);
                }
            }
            await stderrDrain.ConfigureAwait(false);
        }
        finally { client.Dispose(); process.Dispose(); }
    }
}
