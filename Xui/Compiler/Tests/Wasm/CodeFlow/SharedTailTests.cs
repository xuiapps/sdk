using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

[Collection(nameof(ManagedConsoleCollection))]
public class SharedTailTests
{
    [Fact]
    public Task ShortCircuitBranchesShareReturnAndValueTails() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static bool All(int a, int b, int c) => a > 0 && b > 0 && c > 0;
            static int Value(int a, int b, int c) {
                bool all = a > 0 && b > 0 && c > 0;
                bool any = a > 0 || b > 0 || c > 0;
                return (all ? 10 : 0) + (any ? 1 : 0);
            }
            public static int Main() {
                for (int i = 0; i < 8; i++) {
                    Console.WriteLine(All(i & 1, i & 2, i & 4));
                    Console.WriteLine(Value(i & 1, i & 2, i & 4));
                }
                return 42;
            }
        }
        """, "False\n0\nFalse\n1\nFalse\n1\nFalse\n1\nFalse\n1\nFalse\n1\nFalse\n1\nTrue\n11\n", 42);
    [Fact]
    public Task EarlyReturnGuardsRejoinTheOuterFallthrough() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static int Guard(int a, int b, int c) {
                if (a > 0 && b > 0) return 10;
                int result = 1;
                if (b > 0 || c > 0) result += 2;
                return result;
            }
            public static int Main() {
                for (int i = 0; i < 8; i++) Console.WriteLine(Guard(i & 1, i & 2, i & 4));
                return 42;
            }
        }
        """, "1\n1\n3\n10\n3\n3\n3\n10\n", 42);

    [Fact]
    public Task LinearStructValuesCrossConditionalResults() => GcTests.AssertExecution("""
        public struct Size { public int X; public int Y; }
        public static class Program {
            static Size Choose(bool condition) => condition ? new Size { X = 10, Y = 11 } : new Size { X = 20, Y = 1 };
            public static int Main() {
                Size a = Choose(true), b = Choose(false);
                return a.X + a.Y + b.X + b.Y;
            }
        }
        """, "", 42);
    [Fact]
    public Task GuardsInBothArmsKeepTheEnclosingContinuation() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static int Guard(bool select, int value) {
                if (select) { if (value < 0 || value > 2) return -1; value += 1; }
                else if (value == 0) return -2;
                return value > 1 ? value * 10 : value + 1;
            }
            public static int Main() {
                for (int i = -1; i <= 3; i++) Console.WriteLine(Guard(true, i));
                for (int i = -1; i <= 3; i++) Console.WriteLine(Guard(false, i));
                return 42;
            }
        }
        """, "-1\n2\n20\n30\n-1\n0\n-2\n2\n20\n30\n", 42);
}
