using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class OrderingTests
{
    [Fact]
    public Task PrimitiveOrderingPreservesSignednessAndNaNRules() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static int Compare<T>(T a,T b) where T:IComparable<T> => a.CompareTo(b);
            public static int Main() {
                Console.WriteLine(Compare(uint.MaxValue,1u)); Console.WriteLine(Compare(1UL,ulong.MaxValue));
                Console.WriteLine(Compare(int.MinValue,int.MaxValue)); Console.WriteLine(Compare(long.MinValue,long.MaxValue));
                Console.WriteLine(Compare((byte)1,(byte)255)); Console.WriteLine(Compare('z','a'));
                Console.WriteLine(Compare(false,true)); Console.WriteLine(Compare((ushort)65535,(ushort)0));
                Console.WriteLine(Compare((sbyte)-128,(sbyte)127)); Console.WriteLine(Compare((short)-32768,(short)32767));
                Console.WriteLine(Compare(double.NaN,1d)); Console.WriteLine(Compare(1d,double.NaN));
                Console.WriteLine(Compare(double.NaN,double.NaN)); Console.WriteLine(Compare(-0d,0d));
                Console.WriteLine(Compare(float.PositiveInfinity,float.NegativeInfinity));
                Console.WriteLine(Compare(float.NaN,0f));
                uint direct=uint.MaxValue; Console.WriteLine(direct.CompareTo(1u));
                return 42;
            }
        }
        """, "1\n-1\n-1\n-1\n-254\n25\n-1\n65535\n-255\n-65535\n-1\n1\n0\n0\n1\n-1\n1\n",42);
}
