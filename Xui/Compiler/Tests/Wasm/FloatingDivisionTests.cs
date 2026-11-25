using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

[Collection(nameof(ManagedConsoleCollection))]
public class FloatingDivisionTests
{
    [Theory]
    [InlineData("42.9", 42)]
    [InlineData("-42.9", -42)]
    [InlineData("2147483648.0", int.MaxValue)]
    [InlineData("-2147483649.0", int.MinValue)]
    [InlineData("double.NaN", 0)]
    [InlineData("double.PositiveInfinity", int.MaxValue)]
    [InlineData("double.NegativeInfinity", int.MinValue)]
    public Task ConversionToInt32TruncatesAndSaturates(string value, int expected) => GcTests.AssertExecution($$"""
        public static class Program {
            static int Convert(double value) => (int)value;
            public static int Main() => Convert({{value}});
        }
        """, "", expected);

    [Fact]
    public Task DivisionKeepsIeeeZeroAndNanSemantics() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static double Divide(double a, double b) => a / b;
            public static int Main() {
                Console.WriteLine(Divide(10, 4));
                Console.WriteLine(Divide(1, 0) == double.PositiveInfinity);
                Console.WriteLine(Divide(1, -0.0) == double.NegativeInfinity);
                Console.WriteLine(Divide(double.NaN, 2) != 0);
                Console.WriteLine(Divide(0, 0) != 0);
                return 42;
            }
        }
        """, "2.5\nTrue\nTrue\nTrue\nTrue\n", 42);
}
