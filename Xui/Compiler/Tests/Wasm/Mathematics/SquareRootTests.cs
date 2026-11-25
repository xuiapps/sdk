using System.Globalization;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

public class SquareRootTests
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task SquareRootsUseWasmPrecisionAndIeeeSpecialValues(OptimizationLevel optimization)
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System;
            using System.Runtime.InteropServices;
            public static class Program {
                public static int Main() => 0;
                public static double Double(double x) => Math.Sqrt(x);
                public static float Single(float x) => MathF.Sqrt(x);
                public static double DoubleType(double x) => double.Sqrt(x);
                public static float SingleType(float x) => float.Sqrt(x);
                public static double Native(double x) => (double)NFloat.Sqrt((NFloat)x);
            }
            """, optimizationLevel: optimization));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var wasm = new WasmEmitter().Emit(app, namedExports: ExportTests.Methods(app, "Double", "Single", "DoubleType", "SingleType", "Native"));
        double[] values = [0d, -0d, 1d, 4d, 2d, -1d, double.NaN, double.PositiveInfinity, double.NegativeInfinity,
            double.Epsilon, float.Epsilon, double.MaxValue, float.MaxValue, 1.0000000596046448d];
        string cases = string.Join(",", values.Select(value => $"[{Literal(value)},{Literal(Math.Sqrt(value))},{Literal(MathF.Sqrt((float)value))}]"));
        await ExportHost.RunAsync(wasm, $$"""
            assert(!WebAssembly.Module.imports(module).some(i => i.module === 'xui-math'));
            const { exports: api } = await WebAssembly.instantiate(module, {});
            for (const [input, doubleResult, singleResult] of [{{cases}}]) {
                assert.equal(api.Double(input), doubleResult);
                assert.equal(api.DoubleType(input), doubleResult);
                assert.equal(api.Single(input), singleResult);
                assert.equal(api.SingleType(input), singleResult);
                assert.equal(api.Native(input), singleResult);
            }
            """);
    }

    private static string Literal(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
