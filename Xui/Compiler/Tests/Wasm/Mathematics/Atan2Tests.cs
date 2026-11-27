using System.Globalization;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Mathematics;

public class Atan2Tests
{
    [Fact]
    public async Task QuadrantsSignedZerosAndInfinitiesMatchClr()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System; using System.Runtime.InteropServices;
            public static class Program {
                public static int Main() => 0;
                public static double Double(double y, double x) => Math.Atan2(y,x);
                public static float Single(float y, float x) => MathF.Atan2(y,x);
                public static double Native(double y, double x) => (double)NFloat.Atan2((NFloat)y,(NFloat)x);
            }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var wasm = new WasmEmitter().Emit(app, namedExports: ExportTests.Methods(app,"Double","Single","Native"));
        double[] axes = [0, -0d, 1, -1, 2, -3, double.NaN, double.PositiveInfinity, double.NegativeInfinity];
        string literal(double value) => value.ToString("R",CultureInfo.InvariantCulture);
        string cases = string.Join(",", from y in axes from x in axes select $"[{literal(y)},{literal(x)},{literal(Math.Atan2(y,x))},{literal(MathF.Atan2((float)y,(float)x))}]");
        await ExportHost.RunAsync(wasm,$$"""
            const {exports:api}=await WebAssembly.instantiate(module,globalThis.xuiRuntimeImports);
            function same(actual,expected,tolerance) {
                if (expected === 0 || !Number.isFinite(expected)) assert.equal(actual,expected);
                else assert.ok(Math.abs(actual-expected)<=tolerance,`${actual} != ${expected}`);
            }
            for(const [y,x,d,f] of [{{cases}}]) {
                same(api.Double(y,x),d,1e-15);
                same(api.Single(y,x),f,5e-7);
                same(api.Native(y,x),f,5e-7);
            }
            """);
    }
}
