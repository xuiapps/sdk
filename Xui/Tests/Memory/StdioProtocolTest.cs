using System.Text;
using System.Text.Json;
using Xui.Middleware.DevTools.IO;

namespace Xui.Tests.Memory;

public class StdioProtocolTest
{
    [Fact]
    public async Task InvalidRequestsDoNotCloseTheConnectionAndNotificationsHaveNoReply()
    {
        var requests = """
            not json
            {"jsonrpc":"2.0","id":"missing","method":"unknown"}
            {"jsonrpc":"2.0","id":2,"method":"input.tap","params":{"x":1}}
            {"jsonrpc":"2.0","method":"app.invalidate"}
            {"jsonrpc":"2.0","id":3,"method":"profile.read"}
            {"id":4,"method":"app.invalidate"}
            """;
        var handler = new Handler();
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(requests));
        using var output = new MemoryStream();
        using (var connection = new JsonRpcConnection(input, output, leaveOpen: true))
            await new RpcServer(handler).ServeAsync(connection);
        var replies = Encoding.UTF8.GetString(output.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonSerializer.Deserialize<JsonElement>(line)).ToArray();
        Assert.Equal(5, replies.Length);
        Assert.Equal(-32700, replies[0].GetProperty("error").GetProperty("code").GetInt32());
        Assert.Equal("missing", replies[1].GetProperty("id").GetString());
        Assert.Equal(-32601, replies[1].GetProperty("error").GetProperty("code").GetInt32());
        Assert.Equal(-32602, replies[2].GetProperty("error").GetProperty("code").GetInt32());
        Assert.False(replies[2].TryGetProperty("result", out _));
        Assert.Equal(3, replies[3].GetProperty("id").GetInt32());
        Assert.Equal(JsonValueKind.Null, replies[4].GetProperty("result").ValueKind);
        Assert.All(replies, reply => Assert.Equal("2.0", reply.GetProperty("jsonrpc").GetString()));
        Assert.Equal(2, handler.Invalidations);
    }

    private sealed class Handler : IDevToolsHandler
    {
        public int Invalidations;
        public Task<InspectResult> HandleInspect() => throw new NotSupportedException();
        public Task<ScreenshotResult> HandleScreenshot() => throw new NotSupportedException();
        public Task HandleTap(TapParams p) => Task.CompletedTask;
        public Task HandlePointer(PointerParams p) => Task.CompletedTask;
        public Task HandleClick(ClickParams p) => Task.CompletedTask;
        public Task HandleMouseMove(MouseMoveParams p) => Task.CompletedTask;
        public Task HandleInvalidate() { Invalidations++; return Task.CompletedTask; }
        public Task HandleIdentify(IdentifyParams p) => Task.CompletedTask;
        public Task<FrameResult> HandleFrame(FrameParams p) => Task.FromResult(new FrameResult(1, 0, 0));
        public Task<ProfileResult> HandleProfileReset() => HandleProfileRead();
        public Task<ProfileResult> HandleProfileRead() => Task.FromResult(new ProfileResult(0, 0, 0, 0));
    }
}
