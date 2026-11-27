using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet.Reachability;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class InterpolationTests
{
    [Theory]
    [InlineData("0.##")]
    [InlineData("0.0#")]
    [InlineData("+0.00;-0.00;0.00")]
    [InlineData("+0.00;-0.0;0.000")]
    [InlineData("G")]
    public Task FloatingInterpolationMatchesInvariantClr(string format)
    {
        double[] values = [0, -0d, .004, -.004, .005, -.005, 1.25, -1.25, 2.675, 12345.6789, 1e-8, 1e20, double.NaN, double.PositiveInfinity];
        string literal(double value) => double.IsNaN(value) ? "double.NaN" : double.IsPositiveInfinity(value) ? "double.PositiveInfinity" :
            value.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "d";
        string expected = string.Join("\n", values.Select(value =>
            $"[{value.ToString(format, System.Globalization.CultureInfo.InvariantCulture)}]|[{((float)value).ToString(format, System.Globalization.CultureInfo.InvariantCulture)}]")) + "\n";
        return AssertInvariant($$"""
            using System;
            public static class Program {
                static string Format(double value, float single) => $"[{value:{{format}}}]|[{single:{{format}}}]";
                public static int Main() {
                    double[] values=[{{string.Join(",",values.Select(literal))}}];
                    foreach(double value in values) Console.WriteLine(Format(value,(float)value));
                    return 42;
                }
            }
            """, expected, 42);
    }

    [Fact]
    public Task NativeFloatInterpolationAndDefaultFormatsUseNativeStrings() => AssertInvariant("""
        using System; using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            NFloat value=(NFloat)1.25; Console.WriteLine($"scale:{value:0.#}×");
            double d=-0d; float f=1.5f; Console.WriteLine($"[{d}]/[{f}]");
            return 42;
        } }
        """, "scale:1.3×\n[-0]/[1.5]\n",42);

    private static async Task AssertInvariant(string source, string expected, int exitCode)
    {
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            await InlineArrayTests.AssertLinked(source, expected, exitCode);
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public async Task RatingTextExecutesAndSnapshots()
    {
        const string source = """
            using System;
            public static class Program {
                public static int Main() {
                    for (int rating = 1; rating <= 5; rating++)
                        Console.WriteLine($"Rating{rating}: {rating} of 5 stars");
                    return 42;
                }
            }
            """;
        string expected = string.Concat(Enumerable.Range(1, 5).Select(i => $"Rating{i}: {i} of 5 stars\n"));
        await InlineArrayTests.AssertLinked(source, expected, 42);
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile(source));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(app, new ConsoleWriteLineRewriter());
        var result = await Node.ExecuteAsync(wasm);
        string wat = await WasmTools.ToWatAsync(wasm);
        Assert.Contains("array.copy", wat);
        Assert.Contains("i64.div_u", wat);
        Assert.Contains(emitter.Dependencies!.Nodes, node => node is Dependency.Intrinsic { Name: "Interpolation.AppendFormatted" });
        Assert.DoesNotContain(emitter.Dependencies.Methods, method => method.DeclaringType.ToString().Contains("DefaultInterpolatedStringHandler") ||
            method.DeclaringType.ToString().Contains("ArrayPool") || method.DeclaringType.ToString().Contains("Span<Char>"));
        Snapshot.AssertMatches("RatingText", $"# UTF-16 interpolation\n\n## Source\n\n```csharp\n{source}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
    }

    [Fact]
    public Task AllIntegerWidthsAndUnsignedLimits() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Print<T>(T value) { Console.WriteLine($"[{value}]"); }
            public static int Main() {
                Print((sbyte)-128); Print((byte)255); Print((short)-32768); Print((ushort)65535);
                Print(int.MinValue); Print(int.MaxValue); Print(uint.MaxValue);
                Print(long.MinValue); Print(long.MaxValue); Print(ulong.MaxValue); Print(0); Print(-1);
                return 42;
            }
        }
        """, "[-128]\n[255]\n[-32768]\n[65535]\n[-2147483648]\n[2147483647]\n[4294967295]\n[-9223372036854775808]\n[9223372036854775807]\n[18446744073709551615]\n[0]\n[-1]\n", 42);

    [Fact]
    public Task UnicodeNullBooleanAndNestedInterpolation() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static string Text(int value) => $"nested:{value}";
            public static int Main() {
                string absent = null; char letter = 'Ж'; bool yes = true, no = false;
                Console.WriteLine($"🙂 {letter} {yes}/{no} {absent} [{Text(42)}]");
                return 42;
            }
        }
        """, "🙂 Ж True/False  [nested:42]\n", 42);

    [Fact]
    public Task GrowthCopiesBufferAndCompletedStringsAreImmutable() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        public static class Program {
            static void Append(ref DefaultInterpolatedStringHandler value) { value.AppendLiteral("🙂ab"); }
            public static int Main() {
                var value = new DefaultInterpolatedStringHandler(0, 0);
                value.AppendLiteral("start"); string first = value.ToString();
                for (int i = 0; i < 300; i++) Append(ref value);
                Console.WriteLine(first); Console.WriteLine(value.ToStringAndClear());
                value.AppendFormatted(42); Console.WriteLine(value.ToStringAndClear());
                Console.WriteLine(first); return 42;
            }
        }
        """, "start\nstart" + string.Concat(Enumerable.Repeat("🙂ab", 300)) + "\n42\nstart\n", 42);

    [Fact]
    public Task CompletedStringDoesNotAliasMutableCodeUnits() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        public static class Program {
            public static int Main() {
                var value = new DefaultInterpolatedStringHandler(0, 0);
                value.AppendLiteral("prefix:"); var copy = value;
                value.AppendLiteral("before"); string first = value.ToString();
                copy.AppendLiteral("after!");
                Console.WriteLine(first); Console.WriteLine(copy.ToStringAndClear()); return 42;
            }
        }
        """, "prefix:before\nprefix:after!\n", 42);

    [Fact]
    public Task ApplicationNamesakeDoesNotBecomeAnIntrinsic() => InlineArrayTests.AssertLinked("""
        namespace System.Runtime.CompilerServices {
            public ref struct DefaultInterpolatedStringHandler {
                public int Value;
                public DefaultInterpolatedStringHandler(int literalLength, int formattedCount) { Value=literalLength+formattedCount; }
                public void AppendLiteral(string text) { Value+=1; }
            }
        }
        public static class Program {
            public static int Main() {
                var value = new System.Runtime.CompilerServices.DefaultInterpolatedStringHandler(20, 21);
                value.AppendLiteral("ordinary managed method"); return value.Value;
            }
        }
        """, "", 42);

    [Fact]
    public Task CopiesHaveIndependentCountsAndClearingDoesNotClearAnotherCopy() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        public static class Program {
            static DefaultInterpolatedStringHandler Make() { return new DefaultInterpolatedStringHandler(0, 0); }
            static void PrintCopy(DefaultInterpolatedStringHandler value) {
                value.AppendFormatted(42); Console.WriteLine(value.ToStringAndClear());
            }
            public static int Main() {
                var value = Make(); value.AppendLiteral("value:");
                PrintCopy(value); Console.WriteLine(value.ToStringAndClear());
                DefaultInterpolatedStringHandler empty = default;
                Console.WriteLine(empty.ToStringAndClear());
                return 42;
            }
        }
        """, "value:42\nvalue:\n\n", 42);

    [Fact]
    public Task ReferenceSlotsAreReleasedOnExceptionalExit() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        public static class Program {
            static void Fail(int i) {
                var value = new DefaultInterpolatedStringHandler(0, 0);
                value.AppendFormatted(i); throw new InvalidOperationException();
            }
            public static int Main() {
                for(int i=0;i<70000;i++) { try { Fail(i); } catch(InvalidOperationException) {} }
                int final = 42; Console.WriteLine($"finished:{final}"); return 42;
            }
        }
        """, "finished:42\n", 42);

    [Theory]
    [InlineData("int value=42;", "$\"{value,8:X}\"")]
    [InlineData("int value=42;", "$\"{value,8}\"")]
    public void UnsupportedFormattingFailsExplicitly(string declaration, string interpolation)
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile($"using System; public static class Program {{ public static void Main() {{ {declaration} Console.WriteLine({interpolation}); }} }}"));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var error = Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Contains("Interpolation member is not implemented", error.Message);
    }
}
