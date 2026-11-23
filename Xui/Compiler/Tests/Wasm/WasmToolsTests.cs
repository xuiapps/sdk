using Xunit;

namespace Xui.Compiler.Tests.Wasm;

public class WasmToolsTests
{
    [Fact]
    public async Task InvalidBinaryReportsToolFailure()
    {
        // Valid magic, unsupported binary version.
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            WasmTools.ToWatAsync(new byte[] { 0, 0x61, 0x73, 0x6d, 0xff, 0xff, 0xff, 0xff }));
        Assert.Contains("wasm-tools print failed", error.Message);
        Assert.Contains("version", error.Message, StringComparison.OrdinalIgnoreCase);
    }
}
