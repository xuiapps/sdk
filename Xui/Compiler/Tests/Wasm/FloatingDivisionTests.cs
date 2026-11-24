using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

[Collection(nameof(ManagedConsoleCollection))]
public class FloatingDivisionTests
{
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
