using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class PrimitiveStructHashTests
{
    [Theory]
    [InlineData("public struct Payload { public string Text; }")]
    [InlineData("public struct Custom { public int Value; public override int GetHashCode() => 42; } public struct Payload { public Custom Value; }")]
    [InlineData("[System.Runtime.CompilerServices.InlineArray(3)] public struct Payload { private int element; }")]
    public void UnsupportedFieldsAreNotSilentlyFlattened(string declarations)
    {
        using var app = Xui.Compiler.Tests.Wasm.Canvas.CanvasTests.Read(Frontend.Roslyn.Compile(declarations + """
            public static class Program {
                public static int Main() => System.Collections.Generic.EqualityComparer<Payload>.Default.GetHashCode(default);
            }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var error = Assert.Throws<NotSupportedException>(() => new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app));
        Assert.Contains("Default hashing is not implemented for Payload", error.Message);
    }

    [Fact]
    public Task DefaultHashesReadScalarFieldsWithFloatNormalizationAndNoMutation() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        using System.Runtime.InteropServices;
        public enum Small : byte { A = 250 }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Leaf { public byte Small; public double Real; public int Integer; }
        public struct Values<T> { public T Data; public float Single; public long Wide; public Small Kind; public bool Flag; public char Unit; }
        public struct Empty { }
        public struct Matrix { public NFloat A, B, C, D, Tx, Ty; }
        public static class Program {
            static bool Same<T>(T a, T b) => EqualityComparer<T>.Default.GetHashCode(a) == EqualityComparer<T>.Default.GetHashCode(b);
            public static int Main() {
                var a = new Values<Leaf> { Data = new Leaf { Small = 7, Real = 0.0, Integer = 42 }, Single = float.NaN, Wide = -99, Kind = Small.A, Flag = true, Unit = 'Ж' };
                var b = a; b.Data.Real = -0.0; b.Single = BitConverter.Int32BitsToSingle(unchecked((int)0xffc01234));
                Console.WriteLine(Same(a, b));
                Console.WriteLine(a.Data.Small); Console.WriteLine(a.Data.Integer); Console.WriteLine(a.Wide);
                var contract = (IEqualityComparer<Values<Leaf>>)EqualityComparer<Values<Leaf>>.Default;
                Console.WriteLine(contract.GetHashCode(a) == EqualityComparer<Values<Leaf>>.Default.GetHashCode(b));
                Console.WriteLine(Same(new Empty(), new Empty()));
                var matrix = new Matrix { A = 1, D = 1, Tx = 12, Ty = 13 };
                Console.WriteLine(Same(matrix, matrix));
                return 42;
            }
        }
        """, "True\n7\n42\n-99\nTrue\nTrue\nTrue\n", 42);
}
