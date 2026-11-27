using System.Text;
using System.Text.Json;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class StringComparisonTests
{
    [Fact]
    public Task ExplicitOrdinalModesPreserveUnicodeNullAndInvalidModeSemantics() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Compare(string a, string b) {
                Console.WriteLine(string.Equals(a,b,StringComparison.Ordinal));
                Console.WriteLine(string.Equals(a,b,StringComparison.OrdinalIgnoreCase));
                if(a!=null) Console.WriteLine(a.Equals(b,StringComparison.OrdinalIgnoreCase));
            }
            public static int Main() {
                Compare("xml","XmL"); Compare(null,null); Compare(null,""); Compare("",null);
                Compare("Жᾀ𐐨","жᾈ𐐀"); Compare("ß","SS"); Compare("ı","I"); Compare("ſ","S");
                Compare("a\0b","A\0B"); Compare("é","e\u0301"); Compare("\ud800a","\ud800A");
                try { string.Equals(null,null,(StringComparison)(-1)); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                try { "a".Equals("a",(StringComparison)6); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                try { string missing=null; missing.Equals("x",(StringComparison)6); } catch(NullReferenceException) { Console.WriteLine("receiver"); }
                return 42;
            }
        }
        """, "False\nTrue\nTrue\nTrue\nTrue\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nTrue\nTrue\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nTrue\nTrue\nFalse\nFalse\nFalse\nFalse\nTrue\nTrue\ncomparisonType\ncomparisonType\nreceiver\n",42);

    [Fact]
    public async Task HostOrdinalCasingMatchesManagedUnicodePairs()
    {
        var pairs = new List<object[]>();
        void Add(string a, string b) => pairs.Add([a,b,string.Equals(a,b,StringComparison.OrdinalIgnoreCase) ? 1 : 0]);
        // Every scalar's invariant upper/lower pair, plus all ASCII pair combinations.
        for(int code=0;code<=0x10ffff;code++)
        {
            if(!Rune.IsValid(code)) continue;
            var rune=new Rune(code);
            var upper=Rune.ToUpperInvariant(rune); var lower=Rune.ToLowerInvariant(rune);
            if(upper!=rune) Add(rune.ToString(),upper.ToString());
            if(lower!=rune) Add(rune.ToString(),lower.ToString());
        }
        for(int a=0;a<128;a++) for(int b=0;b<128;b++) Add(((char)a).ToString(),((char)b).ToString());
        Add("ß","SS"); Add("ß","ẞ"); Add("ﬃ","FFI"); Add("ᾀ","ἈΙ"); Add("Σ","ς");
        Add("🙂","🙃");
        using var app=CanvasTests.Read(Frontend.Roslyn.Compile("public static class Program { public static int Main() => 42; }"));
        await ExportHost.RunAsync(new WasmEmitter().Emit(app), """
            const equals=globalThis.xuiRuntimeImports['xui-string']['equals-ordinal-ignore-case'];
            // Keep invalid UTF-16 out of the JSON serializer, which replaces it.
            assert.equal(equals('\ud800a','\ud800A'),1);
            assert.equal(equals('\ud800','\ud801'),0);
            for(const [a,b,expected] of PAIRS) {
                assert.equal(equals(a,b),expected,JSON.stringify([a,b]));
                assert.equal(equals(b,a),expected,JSON.stringify([b,a]));
            }
            """.Replace("PAIRS",JsonSerializer.Serialize(pairs)));
    }

    [Fact]
    public async Task CultureComparisonFailsExplicitly()
    {
        using var app=CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System;
            public static class Program { public static int Main() {
                int caught=0;
                for(int mode=0;mode<4;mode++) {
                    try { string.Equals("a","A",(StringComparison)mode); }
                    catch(NotSupportedException) { caught++; }
                }
                return caught;
            } }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app,..framework.Assemblies]);
        await ExportHost.RunAsync(new WasmEmitter().Emit(app), """
            const instance=await WebAssembly.instantiate(module,globalThis.xuiRuntimeImports);
            assert.equal(instance.exports.main(),4);
            """);
    }
}
