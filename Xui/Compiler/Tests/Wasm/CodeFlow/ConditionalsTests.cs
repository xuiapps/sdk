using Xui.Compiler.Tests.Wasm.Rewrite;
using Xui.Compiler.Tests.Wasm.Repl;
using System.Runtime.CompilerServices;
using Xui.Compiler.XuiNet;
using Xui.Compiler.WASM.Emitter;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

public class ConditionalsTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ConditionalsExecutesInNode()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "CodeFlow", "Conditionals.cs"));
        byte[] pe = Frontend.Roslyn.Compile(source);
        using var stream = new MemoryStream(pe);
        using var assembly = AssemblyCompiler.Read(stream);
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());

        string wat = (await WasmTools.ToWatAsync(wasm)).ReplaceLineEndings("\n").TrimEnd();
        var (consoleOutput, exitCode) = await Node.ExecuteAsync(wasm);
        Assert.Equal(15, exitCode);
        Assert.Equal("15\n25\n1.75\n3\nnegative\nzero\npositive\nyes\nno\n", consoleOutput);
        string markdown = $"""
            # Typed Conditionals WASM

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
        Snapshot.AssertMatches("Conditionals", markdown);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
