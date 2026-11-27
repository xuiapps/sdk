using System.Text;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TextEncoding;

[Collection(nameof(ManagedConsoleCollection))]
public class Utf16Tests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ManagedConversionsMatchClrIncludingMalformedUnits(bool bigEndian)
    {
        var expected = new UnicodeEncoding(bigEndian, true);
        var actual = new Xui.Compiler.Runtime.Text.UnicodeEncoding(bigEndian, true);
        var strict = new Xui.Compiler.Runtime.Text.UnicodeEncoding(bigEndian, false, true);
        var strictExpected = new UnicodeEncoding(bigEndian, false, true);
        var random = new Random(794);
        for (int i = 0; i < 2000; i++)
        {
            byte[] bytes = new byte[random.Next(0,20)]; random.NextBytes(bytes);
            Assert.Equal(expected.GetString(bytes), actual.GetString(bytes));
            Assert.Equal(expected.GetCharCount(bytes), actual.GetCharCount(bytes));
            string? decoded = null;
            bool invalid = false;
            try { decoded = strictExpected.GetString(bytes); } catch (DecoderFallbackException) { invalid = true; }
            if (invalid) Assert.Throws<DecoderFallbackException>(() => strict.GetString(bytes));
            else Assert.Equal(decoded, strict.GetString(bytes));
        }
        for (int unit = 0; unit <= 65535; unit++)
        {
            string text = new string((char)unit,1) + "🙂";
            Assert.Equal(expected.GetBytes(text), actual.GetBytes(text));
            if (unit is >= 0xd800 and <= 0xdfff) Assert.Throws<EncoderFallbackException>(() => strict.GetBytes(text));
        }
        foreach (int count in new[] {0,1,2,100,100000})
        {
            Assert.Equal(expected.GetMaxByteCount(count),actual.GetMaxByteCount(count));
            Assert.Equal(expected.GetMaxCharCount(count),actual.GetMaxCharCount(count));
        }
        Assert.Equal(expected.GetMaxCharCount(int.MaxValue),actual.GetMaxCharCount(int.MaxValue));
    }

    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public async Task BrowserProfilePreservesEndianPreamblesAndStrictFailures(OptimizationLevel optimization)
    {
        const string source = """
            using System;
            using System.Text;
            public static class Program {
                static void Bytes(byte[] bytes) { foreach(byte b in bytes) Console.WriteLine((int)b); }
                static void Units(string text) { foreach(char c in text) Console.WriteLine((int)c); }
                public static int Main() {
                    foreach(string name in new[]{"utf-8","UTF-8","utf-16","unicode","utf-16BE","unicodeFFFE"}) {
                        var encoding=Encoding.GetEncoding(name); Console.WriteLine(encoding.CodePage); Bytes(encoding.GetPreamble());
                    }
                    try { Encoding.GetEncoding((string)null); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                    foreach(Encoding encoding in new[]{Encoding.Unicode,Encoding.BigEndianUnicode}) {
                        Console.WriteLine(encoding.CodePage); Console.WriteLine(encoding.WebName);
                        Bytes(encoding.GetPreamble());
                        foreach(string text in new[]{"A\0Ж🙂","\ud800X","\udfff","\ud800\ud800\udfff"}) {
                            var bytes=encoding.GetBytes(text); Bytes(bytes); Units(encoding.GetString(bytes));
                        }
                        Units(encoding.GetString(new byte[]{0,0xd8,1}));
                        Console.WriteLine(encoding.GetMaxCharCount(3)); Console.WriteLine(encoding.GetMaxByteCount(3));
                    }
                    var strict=new UnicodeEncoding(false,false,true);
                    try { strict.GetString(new byte[]{0,0xd8}); } catch(DecoderFallbackException) { Console.WriteLine("decode"); }
                    try { strict.GetBytes("\ud800"); } catch(EncoderFallbackException) { Console.WriteLine("encode"); }
                    try { Encoding.Unicode.GetBytes("abc".AsSpan(),new byte[1]); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                    return 42;
                }
            }
            """;
        byte[] image = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
        var expected = GcTests.RunManaged(image);
        using var app = CanvasTests.Read(image);
        using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Text.Encoding).Assembly.Location));
        using var framework = new Frontend.Framework("System.Memory.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app,runtime,..framework.Assemblies],BrowserEncoding.Replacements(framework.Assemblies[0],runtime));
        byte[] wasm;
        try { wasm = new WasmEmitter().Emit(app,new ConsoleWriteLineRewriter()); }
        catch (Exception e) when (e.Data["Xui.DependencyPath"] is { } path) { throw new Exception(e.Message + "\n" + path,e); }
        Assert.Equal(expected,await Node.ExecuteAsync(wasm));
    }
}
