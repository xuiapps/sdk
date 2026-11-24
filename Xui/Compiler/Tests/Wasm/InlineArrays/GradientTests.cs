using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.InlineArrays;

public class GradientTests
{
    [Fact]
    public async Task XuiGradientStopsUseInlineStorage()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/InlineArrays/Gradients.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] coreImage = File.ReadAllBytes(typeof(Xui.Core.Math2D.Point).Assembly.Location);
            byte[] image = Frontend.Roslyn.Compile(source, optimizationLevel: optimization,
                additionalReferences: [MetadataReference.CreateFromImage(coreImage),
                    MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll"))]);
            using var app = CanvasTests.Read(image);
            using var core = CanvasTests.Read(coreImage);
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation([app, core, .. framework.Assemblies]);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(app, new ConsoleWriteLineRewriter());
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal("0\n100\n0\n1\n0\n1\n0\n1\n", result.Output);
            Assert.Equal(2, result.ExitCode);
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Gradients", $"# Xui gradient stops in inline-array storage\n\nSource: [Gradients.cs](Fixtures/InlineArrays/Gradients.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
