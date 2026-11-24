using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Enums;

[Collection(nameof(ManagedConsoleCollection))]
public class EnumTests
{
    [Fact]
    public async Task EnumWidthsExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/Enums/Widths.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("17\n129\n-7\nTrue\nTrue\n29\n", managed.Output);
            Assert.Equal(42, managed.ExitCode);
            using var stream = new MemoryStream(pe);
            using var assembly = AssemblyCompiler.Read(stream);
            byte[] wasm = new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(managed, result);
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Widths", $"# Enum widths and signedness\n\nSource: [Widths.cs](Fixtures/Enums/Widths.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Theory]
    [InlineData("sbyte", "-101", "-101")]
    [InlineData("byte", "201", "201")]
    [InlineData("short", "-30001", "-30001")]
    [InlineData("ushort", "60001", "60001")]
    [InlineData("int", "-2000000001", "-2000000001")]
    [InlineData("uint", "4000000001U", "4000000001")]
    [InlineData("long", "-8000000000000000001L", "-8000000000000000001")]
    [InlineData("ulong", "16000000000000000001UL", "-2446744073709551615")]
    public Task AllUnderlyingTypesSurviveLocalsFieldsArraysAndCalls(string underlying, string literal, string expected) => GcTests.AssertExecution($$"""
        using System;
        public enum Value : {{underlying}} { None, Item = {{literal}} }
        [System.Runtime.CompilerServices.InlineArray(2)] public struct Buffer { private Value item; }
        public class Box { public Value Item; }
        public struct Packet { public byte Before; public Value Item; public byte After; }
        public interface IValue { Value Echo(Value value); }
        public class Reader : IValue { public Value Echo(Value value) => value; }
        public class Generic<T> { public T Item; public T Get() => Item; }
        public static class Program {
            static Value Choose(bool first, Value value) => first ? value : Value.None;
            static Value Call(IValue target, Value value) => target.Echo(value);
            static Packet Copy(Packet packet) => packet;
            public static int Main() {
                Value value = Choose(true, Value.Item);
                var box = new Box(); box.Item = Call(new Reader(), value);
                var packet = new Packet(); packet.Before = 17; packet.Item = box.Item; packet.After = 29;
                packet = Copy(packet);
                var values = new Value[2]; values[1] = packet.Item;
                Buffer buffer = default; buffer[1] = values[1];
                var generic = new Generic<Value>(); generic.Item = buffer[1];
                Console.WriteLine(unchecked((long)({{underlying}})generic.Get()));
                Console.WriteLine(unchecked((long)({{underlying}})values[0]));
                Console.WriteLine(packet.Before); Console.WriteLine(packet.After);
                return values[1] == Value.Item ? 42 : 1;
            }
        }
        """, expected + "\n0\n17\n29\n", 42);
    [Fact]
    public Task FlagsUndefinedValuesAndOverloadsKeepEnumIdentity() => GcTests.AssertExecution("""
        [System.Flags] public enum Flags : byte { None, A = 1, B = 128 }
        public enum Other : byte { A = 1 }
        public class Ops<T> {
            public T[] Values;
            public T Read(int index) => Values[index];
            public void Write(int index, T value) => Values[index] = value;
        }
        public static class Program {
            static int Pick(Flags value) => 10;
            static int Pick(Other value) => 20;
            static int Pick(byte value) => 30;
            static Flags Cast(int value) => unchecked((Flags)value);
            public static int Main() {
                var ops = new Ops<Flags>(); ops.Values = new Flags[2];
                ops.Write(1, Flags.A | Flags.B);
                var flags = ops.Read(1);
                System.Console.WriteLine((int)flags);
                System.Console.WriteLine((int)Cast(511));
                System.Console.WriteLine((int)(flags & ~Flags.A));
                System.Console.WriteLine(flags > Flags.A);
                System.Console.WriteLine((int)(Flags)37);
                return Pick(flags) + Pick(Other.A) + Pick((byte)1);
            }
        }
        """, "129\n255\n128\nTrue\n37\n", 60);

    [Fact]
    public Task ReadonlySpanOfEnumUsesTheUnderlyingWidth() => InlineArrays.InlineArrayTests.AssertLinked("""
        using System;
        public enum Value : ushort { High = 40000, Low = 255 }
        [System.Runtime.CompilerServices.InlineArray(2)] public struct Buffer { private Value item; }
        public static class Program {
            static int Sum(ReadOnlySpan<Value> values) {
                int sum = 0;
                for (int i = 0; i < values.Length; i++) sum += (int)values[i];
                return sum;
            }
            public static int Main() { Buffer buffer = default; buffer[0] = Value.High; buffer[1] = Value.Low; return Sum(buffer); }
        }
        """, "", 40255);

    [Fact]
    public void EnumGcFieldByrefsStillFailExplicitly()
    {
        byte[] pe = Frontend.Roslyn.Compile("""
            public enum Value : byte { Item }
            public class Box { public Value Item; }
            public static class Program {
                static void Change(ref Value value) { value = Value.Item; }
                public static int Main() { var box = new Box(); Change(ref box.Item); return 0; }
            }
            """);
        Assert.Contains("inline value field addresses", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
