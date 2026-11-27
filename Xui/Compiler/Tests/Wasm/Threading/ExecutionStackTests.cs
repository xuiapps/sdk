using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Threading;

public class ExecutionStackTests
{
    [Fact]
    public async Task DepthBudgetRecoversAfterNormalReturnsAndManagedUnwinding()
    {
        const string source = """
            using System;
            using System.Runtime.CompilerServices;
            public static class Program {
                static int cleaned;
                static int TryDepth(int count) {
                    if(!RuntimeHelpers.TryEnsureSufficientExecutionStack()) return count;
                    if(count>512) return -1;
                    return TryDepth(count+1);
                }
                static void EnsureDepth(int remaining) {
                    Span<int> local=stackalloc int[2]; local[0]=remaining;
                    try {
                        RuntimeHelpers.EnsureSufficientExecutionStack();
                        if(remaining>0) EnsureDepth(remaining-1);
                    } finally { cleaned+=local[0]>=0 ? 1 : 0; }
                }
                static int HardDepth(int count) => count==0 ? 1 : HardDepth(count-1)+1;
                static void ThrowOther() => throw new InvalidOperationException();
                public static int Main() {
                    for(int i=0;i<20;i++) {
                        if(!RuntimeHelpers.TryEnsureSufficientExecutionStack()) return 1;
                        RuntimeHelpers.EnsureSufficientExecutionStack();
                        int depth=TryDepth(0); if(depth<100 || depth>256) return 2;
                        cleaned=0;
                        try { EnsureDepth(512); return 3; } catch(InsufficientExecutionStackException) { }
                        if(cleaned<100 || cleaned>256) return 4;
                        EnsureDepth(3);
                        try { HardDepth(512); return 5; } catch(InsufficientExecutionStackException) { }
                        try { ThrowOther(); } catch(InvalidOperationException) { }
                        if(HardDepth(10)!=11) return 6;
                        if(TryDepth(0)!=depth) return 7;
                    }
                    return 42;
                }
            }
            """;
        foreach(var optimization in new[] { Microsoft.CodeAnalysis.OptimizationLevel.Release, Microsoft.CodeAnalysis.OptimizationLevel.Debug })
        {
            using var app=CanvasTests.Read(Frontend.Roslyn.Compile(source,optimizationLevel:optimization));
            using var framework=new Frontend.Framework();
            _=new Xui.Compiler.XuiNet.Compilation([app,..framework.Assemblies]);
            byte[] wasm=new WasmEmitter().Emit(app);
            await ExportHost.RunAsync(wasm,"""
                for(let i=0;i<2;i++) {
                    const instance=await WebAssembly.instantiate(module,globalThis.xuiRuntimeImports);
                    for(let repeat=0;repeat<3;repeat++) assert.equal(instance.exports.main(),42);
                }
                """);
        }
    }
}
