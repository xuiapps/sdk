using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class IntegerFormattingTests
{
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
