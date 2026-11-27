using System.IO.Compression;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Resources;

[Collection(nameof(Xui.Compiler.Tests.Wasm.Structs.ManagedConsoleCollection))]
public class ResourceTests
{
    [Fact]
    public async Task ProductionAtlasReadsEveryMipThroughBoundedLinearBuffers()
    {
        byte[] source = Frontend.Roslyn.Compile("""
            using System;
            using System.IO;
            using System.IO.Compression;
            public static class Program {
                public static int Main() {
                    using var resource = typeof(Program).Assembly.GetManifestResourceStream("TestApp.FloorplanGpu.atlas.bin.gz");
                    using var gzip = new GZipStream(resource, CompressionMode.Decompress);
                    using var reader = new BinaryReader(gzip);
                    if(reader.ReadInt32()!=0x41504658 || reader.ReadInt32()!=1) return -1;
                    int width=reader.ReadInt32(), height=reader.ReadInt32(), levels=reader.ReadInt32();
                    if(width!=2048 || height!=2048 || levels!=5)return -2;
                    int hash=17;
                    for(int level=0;level<levels;level++) {
                        int length=reader.ReadInt32();
                        if(length!=Math.Max(1,width>>level)*Math.Max(1,height>>level)*4)return -3;
                        byte[] bytes=reader.ReadBytes(length);
                        if(bytes.Length!=length)return -4;
                        for(int i=0;i<bytes.Length;i++)hash=unchecked(hash*31+bytes[i]);
                    }
                    if(reader.BaseStream.ReadByte()!=-1)return -5;
                    return hash;
                }
            }
            """, additionalReferences: [MetadataReference.CreateFromFile(typeof(GZipStream).Assembly.Location)],
            resources: Frontend.TestAppResources.Read().ToArray());
        int expected = Xui.Compiler.Tests.Wasm.Gc.GcTests.RunManaged(source).ExitCode;
        using var app = CanvasTests.Read(source);
        using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Tasks.Task).Assembly.Location));
        using var framework = new Frontend.Framework("System.IO.Compression.dll", "System.Memory.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, runtime, .. framework.Assemblies],
            BrowserResources.Replacements(framework.Assemblies.Single(a => a.Identity.Name == "System.Private.CoreLib"),
                framework.Assemblies.Single(a => a.Identity.Name == "System.IO.Compression"), runtime));
        byte[] wasm = new WasmEmitter().Emit(app, exportMemory: true);
        await ExportHost.RunAsync(wasm, ResourceJavaScript.Source + "\nconst catalogue=" + ResourceCatalog.Json([app]) + ";\n" + $$"""
            let api;
            const host=await createResourceHost(catalogue,()=>api.memory);
            ({exports:api}=await WebAssembly.instantiate(module,{...globalThis.xuiRuntimeImports,'xui-resource':host.imports}));
            assert.equal(api.main(),{{expected}});
            assert.equal(host.stats.opens,host.stats.closes);
            assert.ok(host.stats.bytes>22000000);
            """);
    }

    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task ManifestGzipAndBinaryReadsUseTheOriginalFrameworkSignatures(OptimizationLevel optimization)
    {
        using var payload = new MemoryStream();
        using (var writer = new BinaryWriter(payload, System.Text.Encoding.UTF8, true))
        {
            writer.Write(42); writer.Write(1.5f); writer.Write(2.5d);
            writer.Write(Enumerable.Range(0, 10000).Select(i => (byte)i).ToArray());
        }
        byte[] bytes = payload.ToArray();
        using var compressed = new MemoryStream();
        using (var gzip = new GZipStream(compressed, CompressionLevel.SmallestSize, true)) gzip.Write(bytes);
        byte[] gzipBytes = compressed.ToArray();
        byte[] source = Frontend.Roslyn.Compile("""
            using System;
            using System.IO;
            using System.IO.Compression;
            public static class Program {
                public static int Main() {
                    var assembly = typeof(Program).Assembly;
                    if (!ReferenceEquals(assembly, typeof(Program).Assembly)) return 1;
                    if (assembly.GetManifestResourceStream("missing") != null) return 2;
                    using var raw = assembly.GetManifestResourceStream("data.bin");
                    if (raw.Length != 10016 || !raw.CanSeek || raw.CanWrite) return 3;
                    raw.Seek(4, SeekOrigin.Begin);
                    using (var reader = new BinaryReader(assembly.GetManifestResourceStream("data.bin")))
                        if (reader.ReadInt32() != 42 || raw.Position != 4) return 4;
                    using var zipped = assembly.GetManifestResourceStream("data.gz");
                    using (var gzip = new GZipStream(zipped, CompressionMode.Decompress, true)) {
                        if (gzip.CanSeek || gzip.CanWrite || !gzip.CanRead) return 5;
                        using var reader = new BinaryReader(gzip);
                        if (reader.ReadInt32()!=42 || reader.ReadSingle()!=1.5f || reader.ReadDouble()!=2.5) return 6;
                        byte[] values = reader.ReadBytes(12000);
                        if(values.Length!=10000)return 7;
                        for(int i=0;i<values.Length;i++)if(values[i]!=(byte)i)return 8;
                        try { reader.ReadInt32(); return 9; } catch (EndOfStreamException) { }
                    }
                    if(!zipped.CanRead)return 10;
                    zipped.Dispose();
                    try { zipped.ReadByte(); return 11; } catch(ObjectDisposedException) { }
                    using var broken = assembly.GetManifestResourceStream("broken.gz");
                    using var invalid = new GZipStream(broken,CompressionMode.Decompress);
                    try { invalid.ReadByte(); return 12; } catch(InvalidDataException) { }
                    return 42;
                }
            }
            """, optimizationLevel: optimization,
            additionalReferences: [MetadataReference.CreateFromFile(typeof(GZipStream).Assembly.Location)],
            resources: [Resource("data.bin", bytes), Resource("data.gz", gzipBytes), Resource("broken.gz", [1,2,3])]);
        using var app = CanvasTests.Read(source);
        using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Tasks.Task).Assembly.Location));
        using var framework = new Frontend.Framework("System.IO.Compression.dll", "System.Collections.dll", "System.Memory.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, runtime, .. framework.Assemblies],
            BrowserResources.Replacements(framework.Assemblies.Single(a => a.Identity.Name == "System.Private.CoreLib"),
                framework.Assemblies.Single(a => a.Identity.Name == "System.IO.Compression"), runtime));
        var emitter = new WasmEmitter();
        byte[] wasm;
        try { wasm = emitter.Emit(app, exportMemory: true); }
        catch(Exception error) { throw new InvalidOperationException($"{error.Message}\n{error.Data["Xui.Method"]}\n{error.Data["Xui.DependencyPath"]}", error); }
        await ExportHost.RunAsync(wasm, ResourceJavaScript.Source + "\nconst catalogue=" + ResourceCatalog.Json([app]) + ";\n" + """
            let api;
            const host=await createResourceHost(catalogue,()=>api.memory);
            ({exports:api}=await WebAssembly.instantiate(module,{...globalThis.xuiRuntimeImports,'xui-resource':host.imports}));
            assert.equal(api.main(),42);
            assert.equal(host.stats.opens,host.stats.closes,'Every opened stream should be disposed');
            assert.ok(host.stats.bytes>10000);
            """);
    }

    private static ResourceDescription Resource(string name, byte[] bytes) => new(name, () => new MemoryStream(bytes), true);
}
