using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Json;

[Collection(nameof(ManagedConsoleCollection))]
public class JsonTests
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task NativeDocumentPreservesTokensNumbersCursorCopiesAndLifetime(OptimizationLevel optimization)
    {
        const string source = """
            using System;
            using System.Text.Json;
            public static class Program {
                public static int Main() {
                    using var document = JsonDocument.Parse("{\"dup\":1,\"dup\":2,\"__proto__\":3,\"items\":[1,2,3],\"null\":null,\"bool\":true,\"name\":\"a\\n\\uD83D\\uDE42\",\"big\":18446744073709551615,\"small\":-9223372036854775808,\"double\":1.25}");
                    var root = document.RootElement;
                    Console.WriteLine(root.GetProperty("dup").GetInt32());
                    Console.WriteLine(root.GetProperty("__proto__").GetInt32());
                    Console.WriteLine(root.GetProperty("name").GetString());
                    Console.WriteLine(root.GetProperty("null").GetString() == null);
                    Console.WriteLine(root.GetProperty("bool").GetBoolean());
                    Console.WriteLine(root.GetProperty("big").GetUInt64().ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Console.WriteLine(root.GetProperty("small").GetInt64());
                    Console.WriteLine(root.GetProperty("double").GetDouble());
                    var a = root.GetProperty("items").EnumerateArray(); a.MoveNext(); var b = a;
                    a.MoveNext(); Console.WriteLine(a.Current.GetInt32()); Console.WriteLine(b.Current.GetInt32());
                    foreach(var item in root.GetProperty("items").EnumerateArray()) Console.WriteLine(item.GetInt32());
                    Console.WriteLine(root.TryGetProperty("missing", out var absent)); Console.WriteLine((int)absent.ValueKind);
                    try { root.GetProperty("missing"); } catch(System.Collections.Generic.KeyNotFoundException) { Console.WriteLine("missing"); }
                    try { root.GetProperty("name").GetInt32(); } catch(InvalidOperationException) { Console.WriteLine("kind"); }
                    using var numbers = JsonDocument.Parse("[1.0,1e0,2147483648,-0,4294967295,9223372036854775808,18446744073709551616]");
                    foreach (var value in numbers.RootElement.EnumerateArray()) {
                        Console.WriteLine(value.TryGetInt32(out int signed)); Console.WriteLine(signed);
                        Console.WriteLine(value.TryGetUInt32(out uint unsigned)); Console.WriteLine(unsigned.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        Console.WriteLine(value.TryGetInt64(out long wide)); Console.WriteLine(wide);
                        Console.WriteLine(value.TryGetUInt64(out ulong widest)); Console.WriteLine(widest.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                    try { JsonDocument.Parse("{"); } catch(JsonException error) { Console.WriteLine(((Exception)error) is SystemException); }
                    try { JsonDocument.Parse("[[[0]]]", new JsonDocumentOptions {MaxDepth=2}); } catch(JsonException) {Console.WriteLine("depth");}
                    try { JsonDocument.Parse("{\"a\":1,\"a\":2}", new JsonDocumentOptions {AllowDuplicateProperties=false}); } catch(JsonException) {Console.WriteLine("duplicate");}
                    var saved = root.GetProperty("items"); document.Dispose(); document.Dispose(); a.Dispose();
                    try { saved.GetArrayLength(); } catch(ObjectDisposedException) { Console.WriteLine("disposed"); }
                    return 42;
                }
            }
            """;
        byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization,
            additionalReferences: [MetadataReference.CreateFromFile(typeof(System.Text.Json.JsonDocument).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Memory.dll"))]);
        var expected = GcTests.RunManaged(pe);
        using var app = CanvasTests.Read(pe);
        using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Json.JsonDocument).Assembly.Location));
        using var framework = new Frontend.Framework("System.Text.Json.dll", "System.Memory.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, runtime, .. framework.Assemblies],
            BrowserJson.Replacements(framework.Assemblies.Single(a => a.Identity.Name == "System.Text.Json"), runtime));
        byte[] wasm;
        try { wasm = new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()); }
        catch (Exception error) when (error.Data["Xui.DependencyPath"] is { } path)
        { throw new Exception(error.Message + "\n" + path, error); }
        await ExportHost.RunAsync(wasm, JsonJavaScript.Source + "\n" + $$"""
            const output=[];
            const xui={'console-log':value=>output.push(String(value)), 'console-log-boolean':value=>output.push(value?'True':'False')};
            const {exports:api}=await WebAssembly.instantiate(module,{...globalThis.xuiRuntimeImports,'xui-json':createJsonHost().imports,xui});
            assert.equal(api.main(),{{expected.ExitCode}});
            assert.equal(output.join('\n')+'\n',{{System.Text.Json.JsonSerializer.Serialize(expected.Output)}});
            """);
    }
}
