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
public class DecoderTests
{
    [Theory]
    [InlineData(65001)]
    [InlineData(1200)]
    [InlineData(1201)]
    public void EveryByteBoundaryMatchesClrIncludingMalformedInput(int codePage)
    {
        var random = new Random(1003);
        foreach (bool strict in new[] {false,true})
        {
            Encoding reference = codePage == 65001 ? new UTF8Encoding(false,strict) : new UnicodeEncoding(codePage==1201,false,strict);
            Xui.Compiler.Runtime.Text.Encoding actual = codePage == 65001 ? new Xui.Compiler.Runtime.Text.UTF8Encoding(false,strict) : new Xui.Compiler.Runtime.Text.UnicodeEncoding(codePage==1201,false,strict);
            List<byte[]> cases = [reference.GetBytes("AéЖ🙂\0\ufeff"), [0xf0,0x9f,0x99], [0,0xd8,1], [0xe1,0x80,65]];
            for(int i=0;i<100;i++) { byte[] data=new byte[random.Next(0,15)];random.NextBytes(data);cases.Add(data); }
            foreach(var bytes in cases)
                for(int split=0;split<=bytes.Length;split++)
                {
                    var expectedDecoder=reference.GetDecoder();var decoder=actual.GetDecoder();
                    var expectedChars=new char[40];var actualChars=new char[40];
                    int written=0;
                    bool failed=false;
                    try { written=expectedDecoder.GetChars(bytes,0,split,expectedChars,0,false); }
                    catch(DecoderFallbackException) { failed=true; }
                    if(failed) { Assert.Throws<DecoderFallbackException>(()=>decoder.GetChars(bytes,0,split,actualChars,0,false));continue; }
                    Assert.Equal(written,decoder.GetChars(bytes,0,split,actualChars,0,false));
                    int count=0;
                    try { count=expectedDecoder.GetCharCount(bytes,split,bytes.Length-split,true); }
                    catch(DecoderFallbackException) { failed=true; }
                    if(failed) { Assert.Throws<DecoderFallbackException>(()=>decoder.GetCharCount(bytes,split,bytes.Length-split,true));continue; }
                    Assert.Equal(count,decoder.GetCharCount(bytes,split,bytes.Length-split,true));
                    Assert.Equal(expectedDecoder.GetChars(bytes,split,bytes.Length-split,expectedChars,written,true),decoder.GetChars(bytes,split,bytes.Length-split,actualChars,written,true));
                    Assert.Equal(expectedChars,actualChars);
                }
        }
    }

    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public async Task BrowserDecoderCarriesPartialCharactersAndResets(OptimizationLevel optimization)
    {
        const string source = """
            using System;
            using System.Text;
            public static class Program {
                public static int Main() {
                    foreach(Encoding encoding in new[]{Encoding.UTF8,Encoding.Unicode,Encoding.BigEndianUnicode}) {
                        byte[] input=encoding.GetBytes("AéЖ🙂\0Z");
                        for(int chunk=1;chunk<=5;chunk++) {
                            var decoder=encoding.GetDecoder();var chars=new char[20];int written=0;
                            for(int offset=0;offset<input.Length;) {
                                int count=Math.Min(chunk,input.Length-offset);bool flush=offset+count==input.Length;
                                Console.WriteLine(decoder.GetCharCount(input,offset,count,flush));
                                written+=decoder.GetChars(input,offset,count,chars,written,flush); offset+=count;
                            }
                            Console.WriteLine(new string(chars,0,written));
                        }
                        var convert=encoding.GetDecoder();var small=new char[2];int position=0;string result="";
                        while(position<input.Length) {
                            convert.Convert(input,position,input.Length-position,small,0,2,true,out int used,out int written,out bool done);
                            result+=new string(small,0,written);position+=used;
                        }
                        Console.WriteLine(result);
                    }
                    var strict=new UTF8Encoding(false,true).GetDecoder();var output=new char[4];
                    Console.WriteLine(strict.GetChars(new byte[]{0xf0,0x9f},0,2,output,0,false));
                    try { strict.GetChars(new byte[0],0,0,output,0,true); } catch(DecoderFallbackException) {Console.WriteLine("flush");}
                    strict.Reset();Console.WriteLine(strict.GetChars(new byte[]{65},0,1,output,0,true));Console.WriteLine((int)output[0]);
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
        try {wasm=new WasmEmitter().Emit(app,new ConsoleWriteLineRewriter());}
        catch(Exception e) when(e.Data["Xui.DependencyPath"] is {} path) {throw new Exception(e.Message+"\n"+path,e);}
        Assert.Equal(expected,await Node.ExecuteAsync(wasm));
    }
}
