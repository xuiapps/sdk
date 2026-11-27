using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Threading;

public class WeakReferenceTests
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task TypedAndUntypedHandlesRetainIdentityAndCanBeReset(OptimizationLevel optimization)
    {
        byte[] pe = Frontend.Roslyn.Compile("""
            using System;
            public class Target { public int Value; }
            public static class Program {
                static Target keep;
                public static int Main() {
                    keep=new Target {Value=42};
                    var weak=new WeakReference<Target>(null);
                    if(weak.TryGetTarget(out var missing) || missing!=null)return 1;
                    weak.SetTarget(keep);
                    if(!weak.TryGetTarget(out var found) || !ReferenceEquals(found,keep))return 2;
                    found.Value++;
                    var other=new Target {Value=20}; weak.SetTarget(other);
                    if(!weak.TryGetTarget(out var replaced) || !ReferenceEquals(replaced,other))return 3;
                    weak.SetTarget(null);
                    if(weak.TryGetTarget(out replaced) || replaced!=null)return 4;
                    var untyped=new WeakReference(keep,false);
                    if(!untyped.IsAlive || untyped.TrackResurrection || !ReferenceEquals(untyped.Target,keep))return 5;
                    untyped.Target=other;
                    if(!ReferenceEquals(untyped.Target,other))return 6;
                    untyped.Target=null;
                    if(untyped.IsAlive || untyped.Target!=null)return 7;
                    try { _=new WeakReference<Target>(keep,true); return 8; } catch(NotSupportedException) { }
                    try { _=new WeakReference<string>("native"); return 9; } catch(NotSupportedException) { }
                    return keep.Value-1;
                }
            }
            """, optimizationLevel: optimization);
        using var app = CanvasTests.Read(pe);
        using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Tasks.Task).Assembly.Location));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, runtime, .. framework.Assemblies],
            BrowserWeakReferences.Replacements(framework.Assemblies.Single(a => a.Identity.Name == "System.Private.CoreLib"), runtime));
        var wasm = new WasmEmitter().Emit(app);
        await ExportHost.RunAsync(wasm, WeakReferenceJavaScript.Source + "\n" + """
            const instance = await WebAssembly.instantiate(module, {...globalThis.xuiRuntimeImports,'xui-weak':createWeakReferenceImports()});
            assert.equal(instance.exports.main(),42);
            """);
    }
}
