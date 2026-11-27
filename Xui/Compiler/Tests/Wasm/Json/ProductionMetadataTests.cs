using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Json;

[Collection(nameof(ManagedConsoleCollection))]
public class ProductionMetadataTests
{
    [Fact]
    public async Task EveryFloorplanLabelAndNumericFieldMatchesTheManagedResourceReader()
    {
        byte[] pe = Frontend.Roslyn.Compile("""
            using System;
            using System.Text.Json;
            public static class Program {
                static int Mix(int hash, int value) => unchecked(hash * 31 + value);
                static int Number(int hash, JsonElement value) {
                    long bits = BitConverter.DoubleToInt64Bits(value.GetDouble());
                    return Mix(Mix(hash, (int)bits), (int)(bits >> 32));
                }
                public static int Main() {
                    using var stream = typeof(Program).Assembly.GetManifestResourceStream("TestApp.FloorplanGpu.atlas.json");
                    using var document = JsonDocument.Parse(stream);
                    var root = document.RootElement;
                    int hash = 17;
                    foreach (var name in new[]{"Version","Width","Height","MipCount"}) hash=Mix(hash,root.GetProperty(name).GetInt32());
                    foreach (var count in root.GetProperty("StructureQuadCounts").EnumerateArray()) hash=Mix(hash,count.GetInt32());
                    foreach (var level in root.GetProperty("Labels").EnumerateArray()) {
                        hash=Mix(hash,level.GetArrayLength());
                        foreach (var label in level.EnumerateArray()) {
                            foreach (char c in label.GetProperty("Text").GetString()) hash=Mix(hash,c);
                            foreach(var key in new[]{"X","Y","Size","Weight","Align"}) hash=Number(hash,label.GetProperty(key));
                            foreach(var key in new[]{"Transform","Bounds","Color"})
                                foreach(var value in label.GetProperty(key).EnumerateArray()) hash=Number(hash,value);
                        }
                    }
                    return hash;
                }
            }
            """, additionalReferences: [MetadataReference.CreateFromFile(typeof(System.Text.Json.JsonDocument).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Memory.dll"))],
            resources: [new ResourceDescription("TestApp.FloorplanGpu.atlas.json", () => File.OpenRead(Path.Combine(Frontend.SourceProject.XuiRoot, "Apps/TestApp/Assets/FloorplanGpu/atlas.json")), true)]);
        int expected = GcTests.RunManaged(pe).ExitCode;
        using var app = CanvasTests.Read(pe);
        using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Json.JsonDocument).Assembly.Location));
        using var framework = new Frontend.Framework("System.Text.Json.dll", "System.Memory.dll", "System.IO.Compression.dll", "System.Collections.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, runtime, .. framework.Assemblies],
            [.. BrowserJson.Replacements(framework.Assemblies.Single(a => a.Identity.Name == "System.Text.Json"), runtime),
             .. BrowserResources.Replacements(framework.Assemblies[0], framework.Assemblies.Single(a => a.Identity.Name == "System.IO.Compression"), runtime)]);
        byte[] wasm;
        try { wasm = new WasmEmitter().Emit(app, exportMemory: true); }
        catch (Exception error) when (error.Data["Xui.DependencyPath"] is { } path)
        { throw new Exception(error.Message + "\n" + path, error); }
        string script = ResourceJavaScript.Source + "\n" + JsonJavaScript.Source + "\nconst catalogue=" + ResourceCatalog.Json([app]) + ";\n" + $$"""
            let api;
            const resources=await createResourceHost(catalogue,()=>api.memory);
            ({exports:api}=await WebAssembly.instantiate(module,{...globalThis.xuiRuntimeImports,'xui-json':createJsonHost().imports,'xui-resource':resources.imports}));
            const result=api.main();
            if(result!=={{expected}}) throw new Error('Metadata checksum mismatch: '+result);
            if(resources.stats.opens!==resources.stats.closes) throw new Error('Resource stream leak');
            globalThis.jsonResult={hash:result,bytes:resources.stats.bytes};
            """;
        await ExportHost.RunAsync(wasm, script);
        string directory = Path.Combine(AppContext.BaseDirectory, "Json");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "FloorplanMetadata.html"), $$"""
            <!doctype html><meta charset="utf-8"><title>Floorplan metadata parity</title><pre id="result">Loading...</pre>
            <script>
            {{JavaScript.Source}}
            (async()=>{
                const bytes=Uint8Array.from(atob('{{Convert.ToBase64String(wasm)}}'),c=>c.charCodeAt(0));
                const module=await WebAssembly.compile(bytes,{builtins:['js-string']});
                {{script}}
                document.getElementById('result').textContent='Passed: '+JSON.stringify(globalThis.jsonResult);
            })().catch(error=>{globalThis.jsonError=String(error);document.getElementById('result').textContent=String(error)});
            </script>
            """);
    }
}
