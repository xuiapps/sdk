using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class ClearTests
{
    [Fact]
    public Task ClearsPrimitiveAndReferenceElements() => InlineArrayTests.AssertLinked("""
        using System;
        public class Item { public int Value; }
        public static class Program { public static int Main() {
            var a=new int[4]; for(int i=0;i<4;i++) a[i]=i+1; Array.Clear(a,1,2);
            for(int i=0;i<a.Length;i++) Console.WriteLine(a[i]);
            Array.Clear(a); Console.WriteLine(a[0]); Console.WriteLine(a[3]);
            var wide=new long[1]; wide[0]=long.MinValue; Array.Clear(wide); Console.WriteLine(wide[0]);
            var floats=new float[1]; floats[0]=1.25f; Array.Clear(floats); Console.WriteLine(floats[0]);
            var doubles=new double[1]; doubles[0]=2.5; Array.Clear(doubles); Console.WriteLine(doubles[0]);
            var text=new string[1]; text[0]="hello"; Array.Clear(text); Console.WriteLine(text[0]==null);
            var objects=new Item[1]; objects[0]=new Item { Value=42 }; var saved=objects[0];
            Array.Clear(objects); Console.WriteLine(objects[0]==null); Console.WriteLine(saved.Value);
            Array.Clear(a,4,0); Array.Clear(new int[0]); return 42;
        } }
        """, "1\n0\n0\n4\n0\n0\n0\n0\n0\nTrue\nTrue\n42\n",42);

    [Fact]
    public Task PreservesStructElementAddressesAndClearsNestedReferences() => InlineArrayTests.AssertLinked("""
        using System;
        public struct Point { public double X; public int Y; }
        public struct Entry { public Point Point; public string Text; public object Object; }
        public static class Program { public static int Main() {
            var entries=new Entry[2]; entries[0]=new Entry { Point=new Point { X=2.5,Y=7 },Text="hello",Object=new object() };
            entries[1].Text="keep";
            ref Entry saved=ref entries[0]; ref double x=ref entries[0].Point.X;
            Array.Clear(entries,0,1);
            Console.WriteLine(x); Console.WriteLine(saved.Point.Y); Console.WriteLine(saved.Text==null); Console.WriteLine(saved.Object==null);
            saved.Text="changed"; x=42; Console.WriteLine(entries[0].Text); Console.WriteLine(entries[0].Point.X); Console.WriteLine(entries[1].Text);
            var points=new Point[1]; points[0].X=8; ref Point point=ref points[0]; Array.Clear(points); Console.WriteLine(point.X);
            return 42;
        } }
        """, "0\n0\nTrue\nTrue\nchanged\n42\nkeep\n0\n",42);

    [Fact]
    public Task InvalidRangesThrowBeforeClearing() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            var a=new int[2]; a[0]=42;
            try { Array.Clear(null); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            try { Array.Clear(null,-1,-1); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            try { Array.Clear(a,-1,0); } catch(IndexOutOfRangeException) { Console.WriteLine("index"); }
            try { Array.Clear(a,0,-1); } catch(IndexOutOfRangeException) { Console.WriteLine("length"); }
            try { Array.Clear(a,1,2); } catch(IndexOutOfRangeException) { Console.WriteLine("range"); }
            try { Array.Clear(a,int.MaxValue,int.MaxValue); } catch(IndexOutOfRangeException) { Console.WriteLine("overflow"); }
            Console.WriteLine(a[0]); return 42;
        } }
        """, "array\narray\nindex\nlength\nrange\noverflow\n42\n",42);
}
