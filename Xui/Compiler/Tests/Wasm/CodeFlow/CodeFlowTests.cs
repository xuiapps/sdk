using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

public class CodeFlowTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("If")]
    [InlineData("IfElse")]
    [InlineData("ElseIf")]
    [InlineData("NestedConditionals")]
    [InlineData("EarlyReturn")]
    [InlineData("ConditionalValue")]
    [InlineData("StackPrefix")]
    [InlineData("While")]
    [InlineData("DoWhile")]
    [InlineData("For")]
    [InlineData("Nested")]
    [InlineData("NestedLoops")]
    [InlineData("WhileWithBreak")]
    [InlineData("WhileWithContinue")]
    public async Task FixtureExecutesInNode(string fixture)
    {
        int[] inputs = [-3, 0, 1, 2, 5, 10];
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "CodeFlow", fixture + ".cs"));
        string calls = string.Join("\n", inputs.Select(value => $"        System.Console.WriteLine({fixture}.Run({value}));"));
        source += $$"""

            public static class EntryPoint
            {
                public static int Main()
                {
            {{calls}}
                    return 0;
                }
            }
            """;
        string? snapshot = null;
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] image = Frontend.Roslyn.Compile(source, assemblyName: "Wasm.CodeFlow." + fixture + optimization,
                optimizationLevel: optimization);
            var managed = Assembly.Load(image).GetType("Samples.CodeFlow." + fixture)!.GetMethod("Run")!;
            string expected = string.Join("\n", inputs.Select(value => ((int)managed.Invoke(null, [value])!).ToString(CultureInfo.InvariantCulture))) + "\n";
            using var stream = new MemoryStream(image);
            using var assembly = AssemblyCompiler.Read(stream);
            byte[] wasm = new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(expected, result.Output);
            Assert.Equal(0, result.ExitCode);
            if (optimization == OptimizationLevel.Release)
            {
                string wat = (await WasmTools.ToWatAsync(wasm)).ReplaceLineEndings("\n").TrimEnd();
                snapshot = $"# {fixture} WASM\n\nSource: [{fixture}.cs](Fixtures/CodeFlow/{fixture}.cs)\n\n" +
                    $"## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n" +
                    $"## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n" +
                    $"## WAT\n\n```wat\n{wat}\n```\n";
            }
        }
        output.WriteLine(snapshot);
        Snapshot.AssertMatches(fixture, snapshot!);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
