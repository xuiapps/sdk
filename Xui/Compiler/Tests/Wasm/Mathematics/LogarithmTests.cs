using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class LogarithmTests
{
    [Fact]
    public Task LogarithmsCoverDomainsAndScalarWidths() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            Console.WriteLine(Math.Log(1)); Console.WriteLine(Math.Log(0)); Console.WriteLine(Math.Log(-0d));
            Console.WriteLine(Math.Log(-1)); Console.WriteLine(Math.Log(double.NaN)); Console.WriteLine(Math.Log(double.PositiveInfinity));
            Console.WriteLine(double.Log(2) > .69314718 && double.Log(2) < .69314719);
            Console.WriteLine(MathF.Log(2) > .6931471f && float.Log(2) < .6931473f);
            Console.WriteLine(NFloat.Log((NFloat)2) > (NFloat).6931471 && NFloat.Log((NFloat)2) < (NFloat).6931473);
            return 42;
        } }
        """, "0\n-Infinity\n-Infinity\nNaN\nNaN\nInfinity\nTrue\nTrue\nTrue\n",42);
}
