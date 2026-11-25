using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class TrigonometryTests
{
    [Fact]
    public Task SineAndCosineCoverSpecialValuesAndScalarWidths() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            Console.WriteLine(Math.Cos(0)); Console.WriteLine(Math.Sin(0));
            Console.WriteLine(1 / Math.Sin(-0d));
            Console.WriteLine(Math.Cos(double.NaN)); Console.WriteLine(Math.Sin(double.PositiveInfinity));
            Console.WriteLine(Math.Cos(double.NegativeInfinity));
            Console.WriteLine(Math.Sin(1) > .84147098 && Math.Sin(1) < .84147099);
            Console.WriteLine(double.Cos(1) > .54030230 && double.Cos(1) < .54030231);
            Console.WriteLine(MathF.Sin(1) > .8414709f && float.Cos(1) < .5403024f);
            Console.WriteLine(NFloat.Sin((NFloat)1) > (NFloat).8414709 && NFloat.Cos((NFloat)1) < (NFloat).5403024);
            return 42;
        } }
        """, "1\n0\n-Infinity\nNaN\nNaN\nNaN\nTrue\nTrue\nTrue\nTrue\n", 42);
}
