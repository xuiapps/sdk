using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Statics;

[Collection(nameof(ManagedConsoleCollection))]
public class StaticSnapshotTests
{
    [Fact]
    public async Task StaticStructInitializationExecutesAndSnapshots()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/Statics/Initialization.cs"));
        byte[] image = Frontend.Roslyn.Compile(source);
        var managed = GcTests.RunManaged(image);
        Assert.Equal("Main\nSettings initialized\nXui\n20\n", managed.Output);
        Assert.Equal(42, managed.ExitCode);
        using var app = CanvasTests.Read(image);
        byte[] wasm = new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter());
        Assert.Equal(managed, await Node.ExecuteAsync(wasm));
        string wat = await WasmTools.ToWatAsync(wasm);
        Snapshot.AssertMatches("Initialization", $"# Static fields and type initialization\n\nSource: [Initialization.cs](Fixtures/Statics/Initialization.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{managed.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{managed.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
    }

    private static string SourceDirectory([System.Runtime.CompilerServices.CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
