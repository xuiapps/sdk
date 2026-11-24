using System.Globalization;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Time;

[Collection(nameof(ManagedConsoleCollection))]
public class TimeSpanTests
{
    [Theory]
    [InlineData("FromDays", 864_000_000_000L)]
    [InlineData("FromHours", 36_000_000_000L)]
    [InlineData("FromMinutes", 600_000_000L)]
    [InlineData("FromSeconds", 10_000_000L)]
    [InlineData("FromMilliseconds", 10_000L)]
    [InlineData("FromMicroseconds", 10L)]
    public Task FloatingFactoriesMatchClrAtRoundingAndOverflowBoundaries(string factory, long scale)
    {
        double max = (double)long.MaxValue / scale, min = (double)long.MinValue / scale;
        double[] inputs = [0, -0d, 1d / 60, 2.5, -2.5, 0.9 / scale, -1.9 / scale,
            Math.BitDecrement(max), max, Math.BitIncrement(max), Math.BitIncrement(min), min, Math.BitDecrement(min),
            double.NaN, double.PositiveInfinity, double.NegativeInfinity];
        var convert = typeof(TimeSpan).GetMethod(factory, [typeof(double)])!.CreateDelegate<Func<double, TimeSpan>>();
        string expected = string.Concat(inputs.Select(value => Outcome(() => convert(value))));
        string calls = string.Join("\n", inputs.Select(value => $"Print({Literal(value)});"));
        string source = $$"""
            using System;
            public static class Program {
                static void Print(double value) {
                    try { Console.WriteLine(TimeSpan.{{factory}}(value).Ticks); }
                    catch (ArgumentException error) { Console.WriteLine("argument"); Console.WriteLine(error.Message); }
                    catch (OverflowException error) { Console.WriteLine("overflow"); Console.WriteLine(error.Message); }
                }
                public static int Main() { {{calls}} return 42; }
            }
            """;
        return InlineArrayTests.AssertLinked(source, expected, 42);
    }

    [Theory]
    [InlineData("FromDays", 864_000_000_000L, true)]
    [InlineData("FromHours", 36_000_000_000L, true)]
    [InlineData("FromMinutes", 600_000_000L, false)]
    [InlineData("FromSeconds", 10_000_000L, false)]
    [InlineData("FromMilliseconds", 10_000L, false)]
    [InlineData("FromMicroseconds", 10L, false)]
    public Task IntegralFactoriesUseExactTicksAndArgumentOutOfRange(string factory, long scale, bool int32)
    {
        long max = Math.Min(long.MaxValue / scale, int32 ? int.MaxValue : long.MaxValue);
        long min = Math.Max(long.MinValue / scale, int32 ? int.MinValue : long.MinValue);
        long[] inputs = [0, 1, -1, min, max, min - 1, max + 1];
        string expected = string.Concat(inputs.Select(value => Outcome(() => (TimeSpan)typeof(TimeSpan).GetMethod(factory,
            [int32 ? typeof(int) : typeof(long)])!.Invoke(null, [int32 ? (object)(int)value : value])!)));
        string calls = string.Join("\n", inputs.Select(value => $"Print({value.ToString(CultureInfo.InvariantCulture)}{(int32 ? "" : "L")});"));
        string source = $$"""
            using System;
            public static class Program {
                static void Print({{(int32 ? "int" : "long")}} value) {
                    try { Console.WriteLine(TimeSpan.{{factory}}(value).Ticks); }
                    catch (ArgumentException error) { Console.WriteLine("argument"); Console.WriteLine(error.Message); Console.WriteLine(error is ArgumentOutOfRangeException); Console.WriteLine(error.ParamName); }
                }
                public static int Main() { {{calls}} return 42; }
            }
            """;
        // This helper prints the hierarchy check for integral range failures.
        expected = expected.Replace("TimeSpan overflowed because the duration is too long.\n", "TimeSpan overflowed because the duration is too long.\nTrue\n\n");
        return InlineArrayTests.AssertLinked(source, expected, 42);
    }

    [Fact]
    public Task FactoryResultsHaveIndependentFrameSlotsWithoutCallerStructLocals() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static int Sum(TimeSpan first, TimeSpan second) => (int)(first.Ticks + second.Ticks);
            public static int Main() => Sum(TimeSpan.FromSeconds(0.0000012d), TimeSpan.FromMicroseconds(3d));
        }
        """, "", 42);

    [Fact]
    public void FactoriesDoNotRetainResourceManagerOrFactoryBodies()
    {
        byte[] pe = Frontend.Roslyn.Compile("""
            public static class Program {
                public static int Main() => (int)System.TimeSpan.FromSeconds(0.125d).Ticks;
            }
            """);
        using var app = Xui.Compiler.Tests.Wasm.Canvas.CanvasTests.Read(pe);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var graph = Xui.Compiler.XuiNet.Reachability.Analyzer.Analyze([Xui.Compiler.XuiNet.Reachability.Root.EntryPoint(app)]);
        Assert.Contains(graph.Nodes, node => node is Xui.Compiler.XuiNet.Reachability.Dependency.Intrinsic { Name: "TimeSpan.FromSeconds" });
        Assert.DoesNotContain(graph.Methods, method => method.DeclaringType.ToString().StartsWith("System.Resources.", StringComparison.Ordinal) ||
            method.DeclaringType.ToString() == "System.SR" || method.DeclaringType.Module.GetMethod(method.Handle).Name == "FromSeconds");
    }

    private static string Outcome(Func<TimeSpan> convert)
    {
        try { return convert().Ticks.ToString(CultureInfo.InvariantCulture) + "\n"; }
        catch (System.Reflection.TargetInvocationException error) when (error.InnerException is ArgumentException argument)
        { return "argument\n" + argument.Message + "\n"; }
        catch (ArgumentException error) { return "argument\n" + error.Message + "\n"; }
        catch (OverflowException error) { return "overflow\n" + error.Message + "\n"; }
    }

    private static string Literal(double value) => double.IsNaN(value) ? "double.NaN" :
        double.IsPositiveInfinity(value) ? "double.PositiveInfinity" : double.IsNegativeInfinity(value) ? "double.NegativeInfinity" :
        value.ToString("R", CultureInfo.InvariantCulture) + "d";
}
