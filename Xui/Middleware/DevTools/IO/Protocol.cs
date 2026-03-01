using System.Text.Json;

namespace Xui.Middleware.DevTools.IO;

public static class Methods
{
    public const string UiInspect = "ui.inspect";
    public const string UiScreenshot = "ui.screenshot";
    public const string InputTap = "input.tap";
    public const string InputPointer = "input.pointer";
    public const string InputClick = "input.click";
    public const string AppInvalidate = "app.invalidate";
    public const string AppIdentify = "app.identify";
}

public record RpcRequest(string Method, int Id, JsonElement? Params = null);

// Param payloads — deserialised from RpcRequest.Params
public record TapParams(float X, float Y);

public record ClickParams(float X, float Y);

/// <summary>Phase values: "start" | "move" | "end" | "cancel"</summary>
public record PointerParams(string Phase, float X, float Y, int Index = 0);

/// <summary>Identity label to display next to the AI pointer overlay, e.g. "Claude, VSCode".</summary>
public record IdentifyParams(string Label);

public record RpcResponse(int Id, object? Result = null, RpcError? Error = null);
public record RpcError(int Code, string Message);

public record RpcNotification(string Method, object Params);

public record InspectResult(ViewNode Root);

public record ViewNode(
    string Type,
    float X, float Y, float W, float H,
    bool Visible,
    ViewNode[] Children);

public record ScreenshotResult(string Svg);
