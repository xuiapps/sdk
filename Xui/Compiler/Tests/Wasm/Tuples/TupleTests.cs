using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Tuples;

public class TupleTests
{
    [Fact]
    public async Task FrameworkTuplesExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/Tuples/Values.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] image = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            using var app = CanvasTests.Read(image);
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(app, new ConsoleWriteLineRewriter());
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal("21\n24.5\n1\n21\n9\noriginal\ncopy\n", result.Output);
            Assert.Equal(42, result.ExitCode);
            Assert.Contains(emitter.Dependencies!.Methods, m => m.DeclaringType.Module.Name == "System.Private.CoreLib.dll");
            Assert.DoesNotContain(emitter.Dependencies.Methods, m => m.DeclaringType.Module.GetMethod(m.Handle).Name == "ToString");
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Values", $"# Framework tuples in WASM\n\nSource: [Values.cs](Fixtures/Tuples/Values.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Fact]
    public async Task RealXuiGeometryAcceptsTupleSyntax()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/Tuples/XuiGeometry.cs"));
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
            Assert.Equal("3\n4\n120\n0\n15\n140\n90\n", result.Output);
            Assert.Equal(0, result.ExitCode);
            Assert.Contains(emitter.Dependencies!.Methods, m => m.DeclaringType.Module == core.ManifestModule && m.ToString() == "Xui.Core.Math2D.Point.op_Implicit");
            Assert.DoesNotContain(emitter.Dependencies.Methods, m => m.DeclaringType.Module.GetMethod(m.Handle).Name == ".cctor");
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("XuiGeometry", $"# Xui tuple geometry in WASM\n\nSource: [XuiGeometry.cs](Fixtures/Tuples/XuiGeometry.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task TupleAritiesExecuteTheirFrameworkConstructors(int count)
    {
        string values = string.Join(", ", Enumerable.Range(1, count));
        string sum = string.Join(" + ", Enumerable.Range(1, count).Select(i => $"value.Item{i}"));
        string source = $"public static class Program {{ public static int Main() {{ var value = ({values}); return {sum}; }} }}";
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile(source));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Equal(count * (count + 1) / 2, (await Node.ExecuteAsync(new WasmEmitter().Emit(app))).ExitCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AddressableReferenceTupleCopiesOwnTheirSlots(bool debug)
    {
        const string source = """
            public static class Program {
                static void Keep(ref (string Name, int Value) pair) { }
                static (string Name, int Value) Change((string Name, int Value) pair) { pair.Name = "changed"; pair.Value += 1; return pair; }
                public static int Main() {
                    var pair = (Name: "original", Value: 1); Keep(ref pair);
                    var copy = Change(pair);
                    for (int i = 0; i < 100000; i++) copy = Change(copy);
                    System.Console.WriteLine(pair.Name);
                    System.Console.WriteLine(copy.Name);
                    return copy.Value;
                }
            }
            """;
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile(source, optimizationLevel: debug ? OptimizationLevel.Debug : OptimizationLevel.Release));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var result = await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal("original\nchanged\n", result.Output);
        Assert.Equal(100002, result.ExitCode);
    }

    [Theory]
    [InlineData("public struct S { public static int Value = Init(); static int Init() => 1; public int Read() => 42; }", false)]
    [InlineData("public struct S { static S() { System.Console.WriteLine(99); } public int Read() => 42; }", true)]
    public async Task OnlyRelaxedUnusedStaticInitializationMayBeDeferred(string declaration, bool requiresInitializer)
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile(declaration + " public static class Program { public static int Main() { S s = default; return s.Read(); } }"));
        var emitter = new WasmEmitter();
        if (requiresInitializer) Assert.Throws<NotSupportedException>(() => emitter.Emit(app, new ConsoleWriteLineRewriter()));
        else
        {
            Assert.Equal(42, (await Node.ExecuteAsync(emitter.Emit(app))).ExitCode);
            Assert.DoesNotContain(emitter.Dependencies!.Methods, m => m.DeclaringType.Module.GetMethod(m.Handle).Name == ".cctor");
        }
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
