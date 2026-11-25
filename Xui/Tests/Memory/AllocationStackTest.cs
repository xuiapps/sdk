using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xui.Middleware.DevTools.Client;
using Xunit.Sdk;

namespace Xui.Tests.Memory;

public class AllocationStackTest(Xunit.Abstractions.ITestOutputHelper output)
{
    [AllocationProfilerFact]
    public async Task IdleFrameMatchesEmptySnapshotAndAllocationFailureReportsItsSourceLocation()
    {
        var library = Environment.GetEnvironmentVariable("XUI_TEST_ALLOCATION_PROFILER")!;
        await using var app = await StdioApp.StartAsync(StdioProcessTest.HostPath(), AllocationStacks.EnvironmentFor(library));
        for (var i = 0; i < 8; i++) await app.FrameAsync();
        await app.ResetProfileAsync();
        await app.FrameAsync();
        await AllocationSnapshot.AssertMatchesAsync(app, Path.Combine(SourceDirectory(), "Snapshots", "IdleFrame.verified.txt"));

        await app.SendAsync("input.click", new { x = 1, y = 1 });
        for (var i = 0; i < 8; i++) await app.FrameAsync();
        await app.ResetProfileAsync();
        await app.FrameAsync();
        var log = await app.ReadAllocationLogAsync();
        output.WriteLine(log);
        Assert.Contains("render: System.Byte[]", log);
        Assert.Contains("2072 bytes", log);
        Assert.Contains("animation: System.Byte[]", log);
        Assert.DoesNotContain("capture error", log);
        Assert.DoesNotContain("CAPTURE INCOMPLETE", log);
        var source = Path.Combine(SourceDirectory(), "Host", "Program.cs");
        var lines = await File.ReadAllLinesAsync(source);
        var allocationLine = Array.FindIndex(lines, line => line.Contains("new byte[2048]")) + 1;
        Assert.Matches($@"Program\.cs\({allocationLine},[1-9]\d*\): ProbeWindow.Render", log);

        // Exercise the actual failing assertion without leaving a deliberately
        // failing test or a generated received file in the repository.
        var directory = Path.Combine(Path.GetTempPath(), "xui-allocation-snapshot-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            var verified = Path.Combine(directory, "MeasuredFrame.verified.txt");
            await File.WriteAllTextAsync(verified, "");
            var failure = await Assert.ThrowsAnyAsync<XunitException>(() => AllocationSnapshot.AssertMatchesAsync(app, verified));
            var received = Path.Combine(directory, "MeasuredFrame.received.txt");
            Assert.Contains(received, failure.Message);
            Assert.Equal(log, await File.ReadAllTextAsync(received));
            Assert.Equal("", await File.ReadAllTextAsync(verified));
            await app.SendAsync("input.click", new { x = 0, y = 0 });
            await app.ResetProfileAsync();
            await app.FrameAsync();
            await AllocationSnapshot.AssertMatchesAsync(app, verified);
            Assert.False(File.Exists(received));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}

public sealed class AllocationProfilerFactAttribute : FactAttribute
{
    public AllocationProfilerFactAttribute()
    {
#if DEBUG
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("XUI_TEST_ALLOCATION_PROFILER")))
            Skip = "Build the native allocation profiler and set XUI_TEST_ALLOCATION_PROFILER to its library path.";
#else
        Skip = "The allocation-stack probe uses Debug stdio control.";
#endif
    }
}
