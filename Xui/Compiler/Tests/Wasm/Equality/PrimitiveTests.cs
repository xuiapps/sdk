using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class PrimitiveTests
{
    [Theory]
    [InlineData("bool", "true", "false", 1)]
    [InlineData("char", "'\\uffff'", "'a'", -1)]
    [InlineData("sbyte", "sbyte.MinValue", "0", -128)]
    [InlineData("byte", "byte.MaxValue", "0", 255)]
    [InlineData("short", "short.MinValue", "0", -32768)]
    [InlineData("ushort", "ushort.MaxValue", "0", 65535)]
    [InlineData("int", "int.MinValue", "0", int.MinValue)]
    [InlineData("uint", "uint.MaxValue", "0", -1)]
    [InlineData("long", "0x1234567887654321L", "0L", unchecked((int)0x95511559))]
    [InlineData("ulong", "ulong.MaxValue", "0UL", 0)]
    [InlineData("float", "float.NaN", "0f", 0x7f800000)]
    [InlineData("float", "-0f", "1f", 0)]
    [InlineData("float", "1.5f", "1f", 0x3fc00000)]
    [InlineData("float", "float.NegativeInfinity", "1f", unchecked((int)0xff800000))]
    [InlineData("double", "double.NaN", "0d", 0x7ff00000)]
    [InlineData("double", "-0d", "1d", 0)]
    [InlineData("double", "1.5d", "1d", 0x3ff80000)]
    [InlineData("double", "double.NegativeInfinity", "1d", unchecked((int)0xfff00000))]
    [InlineData("nint", "(nint)42", "(nint)0", 42)]
    [InlineData("nuint", "(nuint)42", "(nuint)0", 42)]
    public Task DirectGenericAndComparerCallsSharePrimitiveSemantics(string type, string value, string different, int hash) =>
        InlineArrayTests.AssertLinked($$"""
        using System;
        using System.Collections.Generic;
        public static class Program {
            static int Hash<T>(T value) => value.GetHashCode();
            static bool Equal<T>(T left, T right) where T : IEquatable<T> => left.Equals(right);
            static int ViaInterface<T>(IEqualityComparer<T> comparer, T value) => comparer.GetHashCode(value);
            public static int Main() {
                {{type}} value = {{value}}, other = {{different}};
                Console.WriteLine(value.GetHashCode());
                Console.WriteLine(Hash(value));
                Console.WriteLine(EqualityComparer<{{type}}>.Default.GetHashCode(value));
                Console.WriteLine(ViaInterface(EqualityComparer<{{type}}>.Default, value));
                Console.WriteLine(value.Equals(value)); Console.WriteLine(value.Equals(other));
                Console.WriteLine(Equal(value, value)); Console.WriteLine(Equal(value, other));
                return 42;
            }
        }
        """, $"{hash}\n{hash}\n{hash}\n{hash}\nTrue\nFalse\nTrue\nFalse\n", 42);

    [Fact]
    public Task FloatingPayloadsZerosAndCallRoundingMatchCoreLib() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        using System.Runtime.InteropServices;
        [StructLayout(LayoutKind.Explicit)] public struct SingleBits {
            [FieldOffset(0)] public int Bits; [FieldOffset(0)] public float Value;
        }
        [StructLayout(LayoutKind.Explicit)] public struct DoubleBits {
            [FieldOffset(0)] public long Bits; [FieldOffset(0)] public double Value;
        }
        public static class Program {
            static int Hash<T>(T value) => value.GetHashCode();
            static bool Equal<T>(T left, T right) where T : IEquatable<T> => left.Equals(right);
            public static int Main() {
                float snan = new SingleBits { Bits = 0x7fc12345 }.Value;
                float szero = new SingleBits { Bits = int.MinValue }.Value;
                double dnan = new DoubleBits { Bits = unchecked((long)0xfff9123456789abcUL) }.Value;
                double dzero = new DoubleBits { Bits = long.MinValue }.Value;
                Console.WriteLine(Hash(snan)); Console.WriteLine(Hash(szero));
                Console.WriteLine(Hash(dnan)); Console.WriteLine(Hash(dzero));
                Console.WriteLine(Equal(snan, float.NaN)); Console.WriteLine(Equal(szero, 0f));
                Console.WriteLine(Equal(dnan, double.NaN)); Console.WriteLine(Equal(dzero, 0d));
                float large = 16777216f, small = 1f, zero = 0;
                Console.WriteLine(large.Equals(large + small));
                Console.WriteLine(EqualityComparer<float>.Default.Equals(large, large + small));
                Console.WriteLine(EqualityComparer<float>.Default.GetHashCode(-float.Epsilon / 2f) == zero.GetHashCode());
                return 42;
            }
        }
        """, "2139095040\n0\n2146435072\n0\nTrue\nTrue\nTrue\nTrue\nTrue\nTrue\nTrue\n", 42);

    [Fact]
    public Task NativeSizeAndNativeFloatHashFollowWasm32Storage() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        using System.Runtime.InteropServices;
        public static class Program {
            static int Hash<T>(T value) => value.GetHashCode();
            public static int Main() {
                nint signed = -1; nuint unsigned = uint.MaxValue;
                Console.WriteLine(Hash(signed) == signed.GetHashCode());
                Console.WriteLine(Hash(unsigned) == unsigned.GetHashCode());
                Console.WriteLine(EqualityComparer<nint>.Default.GetHashCode(signed) == Hash(signed));
                Console.WriteLine(EqualityComparer<nuint>.Default.GetHashCode(unsigned) == Hash(unsigned));
                NFloat value = (NFloat)1.5;
                Console.WriteLine(Hash(value) == value.GetHashCode());
                Console.WriteLine(EqualityComparer<NFloat>.Default.GetHashCode(value) == Hash(value));
                return 42;
            }
        }
        """, "True\nTrue\nTrue\nTrue\nTrue\nTrue\n", 42);

    [Fact]
    public Task EnumWidthsUseUnderlyingHashAndEquality() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        enum Tiny : byte { Max = 255 }
        enum Small : short { Min = -32768 }
        enum Big : long { Bits = 0x1234567887654321L }
        enum Huge : ulong { Max = ulong.MaxValue }
        public static class Program {
            static void Check<T>(T value) where T : struct {
                Console.WriteLine(value.GetHashCode());
                Console.WriteLine(EqualityComparer<T>.Default.GetHashCode(value));
                Console.WriteLine(EqualityComparer<T>.Default.Equals(value, value));
                Console.WriteLine(EqualityComparer<T>.Default.Equals(value, default));
            }
            public static int Main() { Check(Tiny.Max); Check(Small.Min); Check(Big.Bits); Check(Huge.Max); return 42; }
        }
        """, "255\n255\nTrue\nFalse\n-32768\n-32768\nTrue\nFalse\n-1789848231\n-1789848231\nTrue\nFalse\n0\n0\nTrue\nFalse\n", 42);

    [Fact]
    public Task OrdinalStringHashesAgreeAcrossCallsAndCheckNull() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public static class Program {
            static int Hash<T>(T value) => value.GetHashCode();
            static bool Equal<T>(T left, T right) where T : IEquatable<T> => left.Equals(right);
            public static int Main() {
                string first = "Ж😀\ud800", same = "Ж" + "😀\ud800";
                IEqualityComparer<string> comparer = EqualityComparer<string>.Default;
                Console.WriteLine(first.GetHashCode() == Hash(same));
                Console.WriteLine(first.GetHashCode() == comparer.GetHashCode(same));
                Console.WriteLine(first.GetHashCode() == EqualityComparer<string>.Default.GetHashCode(same));
                Console.WriteLine(Equal(first, same)); Console.WriteLine(Equal(first, "other"));
                Console.WriteLine(comparer.GetHashCode(null));
                string empty = null;
                try { Console.WriteLine(empty.GetHashCode()); } catch (NullReferenceException) { Console.WriteLine("direct null"); }
                try { Console.WriteLine(Hash(empty)); } catch (NullReferenceException) { Console.WriteLine("generic null"); }
                return 42;
            }
        }
        """, "True\nTrue\nTrue\nTrue\nFalse\n0\ndirect null\ngeneric null\n", 42);
}
