using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

[Collection(nameof(ManagedConsoleCollection))]
public class RoundingTests
{
    [Fact]
    public Task RoundingModesPreserveMidpointNeighborsAndRejectInvalidMode() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            Console.WriteLine(Math.Round(0.49999999999999994,MidpointRounding.AwayFromZero));
            Console.WriteLine(Math.Round(-0.49999999999999994,MidpointRounding.AwayFromZero)==0);
            Console.WriteLine(1/Math.Round(-0.25,MidpointRounding.AwayFromZero));
            Console.WriteLine(Math.Round(double.PositiveInfinity,MidpointRounding.AwayFromZero));
            Console.WriteLine(Math.Round(double.NaN,MidpointRounding.AwayFromZero));
            Console.WriteLine(Math.Round(4503599627370495d,MidpointRounding.AwayFromZero)==4503599627370495d);
            try { Math.Round(1d,(MidpointRounding)5); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
            try { Math.Round(1d,(MidpointRounding)(-1)); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
            return 42;
        } }
        """, "0\nTrue\n-Infinity\nInfinity\nNaN\nTrue\nmode\nmode\n",42);

    [Theory]
    [InlineData("Math", "double")]
    [InlineData("double", "double")]
    [InlineData("MathF", "float")]
    [InlineData("float", "float")]
    [InlineData("NFloat", "NFloat")]
    public Task RoundingPreservesSignsSpecialValuesAndTieBreaking(string math, string type) => InlineArrayTests.AssertLinked($$"""
        using System;
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            Console.WriteLine((double){{math}}.Floor(({{type}})(-1.25)));
            Console.WriteLine((double){{math}}.Ceiling(({{type}})1.25));
            Console.WriteLine((double){{math}}.Truncate(({{type}})(-1.75)));
            Console.WriteLine((double){{math}}.Round(({{type}})2.5));
            Console.WriteLine((double){{math}}.Round(({{type}})3.5));
            Console.WriteLine((double){{math}}.Round(({{type}})(-2.5)));
            Console.WriteLine(1 / (double){{math}}.Ceiling(({{type}})(-0.25)));
            Console.WriteLine(1 / (double){{math}}.Truncate(({{type}})(-0.25)));
            Console.WriteLine(1 / (double){{math}}.Round(({{type}})(-0.25)));
            Console.WriteLine((double){{math}}.Floor(({{type}})double.PositiveInfinity));
            Console.WriteLine((double){{math}}.Ceiling(({{type}})double.NegativeInfinity));
            Console.WriteLine((double){{math}}.Round(({{type}})double.NaN));
            return 42;
        } }
        """, "-2\n2\n-1\n2\n4\n-2\n-Infinity\n-Infinity\n-Infinity\nInfinity\n-Infinity\nNaN\n",42);
}
