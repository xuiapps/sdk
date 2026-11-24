using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Delegates;

[Collection(nameof(ManagedConsoleCollection))]
public class SnapshotTests
{
    [Fact]
    public async Task EscapingClosuresExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(Directory(), "../../Fixtures/Delegates/Closures.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("15\n107\n", managed.Output);
            Assert.Equal(122, managed.ExitCode);
            byte[] wasm = GcTests.Emit(pe);
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(managed, result);
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Closures", $"""
                # Delegates and escaping closures

                Source: [Closures.cs](Fixtures/Delegates/Closures.cs)

                Each Create call owns a separate captured count. Its two callbacks share
                that captured variable after Create returns. The delegate keeps the
                compiler-generated closure object reachable through a GC reference.

                ## Source

                ```csharp
                {source.TrimEnd()}
                ```

                ## Output

                ```text
                {result.Output.TrimEnd()}
                ```

                ## Exit Code

                ```text
                {result.ExitCode}
                ```

                ## WAT

                ```wat
                {wat.TrimEnd()}
                ```
                """ + "\n");
        }
    }

    private static string Directory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
