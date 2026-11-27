using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Memory;

public class MemoryGrowthTests
{
    [Fact]
    public async Task OrdinaryModulesCanGrowWithoutMovingStaticDataOrChangingStackBounds()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System;
            public static class Program {
                public static int Main() {
                    ReadOnlySpan<int> constants=[7,11,13,17];
                    Span<int> values=stackalloc int[4];
                    constants.CopyTo(values);
                    return values[0]+values[1]+values[2]+values[3];
                }
            }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        byte[] wasm = new WasmEmitter().Emit(app, exportMemory: true);
        await ExportHost.RunAsync(wasm, """
            assert.ok(!WebAssembly.Module.imports(module).some(i=>i.module==='xui-memory'));
            const {exports:api}=await WebAssembly.instantiate(module,globalThis.xuiRuntimeImports);
            assert.equal(api.main(),48);
            const pages=api.memory.buffer.byteLength/65536;
            assert.equal(api.memory.grow(2),pages);
            assert.equal(api.memory.buffer.byteLength,(pages+2)*65536);
            assert.equal(api.main(),48);
            assert.ok(new Uint8Array(api.memory.buffer,pages*65536).every(value=>value===0));
            """);
    }
}
