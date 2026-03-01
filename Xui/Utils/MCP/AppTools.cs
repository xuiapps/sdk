using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xui.MCP;

/// <summary>
/// MCP tools for launching, connecting to, and interacting with a running Xui app via DevTools.
/// </summary>
[McpServerToolType]
public static class AppTools
{
    private static Process? _process;
    private static DevToolsClient? _client;

    [McpServerTool]
    [Description("Start a Xui app in Debug mode with DevTools enabled. projectPath is relative to the solution root (e.g. Xui/Apps/BlankApp/BlankApp.Desktop.csproj).")]
    public static async Task<string> StartApp(string projectPath)
    {
        if (_process != null && !_process.HasExited)
            return "App already running. Call StopApp first.";

        _process?.Dispose();
        _process = null;

        var fullPath = Path.IsPathRooted(projectPath)
            ? projectPath
            : Path.Combine(Directory.GetCurrentDirectory(), projectPath);

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{fullPath}\" -c Debug",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            CreateNoWindow = true,
        };

        _process = new Process { StartInfo = psi };
        _process.Start();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        string? pipeName = null;
        try
        {
            while (true)
            {
                var line = await _process.StandardOutput.ReadLineAsync(cts.Token);
                if (line == null) break;
                if (line.StartsWith("DEVTOOLS_READY:"))
                {
                    pipeName = line["DEVTOOLS_READY:".Length..];
                    break;
                }
            }
        }
        catch (OperationCanceledException) { }

        if (pipeName == null)
        {
            try { _process.Kill(entireProcessTree: true); } catch { }
            _process.Dispose();
            _process = null;
            return "Timed out waiting for DEVTOOLS_READY. Ensure the project is built with Debug configuration and DevTools support is enabled.";
        }

        try
        {
            _client = new DevToolsClient(pipeName);
            await _client.ConnectAsync();
        }
        catch (Exception ex)
        {
            try { _process.Kill(entireProcessTree: true); } catch { }
            _process.Dispose();
            _process = null;
            _client = null;
            return $"App started but could not connect to DevTools pipe '{pipeName}': {ex.Message}";
        }

        return $"Connected to {pipeName}";
    }

    [McpServerTool]
    [Description("Connect to an already-running Xui app by its DevTools pipe name (e.g. xui-devtools-12345 from the DEVTOOLS_READY line). Use this when the app was launched externally, e.g. under a debugger.")]
    public static async Task<string> ConnectApp(string pipeName)
    {
        _client?.Dispose();
        _client = null;

        try
        {
            _client = new DevToolsClient(pipeName);
            await _client.ConnectAsync();
        }
        catch (Exception ex)
        {
            _client = null;
            return $"Could not connect to '{pipeName}': {ex.Message}";
        }

        return $"Connected to {pipeName}";
    }

    [McpServerTool]
    [Description("Stop the running Xui app.")]
    public static Task<string> StopApp()
    {
        _client?.Dispose();
        _client = null;

        if (_process != null)
        {
            try { _process.Kill(entireProcessTree: true); } catch { }
            _process.Dispose();
            _process = null;
            return Task.FromResult("App stopped.");
        }
        return Task.FromResult("No app is running.");
    }

    [McpServerTool]
    [Description("Inspect the visual UI tree of the running Xui app. Returns a JSON view tree.")]
    public static async Task<string> InspectUi()
    {
        if (_client == null) return "No app connected. Call StartApp first.";
        var result = await _client.SendAsync("ui.inspect");
        return result?.ToString() ?? "null";
    }

    [McpServerTool]
    [Description("Take an SVG screenshot of the running Xui app. Returns the SVG markup.")]
    public static async Task<string> Screenshot()
    {
        if (_client == null) return "No app connected. Call StartApp first.";
        var result = await _client.SendAsync("ui.screenshot");
        if (result is JsonElement el && el.TryGetProperty("svg", out var svg))
            return svg.GetString() ?? "<empty>";
        return result?.ToString() ?? "null";
    }

    [McpServerTool]
    [Description("Click (mouse down + up) at coordinates (x, y) in the running Xui app.")]
    public static async Task<string> Click(float x, float y)
    {
        if (_client == null) return "No app connected. Call StartApp first.";
        await _client.SendAsync("input.click", new { x, y });
        return $"Clicked ({x}, {y})";
    }

    [McpServerTool]
    [Description("Tap at coordinates (x, y) in the running Xui app.")]
    public static async Task<string> Tap(float x, float y)
    {
        if (_client == null) return "No app connected. Call StartApp first.";
        await _client.SendAsync("input.tap", new { x, y });
        return $"Tapped ({x}, {y})";
    }

    [McpServerTool]
    [Description("Send a synthetic pointer event. phase: start | move | end | cancel. index: touch index for multi-touch.")]
    public static async Task<string> Pointer(string phase, float x, float y, int index = 0)
    {
        if (_client == null) return "No app connected. Call StartApp first.";
        await _client.SendAsync("input.pointer", new { phase, x, y, index });
        return $"Pointer {phase} ({x}, {y}) index={index}";
    }

    [McpServerTool]
    [Description("Set the AI identity label shown next to the pointer overlay (e.g. \"Claude, VSCode\"). Pass an empty string to clear.")]
    public static async Task<string> Identify(string identity)
    {
        if (_client == null) return "No app connected. Call StartApp first.";
        await _client.SendAsync("app.identify", new { label = identity });
        return string.IsNullOrWhiteSpace(identity) ? "Identity cleared." : $"Identity set to \"{identity}\".";
    }

    [McpServerTool]
    [Description("Force a redraw of the running Xui app.")]
    public static async Task<string> Invalidate()
    {
        if (_client == null) return "No app connected. Call StartApp first.";
        await _client.SendAsync("app.invalidate");
        return "Invalidated.";
    }
}

/// <summary>Named-pipe JSON-RPC client for the Xui DevTools protocol.</summary>
internal sealed class DevToolsClient : IDisposable
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly NamedPipeClientStream _pipe;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private int _nextId = 1;

    public DevToolsClient(string pipeName)
        => _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

    public async Task ConnectAsync()
    {
        await _pipe.ConnectAsync(10_000);
        _writer = new StreamWriter(_pipe, new UTF8Encoding(false), bufferSize: 1024, leaveOpen: true) { AutoFlush = true };
        _reader = new StreamReader(_pipe, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
    }

    /// <summary>Sends a JSON-RPC request and returns the result element (may be null).</summary>
    public async Task<object?> SendAsync(string method, object? @params = null)
    {
        if (_writer == null || _reader == null)
            throw new InvalidOperationException("Not connected.");

        var id = _nextId++;
        var json = JsonSerializer.Serialize(new { method, id, @params }, _opts);
        await _writer.WriteLineAsync(json);

        var line = await _reader.ReadLineAsync();
        if (line == null) throw new IOException("DevTools connection closed.");

        using var doc = JsonDocument.Parse(line);
        var root = doc.RootElement;

        if (root.TryGetProperty("error", out var err))
            throw new Exception($"RPC error {err.GetProperty("code").GetInt32()}: {err.GetProperty("message").GetString()}");

        if (root.TryGetProperty("result", out var result) && result.ValueKind != JsonValueKind.Null)
            return result.Clone();

        return null;
    }

    public void Dispose() => _pipe.Dispose();
}
