using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.Tests.Wasm.Repl;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Canvas;

[Collection(nameof(ManagedConsoleCollection))]
public class NativeFloatMathTests
{
    [Fact]
    public Task LayoutHelpersPreserveNanBoundsAndSignedZeros() => GcTests.AssertExecution("""
        using System;
        using System.Runtime.InteropServices;
        public static class Program {
            static void Print(NFloat value) => Console.WriteLine((double)value);
            public static int Main() {
                Print(NFloat.Min((NFloat)2, (NFloat)4)); Print(NFloat.Max((NFloat)2, (NFloat)4));
                Print(NFloat.Abs((NFloat)(-3))); Print(-(NFloat)2);
                Console.WriteLine(NFloat.IsNaN(NFloat.Min(NFloat.NaN, (NFloat)1)));
                Console.WriteLine(NFloat.IsNaN(NFloat.Max((NFloat)1, NFloat.NaN)));
                Console.WriteLine(NFloat.IsFinite((NFloat)float.MaxValue));
                Console.WriteLine(NFloat.IsFinite(NFloat.PositiveInfinity));
                Console.WriteLine(NFloat.IsFinite(NFloat.NaN));
                Console.WriteLine(NFloat.IsInfinity(NFloat.NegativeInfinity));
                Console.WriteLine(NFloat.IsInfinity(NFloat.NaN));
                NFloat minusZero = (NFloat)(-0.0), plusZero = (NFloat)0;
                Console.WriteLine((NFloat)1 / NFloat.Min(plusZero, minusZero) == NFloat.NegativeInfinity);
                Console.WriteLine((NFloat)1 / NFloat.Max(minusZero, plusZero) == NFloat.PositiveInfinity);
                Print(NFloat.Clamp((NFloat)(-1), (NFloat)0, (NFloat)10));
                Print(NFloat.Clamp((NFloat)11, (NFloat)0, (NFloat)10));
                Print(NFloat.Clamp((NFloat)3, NFloat.NaN, (NFloat)10));
                Print(NFloat.Clamp((NFloat)3, (NFloat)0, NFloat.NaN));
                Console.WriteLine(NFloat.IsNaN(NFloat.Clamp(NFloat.NaN, (NFloat)0, (NFloat)10)));
                Console.WriteLine((NFloat)1 / NFloat.Clamp(minusZero, plusZero, (NFloat)1) == NFloat.NegativeInfinity);
                int result = 1;
                try { Print(NFloat.Clamp((NFloat)0, (NFloat)10, (NFloat)1)); }
                catch (ArgumentException) { result = 42; }
                return result;
            }
        }
        """, "2\n4\n3\n-2\nTrue\nTrue\nTrue\nFalse\nFalse\nTrue\nFalse\nTrue\nTrue\n0\n10\nNaN\nNaN\nTrue\nFalse\n", 42);

    [Fact]
    public Task PiHasTargetFloatPrecision() => GcTests.AssertExecution("""
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() => (float)NFloat.Pi == float.Pi ? 42 : 1; }
        """, "", 42);

    [Fact]
    public async Task LimitsHaveTargetFloatPrecision()
    {
        // WASM32 limits intentionally differ from the host's 64-bit NFloat limits.
        var image = Frontend.Roslyn.Compile("""
            using System.Runtime.InteropServices;
            public static class Program { public static int Main() =>
                (float)NFloat.MaxValue == float.MaxValue && (float)NFloat.MinValue == float.MinValue && (float)NFloat.Epsilon == float.Epsilon ? 42 : 1; }
            """);
        Assert.Equal(42, (await Node.ExecuteAsync(GcTests.Emit(image))).ExitCode);
    }

    [Fact]
    public Task InitOnlyPropertiesUseTheOrdinarySetterAbi() => GcTests.AssertExecution("""
        public struct Point { public int X { get; init; } }
        public class Box { public Point Point { get; init; } }
        public static class Program { public static int Main() => new Box { Point = new Point { X = 42 } }.Point.X; }
        """, "", 42);
}
