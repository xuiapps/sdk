using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class SignTests
{
    [Theory]
    [InlineData("Math", "double")]
    [InlineData("double", "double")]
    [InlineData("MathF", "float")]
    [InlineData("float", "float")]
    [InlineData("NFloat", "NFloat")]
    public Task SignOperationsPreserveZerosAndMagnitudes(string math, string type) => InlineArrayTests.AssertLinked($$"""
        using System;
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            Console.WriteLine((double){{math}}.Abs(({{type}})(-42)));
            Console.WriteLine(1 / (double){{math}}.Abs(({{type}})(-0d)));
            Console.WriteLine((double){{math}}.Abs(({{type}})double.NegativeInfinity));
            Console.WriteLine((double){{math}}.Abs(({{type}})double.NaN));
            Console.WriteLine((double){{math}}.CopySign(({{type}})42,({{type}})(-1)));
            Console.WriteLine((double){{math}}.CopySign(({{type}})(-42),({{type}})0));
            Console.WriteLine(1 / (double){{math}}.CopySign(({{type}})0,({{type}})(-0d)));
            return 42;
        } }
        """, "42\nInfinity\nInfinity\nNaN\n-42\n42\n-Infinity\n",42);

    [Fact]
    public Task BclRoundingModesUseWasmScalarHelpers() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            Console.WriteLine(Math.Round(2.5,MidpointRounding.AwayFromZero));
            Console.WriteLine(Math.Round(-2.5,MidpointRounding.AwayFromZero));
            Console.WriteLine(Math.Round(2.5,MidpointRounding.ToEven));
            Console.WriteLine(Math.Round(-2.5,MidpointRounding.ToZero));
            Console.WriteLine(Math.Round(-2.5,MidpointRounding.ToNegativeInfinity));
            Console.WriteLine(Math.Round(-2.5,MidpointRounding.ToPositiveInfinity));
            return 42;
        } }
        """, "3\n-3\n2\n-2\n-3\n-2\n",42);
}
