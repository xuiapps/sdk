using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.Tests.Wasm.TestApp;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Gpu;

public class UploadTests
{
    [Fact]
    public async Task GenericInterfaceUploadsCopyGcVerticesIntoBorrowedLinearStorage()
    {
        byte[] coreImage = Frontend.CoreCanvasProfile.Image;
        string source = """
            using System;
            using TestAppCanvas;
            using Xui.GPU.Hardware;
            public struct Point { public float X,Y,Z; }
            public struct Vertex { public Point Position; public int Color; }
            public static class Program {
                public static int Main()=>0;
                public static void Upload() {
                    IGpuCommandList commands=new BrowserGpuCommandList(7);
                    var vertices=new Vertex[] {
                        new() {Position=new Point {X=1.25f,Y=-2,Z=3.5f},Color=42},
                        new() {Position=new Point {X=4,Y=5,Z=6},Color=-1}};
                    commands.SetVertexBuffer<Vertex>(vertices);
                    Vertex uniform=vertices[1]; commands.SetConstantBuffer(in uniform);
                    commands.SetVertexBuffer<Vertex>(ReadOnlySpan<Vertex>.Empty);
                }
            }
            """;
        byte[] image = Frontend.Roslyn.Compile(source, allowUnsafe: true,
            additionalReferences: [MetadataReference.CreateFromImage(coreImage), MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location)],
            additionalSources: [File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp/Browser/Gpu.cs"))]);
        using var app = CanvasTests.Read(image);
        using var core = CanvasTests.Read(coreImage);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, core, .. framework.Assemblies]);
        var emitter = new WasmEmitter();
        byte[] wasm;
        try { wasm = emitter.Emit(app, new CanvasCommandRewriter(), namedExports: ExportTests.Methods(app, "Upload"), exportMemory: true); }
        catch (Exception error) { throw new Exception(error.Message + "\n" + string.Join("\n", error.Data.Values.Cast<object>()), error); }
        await ExportHost.RunAsync(wasm, """
            let api;const calls=[];
            const {exports}=await WebAssembly.instantiate(module,{...globalThis.xuiRuntimeImports,gpu:{
                Invoke(op,handle,pointer,length,stride,count) {
                    assert.equal(handle,7);
                    calls.push({op,bytes:Array.from(new Uint8Array(api.memory.buffer,pointer,length)),pointer,stride,count});return 0;
                }
            }}); api=exports;
            for(let i=0;i<1000;i++) {
                calls.length=0;api.Upload();assert.equal(calls.length,3);
                const [vertices,uniform,empty]=calls;
                assert.deepEqual([vertices.op,vertices.bytes.length,vertices.stride,vertices.count],['Vertices',32,16,2]);
                const data=new DataView(Uint8Array.from(vertices.bytes).buffer);
                assert.equal(data.getFloat32(0,true),1.25);assert.equal(data.getFloat32(4,true),-2);
                assert.equal(data.getFloat32(8,true),3.5);assert.equal(data.getInt32(12,true),42);
                assert.deepEqual(uniform.bytes,vertices.bytes.slice(16));assert.equal(uniform.op,'Constants');
                assert.deepEqual([empty.op,empty.bytes.length,empty.count],['Vertices',0,0]);
            }
            """);
    }
}
