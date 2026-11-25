using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class RemainderTests
{
    [Fact]
    public Task FloatingRemainderMatchesClrSpecialValuesAndSignedZeros() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public static class Program {
            static double Rem(double x,double y) => x%y;
            static float RemSingle(float x,float y) => x%y;
            static NFloat RemNative(NFloat x,NFloat y) => x%y;
            public static int Main() {
                Console.WriteLine(Rem(10.5,3)); Console.WriteLine(Rem(-10.5,3)); Console.WriteLine(Rem(10.5,-3));
                Console.WriteLine(1/Rem(-6,3)); Console.WriteLine(1/Rem(-0d,3));
                Console.WriteLine(Rem(3,0)); Console.WriteLine(Rem(double.PositiveInfinity,3)); Console.WriteLine(Rem(double.NaN,3));
                Console.WriteLine(Rem(3,double.PositiveInfinity)); Console.WriteLine(Rem(1e300,1e-300)>0);
                Console.WriteLine(RemSingle(10.5f,3)); Console.WriteLine((double)RemNative((NFloat)370,(NFloat)360));
                return 42;
            }
        }
        """, "1.5\n-1.5\n1.5\n-Infinity\n-Infinity\nNaN\nNaN\nNaN\n3\nTrue\n1.5\n10\n", 42);
}
