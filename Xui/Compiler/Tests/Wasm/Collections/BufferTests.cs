using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Collections;

[Collection(nameof(ManagedConsoleCollection))]
public class BufferTests
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task PrimitiveByteViewsMatchClrForUnalignedAndOverlappingCopies(OptimizationLevel optimization)
    {
        byte[] pe = Frontend.Roslyn.Compile("""
            using System;
            public static class Program {
                static int Hash(byte[] bytes) { int h=17; foreach(byte b in bytes)h=unchecked(h*31+b); return h; }
                static void Check<T>(T[] values, int size) {
                    byte[] bytes=new byte[values.Length*size]; Buffer.BlockCopy(values,0,bytes,0,bytes.Length);
                    Console.WriteLine(Hash(bytes));
                    var copy=new T[values.Length]; Buffer.BlockCopy(bytes,0,copy,0,bytes.Length);
                    var result=new byte[bytes.Length]; Buffer.BlockCopy(copy,0,result,0,result.Length);
                    Console.WriteLine(Hash(result)==Hash(bytes));
                }
                public static int Main() {
                    Check(new byte[]{0,128,255},1); Check(new sbyte[]{0,-128,127,-1},1); Check(new bool[]{false,true,true,false},1);
                    Check(new char[]{'x','Ж','\ud800','\udfff'},2); Check(new short[]{-1,short.MinValue,3},2); Check(new ushort[]{0,ushort.MaxValue,3},2);
                    Check(new int[]{-1,int.MinValue,42},4); Check(new uint[]{0,uint.MaxValue,42},4);
                    Check(new long[]{-1,long.MinValue,42},8); Check(new ulong[]{0,ulong.MaxValue,42},8);
                    Check(new float[]{1.5f,-0f,BitConverter.Int32BitsToSingle(unchecked((int)0x7fc12345))},4);
                    Check(new double[]{1.5d,-0d,BitConverter.Int64BitsToDouble(unchecked((long)0x7ff8123456789abc))},8);
                    int[] overlap={0x12345678,0x23456789,0x3456789a,0x456789ab};
                    Buffer.BlockCopy(overlap,1,overlap,3,9); Check(overlap,4);
                    Buffer.BlockCopy(overlap,4,overlap,1,9); Check(overlap,4);
                    byte[] raw={1,2,3,4,5}; Buffer.BlockCopy(raw,0,raw,1,4); Check(raw,1);
                    Buffer.BlockCopy(raw,1,raw,0,4); Check(raw,1);
                    Buffer.BlockCopy(raw,raw.Length,raw,raw.Length,0);
                    try { Buffer.BlockCopy(null,0,raw,0,0); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                    try { Buffer.BlockCopy(raw,0,null,0,0); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                    try { Buffer.BlockCopy(raw,-1,raw,0,0); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { Buffer.BlockCopy(raw,0,raw,-1,0); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { Buffer.BlockCopy(raw,0,raw,0,-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { Buffer.BlockCopy(raw,int.MaxValue,raw,0,int.MaxValue); } catch(ArgumentException) { Console.WriteLine("range"); }
                    try { Buffer.BlockCopy(new object[1],0,raw,0,1); } catch(ArgumentException) { Console.WriteLine("references"); }
                    return 42;
                }
            }
            """, optimizationLevel: optimization);
        var expected = GcTests.RunManaged(pe);
        using var app = CanvasTests.Read(pe);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Equal(expected, await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter())));
    }
}
