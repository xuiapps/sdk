using System.Runtime.CompilerServices;
using System.Text.Json;
using Xui.Middleware.DevTools.Client;

namespace Xui.Tests.Memory;

public class StdioProcessTest
{
    [DebugProcessFact]
    public async Task ChildProcessSeparatesSetupAndProtocolAllocationsFromMeasuredFrames()
    {
        await using var app = await StdioApp.StartAsync(HostPath(), new Dictionary<string, string?> { ["XUI_PROBE_ENV"] = "attached" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => app.ReadAllocationLogAsync());
        for (var i = 0; i < 8; i++) await app.FrameAsync();
        await app.ResetProfileAsync();
        var empty = (await app.FrameAsync())!.Value;
        Assert.Equal(0, empty.GetProperty("renderBytes").GetInt64());
        Assert.Equal(0, empty.GetProperty("animationBytes").GetInt64());
        // Input changes application state; the acknowledgement must be ordered before the frame.
        await app.SendAsync("input.click", new { x = 1, y = 1 });
        for (var i = 0; i < 8; i++) await app.FrameAsync();
        await app.ResetProfileAsync();
        var allocated = (await app.FrameAsync())!.Value;
        Assert.InRange(allocated.GetProperty("renderBytes").GetInt64(), 2048, 2112);
        Assert.InRange(allocated.GetProperty("animationBytes").GetInt64(), 256, 320);
        var profile = (await app.ReadProfileAsync())!.Value;
        Assert.Equal(1, profile.GetProperty("frames").GetInt64());
        Assert.Equal(allocated.GetProperty("renderBytes").GetInt64(), profile.GetProperty("renderBytes").GetInt64());
        await app.SendAsync("ui.screenshot");
        await app.InspectAsync();
        Assert.Equal(profile.GetRawText(), (await app.ReadProfileAsync())!.Value.GetRawText());
        await app.ResetProfileAsync();
        Assert.Equal(0, (await app.ReadProfileAsync())!.Value.GetProperty("frames").GetInt64());
        Assert.Contains("Application logging", app.GetLog());
        Assert.Contains("PROFILE_ENV=attached", app.GetLog());
        await app.SendAsync("app.quit");
    }

    [DebugProcessFact]
    public async Task PointerCancelAndInvalidParametersAreObservableAcrossStdio()
    {
        await using var app = await StdioApp.StartAsync(HostPath());
        await app.SendAsync("input.pointer", new { phase = "cancel", x = 1, y = 1 });
        var frame = (await app.FrameAsync())!.Value;
        Assert.InRange(frame.GetProperty("renderBytes").GetInt64(), 512, 576);
        await Assert.ThrowsAsync<InvalidOperationException>(() => app.SendAsync("input.pointer", new { phase = "oops", x = 1, y = 1 }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => app.FrameAsync(-1));
        Assert.NotNull(await app.FrameAsync());
        // Disposal closes stdin and waits for the child to exit.
    }

    [DebugProcessFact]
    public async Task FailedRenderReturnsAnErrorAndDoesNotLeaveAPendingFrame()
    {
        await using var app = await StdioApp.StartAsync(HostPath());
        await app.SendAsync("input.click", new { x = -1, y = 1 });
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => app.FrameAsync());
        Assert.Contains("probe-render-failure", error.Message);
        Assert.NotNull(await app.FrameAsync());
        Assert.Equal(1, (await app.ReadProfileAsync())!.Value.GetProperty("frames").GetInt64());
    }

    internal static string HostPath([CallerFilePath] string file = "") =>
        Path.Combine(Path.GetDirectoryName(file)!, "Host", "bin", "Debug", "net10.0", "Xui.Tests.Memory.Host.dll");
}

public sealed class DebugProcessFactAttribute : FactAttribute
{
    public DebugProcessFactAttribute()
    {
#if !DEBUG
        Skip = "Stdio control is available only in Debug builds.";
#endif
    }
}
