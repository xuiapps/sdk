using Xui.Middleware.DevTools.Client;

namespace Xui.Tests.Memory;

/// <summary>Never updates approved snapshots. Failures leave an IDE-readable .received.txt beside the baseline.</summary>
public static class AllocationSnapshot
{
    public static async Task AssertMatchesAsync(StdioApp app, string verifiedPath)
    {
        if (!verifiedPath.EndsWith(".verified.txt", StringComparison.Ordinal))
            throw new ArgumentException("Expected a .verified.txt snapshot path.", nameof(verifiedPath));
        var receivedPath = verifiedPath[..^".verified.txt".Length] + ".received.txt";
        var actual = (await app.ReadAllocationLogAsync()).Replace("\r\n", "\n");
        var expected = File.Exists(verifiedPath) ? (await File.ReadAllTextAsync(verifiedPath)).Replace("\r\n", "\n") : null;
        if (expected == actual)
        {
            if (File.Exists(receivedPath)) File.Delete(receivedPath);
            return;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(receivedPath))!);
        await File.WriteAllTextAsync(receivedPath, actual);
        Assert.Fail($"Allocation snapshot differs: {receivedPath}\nExpected: {verifiedPath}\n{actual}");
    }
}
