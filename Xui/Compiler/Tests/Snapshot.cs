using System.Runtime.CompilerServices;
using Xunit;

namespace Xui.Compiler.Tests;

internal static class Snapshot
{
    public static void AssertMatches(string name, string content, [CallerFilePath] string sourceFile = "")
    {
        content = content.ReplaceLineEndings("\n");
        string directory = Path.Combine(Path.GetDirectoryName(sourceFile)!, "Snapshots");
        string expectedPath = Path.Combine(directory, name + ".md");
        string receivedPath = Path.Combine(directory, name + ".received.md");
        Directory.CreateDirectory(directory);
        if (Environment.GetEnvironmentVariable("XUI_UPDATE_SNAPSHOTS") == "1")
            File.WriteAllText(expectedPath, content);
        string? expected = File.Exists(expectedPath) ? File.ReadAllText(expectedPath).ReplaceLineEndings("\n") : null;
        if (expected != content)
        {
            File.WriteAllText(receivedPath, content);
            Assert.Fail($"Snapshot differs: {expectedPath}\nActual dump: {receivedPath}\nReview the diff before updating the snapshot.");
        }
        if (File.Exists(receivedPath)) File.Delete(receivedPath);
    }
}
