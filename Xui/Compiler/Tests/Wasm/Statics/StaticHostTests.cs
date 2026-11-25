using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Statics;

public class StaticHostTests
{
    [Fact]
    public async Task BeforeFieldInitIsLazyAndStorageBelongsToEachModuleInstance()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            public class Lazy {
                public static int Value=Initialize();
                static int Initialize() { System.Console.WriteLine("initialize"); return 40; }
                public static int NoFields() => 1;
            }
            public class Unused { static Unused() { System.Environment.Exit(99); } }
            public static class Program {
                public static int Main() => Lazy.NoFields();
                public static int Next() => ++Lazy.Value;
            }
            """));
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(app, new ConsoleWriteLineRewriter(), namedExports: ExportTests.Methods(app, "Next"));
        Assert.DoesNotContain(emitter.Dependencies!.Methods, method => method.DeclaringType.ToString() == "Unused");
        await ExportHost.RunAsync(wasm, """
            const output=[];
            const imports={xui:{'console-log': value=>output.push(value)}};
            const a=(await WebAssembly.instantiate(module, imports)).exports;
            assert.deepEqual(output, []);
            assert.equal(a.main(), 1); assert.deepEqual(output, []);
            assert.equal(a.Next(), 41); assert.equal(a.Next(), 42);
            assert.deepEqual(output, ['initialize']);
            const b=(await WebAssembly.instantiate(module, imports)).exports;
            assert.equal(b.Next(), 41); assert.equal(a.Next(), 43);
            assert.deepEqual(output, ['initialize','initialize']);
            """);
    }

    [Fact]
    public async Task HostFailurePoisonsInitializationWithoutRerunningIt()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            public static class Program {
                static Program() { System.Console.WriteLine("initialize"); }
                public static int Main() => 42;
            }
            """));
        byte[] wasm = new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter());
        await ExportHost.RunAsync(wasm, """
            let calls=0;
            const failure=new Error('host failure');
            const {exports:api}=await WebAssembly.instantiate(module, {xui:{'console-log':()=>{calls++; throw failure;}}});
            for(let i=0;i<2;i++) assert.throws(()=>api.main(), error=>error===failure);
            assert.equal(calls,1);
            """);
    }

    [Fact]
    public async Task ForeignModuleInitializerRunsBeforeAFieldOnlyAccess()
    {
        byte[] library = Frontend.Roslyn.Compile("""
            using System.Runtime.CompilerServices;
            public static class Library {
                public static int Value;
                [ModuleInitializer] public static void Initialize() { Value=42; }
            }
            """, OutputKind.DynamicallyLinkedLibrary, "Library");
        using var dependency = CanvasTests.Read(library);
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            public static class Program { public static int Main() => Library.Value; }
            """, additionalReferences: [MetadataReference.CreateFromImage(library)]));
        _ = new Xui.Compiler.XuiNet.Compilation([app, dependency]);
        Assert.Equal(42, (await Node.ExecuteAsync(new WasmEmitter().Emit(app))).ExitCode);
    }
}
