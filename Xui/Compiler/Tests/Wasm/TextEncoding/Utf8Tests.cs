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
public class Utf8Tests
{
    [Fact]
    public void ManagedDecoderMatchesClrForMalformedAndTruncatedSequences()
    {
        var reference=new UTF8Encoding(false,false);
        var actual=new Xui.Compiler.Runtime.Text.UTF8Encoding(false,false);
        var strict=new Xui.Compiler.Runtime.Text.UTF8Encoding(false,true);
        var strictReference=new UTF8Encoding(false,true);
        var random=new Random(9371);
        List<byte[]> cases=Enumerable.Range(0,256).Select(i=>new[]{(byte)i}).ToList();
        cases.AddRange([new byte[]{0xe1,0x80},[0xe0,0x80,0x80],[0xed,0xa0,0x80],[0xf4,0x90,0x80,0x80],
            [0xef,0xbb,0xbf],[0xf0,0x9f,0x98,0x80],[0xe1,0x80,0x41]]);
        for(int i=0;i<2000;i++) { var bytes=new byte[random.Next(0,14)]; random.NextBytes(bytes); cases.Add(bytes); }
        foreach(var bytes in cases)
        {
            Assert.Equal(reference.GetString(bytes),actual.GetString(bytes));
            Assert.Equal(reference.GetCharCount(bytes),actual.GetCharCount(bytes));
            string? expected = null;
            bool invalid = false;
            try { expected = strictReference.GetString(bytes); }
            catch (DecoderFallbackException) { invalid = true; }
            if (invalid) Assert.Throws<DecoderFallbackException>(() => strict.GetString(bytes));
            else Assert.Equal(expected, strict.GetString(bytes));
        }
    }

    [Fact]
    public void ManagedEncoderMatchesClrForUnicodeAndUnpairedSurrogates()
    {
        var reference=new UTF8Encoding(false,false); var actual=new Xui.Compiler.Runtime.Text.UTF8Encoding(false,false);
        foreach(int scalar in Enumerable.Range(0,0x110000/257).Select(i=>i*257).Concat([0x7f,0x80,0x7ff,0x800,0xffff,0x10000,0x10ffff]))
        {
            if(scalar is >=0xd800 and <=0xdfff) continue;
            string text=char.ConvertFromUtf32(scalar);
            Assert.Equal(reference.GetBytes(text),actual.GetBytes(text));
            Assert.Equal(text,actual.GetString(actual.GetBytes(text)));
        }
        for(int value=0xd800;value<=0xdfff;value++)
        {
            string text=new string((char)value,1)+"x";
            Assert.Equal(reference.GetBytes(text),actual.GetBytes(text));
            Assert.Throws<EncoderFallbackException>(()=>new Xui.Compiler.Runtime.Text.UTF8Encoding(false,true).GetBytes(text));
        }
    }

    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public async Task BrowserProfileConvertsManagedSpansAndNativeStrings(OptimizationLevel optimization)
    {
        const string source="""
            using System;
            using System.Text;
            public static class Program {
                static void Bytes(byte[] values) { foreach(byte value in values) Console.WriteLine((int)value); }
                static void Units(string value) { Console.WriteLine(value.Length); foreach(char unit in value) Console.WriteLine((int)unit); }
                public static int Main() {
                    Encoding utf8=Encoding.UTF8;
                    foreach(string text in new string[] {"ASCII\0","Résumé","Български","😀","\ud800X","\udfff"}) {
                        byte[] bytes=utf8.GetBytes(text); Bytes(bytes); Units(utf8.GetString(bytes));
                        Console.WriteLine(utf8.GetByteCount(text)); Console.WriteLine(utf8.GetCharCount(bytes));
                    }
                    Units(utf8.GetString(new byte[]{0xe1,0x80,0x41,0xff}));
                    Bytes(utf8.GetPreamble()); Bytes(new UTF8Encoding().GetPreamble());
                    var strict=new UTF8Encoding(false,true);
                    try { strict.GetString(new byte[]{0xed,0xa0,0x80}); } catch(DecoderFallbackException) { Console.WriteLine("decode"); }
                    try { strict.GetBytes("\ud800"); } catch(EncoderFallbackException) { Console.WriteLine("encode"); }
                    var buffer=new byte[10]; Console.WriteLine(utf8.GetBytes("é",0,1,buffer,2)); Bytes(buffer);
                    var chars=new char[5]; Console.WriteLine(utf8.GetChars(buffer,2,2,chars,1)); Console.WriteLine((int)chars[1]);
                    Console.WriteLine(utf8.GetMaxByteCount(2)); Console.WriteLine(utf8.GetMaxCharCount(2));
                    Console.WriteLine(utf8.CodePage); Console.WriteLine(utf8.WebName);
                    try { utf8.GetBytes((string)null); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                    try { utf8.GetBytes("é".AsSpan(),new byte[1]); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                    return 42;
                }
            }
            """;
        byte[] image=Frontend.Roslyn.Compile(source,optimizationLevel:optimization);
        var expected=GcTests.RunManaged(image);
        using var app=CanvasTests.Read(image);
        using var runtime=CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Text.Encoding).Assembly.Location));
        using var framework=new Frontend.Framework("System.Memory.dll");
        _=new Xui.Compiler.XuiNet.Compilation([app,runtime,..framework.Assemblies],BrowserEncoding.Replacements(framework.Assemblies[0],runtime));
        byte[] wasm;
        try { wasm=new WasmEmitter().Emit(app,new ConsoleWriteLineRewriter()); }
        catch(NotSupportedException e) { throw new NotSupportedException(e.Message+"\n"+e.Data["Xui.DependencyPath"],e); }
        Assert.Equal(expected,await Node.ExecuteAsync(wasm));
    }
}
