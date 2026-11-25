using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class CopyTests
{
    [Fact]
    public Task CopiesAndOverlapsPreserveElementOrder() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Print(int[] a) { for(int i=0;i<a.Length;i++) Console.WriteLine(a[i]); }
            public static int Main() {
                var a=new int[5]; for(int i=0;i<a.Length;i++) a[i]=i+1;
                var b=new int[5]; Array.Copy(a,b,5); a[0]=99; Print(b);
                Array.Copy(b,0,b,1,4); Print(b);
                Array.Copy(b,1,b,0,4); Print(b);
                Array.Copy(b,5,b,5,0);
                var wide=new long[1]; wide[0]=long.MinValue; var copy=new long[1]; Array.Copy(wide,copy,1); Console.WriteLine(copy[0]);
                var floats=new float[1]; floats[0]=1.25f; var target=new float[1]; Array.Copy(floats,target,1); Console.WriteLine(target[0]);
                return 42;
            }
        }
        """, "1\n2\n3\n4\n5\n1\n1\n2\n3\n4\n1\n2\n3\n4\n4\n-9223372036854775808\n1.25\n", 42);

    [Fact]
    public Task ReferenceCopiesPreserveIdentityAndNulls() => InlineArrayTests.AssertLinked("""
        using System;
        public class Item { public int Value; }
        public static class Program { public static int Main() {
            var source=new Item[2]; source[0]=new Item { Value=1 };
            var target=new Item[2]; Array.Copy(source,target,2);
            target[0].Value=42; Console.WriteLine(source[0].Value); Console.WriteLine(target[1]==null);
            var text=new string[2]; text[0]="🙂"; var copied=new string[2]; Array.Copy(text,copied,2);
            Console.WriteLine(copied[0]); Console.WriteLine(copied[1]==null);
            return 42;
        } }
        """, "42\nTrue\n🙂\nTrue\n", 42);

    [Fact]
    public Task InvalidArgumentsThrowBeforeChangingDestination() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            var a=new int[2]; a[0]=42;
            try { Array.Copy(null,a,0); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            try { Array.Copy(a,null,0); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            try { Array.Copy(null,null,0); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            try { Array.Copy(a,a,-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
            try { Array.Copy(a,-1,a,0,0); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
            try { Array.Copy(a,0,a,-1,0); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
            try { Array.Copy(a,0,a,1,2); } catch(ArgumentException) { Console.WriteLine("destination range"); }
            try { Array.Copy(a,1,a,0,2); } catch(ArgumentException) { Console.WriteLine("source range"); }
            try { Array.Copy(a,int.MaxValue,a,0,int.MaxValue); } catch(ArgumentException) { Console.WriteLine("overflow range"); }
            Console.WriteLine(a[0]); Console.WriteLine(a[1]); return 42;
        } }
        """, "sourceArray\ndestinationArray\nsourceArray\nlength\nsourceIndex\ndestinationIndex\ndestination range\nsource range\noverflow range\n42\n0\n", 42);

    [Fact]
    public Task StructCopiesPreserveElementStorageAndOverlapOrder() => InlineArrayTests.AssertLinked("""
        using System;
        public class Item { public int Value; }
        public struct Point { public int X; public string Text; public Item Item; }
        public static class Program {
            static void Print(Point[] values) { for(int i=0;i<values.Length;i++) Console.WriteLine(values[i].X); }
            public static int Main() {
                var a = new Point[4]; for(int i=0;i<4;i++) a[i] = new Point { X=i+1, Text="source", Item=new Item { Value=i } };
                var b = new Point[4]; ref Point saved = ref b[1]; Array.Copy(a,b,4);
                saved.X = 12; saved.Text = "changed"; saved.Item.Value = 42;
                Console.WriteLine(a[1].X); Console.WriteLine(a[1].Text); Console.WriteLine(a[1].Item.Value);
                Array.Copy(b,0,b,1,3); Print(b); Console.WriteLine(saved.X);
                Array.Copy(b,1,b,0,3); Print(b); Array.Copy(b,4,b,4,0);
                return 42;
            }
        }
        """, "2\nsource\n42\n1\n1\n12\n3\n1\n1\n12\n3\n3\n", 42);
}
