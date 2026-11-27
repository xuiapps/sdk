using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class IntegerInterpolationTests
{
    [Fact]
    public Task DecimalAndHexFormatsPreserveAllIntegerWidths() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static string Format<T>(T value) => $"{value:X2}|{value:x}|{value:D4}|{value:G}";
            public static int Main() {
                Console.WriteLine(Format((sbyte)-1)); Console.WriteLine(Format((byte)255));
                Console.WriteLine(Format((short)-1)); Console.WriteLine(Format((ushort)65535));
                Console.WriteLine(Format(-1)); Console.WriteLine(Format(uint.MaxValue));
                Console.WriteLine(Format(long.MinValue)); Console.WriteLine(Format(ulong.MaxValue));
                Console.WriteLine($"{0:X2}:{42:D5}");
                try { Console.WriteLine($"{42:Q}"); } catch(FormatException) { Console.WriteLine("format"); }
                return 42;
            }
        }
        """, "FF|ff|-0001|-1\nFF|ff|0255|255\nFFFF|ffff|-0001|-1\nFFFF|ffff|65535|65535\nFFFFFFFF|ffffffff|-0001|-1\nFFFFFFFF|ffffffff|4294967295|4294967295\n8000000000000000|8000000000000000|-9223372036854775808|-9223372036854775808\nFFFFFFFFFFFFFFFF|ffffffffffffffff|18446744073709551615|18446744073709551615\n00:00042\nformat\n",42);
}
