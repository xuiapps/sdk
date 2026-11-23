using Xui.Compiler.Playground;
using System.Runtime.CompilerServices;
using Xui.Compiler.XuiNet;
using Xui.Compiler.WASM.Emitter;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.Wasm.Repl;

public class PrimitivesTests(ITestOutputHelper output)
{
    [Fact]
    public async Task PrimitivesExecuteInNode()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Repl", "Primitives.cs"));
        byte[] pe = Frontend.Roslyn.Compile(source);
        using var stream = new MemoryStream(pe);
        using var assembly = AssemblyCompiler.Read(stream);
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());

        string wat = (await WasmTools.ToWatAsync(wasm)).ReplaceLineEndings("\n").TrimEnd();
        var (consoleOutput, exitCode) = await Node.ExecuteAsync(wasm);
        Assert.Equal(0, exitCode);
        Assert.Equal("1\n2\n-42\n123456\n1.25\n2.5\nTrue\nFalse\n", consoleOutput);
        string markdown = $"""
            # Primitives WASM

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
        Snapshot.AssertMatches("Primitives", markdown);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
