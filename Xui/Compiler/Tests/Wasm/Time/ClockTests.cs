using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Time;

[Collection(nameof(Xui.Compiler.Tests.Wasm.Structs.ManagedConsoleCollection))]
public class ClockTests
{
    [Fact]
    public Task CustomTimeProviderKeepsItsVirtualClockAndFrequency() =>
        Xui.Compiler.Tests.Wasm.InlineArrays.InlineArrayTests.AssertLinked("""
        using System;
        public class TestClock : TimeProvider {
            public long Current;
            public long Frequency = 100;
            public override long TimestampFrequency => Frequency;
            public override long GetTimestamp() => Current;
        }
        public static class Program {
            public static int Main() {
                var custom = new TestClock { Current = 150 };
                TimeProvider clock = custom;
                Console.WriteLine(clock.GetElapsedTime(100).Ticks);
                custom.Current = 125;
                Console.WriteLine(clock.GetElapsedTime(100).Ticks);
                custom.Frequency = 0;
                try { clock.GetElapsedTime(100); } catch (InvalidOperationException) { Console.WriteLine("frequency"); }
                return 42;
            }
        }
        """, "5000000\n2500000\nfrequency\n", 42);

    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task ElapsedClockUsesInt64AndTheSharedMonotonicHost(OptimizationLevel optimization)
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System;
            using System.Diagnostics;
            public static class Program {
                public static int Main() => 0;
                public static long Read() => Environment.TickCount64;
                public static long Elapsed(long previous) => Environment.TickCount64 - previous;
                public static long Timestamp() => Stopwatch.GetTimestamp();
                public static long Frequency() => Stopwatch.Frequency;
                public static int Resolution() => Stopwatch.IsHighResolution ? 1 : 0;
                public static long Ticks(long start, long end) => Stopwatch.GetElapsedTime(start, end).Ticks;
            }
            """, optimizationLevel: optimization));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(app, namedExports: ExportTests.Methods(app, "Read", "Elapsed", "Timestamp", "Frequency", "Resolution", "Ticks"));
        Assert.DoesNotContain(emitter.Dependencies!.Methods, m => m.ToString().Contains("GetLowResolutionTimestamp", StringComparison.Ordinal));
        await ExportHost.RunAsync(wasm, """
            const { exports: api } = await WebAssembly.instantiate(module, globalThis.xuiRuntimeImports);
            const before = BigInt(Math.floor(performance.now()));
            const first = api.Read(), second = api.Read();
            const after = BigInt(Math.floor(performance.now()));
            assert(first >= before && second >= first && second <= after);
            assert(api.Elapsed(first) >= 0n);
            assert.equal(api.Frequency(), 1_000_000_000n);
            assert.equal(api.Resolution(), 1);
            const stamp = api.Timestamp();
            assert(stamp <= BigInt(Math.floor(performance.now() * 1_000_000)));
            assert.equal(api.Ticks(0n, 1_250_000_000n), 12_500_000n);
            assert.equal(api.Ticks(1_250_000_000n, 0n), -12_500_000n);
            let clock = 9007199254740993n;
            const controlled = await WebAssembly.instantiate(module, {
                'xui-clock': { 'tick-count64': () => clock, 'timestamp': () => clock * 1_000_000n }
            });
            assert.equal(controlled.exports.Read(), clock);
            // Timestamp is a separately typed i64 import, never coerced through JS Number.
            clock = 4_000_000n;
            assert.equal(controlled.exports.Timestamp(), 4_000_000_000_000n);
            clock = 9007199254740993n;
            clock += 25n;
            assert.equal(controlled.exports.Elapsed(9007199254740993n), 25n);
            """);
    }
}
