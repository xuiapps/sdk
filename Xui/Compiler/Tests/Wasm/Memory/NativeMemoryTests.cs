using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Memory;

public class NativeMemoryTests
{
    [Fact]
    public async Task UnmanagedBuffersSurviveGrowthAndAreReleasedAndZeroedOnReuse()
    {
        const string source = """
            using System;
            using System.Runtime.InteropServices;
            public static unsafe class Program {
                public static int Main() {
                    void* first=NativeMemory.AllocZeroed(16,4);
                    var values=new Span<int>(first,16);
                    for(int i=0;i<values.Length;i++) { if(values[i]!=0) return 1; values[i]=i+100; }
                    void* large=NativeMemory.Alloc(700000);
                    var other=new Span<byte>(large,700000); other.Fill(42);
                    for(int i=0;i<values.Length;i++) if(values[i]!=i+100) return 2;
                    if(other[699999]!=42) return 3;
                    NativeMemory.Free(first);
                    void* reused=NativeMemory.AllocZeroed(64);
                    var cleared=new Span<int>(reused,16);
                    for(int i=0;i<cleared.Length;i++) if(cleared[i]!=0) return 4;
                    NativeMemory.Free(large); NativeMemory.Free(reused); NativeMemory.Free(null);
                    void* empty=NativeMemory.Alloc(0); if(empty==null) return 5; NativeMemory.Free(empty);
                    try { NativeMemory.Alloc(nuint.MaxValue,nuint.MaxValue); }
                    catch(OutOfMemoryException) { return 42; }
                    return 6;
                }
            }
            """;
        foreach (bool debug in new[] { false, true })
        {
            using var app = CanvasTests.Read(Frontend.Roslyn.Compile(source, optimizationLevel: debug ? Microsoft.CodeAnalysis.OptimizationLevel.Debug : Microsoft.CodeAnalysis.OptimizationLevel.Release, allowUnsafe: true));
            using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Memory.NativeMemory).Assembly.Location));
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation([app, runtime, .. framework.Assemblies], BrowserNativeMemory.Replacements(framework.Assemblies[0], runtime));
            byte[] wasm;
            try { wasm = new WasmEmitter().Emit(app); }
            catch (Exception e) when (e.Data["Xui.DependencyPath"] is { } path) { throw new Exception(e.Message + "\n" + path, e); }
            await ExportHost.RunAsync(wasm, NativeMemoryJavaScript.Source + "\n" + """
                for(let instance=0;instance<2;instance++) {
                    let api;
                    const heap=createNativeMemoryHost(()=>api.memory);
                    ({exports:api}=await WebAssembly.instantiate(module,{...globalThis.xuiRuntimeImports,'xui-memory':heap.imports}));
                    for(let run=0;run<3;run++) {
                        assert.equal(api.main(),42);
                        assert.equal(heap.stats.liveBytes,0);
                        assert.equal(heap.stats.allocations,heap.stats.frees);
                    }
                    assert.ok(heap.stats.grows>0);
                    const {allocate,free}=heap.imports;
                    const a=allocate(16,1,0), b=allocate(16,1,0), c=allocate(16,1,0), guard=allocate(16,1,0);
                    assert.equal(a%16,0); free(b); free(a); free(c);
                    const merged=allocate(48,1,1); assert.equal(merged,a); free(merged); free(guard);
                    assert.equal(heap.stats.liveBytes,0);
                }
                const tiny=new WebAssembly.Memory({initial:1,maximum:1});
                const bounded=createNativeMemoryHost(()=>tiny);
                assert.equal(bounded.imports.allocate(1,1,0),0);
                assert.equal(bounded.stats.liveBytes,0);
                """);
        }
    }
}
