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
    [Theory]
    [InlineData("Closures", "15\n107\n", 122)]
    [InlineData("Multicast", "2\n12\n4\n4\n7\n15\n15\n8\n16\n9\n", 9)]
    public async Task DelegatesExecuteAndSnapshot(string fixture, string output, int exitCode)
    {
        string source = File.ReadAllText(Path.Combine(Directory(), $"../../Fixtures/Delegates/{fixture}.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal(output, managed.Output);
            Assert.Equal(exitCode, managed.ExitCode);
            byte[] wasm = GcTests.Emit(pe);
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(managed, result);
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches(fixture, $"""
                # {(fixture == "Closures" ? "Delegates and escaping closures" : "Multicast delegates")}

                Source: [{fixture}.cs](Fixtures/Delegates/{fixture}.cs)

                {(fixture == "Closures" ? "Each Create call owns a separate captured count. Its two callbacks share\nthat captured variable after Create returns. The delegate keeps the\ncompiler-generated closure object reachable through a GC reference." : "Handlers run in order and the last result is returned. Removal matches\nthe last occurrence by target and method, preserving the original list.")}

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
