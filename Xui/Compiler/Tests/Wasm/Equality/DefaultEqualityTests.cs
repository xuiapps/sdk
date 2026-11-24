using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class DefaultEqualityTests
{
    [Fact]
    public Task StringEqualityIsOrdinalAndNullSafe() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public static class Program {
            public static int Main() {
                var c=EqualityComparer<string>.Default;
                Console.WriteLine(c.Equals(null,null)); Console.WriteLine(c.Equals("",null));
                Console.WriteLine(c.Equals(null,"")); Console.WriteLine(c.Equals("",""));
                Console.WriteLine(c.Equals("🙂Ж", "🙂Ж")); Console.WriteLine(c.Equals("A", "a"));
                Console.WriteLine(c==EqualityComparer<string>.Default);
                c=null; try { c.Equals("a","a"); } catch(NullReferenceException) { Console.WriteLine("null comparer"); }
                return 42;
            }
        }
        """, "True\nFalse\nFalse\nTrue\nTrue\nFalse\nTrue\nnull comparer\n", 42);

    [Fact]
    public Task NumericEqualityMatchesNaNZeroAndEnumSemantics() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public enum Tiny : byte { A=1, B=2 }
        public enum Large : ulong { A=ulong.MaxValue }
        public static class Program {
            static void Print<T>(T a,T b) { Console.WriteLine(EqualityComparer<T>.Default.Equals(a,b)); }
            public static int Main() {
                Print(42,42); Print(42,43); Print(ulong.MaxValue,ulong.MaxValue); Print(long.MinValue,long.MaxValue);
                Print(double.NaN,double.NaN); Print(double.NaN,0d); Print(0d,-0d); Print(double.PositiveInfinity,double.PositiveInfinity);
                Print(float.NaN,float.NaN); Print(0f,-0f); Print(true,false); Print('Ж','Ж'); Print(Tiny.A,Tiny.B); Print(Large.A,Large.A);
                return 42;
            }
        }
        """, "True\nFalse\nTrue\nFalse\nTrue\nFalse\nTrue\nTrue\nTrue\nTrue\nFalse\nTrue\nFalse\nTrue\n", 42);

    [Fact]
    public Task StructComparersCallTypedEqualityAndPreserveValueCopies() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public struct Parity : IEquatable<Parity> {
            public int Value;
            public bool Equals(Parity other) { Value++; return (Value & 1) == (other.Value & 1); }
        }
        public struct Explicit<T> : IEquatable<Explicit<T>> {
            public T Value;
            bool IEquatable<Explicit<T>>.Equals(Explicit<T> other) => true;
        }
        public static class Program { public static int Main() {
            Parity a=new Parity { Value=40 }, b=new Parity { Value=1 };
            Console.WriteLine(EqualityComparer<Parity>.Default.Equals(a,b)); Console.WriteLine(a.Value);
            Console.WriteLine(EqualityComparer<Explicit<int>>.Default.Equals(default,default));
            Console.WriteLine(EqualityComparer<Explicit<string>>.Default.Equals(default,default));
            return 42;
        } }
        """, "True\n40\nTrue\nTrue\n", 42);

    [Fact]
    public Task ColorStyleNativeFloatFieldsUseTypedEquals() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        using System.Runtime.InteropServices;
        public struct Color : IEquatable<Color> {
            public NFloat R, G;
            public bool Equals(Color other) => R.Equals(other.R) && G.Equals(other.G);
        }
        public class Box { public NFloat Value; }
        public static class Program { public static int Main() {
            var a=new Color { R=NFloat.NaN, G=(NFloat)0 };
            var b=new Color { R=NFloat.NaN, G=(NFloat)(-0.0) };
            Console.WriteLine(EqualityComparer<Color>.Default.Equals(a,b));
            b.G=(NFloat)1; Console.WriteLine(EqualityComparer<Color>.Default.Equals(a,b));
            var box=new Box { Value=NFloat.NaN }; Console.WriteLine(box.Value.Equals(NFloat.NaN));
            return 42;
        } }
        """, "True\nFalse\nTrue\n", 42);

    [Fact]
    public void InterfaceDispatchDoesNotTreatTheDefaultSingletonAsAnOrdinaryClass()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System.Collections.Generic;
            public static class Program { public static int Main() {
                IEqualityComparer<string> c = EqualityComparer<string>.Default;
                return c.Equals("a", "a") ? 42 : 0;
            } }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Contains("IEqualityComparer dispatch", Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app)).Message);
    }

    [Fact]
    public void CustomComparerDoesNotSilentlyUseDefaultSemantics()
    {
        using var app=CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System.Collections.Generic;
            public class Custom : EqualityComparer<string> {
                public override bool Equals(string a,string b)=>false;
                public override int GetHashCode(string a)=>0;
            }
            public static class Program { public static int Main(){ EqualityComparer<string> c=new Custom(); return c.Equals("a","a")?1:42; } }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app,..framework.Assemblies]);
        var error=Assert.Throws<NotSupportedException>(()=>new WasmEmitter().Emit(app));
        Assert.Contains("Custom EqualityComparer subclasses",error.Message);
    }
}
