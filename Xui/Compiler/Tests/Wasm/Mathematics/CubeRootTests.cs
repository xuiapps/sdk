using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class CubeRootTests
{
    [Fact]
    public Task CubeRootsPreserveSignsDomainsAndScalarWidths() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            Console.WriteLine(Math.Cbrt(27)); Console.WriteLine(Math.Cbrt(-8));
            Console.WriteLine(1 / Math.Cbrt(-0d)); Console.WriteLine(1 / Math.Cbrt(0d));
            Console.WriteLine(Math.Cbrt(double.NegativeInfinity)); Console.WriteLine(Math.Cbrt(double.PositiveInfinity));
            Console.WriteLine(double.IsNaN(Math.Cbrt(double.NaN)));
            double root = double.Cbrt(2);
            Console.WriteLine(root > 1.25992104989487 && root < 1.25992104989488);
            Console.WriteLine(MathF.Cbrt(-27)); Console.WriteLine(float.Cbrt(64));
            Console.WriteLine(NFloat.Cbrt((NFloat)(-125)) == (NFloat)(-5));
            return 42;
        } }
        """, "3\n-2\n-Infinity\nInfinity\n-Infinity\nInfinity\nTrue\nTrue\n-3\n4\nTrue\n",42);
}
