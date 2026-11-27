using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class InitializerTests
{
    [Fact]
    public Task PrimitiveRvaInitializersCreateIndependentMutableArrays() => InlineArrayTests.AssertLinked("""
        using System;
        enum Small : byte { A=3, B=251 }
        public static class Program {
            static int[] Values() => new int[] { 12, -30, 42, int.MinValue, int.MaxValue, 27 };
            public static int Main() {
                var a=Values(); var b=Values(); a[2]=99;
                Console.WriteLine(b[2]); Console.WriteLine(a[2]); Console.WriteLine(b[3]); Console.WriteLine(b[4]);
                byte[] bytes={0,127,128,255,42}; sbyte[] signed={0,127,-128,-1,42};
                short[] shorts={0,32767,-32768,-1,42}; ushort[] ushorts={0,32767,32768,65535,42};
                uint[] uints={0,1,2147483648,4294967295,42};
                long[] longs={0,1,-1,long.MinValue,long.MaxValue}; ulong[] ulongs={0,1,ulong.MaxValue,42};
                float[] singles={0,1.5f,-2.25f,42}; double[] doubles={0,1.5,-2.25,42};
                bool[] flags={true,false,true,false,true}; char[] chars={'a','Ж','\0','\ud800','z'};
                Small[] enums={Small.A,Small.B,Small.A,Small.B,Small.A};
                Console.WriteLine((int)bytes[3]); Console.WriteLine((int)signed[2]);
                Console.WriteLine((int)shorts[2]); Console.WriteLine((int)ushorts[3]);
                Console.WriteLine(uints[3]==uint.MaxValue); Console.WriteLine(longs[3]); Console.WriteLine(ulongs[2]==ulong.MaxValue);
                Console.WriteLine(singles[2]); Console.WriteLine(doubles[1]);
                Console.WriteLine(flags[4]); Console.WriteLine((int)chars[3]); Console.WriteLine((int)enums[1]);
                return b[2];
            }
        }
        """, "42\n99\n-2147483648\n2147483647\n255\n-128\n-32768\n65535\nTrue\n-9223372036854775808\nTrue\n-2.25\n1.5\nTrue\n55296\n251\n", 42);
}
