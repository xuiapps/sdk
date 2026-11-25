using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Canvas;

[Collection(nameof(Xui.Compiler.Tests.Wasm.Structs.ManagedConsoleCollection))]
public class NativeFloatConversionTests
{
    [Fact]
    public Task NativeFloatSignedConversionsSaturateLikeClr() => Xui.Compiler.Tests.Wasm.InlineArrays.InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public static class Program {
            static int Int(NFloat value) => (int)value;
            static long Long(NFloat value) => (long)value;
            public static int Main() {
                Console.WriteLine(Int((NFloat)3.75)); Console.WriteLine(Int((NFloat)(-3.75)));
                Console.WriteLine(Int(NFloat.NaN)); Console.WriteLine(Int(NFloat.PositiveInfinity)); Console.WriteLine(Int(NFloat.NegativeInfinity));
                Console.WriteLine(Long((NFloat)3.75)); Console.WriteLine(Long(NFloat.NaN));
                Console.WriteLine(Long(NFloat.PositiveInfinity)); Console.WriteLine(Long(NFloat.NegativeInfinity));
                return 42;
            }
        }
        """, "3\n-3\n0\n2147483647\n-2147483648\n3\n0\n9223372036854775807\n-9223372036854775808\n", 42);

    [Theory]
    [InlineData("byte", "255", "255f")]
    [InlineData("sbyte", "-128", "-128f")]
    [InlineData("short", "-32768", "-32768f")]
    [InlineData("ushort", "65535", "65535f")]
    [InlineData("char", "'\\uffff'", "65535f")]
    [InlineData("int", "int.MinValue", "-2147483648f")]
    [InlineData("uint", "uint.MaxValue", "4294967296f")]
    [InlineData("nint", "-42", "-42f")]
    [InlineData("nuint", "42", "42f")]
    [InlineData("long", "-9223371212221054975L", "-9223370937343148032f")]
    [InlineData("ulong", "9223372586610589697UL", "9223373136366403584f")]
    public async Task IntegralConversionsUseTargetWidthAndSingleRounding(string type, string value, string expected)
    {
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            using var app = CanvasTests.Read(Frontend.Roslyn.Compile($$"""
                using System.Runtime.InteropServices;
                public static class Program {
                    static NFloat Convert({{type}} value) => value;
                    public static int Main() => (double)Convert({{value}}) == (double){{expected}} ? 42 : 1;
                }
                """, optimizationLevel: optimization));
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
            var emitter = new WasmEmitter();
            Assert.Equal(42, (await Node.ExecuteAsync(emitter.Emit(app, new ConsoleWriteLineRewriter()))).ExitCode);
            Assert.DoesNotContain(emitter.Dependencies!.Methods, method => method.DeclaringType.ToString() == "System.Runtime.InteropServices.NFloat");
        }
    }
}
