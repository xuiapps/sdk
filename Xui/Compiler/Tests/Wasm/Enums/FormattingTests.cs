using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Enums;

[Collection(nameof(ManagedConsoleCollection))]
public class FormattingTests
{
    [Fact]
    public Task NamesFlagsAndNumericFallbackPreserveUnderlyingWidths() => InlineArrayTests.AssertLinked("""
        using System;
        enum Tiny : sbyte { Minus=-1, Zero=0, One=1 }
        enum Wide : ulong { High=0x8000000000000000, All=ulong.MaxValue }
        [Flags] enum Bits : byte { None=0, Read=1, Write=2, Both=3, Execute=4 }
        [Flags] enum Signed : long { Low=1, High=long.MinValue }
        public static class Program {
            static string Text<T>(T value) where T : struct, Enum => value.ToString();
            static string Interpolate<T>(T value) => $"[{value}]";
            public static int Main() {
                Console.WriteLine(Text(Tiny.Minus)); Console.WriteLine(Text((Tiny)(-2)));
                Console.WriteLine(Text(Wide.High)); Console.WriteLine(Text((Wide)(ulong.MaxValue-1)));
                Console.WriteLine(Text(Bits.None)); Console.WriteLine(Text(Bits.Both));
                Console.WriteLine(Text(Bits.Read|Bits.Execute)); Console.WriteLine(Text((Bits)255));
                Console.WriteLine(Text(Signed.High|Signed.Low)); Console.WriteLine(Text((Signed)(-2)));
                Console.WriteLine(Interpolate(Bits.Both|Bits.Execute)); Console.WriteLine(Interpolate(Wide.All));
                Tiny local=Tiny.One; Console.WriteLine(local.ToString());
                return 42;
            }
        }
        """, "Minus\n-2\nHigh\n18446744073709551614\nNone\nBoth\nRead, Execute\n255\nLow, High\n-2\n[Both, Execute]\n[All]\nOne\n",42);
}
