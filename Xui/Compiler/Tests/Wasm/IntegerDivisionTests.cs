using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

[Collection(nameof(ManagedConsoleCollection))]
public class IntegerDivisionTests
{
    [Theory]
    [InlineData("int", "-43", "5", "-8", "-3")]
    [InlineData("int", "43", "-5", "-8", "3")]
    [InlineData("long", "long.MinValue", "3", "-3074457345618258602", "-2")]
    [InlineData("uint", "uint.MaxValue", "2", "2147483647", "1")]
    [InlineData("ulong", "ulong.MaxValue", "2", "9223372036854775807", "1")]
    [InlineData("nint", "-43", "5", "-8", "-3")]
    public Task QuotientAndRemainder(string type, string left, string right, string quotient, string remainder) => GcTests.AssertExecution($$"""
        public static class Program {
            static {{type}} Divide({{type}} a, {{type}} b) => a / b;
            static {{type}} Remainder({{type}} a, {{type}} b) => a % b;
            public static int Main() {
                System.Console.WriteLine((long)Divide({{left}}, {{right}}));
                System.Console.WriteLine((long)Remainder({{left}}, {{right}}));
                return 42;
            }
        }
        """, quotient + "\n" + remainder + "\n", 42);

    [Theory]
    [InlineData("int", "/")]
    [InlineData("int", "%")]
    [InlineData("long", "/")]
    [InlineData("long", "%")]
    [InlineData("uint", "/")]
    [InlineData("uint", "%")]
    [InlineData("ulong", "/")]
    [InlineData("ulong", "%")]
    public Task ZeroDivisorIsAManagedException(string type, string operation) => GcTests.AssertExecution($$"""
        using System;
        public static class Program {
            static {{type}} Calculate({{type}} a, {{type}} b) => a {{operation}} b;
            public static int Main() {
                int result = 0;
                try { Calculate(42, 0); }
                catch (DivideByZeroException e) { Console.WriteLine(e.Message); result = 21; }
                try { Calculate(42, 0); }
                catch (ArithmeticException) { result += 21; }
                return result;
            }
        }
        """, "Attempted to divide by zero.\n", 42);

    [Theory]
    [InlineData("int", "/")]
    [InlineData("int", "%")]
    [InlineData("long", "/")]
    [InlineData("long", "%")]
    public Task SignedMinimumAndNegativeOneOverflow(string type, string operation) => GcTests.AssertExecution($$"""
        using System;
        public static class Program {
            static {{type}} Calculate({{type}} a, {{type}} b) => a {{operation}} b;
            public static int Main() {
                int result = 0;
                try { Calculate({{type}}.MinValue, -1); }
                catch (OverflowException) { result = 21; }
                try { Calculate({{type}}.MinValue, -1); }
                catch (ArithmeticException) { result += 21; }
                return result;
            }
        }
        """, "", 42);
}
