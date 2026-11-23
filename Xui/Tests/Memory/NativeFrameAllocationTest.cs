using System.Text.Json;
using Xui.Middleware.DevTools.Client;
using Xunit.Abstractions;

namespace Xui.Tests.Memory;

/// <summary>Opt-in because native desktop/emulator windows need an interactive display session.</summary>
public class NativeFrameAllocationTest(ITestOutputHelper output)
{
    [NativeMemoryFact]
    [Trait("Category", "NativeMemory")]
    public async Task WarmFramesAreMeasuredInTheLaunchedApplication()
    {
        var path = Environment.GetEnvironmentVariable("XUI_MEMORY_APP")!;
        var collector = Environment.GetEnvironmentVariable("XUI_TEST_ALLOCATION_PROFILER");
        var snapshot = Environment.GetEnvironmentVariable("XUI_MEMORY_SNAPSHOT");
        Assert.True(string.IsNullOrEmpty(snapshot) || !string.IsNullOrEmpty(collector),
            "XUI_MEMORY_SNAPSHOT requires XUI_TEST_ALLOCATION_PROFILER.");
        await using var app = await StdioApp.StartAsync(path,
            string.IsNullOrWhiteSpace(collector) ? null : AllocationStacks.EnvironmentFor(collector));
        var targetId = Environment.GetEnvironmentVariable("XUI_MEMORY_TARGET");
        if (!string.IsNullOrWhiteSpace(targetId))
        {
            var root = await app.InspectAsync();
            var target = root?.FindById(targetId);
            Assert.True(target is not null, $"Cannot find element '{targetId}' while preparing the native memory scenario.");
            await app.SendAsync("input.click", new { x = target.CenterX, y = target.CenterY });
        }
        // Finish navigation and warm native drawing caches before resetting the profiler.
        for (var i = 0; i < 60; i++) await app.FrameAsync();
        if (!string.IsNullOrWhiteSpace(targetId))
        {
            var expectedId = Environment.GetEnvironmentVariable("XUI_MEMORY_EXPECT_ID");
            Assert.False(string.IsNullOrWhiteSpace(expectedId), "Set XUI_MEMORY_EXPECT_ID to verify the navigation destination before profiling.");
            Assert.True((await app.InspectAsync())?.FindById(expectedId!) is not null,
                $"Cannot find destination '{expectedId}' after clicking '{targetId}'.");
        }
        await app.ResetProfileAsync();
        var frames = new List<JsonElement>();
        for (var i = 0; i < 10; i++) frames.Add((await app.FrameAsync())!.Value);
        var profile = (await app.ReadProfileAsync())!.Value;
        output.WriteLine(profile.GetRawText());
        output.WriteLine(JsonSerializer.Serialize(frames));
        Assert.Equal(10, profile.GetProperty("frames").GetInt64());
        Assert.Equal(frames.Sum(f => f.GetProperty("renderBytes").GetInt64()), profile.GetProperty("renderBytes").GetInt64());
        if (long.TryParse(Environment.GetEnvironmentVariable("XUI_MEMORY_MAX_FRAME_BYTES"), out var budget))
            Assert.All(frames, frame => Assert.InRange(
                frame.GetProperty("renderBytes").GetInt64() + frame.GetProperty("animationBytes").GetInt64(), 0, budget));
        if (!string.IsNullOrEmpty(snapshot)) await AllocationSnapshot.AssertMatchesAsync(app, snapshot);
        else if (!string.IsNullOrEmpty(collector)) output.WriteLine(await app.ReadAllocationLogAsync());
        await app.SendAsync("app.quit");
    }
}

public sealed class NativeMemoryFactAttribute : FactAttribute
{
    public NativeMemoryFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("XUI_MEMORY_APP")))
            Skip = "Set XUI_MEMORY_APP to a built Debug desktop/emulator executable or DLL; requires a display session.";
    }
}
