using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Xui.MCP;

[McpServerResourceType]
public static class DocsResources
{
    private static readonly IReadOnlyList<string> DocNames = KnowledgeBase.Paths
        .Where(n => n.StartsWith("docs/") && n.EndsWith(".md"))
        .Select(n => n["docs/".Length..^".md".Length])
        .OrderBy(n => n)
        .ToList();

    [McpServerResource(UriTemplate = "xui://docs", Name = "Xui Docs — Table of Contents", MimeType = "text/markdown")]
    [Description("Lists all available Xui documentation pages with their resource URIs.")]
    public static string GetToc() =>
        "# Xui Documentation\n\n" +
        string.Join("\n", DocNames.Select(n => $"- [{n}](xui://docs/{n})"));

    [McpServerResource(UriTemplate = "xui://docs/{name}", Name = "Xui Doc Page", MimeType = "text/markdown")]
    [Description("Returns the content of a Xui documentation page by name (e.g. getting-started, canvas, views). SVG images are inlined.")]
    public static TextResourceContents GetDoc(RequestContext<ReadResourceRequestParams> ctx, string name)
    {
        var markdown = KnowledgeBase.Read($"docs/{name}.md");
        var inlined = InlineSvgs(markdown, baseDir: Path.GetDirectoryName(name) ?? "");
        return new TextResourceContents
        {
            Uri = ctx.Params?.Uri ?? $"xui://docs/{name}",
            MimeType = "text/markdown",
            Text = inlined,
        };
    }

    // Replaces ![alt](path/to/file.svg) with the raw SVG content if bundled.
    private static readonly Regex SvgImagePattern = new(@"!\[([^\]]*)\]\(([^)]+\.svg)\)", RegexOptions.Compiled);

    private static string InlineSvgs(string markdown, string baseDir)
    {
        return SvgImagePattern.Replace(markdown, match =>
        {
            var alt = match.Groups[1].Value;
            var path = match.Groups[2].Value;
            var key = "docs/" + (baseDir.Length > 0 ? $"{baseDir}/{path}" : path).Replace('\\', '/');
            return KnowledgeBase.TryRead(key, out var svg)
                ? $"<!-- {alt} -->\n{svg}"
                : match.Value; // not bundled — leave as-is
        });
    }
}
