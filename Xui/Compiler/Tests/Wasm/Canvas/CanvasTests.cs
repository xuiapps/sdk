using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.Reachability;
using Model = Xui.Compiler.XuiNet.DotNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Canvas;

public class CanvasTests
{
    [Theory]
    [InlineData("Shapes", 0)]
    [InlineData("Grid", 18)]
    [InlineData("Events", 42)]
    [InlineData("Values", 42)]
    public async Task ActualXuiGeometryProducesStandaloneCanvas(string fixture, int exitCode)
    {
        string source = Source(fixture), host = Source("Host");
        byte[] coreImage = File.ReadAllBytes(typeof(Xui.Core.Math2D.Rect).Assembly.Location);
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var image = Frontend.Roslyn.Compile(source, assemblyName: "CanvasDemo", optimizationLevel: optimization,
                additionalReferences: [MetadataReference.CreateFromImage(coreImage), MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll"))], additionalSources: [host]);
            using var core = Read(coreImage); using var app = Read(image);
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation(fixture == "Values" ? [app, core, .. framework.Assemblies] : [app, core]);
            var emitter = new WasmEmitter(); byte[] wasm = emitter.Emit(app, new CanvasRewriter());
            var result = await CanvasHost.Record(wasm);
            Assert.Equal(exitCode, result.ExitCode);
            Assert.Equal(JsonSerializer.Serialize(Expected(fixture)), JsonSerializer.Serialize(JsonDocument.Parse(result.Commands).RootElement));
            Assert.Contains(emitter.Dependencies!.Nodes.OfType<Dependency.Layout>(), n => n.Identity.Module == core.ManifestModule && n.Identity.ToString() == "Xui.Core.Math2D.Rect");
            Assert.DoesNotContain(emitter.Dependencies.Methods, m => m.DeclaringType.ToString() == "CanvasDemo.NativeCanvas");
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            string retained = string.Join("\n", emitter.Dependencies.Methods.Select(m => $"- `{m.DeclaringType.Module.Name}`: `{m}`"));
            string markdown = $"# {fixture}: Xui geometry → WASM → Canvas\n\n[Open standalone demo]({fixture}.html)\n\nSource: [{fixture}.cs](Fixtures/Canvas/{fixture}.cs), [Host.cs](Fixtures/Canvas/Host.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Drawing commands\n\n```json\n{result.Commands}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## Retained methods\n\n{retained}\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n";
            Snapshot.AssertFileMatches(fixture, ".html", CanvasHost.Html(fixture, source + "\n" + host, wasm));
            Snapshot.AssertMatches(fixture, markdown);
        }
    }

    [Fact]
    public async Task NativeFloatRoundsAtWasm32Boundaries()
    {
        var image = Frontend.Roslyn.Compile("""
            using System;
            using System.Runtime.InteropServices;
            public static class Program {
                static NFloat Half(NFloat value) => value * (NFloat)0.5;
                public static int Main() {
                    NFloat value = (NFloat)16777217.0;
                    Console.WriteLine((double)value);
                    value = value + (NFloat)1;
                    Console.WriteLine((double)value);
                    Console.WriteLine((double)Half(value));
                    return 0;
                }
            }
            """);
        using var app = Read(image);
        var result = await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal("16777216\n16777216\n8388608\n", result.Output);
    }

    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task NativeFloatDivisionAndComparisonsKeepIeeeSemantics(OptimizationLevel optimization)
    {
        byte[] image = Frontend.Roslyn.Compile("""
            using System;
            using System.Runtime.InteropServices;
            public static class Program {
                static void Compare(NFloat a, NFloat b) {
                    Console.WriteLine(a == b); Console.WriteLine(a != b);
                    Console.WriteLine(a < b); Console.WriteLine(a <= b);
                    Console.WriteLine(a > b); Console.WriteLine(a >= b);
                }
                public static int Main() {
                    Compare((NFloat)1, (NFloat)2);
                    Compare(NFloat.NaN, NFloat.NaN);
                    Compare((NFloat)0.0, (NFloat)(-0.0));
                    Console.WriteLine((NFloat)1 / (NFloat)0 == NFloat.PositiveInfinity);
                    Console.WriteLine((NFloat)1 / (NFloat)(-0.0) == NFloat.NegativeInfinity);
                    Console.WriteLine((double)((NFloat)1 / (NFloat)10));
                    return 0;
                }
            }
            """, optimizationLevel: optimization);
        using var app = Read(image);
        var result = await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal("False\nTrue\nTrue\nTrue\nFalse\nFalse\n" +
            "False\nTrue\nFalse\nFalse\nFalse\nFalse\n" +
            "True\nFalse\nFalse\nTrue\nFalse\nTrue\n" +
            "True\nTrue\n0.10000000149011612\n", result.Output);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void UnsupportedNativeFloatMembersFailExplicitly()
    {
        var image = Frontend.Roslyn.Compile("using System.Runtime.InteropServices; NFloat a = (NFloat)2; NFloat b = NFloat.Exp(a); return 0;");
        using var app = Read(image);
        Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app));
    }

    [Fact]
    public async Task NestedStructsResolveTheirOwnModulesAndFieldRows()
    {
        byte[] coordinates = Frontend.Roslyn.Compile(Source("Libraries/Coordinates"), OutputKind.DynamicallyLinkedLibrary, "Coordinates");
        byte[] geometry = Frontend.Roslyn.Compile(Source("Libraries/Geometry"), OutputKind.DynamicallyLinkedLibrary, "Geometry",
            additionalReferences: [MetadataReference.CreateFromImage(coordinates)]);
        byte[] program = Frontend.Roslyn.Compile(Source("Libraries/Program"), additionalReferences:
            [MetadataReference.CreateFromImage(coordinates), MetadataReference.CreateFromImage(geometry)]);
        using var points = Read(coordinates); using var boxes = Read(geometry); using var app = Read(program);
        _ = new Xui.Compiler.XuiNet.Compilation(app, boxes, points);
        var emitter = new WasmEmitter();
        var result = await Node.ExecuteAsync(emitter.Emit(app));
        Assert.Equal(42, result.ExitCode);
        Assert.Equal(3, emitter.Dependencies!.Nodes.OfType<Dependency.Layout>().Select(n => n.Identity.Module).Distinct().Count());
    }

    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task LinkedClassHierarchyPreservesFieldsAndStructDispatch(OptimizationLevel optimization)
    {
        byte[] coreImage = File.ReadAllBytes(typeof(Xui.Core.Math2D.Rect).Assembly.Location);
        List<MetadataReference> references = [MetadataReference.CreateFromImage(coreImage),
            MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll"))];
        byte[] contracts = Frontend.Roslyn.Compile(Source("LinkedViews/Contracts"), OutputKind.DynamicallyLinkedLibrary,
            "ViewContracts", optimization, references);
        references.Add(MetadataReference.CreateFromImage(contracts));
        byte[] panel = Frontend.Roslyn.Compile(Source("LinkedViews/Panel"), OutputKind.DynamicallyLinkedLibrary,
            "ViewPanel", optimization, references);
        references.Add(MetadataReference.CreateFromImage(panel));
        byte[] program = Frontend.Roslyn.Compile(Source("LinkedViews/Program"), assemblyName: "ViewApp",
            optimizationLevel: optimization, additionalReferences: references);
        using var core = Read(coreImage); using var contractAssembly = Read(contracts);
        using var panelAssembly = Read(panel); using var app = Read(program);
        _ = new Xui.Compiler.XuiNet.Compilation(app, core, contractAssembly, panelAssembly);
        var emitter = new WasmEmitter();
        var result = await Node.ExecuteAsync(emitter.Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal(42, result.ExitCode);
        Assert.Equal("Panel\n1\n11\n22\nTrue\n", result.Output);
        var child = Assert.Single(emitter.Types, t => t.Name == "Child");
        Assert.Equal(new[] { "ViewContracts.dll", "ViewContracts.dll", "ViewPanel.dll", "ViewApp.exe" },
            child.Fields.Select(f => f.DeclaringType.Module.Name));
        Assert.Equal(new uint[] { 1, 5, 6, 10 }, child.Fields.Select(f => f.First.Value));
    }

    private static List<object[]> Expected(string fixture)
    {
        List<object[]> commands = [["Clear", 0.0625, 0.125, 0.25, 1]];
        if (fixture == "Shapes") commands.AddRange([
            ["FillRect", 40, 100, 240, 180, 0.125, 0.5, 1, 1],
            ["FillRect", 320, 100, 240, 180, 1, 0.375, 0.25, 1],
            ["FillText", "Hello from Xui WASM", 40, 60, 28, 1, 1, 1, 1]]);
        else if (fixture == "Grid")
        {
            for (int row = 0; row < 3; row++) for (int column = 0; column < 6; column++)
                commands.Add(["FillRect", 40 + column * 88, 96 + row * 72, 72, 56, 0.25, 0.75, 1, 1]);
            commands.Add(["FillText", "Geometry calculated in C#", 40, 56, 24, 0.25, 0.75, 1, 1]);
        }
        else if (fixture == "Values") commands.AddRange([
            ["FillRect", 40, 100, 520, 180, 0.125, 0.5, 1, 1],
            ["FillRect", 60, 120, 480, 140, 1, 0.375, 0.25, 1],
            ["FillText", "Rect values through interface calls", 40, 60, 26, 1, 1, 1, 1]]);
        else commands.AddRange([
            ["FillRect", 40, 100, 520, 180, 0.5, 1, 0.75, 1],
            ["FillText", "Stack event → interface → canvas", 40, 60, 26, 0.5, 1, 0.75, 1]]);
        return commands;
    }

    internal static Model.Assembly Read(byte[] image) { using var stream = new MemoryStream(image); return AssemblyCompiler.Read(stream); }
    private static string Source(string name) => File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/Canvas", name + ".cs"));
    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
