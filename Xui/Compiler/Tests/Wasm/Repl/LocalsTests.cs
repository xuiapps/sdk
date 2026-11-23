using Xui.Compiler.Playground;
using System.Runtime.CompilerServices;
using Xui.Compiler.XuiNet;
using Xui.Compiler.WASM.Emitter;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.Wasm.Repl;

public class LocalsTests(ITestOutputHelper output)
{
    [Fact]
    public async Task LocalsExecuteInNode()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Repl", "Locals.cs"));
        // Preserve declared locals so this snapshot exercises their storage and load/store opcodes.
        byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: Microsoft.CodeAnalysis.OptimizationLevel.Debug);
        using var stream = new MemoryStream(pe);
        using var assembly = AssemblyCompiler.Read(stream);
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());

        string wat = (await WasmTools.ToWatAsync(wasm)).ReplaceLineEndings("\n").TrimEnd();
        var (consoleOutput, exitCode) = await Node.ExecuteAsync(wasm);
        Assert.Equal(0, exitCode);
        Assert.Equal("True\n65\n-12\n250\n-1234\n60000\n-42\n4000000000\n-5000000000\n10000000000\n1.25\n2.5\n-7\n8\nLocal strings\n", consoleOutput);
        string markdown = $"""
            # Locals WASM

            ## Source

            ```csharp
            {source.TrimEnd()}
            ```

            ## Output

            ```text
            {consoleOutput.TrimEnd()}
            ```

            ## Exit Code

            ```text
            {exitCode}
            ```

            ## WAT

            ```wat
            {wat}
            ```
            """ + "\n";
        output.WriteLine(markdown);
        Snapshot.AssertMatches("Locals", markdown);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
