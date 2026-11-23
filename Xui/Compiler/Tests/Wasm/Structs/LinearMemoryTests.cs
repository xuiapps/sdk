using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Structs;

public class LinearMemoryTests
{
    [Fact]
    public async Task NestedStructCopiesExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Structs", "LinearMemory.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] wasm = Emit(Frontend.Roslyn.Compile(source, optimizationLevel: optimization));
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal("10\n2\n1\n20\n", result.Output);
            Assert.Equal(7, result.ExitCode);
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("LinearMemory", $"# Linear-memory struct copies\n\nSource: [LinearMemory.cs](Fixtures/Structs/LinearMemory.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Theory]
    [InlineData("byte", "250", 250)]
    [InlineData("sbyte", "-120", -120)]
    [InlineData("short", "-30000", -30000)]
    [InlineData("ushort", "60000", 60000)]
    [InlineData("char", "'Z'", 90)]
    [InlineData("int", "-123456", -123456)]
    [InlineData("uint", "4000000000u", -294967296)]
    [InlineData("long", "-1234567890123L", -1912276171)]
    [InlineData("ulong", "1234567890123UL", 1912276171)]
    [InlineData("float", "3.75f", 3)]
    [InlineData("double", "-7.5", -7)]
    [InlineData("nint", "-23", -23)]
    [InlineData("nuint", "23", 23)]
    public async Task PackedScalarFieldsUseTheirStorageWidths(string type, string value, int expected)
    {
        string read = type is "float" or "double" ? $"(data.Value == {value} ? {expected} : 999)" : "unchecked((int)data.Value)";
        string source = $$"""
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
            public struct Data { public byte Head; public {{type}} Value; public byte Tail; }
            public static class Program
            {
                public static int Main()
                {
                    Data data = default;
                    data.Head = 17; data.Value = {{value}}; data.Tail = 29;
                    return {{read}} + (data.Head - 17) * 1000 + (data.Tail - 29) * 10000;
                }
            }
            """;
        await AssertExecution(source, expected);
    }

    [Fact]
    public Task BooleanAndDefaultInitialization() => AssertExecution("""
        public struct Data { public bool Enabled; public int Value; }
        public static class Program
        {
            public static int Main() {
                Data a = default; if (a.Enabled) return 99; if (a.Value != 0) return 99;
                a.Enabled = true; a.Value = 42;
                Data b = a; a = default;
                if (a.Enabled) return 99;
                return b.Enabled ? b.Value + a.Value : 99;
            }
        }
        """, 42);

    [Fact]
    public Task AssignmentExpressionCopiesTheDuplicatedValue() => AssertExecution("""
        public struct Data { public int Value; }
        public static class Program
        {
            public static int Main() {
                Data a = default; Data b = default; a.Value = 7;
                int first = (b = a).Value;
                a.Value = 90;
                return first + b.Value;
            }
        }
        """, 14);

    [Fact]
    public Task FramesRemainIndependentAcrossRecursiveCalls() => AssertExecution("""
        public struct Data { public int Value; }
        public static class Program
        {
            static int Recurse(int n) {
                Data data = default; data.Value = n;
                if (n == 0) return data.Value;
                int child = Recurse(n - 1);
                return data.Value + child;
            }
            public static int Main() => Recurse(10);
        }
        """, 55);

    [Fact]
    public Task RepeatedCallsRestoreFramesAndZeroReusedLocals() => AssertExecution("""
        public struct Data { public int Value; }
        public static class Program
        {
            static int Read(int n) {
                Data data = default;
                int before = data.Value;
                data.Value = n;
                if (n > 0) return before + data.Value;
                return before;
            }
            public static int Main() {
                int total = 0;
                for (int i = 0; i < 100000; i++) total += Read(1);
                return total + Read(0);
            }
        }
        """, 100000);

    [Fact]
    public Task LoopUpdatesStructLocalWithoutAllocatingPerIteration() => AssertExecution("""
        public struct Data { public int Value; }
        public static class Program
        {
            public static int Main() {
                Data data = default;
                for (int i = 0; i < 100000; i++) {
                    Data copy = data; copy.Value = copy.Value + 1; data = copy;
                }
                return data.Value;
            }
        }
        """, 100000);

    [Fact]
    public Task ExplicitOverlappingFieldsShareBytes() => AssertExecution("""
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        public struct Data {
            [System.Runtime.InteropServices.FieldOffset(0)] public int Bits;
            [System.Runtime.InteropServices.FieldOffset(0)] public float Value;
        }
        public static class Program {
            public static int Main() { Data d = default; d.Bits = 1065353216; return d.Value == 1f ? 1 : 99; }
        }
        """, 1);

    [Fact]
    public async Task ReferenceContainingStructsUseGcStorage()
    {
        byte[] pe = Frontend.Roslyn.Compile("public struct Data { public string Text; } public static class Program { public static int Main() { Data d = default; System.Console.WriteLine(d.Text); return 0; } }");
        var result = await Node.ExecuteAsync(Emit(pe));
        Assert.Equal("\n", result.Output);
        Assert.Equal(0, result.ExitCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExactIlCopiesSnapshotValuesBeforeLaterMutation(bool withReferenceField)
    {
        byte[] image = StructMemoryIl.Compile(withReferenceField);
        Assert.Equal(297, (int)Assembly.Load(image).EntryPoint!.Invoke(null, null)!);
        Assert.Equal(297, (await Node.ExecuteAsync(Emit(image))).ExitCode);
    }

    private static async Task AssertExecution(string source, int expected)
    {
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            Assert.Equal(expected, (int)Assembly.Load(pe).EntryPoint!.Invoke(null, null)!);
            Assert.Equal(expected, (await Node.ExecuteAsync(Emit(pe))).ExitCode);
        }
    }

    private static byte[] Emit(byte[] pe)
    {
        using var stream = new MemoryStream(pe);
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
    }

    private static string SourceDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}
