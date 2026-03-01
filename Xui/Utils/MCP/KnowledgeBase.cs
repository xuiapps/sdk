using System.IO.Compression;
using System.Reflection;

namespace Xui.MCP;

/// <summary>
/// Lazy-loaded in-memory zip archive embedded as "xui-knowledge.zip".
/// Entry paths are normalized to forward slashes (e.g. "docs/canvas.md", "api/Xui.Core/Math2D/Vector.cs").
/// </summary>
internal static class KnowledgeBase
{
    private static readonly Lazy<(ZipArchive Zip, Dictionary<string, ZipArchiveEntry> Index)> _lazy = new(() =>
    {
        var stream = typeof(KnowledgeBase).Assembly
            .GetManifestResourceStream("xui-knowledge.zip")
            ?? throw new InvalidOperationException("xui-knowledge.zip not embedded.");
        var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        var index = zip.Entries.ToDictionary(
            e => e.FullName.Replace('\\', '/'),
            e => e);
        return (zip, index);
    });

    public static IEnumerable<string> Paths => _lazy.Value.Index.Keys;

    public static string Read(string path)
    {
        if (!_lazy.Value.Index.TryGetValue(path, out var entry))
            throw new KeyNotFoundException($"Not found in knowledge base: {path}");
        using var reader = new StreamReader(entry.Open());
        return reader.ReadToEnd();
    }

    public static bool TryRead(string path, out string content)
    {
        if (_lazy.Value.Index.TryGetValue(path, out var entry))
        {
            using var reader = new StreamReader(entry.Open());
            content = reader.ReadToEnd();
            return true;
        }
        content = "";
        return false;
    }
}
