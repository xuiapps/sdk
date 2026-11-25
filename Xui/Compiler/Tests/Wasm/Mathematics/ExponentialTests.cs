using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class ExponentialTests
{
    [Fact]
    public Task ExponentialsCoverDomainsAndScalarWidths() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            Console.WriteLine(Math.Exp(0)); Console.WriteLine(Math.Exp(-0d));
            Console.WriteLine(Math.Exp(double.NegativeInfinity)); Console.WriteLine(Math.Exp(double.PositiveInfinity));
            Console.WriteLine(Math.Exp(double.NaN)); Console.WriteLine(Math.Exp(1000)); Console.WriteLine(Math.Exp(-1000));
            Console.WriteLine(double.Exp(1) > 2.71828182 && double.Exp(1) < 2.71828183);
            Console.WriteLine(MathF.Exp(1) > 2.7182815f && float.Exp(1) < 2.718282f);
            Console.WriteLine(NFloat.Exp((NFloat)1) > (NFloat)2.7182815 && NFloat.Exp((NFloat)1) < (NFloat)2.718282);
            return 42;
        } }
        """, "1\n1\n0\nInfinity\nNaN\nInfinity\n0\nTrue\nTrue\nTrue\n",42);
}
