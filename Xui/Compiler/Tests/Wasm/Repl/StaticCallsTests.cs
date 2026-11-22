using Xui.Compiler.Tests.Wasm.Rewrite;
using System.Runtime.CompilerServices;
using Xui.Compiler.XuiNet;
using Xui.Compiler.WASM.Emitter;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.Wasm.Repl;

public class StaticCallsTests(ITestOutputHelper output)
{
    [Fact]
    public async Task StaticCallsExecutesInNode()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Repl", "StaticCalls.cs"));
        byte[] pe = Frontend.Roslyn.Compile(source);
        using var stream = new MemoryStream(pe);
        using var assembly = AssemblyCompiler.Read(stream);
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());

        string wat = (await WasmTools.ToWatAsync(wasm)).ReplaceLineEndings("\n").TrimEnd();
        var (consoleOutput, exitCode) = await Node.ExecuteAsync(wasm);
        Assert.Equal(0, exitCode);
        Assert.Equal("2\n", consoleOutput);
        string markdown = $"""
            # Static Calls WASM

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
        Snapshot.AssertMatches("StaticCalls", markdown);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
