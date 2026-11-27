using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class BitConversionTests
{
    [Fact]
    public Task FiniteSubnormalInfinityAndSignedZeroBitsRoundTrip() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            uint[] single={0,0x80000000,1,0x007fffff,0x00800000,0x3f800000,0x7f7fffff,0x7f800000,0xff800000};
            foreach(uint bits in single) {
                float value=BitConverter.UInt32BitsToSingle(bits);
                if(BitConverter.SingleToUInt32Bits(value)!=bits)return 1;
                if(BitConverter.SingleToInt32Bits(BitConverter.Int32BitsToSingle(unchecked((int)bits)))!=unchecked((int)bits))return 2;
            }
            ulong[] doubles={0,0x8000000000000000,1,0x000fffffffffffff,0x0010000000000000,0x3ff0000000000000,0x7fefffffffffffff,0x7ff0000000000000,0xfff0000000000000};
            foreach(ulong bits in doubles) {
                double value=BitConverter.UInt64BitsToDouble(bits);
                if(BitConverter.DoubleToUInt64Bits(value)!=bits)return 3;
                if(BitConverter.DoubleToInt64Bits(BitConverter.Int64BitsToDouble(unchecked((long)bits)))!=unchecked((long)bits))return 4;
            }
            if(!float.IsNaN(BitConverter.Int32BitsToSingle(0x7fc00000)))return 5;
            if(!double.IsNaN(BitConverter.Int64BitsToDouble(0x7ff8000000000000)))return 6;
            return 42;
        } }
        """, "",42);
}
