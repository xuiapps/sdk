using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class IntegerFormattingTests
{
    [Theory]
    [InlineData("sbyte", "sbyte.MinValue", "sbyte.MaxValue", "-128", "127")]
    [InlineData("byte", "byte.MinValue", "byte.MaxValue", "0", "255")]
    [InlineData("short", "short.MinValue", "short.MaxValue", "-32768", "32767")]
    [InlineData("ushort", "ushort.MinValue", "ushort.MaxValue", "0", "65535")]
    [InlineData("int", "int.MinValue", "int.MaxValue", "-2147483648", "2147483647")]
    [InlineData("uint", "uint.MinValue", "uint.MaxValue", "0", "4294967295")]
    [InlineData("long", "long.MinValue", "long.MaxValue", "-9223372036854775808", "9223372036854775807")]
    [InlineData("ulong", "ulong.MinValue", "ulong.MaxValue", "0", "18446744073709551615")]
    public Task InvariantFormattingPreservesEveryIntegerWidth(string type, string min, string max, string low, string high) =>
        InlineArrayTests.AssertLinked($$"""
            using System;
            using System.Globalization;
            using System.Xml;
            public class Owner { public {{type}} Value; }
            public static class Program {
                public static int Main() {
                    var values = new {{type}}[] { {{min}}, {{max}} };
                    var owner = new Owner();
                    for (int i = 0; i < values.Length; i++) {
                        {{type}} local = values[i]; owner.Value = local;
                        Console.WriteLine(local.ToString(CultureInfo.InvariantCulture));
                        Console.WriteLine(owner.Value.ToString(null, NumberFormatInfo.InvariantInfo));
                        Console.WriteLine(values[i].ToString(CultureInfo.InvariantCulture));
                        Console.WriteLine(XmlConvert.ToString(local));
                    }
                    return 42;
                }
            }
            """, string.Concat(Enumerable.Repeat(low + "\n", 4)) + string.Concat(Enumerable.Repeat(high + "\n", 4)), 42, typeof(System.Xml.XmlConvert).Assembly);

    [Fact]
    public void NonNullIntegerFormatsAreNotDiscardedByTheNullFormatSpecialization()
    {
        using var app = Xui.Compiler.Tests.Wasm.Canvas.CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System.Globalization;
            public static class Program {
                static string Format(int value, bool hex) => value.ToString(hex ? "X" : null, CultureInfo.InvariantCulture);
                public static int Main() => Format(42, true).Length;
            }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Throws<NotSupportedException>(() => new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app));
    }

    [Fact]
    public Task XmlIntegerConversionUsesNullFormatAndInvariantProvider() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Globalization;
        using System.Xml;
        public static class Program {
            public static int Main() {
                foreach (int value in new[] { int.MinValue, -1, 0, 1, int.MaxValue }) {
                    Console.WriteLine(value.ToString(null, CultureInfo.InvariantCulture));
                    Console.WriteLine(XmlConvert.ToString(value));
                }
                return 42;
            }
        }
        """, "-2147483648\n-2147483648\n-1\n-1\n0\n0\n1\n1\n2147483647\n2147483647\n", 42, typeof(System.Xml.XmlConvert).Assembly);

    [Fact]
    public Task DecimalFormattingReadsCurrentLocalArgumentAndFieldValues() => InlineArrayTests.AssertLinked("""
        using System;
        public class Box { public int Value; }
        public struct Point { public int X; }
        public static class Program {
            static string Format(int value) { string first=value.ToString(); value=-42; return value.ToString(); }
            public static int Main() {
                int a=int.MinValue; Console.WriteLine(a.ToString()); a=int.MaxValue; Console.WriteLine(a.ToString());
                Console.WriteLine(0.ToString()); Console.WriteLine(Format(7));
                Console.WriteLine(a.ToString(System.Globalization.CultureInfo.InvariantCulture));
                var box=new Box { Value=123 }; Console.WriteLine(box.Value.ToString());
                var point=new Point { X=-456 }; Console.WriteLine(point.X.ToString());
                return 42;
            }
        }
        """, "-2147483648\n2147483647\n0\n-42\n2147483647\n123\n-456\n", 42);
}
