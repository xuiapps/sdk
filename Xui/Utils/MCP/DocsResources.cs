using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Xui.MCP;

[McpServerResourceType]
public static class DocsResources
{
    private static readonly Assembly Assembly = typeof(DocsResources).Assembly;

    private static readonly IReadOnlyList<string> DocNames = Assembly
        .GetManifestResourceNames()
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
        var resourceName = $"docs/{name}.md";
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new KeyNotFoundException($"No doc found: {name}. Available: {string.Join(", ", DocNames)}");
        using var reader = new StreamReader(stream);
        var markdown = InlineSvgs(reader.ReadToEnd(), baseDir: System.IO.Path.GetDirectoryName(name) ?? "");
        return new TextResourceContents
        {
            Uri = ctx.Params?.Uri ?? $"xui://docs/{name}",
            MimeType = "text/markdown",
            Text = markdown,
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
            var resourceName = "docs/" + (baseDir.Length > 0 ? $"{baseDir}/{path}" : path).Replace('\\', '/');

            using var stream = Assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
                return match.Value; // not bundled — leave as-is

            using var reader = new StreamReader(stream);
            return $"<!-- {alt} -->\n{reader.ReadToEnd()}";
        });
    }
}
