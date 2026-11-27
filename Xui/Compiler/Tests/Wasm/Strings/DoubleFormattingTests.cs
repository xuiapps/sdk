using System.Globalization;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class DoubleFormattingTests
{
    [Fact]
    public Task SingleAndNativeFloatUseTheirStoragePrecision() => InlineArrayTests.AssertLinked("""
        using System; using System.Globalization; using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            float value=-1.25f;NFloat native=(NFloat)1.25;
            Console.WriteLine(value.ToString("+0.00;-0.00;0.00",CultureInfo.InvariantCulture));
            Console.WriteLine(native.ToString("+0.00;-0.00;0.00",CultureInfo.InvariantCulture));
            return 42;
        } }
        ""","-1.25\n+1.25\n",42);

    [Theory]
    [InlineData("0")]
    [InlineData("0.0")]
    [InlineData("0.00")]
    [InlineData("0.#")]
    [InlineData("0.##")]
    [InlineData("0.0##")]
    [InlineData("0.00000000000000000")]
    [InlineData("+0.00;-0.00;0.00")]
    [InlineData("+0.00;-0.00")]
    [InlineData("0.00;0.00;0.000")]
    [InlineData("+0.00;-0.0;0.000")]
    [InlineData("+0.##")]
    [InlineData("G")]
    [InlineData(null)]
    public Task InvariantValuesMatchClrIncludingDecimalRounding(string? format)
    {
        double[] values = [0, -0d, 0.004, -0.004, 0.005, -0.005, 0.05, -0.05, 0.5, -0.5, 1.25, 2.25, 2.675, 9.95, 9.999999999999996,
            1234.5678901234567, 999999999999999.5, 1e20, 1e-20, double.Epsilon, -double.Epsilon,
            double.MaxValue, double.MinValue, double.NaN, double.PositiveInfinity, double.NegativeInfinity];
        return AssertValues(values, format);
    }

    [Fact]
    public async Task CustomDecimalBufferMatchesAcrossBinaryExponents()
    {
        var random = new Random(1729);
        double[] values = Enumerable.Range(0, 512).Select(_ => Math.ScaleB(random.NextDouble() * 2 - 1, random.Next(-1074, 1024))).ToArray();
        await AssertValues(values, "0.0");
        await AssertValues(values, "0.00000000000000000");
    }

    private static Task AssertValues(double[] values, string? format)
    {
        string literal(double x) => double.IsNaN(x) ? "double.NaN" : double.IsPositiveInfinity(x) ? "double.PositiveInfinity" :
            double.IsNegativeInfinity(x) ? "double.NegativeInfinity" : x.ToString("R", CultureInfo.InvariantCulture) + "d";
        string source = $$"""
            using System;
            using System.Globalization;
            public static class Program {
                static string Format(double value, string format) => value.ToString(format, CultureInfo.InvariantCulture);
                public static int Main() {
                    double[] values=[{{string.Join(",", values.Select(literal))}}];
                    foreach(double value in values) Console.WriteLine(Format(value, {{(format is null ? "null" : "\"" + format + "\"")}}));
                    return 42;
                }
            }
            """;
        return InlineArrayTests.AssertLinked(source, string.Join("\n", values.Select(x => x.ToString(format, CultureInfo.InvariantCulture))) + "\n", 42);
    }

    [Fact]
    public async Task UnsupportedFormatsThrowManagedExceptions()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program { public static int Main() {
                double value=1.25;
                try { value.ToString("C", System.Globalization.CultureInfo.InvariantCulture); return 1; }
                catch(System.NotSupportedException) { return 42; }
            } }
            """);
        using var app = Canvas.CanvasTests.Read(image);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        byte[] wasm = new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app);
        Assert.Equal(42, (await Repl.Node.ExecuteAsync(wasm)).ExitCode);
    }

    [Fact]
    public Task FieldAndReferenceReceiversUseTheirCurrentValue() => InlineArrayTests.AssertLinked("""
        using System; using System.Globalization;
        public class Box { public double Value; }
        public static class Program {
            static string Format(ref double value) => value.ToString("0.0", CultureInfo.InvariantCulture);
            public static int Main() {
                var b=new Box {Value=1.25}; Console.WriteLine(Format(ref b.Value));
                b.Value=-2.25; Console.WriteLine(b.Value.ToString("0.0",CultureInfo.InvariantCulture));
                return 42;
            }
        }
        """, "1.3\n-2.3\n", 42);
}
