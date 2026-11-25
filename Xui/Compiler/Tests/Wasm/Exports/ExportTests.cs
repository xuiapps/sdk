using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet.DotNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exports;

public class ExportTests
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task HostKeepsIndependentManagedSessionsAcrossCalls(OptimizationLevel optimization)
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            public class Session { public int Value; public string Text; public Session(int value, string text) { Value=value; Text=text; } }
            public static class Program {
                public static int Main() => 0;
                public static Session Create(int value, string text) => new Session(value, text);
                public static int Add(Session session, int delta) { session.Value += delta; return session.Value; }
                public static string Text(Session session) => session.Text;
                public static int Unused() => 999;
            }
            """, optimizationLevel: optimization));
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(app, namedExports: Methods(app, "Create", "Add", "Text"));
        Assert.DoesNotContain(emitter.Dependencies!.Methods, m => m.ToString().EndsWith(".Unused", StringComparison.Ordinal));
        await ExportHost.RunAsync(wasm, """
            const { exports: api } = await WebAssembly.instantiate(module, {});
            assert.equal(api.main(), 0);
            assert.equal(api.Unused, undefined);
            const a = api.Create(1, 'first'), b = api.Create(100, 'second');
            for (let i = 0; i < 10000; i++) api.Add(a, 1);
            assert.equal(api.Add(a, 0), 10001);
            assert.equal(api.Add(b, 2), 102);
            assert.equal(api.Text(a), 'first'); assert.equal(api.Text(b), 'second');
            """);
    }

    [Theory]
    [InlineData("public struct Point { public int X; }", "public static Point Read() => default;")]
    [InlineData("public struct Point { public int X; }", "public static int Read(ref Point value) => value.X;")]
    [InlineData("", "public static int Read(byte value) => value;")]
    public void UnsupportedHostSignaturesReject(string types, string method)
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile(types + " public class Program { public static int Main() => 0; " + method + " }"));
        Assert.Contains("host ABI", Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app, namedExports: Methods(app, "Read"))).Message);
    }

    internal static Dictionary<string, MethodIdentity> Methods(Assembly assembly, params string[] names)
    {
        var module = assembly.ManifestModule;
        var methods = module.Types.SelectMany(t => module.GetType(t).Methods)
            .Where(h => names.Contains(module.GetMethod(h).Name, StringComparer.Ordinal));
        return methods.ToDictionary(h => module.GetMethod(h).Name, h => MethodIdentity.FromDefinition(module, h));
    }
}
