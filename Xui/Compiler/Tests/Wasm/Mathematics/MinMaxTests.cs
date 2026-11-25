using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class MinMaxTests
{
    [Theory]
    [InlineData("Math", "double")]
    [InlineData("double", "double")]
    [InlineData("MathF", "float")]
    [InlineData("float", "float")]
    [InlineData("NFloat", "NFloat")]
    public Task MinMaxPropagateNanAndChooseSignedZero(string math, string type) => InlineArrayTests.AssertLinked($$"""
        using System;
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            Console.WriteLine((double){{math}}.Min(({{type}})(-42),({{type}})0));
            Console.WriteLine((double){{math}}.Max(({{type}})(-42),({{type}})0));
            Console.WriteLine((double){{math}}.Min(({{type}})1,({{type}})double.NaN));
            Console.WriteLine((double){{math}}.Min(({{type}})double.NaN,({{type}})1));
            Console.WriteLine((double){{math}}.Max(({{type}})1,({{type}})double.NaN));
            Console.WriteLine((double){{math}}.Max(({{type}})double.NaN,({{type}})1));
            Console.WriteLine(1 / (double){{math}}.Min(({{type}})0,({{type}})(-0d)));
            Console.WriteLine(1 / (double){{math}}.Min(({{type}})(-0d),({{type}})0));
            Console.WriteLine(1 / (double){{math}}.Max(({{type}})0,({{type}})(-0d)));
            Console.WriteLine(1 / (double){{math}}.Max(({{type}})(-0d),({{type}})0));
            return 42;
        } }
        """, "-42\n0\nNaN\nNaN\nNaN\nNaN\n-Infinity\n-Infinity\nInfinity\nInfinity\n",42);
}
