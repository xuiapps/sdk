using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Canvas;

[Collection(nameof(ManagedConsoleCollection))]
public class TimeSpanTests
{
    [Fact]
    public Task FrameworkZeroIsAnIndependentStructValue() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Change(ref TimeSpan value) { value = new TimeSpan(42); }
            public static int Main() {
                TimeSpan value = TimeSpan.Zero;
                TimeSpan copy = value;
                Change(ref value);
                Console.WriteLine(value.Ticks);
                Console.WriteLine(copy.Ticks);
                TimeSpan fresh = TimeSpan.Zero;
                Console.WriteLine(fresh.Ticks);
                return 42;
            }
        }
        """, "42\n0\n0\n", 42);
}
