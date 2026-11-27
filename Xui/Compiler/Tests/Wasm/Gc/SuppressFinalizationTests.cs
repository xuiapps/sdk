using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Gc;

[Collection(nameof(ManagedConsoleCollection))]
public class SuppressFinalizationTests
{
    [Fact]
    public Task NonFinalizableObjectsAndNullMatchClr() => InlineArrayTests.AssertLinked("""
        using System;
        sealed class Resource { public int Value=42; }
        public static class Program { public static int Main() {
            var value=new Resource(); GC.SuppressFinalize(value);GC.SuppressFinalize(value);
            GC.SuppressFinalize(new object());GC.SuppressFinalize("native string");
            try { GC.SuppressFinalize(null); } catch(ArgumentNullException error) { Console.WriteLine(error.ParamName); }
            return value.Value;
        } }
        ""","obj\n",42);

    [Fact]
    public void FinalizableTypesAreRejectedInsteadOfSilentlySuppressingBehavior()
    {
        using var app=CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System;
            sealed class Resource { ~Resource() { } }
            public static class Program { public static int Main() { GC.SuppressFinalize(new Resource());return 0; } }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app,..framework.Assemblies]);
        Assert.Throws<NotSupportedException>(()=>new WasmEmitter().Emit(app));
    }
}
