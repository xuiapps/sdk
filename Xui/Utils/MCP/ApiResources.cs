using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Xui.MCP;

[McpServerResourceType]
public static class ApiResources
{
    private static readonly IReadOnlyList<string> ApiPaths = KnowledgeBase.Paths
        .Where(n => n.StartsWith("api/") && n.EndsWith(".cs"))
        .Select(n => n["api/".Length..])
        .OrderBy(n => n)
        .ToList();

    [McpServerResource(UriTemplate = "xui://api", Name = "Xui API — Table of Contents", MimeType = "text/markdown")]
    [Description("Lists all available Xui API source files (stripped to signatures + XML docs) grouped by project.")]
    public static string GetToc()
    {
        var sb = new StringBuilder("# Xui API Reference\n\n");
        foreach (var group in ApiPaths.GroupBy(p => p.Split('/')[0]).OrderBy(g => g.Key))
        {
            sb.AppendLine($"## {group.Key}");
            sb.AppendLine();
            foreach (var path in group)
                sb.AppendLine($"- [{path}](xui://api/{path})");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    [McpServerResource(UriTemplate = "xui://api/{+path}", Name = "Xui API File", MimeType = "text/x-csharp")]
    [Description("Returns a stripped Xui C# source file (signatures + XML doc comments, no implementations). " +
                 "Path format: {ProjectName}/{relative/path/to/File.cs}, e.g. Xui.Core/Math2D/Vector.cs")]
    public static TextResourceContents GetApiFile(RequestContext<ReadResourceRequestParams> ctx, string path)
    {
        return new TextResourceContents
        {
            Uri = ctx.Params?.Uri ?? $"xui://api/{path}",
            MimeType = "text/x-csharp",
            Text = KnowledgeBase.Read($"api/{path}"),
        };
    }
}
