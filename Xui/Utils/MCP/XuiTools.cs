using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Xui.MCP;

[McpServerToolType]
public static class XuiTools
{
    [McpServerTool, Description("Returns the current Xui SDK version.")]
    public static string GetVersion() => "Xui MCP server running.";
}
