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
    public Task ClassEqualityPreservesNullIdentityAndVirtualOverrides() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public class Plain { }
        public class Base {
            public int Value;
            public override bool Equals(object other) => other is Base b && Value == b.Value;
            public override int GetHashCode() => Value;
        }
        public class Derived : Base {
            public int Calls;
            public override bool Equals(object other) { Calls++; return false; }
            public override int GetHashCode() => 0;
        }
        public class Hidden : Base { public new virtual bool Equals(object other) => false; }
        public class HiddenChild : Hidden { public override bool Equals(object other) => false; }
        public static class Program { public static int Main() {
            var identity=EqualityComparer<Plain>.Default; var p=new Plain();
            Console.WriteLine(identity.Equals(null,null)); Console.WriteLine(identity.Equals(p,null));
            Console.WriteLine(identity.Equals(null,p)); Console.WriteLine(identity.Equals(p,p));
            Console.WriteLine(identity.Equals(p,new Plain()));
            var c=EqualityComparer<Base>.Default;
            Console.WriteLine(c.Equals(new Base { Value=7 },new Base { Value=7 }));
            Console.WriteLine(c.Equals(new Base { Value=7 },new Base { Value=8 }));
            var d=new Derived(); Console.WriteLine(c.Equals(d,d)); Console.WriteLine(d.Calls);
            Console.WriteLine(c.Equals(new HiddenChild { Value=7 },new Base { Value=7 }));
            return 42;
        } }
        """, "True\nFalse\nFalse\nTrue\nFalse\nTrue\nFalse\nFalse\n1\nTrue\n", 42);

    [Fact]
    public Task ArraySearchUsesDefaultClassEquality() => InlineArrayTests.AssertLinked("""
        using System;
        public class Item {
            public int Value;
            public override bool Equals(object other) => other is Item item && Value == item.Value;
            public override int GetHashCode() => Value;
        }
        public static class Program { public static int Main() {
            var a=new Item[4]; a[1]=new Item { Value=42 }; a[3]=new Item { Value=42 };
            Console.WriteLine(Array.IndexOf(a,new Item { Value=42 }));
            Console.WriteLine(Array.IndexOf(a,new Item { Value=42 },2,2));
            Console.WriteLine(Array.IndexOf(a,new Item { Value=43 }));
            Console.WriteLine(Array.IndexOf(a,null));
            Console.WriteLine(Array.IndexOf(a,null,4,0));
            return 42;
        } }
        """, "1\n3\n-1\n0\n-1\n", 42);

    [Fact]
    public Task ArraySearchUsesValueEqualityAndManagedBoundsFailures() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            var numbers=new double[2]; numbers[0]=double.NaN; numbers[1]=-0d;
            Console.WriteLine(Array.IndexOf(numbers,double.NaN)); Console.WriteLine(Array.IndexOf(numbers,0d));
            var text=new string[3]; text[1]="🙂"; text[2]="A";
            Console.WriteLine(Array.IndexOf(text,"🙂")); Console.WriteLine(Array.IndexOf(text,"a"));
            Console.WriteLine(Array.IndexOf(text,null)); Console.WriteLine(Array.IndexOf(text,"A",2));
            try { Array.IndexOf<string>(null,"x"); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            try { Array.IndexOf(text,"x",-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
            try { Array.IndexOf(text,"x",4,0); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
            try { Array.IndexOf(text,"x",0,-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
            try { Array.IndexOf(text,"x",2,int.MaxValue); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
            return 42;
        } }
        """, "0\n1\n1\n-1\n0\n2\narray\nstartIndex\nstartIndex\ncount\ncount\n", 42);

    [Fact]
    public Task InterfaceDispatchRecognizesTheDefaultSingleton() => InlineArrayTests.AssertLinked("""
        using System.Collections.Generic;
        public static class Program { public static int Main() {
            IEqualityComparer<string> c = EqualityComparer<string>.Default;
            return c.Equals("a", "a") ? 42 : 0;
        } }
        """, "", 42);

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
