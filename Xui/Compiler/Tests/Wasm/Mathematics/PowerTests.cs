using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class PowerTests
{
    [Fact]
    public Task PowerPreservesFrameworkIdentitiesAndSpecialValues() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            Console.WriteLine(Math.Pow(2,3)); Console.WriteLine(Math.Pow(4,-.5));
            Console.WriteLine(Math.Pow(-2,3)); Console.WriteLine(Math.Pow(-2,.5));
            Console.WriteLine(Math.Pow(double.NaN,0)); Console.WriteLine(Math.Pow(1,double.NaN));
            Console.WriteLine(Math.Pow(1,double.PositiveInfinity)); Console.WriteLine(Math.Pow(-1,double.NegativeInfinity));
            Console.WriteLine(Math.Pow(0,-1)); Console.WriteLine(Math.Pow(-0d,-3));
            Console.WriteLine(Math.Pow(double.PositiveInfinity,-1)); Console.WriteLine(Math.Pow(double.NaN,1));
            return 42;
        } }
        """, "8\n0.5\n-8\nNaN\n1\n1\n1\n1\nInfinity\n-Infinity\n0\nNaN\n", 42);

    [Fact]
    public Task FloatAndNativeFloatPowerRetainTargetRounding() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            Console.WriteLine(MathF.Pow(2,3)); Console.WriteLine(float.Pow(4,.5f));
            Console.WriteLine(double.Pow(2,-3)); Console.WriteLine((double)NFloat.Pow((NFloat)2,(NFloat)3));
            Console.WriteLine(MathF.Pow(1,float.NaN)); Console.WriteLine(MathF.Pow(-1,float.PositiveInfinity));
            Console.WriteLine(MathF.Pow(1.1f,3) == (float)Math.Pow(1.1f,3));
            return 42;
        } }
        """, "8\n2\n0.125\n8\n1\n1\nTrue\n", 42);
}
