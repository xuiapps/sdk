using System.Globalization;
using System.Numerics;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

[Collection(nameof(ManagedConsoleCollection))]
public class CheckedArithmeticTests
{
    [Theory]
    [InlineData(false,false)] [InlineData(false,true)]
    [InlineData(true,false)] [InlineData(true,true)]
    public async Task NativeCheckedArithmeticUsesWasm32Bounds(bool unsigned, bool subtract)
    {
        string type=unsigned ? "nuint" : "nint";
        string boundary=unsigned ? subtract ? "0" : "uint.MaxValue" : subtract ? "int.MinValue" : "int.MaxValue";
        using var app=Canvas.CanvasTests.Read(Frontend.Roslyn.Compile($$"""
            using System;
            public static class Program {
                static {{type}} Compute({{type}} a,{{type}} b)=>checked(a {{(subtract ? "-" : "+")}} b);
                public static int Main() {
                    try { Compute(({{type}}){{boundary}},({{type}})1); return 1; }
                    catch(OverflowException) { return 42; }
                }
            }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app,..framework.Assemblies]);
        Assert.Equal(42,(await Repl.Node.ExecuteAsync(new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app))).ExitCode);
    }

    [Theory]
    [InlineData("int",false)] [InlineData("int",true)]
    [InlineData("uint",false)] [InlineData("uint",true)]
    [InlineData("long",false)] [InlineData("long",true)]
    [InlineData("ulong",false)] [InlineData("ulong",true)]
    public Task CheckedAddAndSubtractMatchClrAtBothBounds(string type, bool subtract)
    {
        bool unsigned=type is "uint" or "ulong";
        int bits=type is "int" or "uint" ? 32 : 64;
        BigInteger min=unsigned ? 0 : -(BigInteger.One<<(bits-1));
        BigInteger max=(BigInteger.One<<(unsigned ? bits : bits-1))-1;
        List<(BigInteger A,BigInteger B)> pairs=[(0,1),(max,0),(max,1),(min,0),(min,1),(min,min),(max,max),(0,min)];
        if(!unsigned) pairs.AddRange([(min,-1),(max,-1),(-1,min),(42,-1)]);
        string suffix=type switch { "uint"=>"U", "long"=>"L", "ulong"=>"UL", _=>"" };
        string Literal(BigInteger value)=>value.ToString(CultureInfo.InvariantCulture)+suffix;
        string calls=string.Join("\n",pairs.Select(p=>{
            var result=subtract ? p.A-p.B : p.A+p.B;
            return $"Show({Literal(p.A)},{Literal(p.B)},{Literal(result<min || result>max ? 0 : result)});";
        }));
        string output=string.Concat(pairs.Select(p=>{
            var result=subtract ? p.A-p.B : p.A+p.B;
            return (result<min || result>max ? "overflow" : "True")+"\n";
        }));
        return InlineArrayTests.AssertLinked($$"""
            using System;
            public static class Program {
                static {{type}} Compute({{type}} a,{{type}} b)=>checked(a {{(subtract ? "-" : "+")}} b);
                static void Show({{type}} a,{{type}} b,{{type}} expected) {
                    try { Console.WriteLine(Compute(a,b)==expected); } catch(OverflowException) { Console.WriteLine("overflow"); }
                }
                public static int Main() { {{calls}} return 42; }
            }
            """,output,42);
    }
}
