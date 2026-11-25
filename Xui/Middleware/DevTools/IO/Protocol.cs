using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xui.Middleware.DevTools.IO;

public static class Methods
{
    public const string UiInspect = "ui.inspect";
    public const string UiScreenshot = "ui.screenshot";
    public const string InputTap = "input.tap";
    public const string InputPointer = "input.pointer";
    public const string InputClick = "input.click";
    public const string InputMouseMove = "input.mousemove";
    public const string AppInvalidate = "app.invalidate";
    public const string AppFrame = "app.frame";
    public const string ProfileReset = "profile.reset";
    public const string ProfileRead = "profile.read";
    public const string AppQuit = "app.quit";
    public const string AppIdentify = "app.identify";
}

public record RpcRequest(string Method, int Id, JsonElement? Params = null);

// Param payloads — deserialised from RpcRequest.Params
public record TapParams([property: JsonRequired] float X, [property: JsonRequired] float Y);

public record ClickParams([property: JsonRequired] float X, [property: JsonRequired] float Y);

/// <summary>Phase values: "start" | "move" | "end" | "cancel"</summary>
public record PointerParams([property: JsonRequired] string Phase, [property: JsonRequired] float X, [property: JsonRequired] float Y, int Index = 0);

public record MouseMoveParams([property: JsonRequired] float X, [property: JsonRequired] float Y);

/// <summary>Identity label to display next to the AI pointer overlay, e.g. "Claude, VSCode".</summary>
public record IdentifyParams([property: JsonRequired] string Label);

public record RpcResponse(int Id, object? Result = null, RpcError? Error = null);
public record RpcError(int Code, string Message);

public record RpcNotification(string Method, object Params);

public record InspectResult(ViewNode Root);

public record ViewNode(
    string Type,
    float X, float Y, float W, float H,
    float CenterX, float CenterY,
    bool Visible,
    string? Id,
    string? ClassName,
    ViewNode[] Children);

public record ScreenshotResult(string Svg);

/// <summary>Advance the controlled animation clock, then await a real render callback.</summary>
public record FrameParams(int ElapsedMilliseconds = 16);
public record FrameResult(long Frame, long AnimationBytes, long RenderBytes);
public record ProfileResult(long Frames, long AnimationBytes, long RenderBytes, long InputBytes, string? AllocationTrace = null);
