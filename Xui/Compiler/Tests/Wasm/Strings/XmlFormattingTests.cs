using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class XmlFormattingTests
{
    [Fact]
    public Task XmlRoundtripFloatingPointUsesInvariantNumberInfo() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Globalization;
        using System.Xml;
        public static class Program {
            public static int Main() {
                Console.WriteLine(1.25.ToString("R", NumberFormatInfo.InvariantInfo));
                Console.WriteLine((-42).ToString(null, NumberFormatInfo.InvariantInfo));
                foreach (double value in new[] { 1.25, -0.0, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                    Console.WriteLine(XmlConvert.ToString(value));
                foreach (float value in new[] { 1.25f, -0.0f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                    Console.WriteLine(XmlConvert.ToString(value));
                return 42;
            }
        }
        """, "1.25\n-42\n1.25\n-0\nNaN\nINF\n-INF\n1.25\n-0\nNaN\nINF\n-INF\n", 42, typeof(System.Xml.XmlConvert).Assembly);
}
