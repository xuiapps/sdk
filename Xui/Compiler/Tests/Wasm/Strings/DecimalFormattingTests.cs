using System.Globalization;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class DecimalFormattingTests
{
    [Fact]
    public void DecimalCustomFormatsAreNotSilentlyTreatedAsGeneralFormat()
    {
        using var app = Canvas.CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System.Globalization;
            public static class Program {
                public static int Main() => 1.25m.ToString("F1", CultureInfo.InvariantCulture).Length;
            }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Throws<NotSupportedException>(() => new WASM.Emitter.WasmEmitter().Emit(app));
    }

    [Fact]
    public Task InvariantDecimalFormattingPreservesCoefficientScaleAndAddressOwners()
    {
        decimal[] values = [0m, 1.00m, -42.1250m, decimal.MaxValue, decimal.MinValue,
            new(0, 0, 0, true, 28), new(1, 0, 0, false, 28), new(-1, -1, -1, true, 28)];
        var random = new Random(42);
        values = [.. values, .. Enumerable.Range(0, 80).Select(_ => new decimal(
            (int)random.NextInt64(int.MinValue, (long)int.MaxValue + 1),
            (int)random.NextInt64(int.MinValue, (long)int.MaxValue + 1),
            (int)random.NextInt64(int.MinValue, (long)int.MaxValue + 1), random.Next(2) != 0, (byte)random.Next(29)))];
        string initializers = string.Join(",", values.Select(value => {
            int[] bits = decimal.GetBits(value);
            return $"new decimal({bits[0]}, {bits[1]}, {bits[2]}, {(bits[3] < 0 ? "true" : "false")}, {(bits[3] >> 16) & 255})";
        }));
        string expected = string.Concat(values.Select(value => string.Concat(Enumerable.Repeat(value.ToString(CultureInfo.InvariantCulture) + "\n", 4))));
        return InlineArrayTests.AssertLinked($$"""
            using System;
            using System.Globalization;
            using System.Xml;
            public class Owner { public decimal Value; }
            public static class Program {
                static string Format(ref decimal value) => value.ToString(CultureInfo.InvariantCulture);
                public static int Main() {
                    var values = new decimal[] { {{initializers}} };
                    var owner = new Owner();
                    for (int i = 0; i < values.Length; i++) {
                        decimal local = values[i]; owner.Value = local;
                        Console.WriteLine(local.ToString(null, CultureInfo.InvariantCulture));
                        Console.WriteLine(Format(ref owner.Value));
                        Console.WriteLine(Format(ref values[i]));
                        Console.WriteLine(XmlConvert.ToString(local));
                    }
                    return 42;
                }
            }
            """, expected, 42, typeof(System.Xml.XmlConvert).Assembly);
    }
}
