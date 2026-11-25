using System.Reflection;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.Playground;
using Xui.Compiler.XuiNet.Reachability;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.InlineArrays;

[Collection(nameof(ManagedConsoleCollection))]
public class StaticSpanTests
{
    [Fact]
    public async Task LinkedEnumAndApplicationBlobsKeepSeparateModuleIdentities()
    {
        string name = "StaticSpanLibrary" + Guid.NewGuid().ToString("N");
        byte[] libraryImage = Frontend.Roslyn.Compile("""
            using System;
            public enum Code : ushort { First=1, Second=2, Last=65535 }
            public static class Values {
                public static ReadOnlySpan<Code> Read() => [Code.First, Code.Second, Code.Last];
            }
            """, OutputKind.DynamicallyLinkedLibrary, name);
        byte[] appImage = Frontend.Roslyn.Compile("""
            using System;
            public static class Program {
                static ReadOnlySpan<int> Local() => [10, 20, 30];
                public static int Main() {
                    var remote=Values.Read(); var local=Local();
                    return (int)remote[2] + local[2];
                }
            }
            """, additionalReferences: [MetadataReference.CreateFromImage(libraryImage)]);
        var runtime = new System.Runtime.Loader.AssemblyLoadContext(name, isCollectible: true);
        int expected;
        try
        {
            using var libraryStream = new MemoryStream(libraryImage);
            using var appStream = new MemoryStream(appImage);
            runtime.LoadFromStream(libraryStream);
            expected = (int)runtime.LoadFromStream(appStream).EntryPoint!.Invoke(null, null)!;
        }
        finally { runtime.Unload(); }
        Assert.Equal(65565, expected);
        using var app = CanvasTests.Read(appImage);
        using var library = CanvasTests.Read(libraryImage);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, library, .. framework.Assemblies]);
        var emitter = new WasmEmitter();
        var result = await Node.ExecuteAsync(emitter.Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal("", result.Output); Assert.Equal(expected, result.ExitCode);
        var blobs = emitter.Dependencies!.Nodes.OfType<Dependency.Field>().Where(f =>
            (f.Identity.DeclaringType.Module.GetField(f.Identity.Handle).Attributes & FieldAttributes.HasFieldRVA) != 0).ToArray();
        Assert.Equal(2, blobs.Length);
        Assert.Equal(2, blobs.Select(f => f.Identity.DeclaringType.Module).Distinct().Count());
    }

    [Theory]
    [InlineData("byte", "1, 2, 255, 4", "255")]
    [InlineData("sbyte", "1, 2, -128, 4", "-128")]
    [InlineData("short", "1, 2, -32768, 4", "-32768")]
    [InlineData("ushort", "1, 2, 65535, 4", "65535")]
    [InlineData("int", "1, 2, int.MinValue, 4", "int.MinValue")]
    [InlineData("uint", "1, 2, uint.MaxValue, 4", "uint.MaxValue")]
    [InlineData("long", "1, 2, long.MinValue, 4", "long.MinValue")]
    [InlineData("ulong", "1, 2, ulong.MaxValue, 4", "ulong.MaxValue")]
    [InlineData("float", "1, 2, -1.25f, 4", "-1.25f")]
    [InlineData("double", "1, 2, -1.25d, 4", "-1.25d")]
    [InlineData("char", "'a', 'b', '\\uffff', 'd'", "'\\uffff'")]
    [InlineData("bool", "true, false, true, false", "true")]
    public async Task PrimitiveDataSurvivesReturningFramesAndIsRetainedOnce(string type, string values, string expected)
    {
        string source = $$"""
            using System;
            public static class Program {
                static ReadOnlySpan<{{type}}> Values() => [{{values}}];
                static ReadOnlySpan<int> Unused() => [12345, 67890, 11111, 22222];
                public static int Main() {
                    var first=Values(); var second=Values();
                    Console.WriteLine(first.Length); Console.WriteLine(first[2]=={{expected}});
                    Console.WriteLine(second.Slice(1,2)[1]=={{expected}});
                    return 42;
                }
            }
            """;
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("4\nTrue\nTrue\n", managed.Output); Assert.Equal(42, managed.ExitCode);
            using var app = CanvasTests.Read(pe);
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(app, new ConsoleWriteLineRewriter());
            Assert.Equal(managed, await Node.ExecuteAsync(wasm));
            var data = emitter.Dependencies!.Nodes.OfType<Dependency.Field>().Where(f =>
                (f.Identity.DeclaringType.Module.GetField(f.Identity.Handle).Attributes & FieldAttributes.HasFieldRVA) != 0);
            Assert.Single(data);
            Assert.DoesNotContain(emitter.Dependencies.Methods, m => m.ToString().EndsWith(".Unused", StringComparison.Ordinal));
        }
    }
}
