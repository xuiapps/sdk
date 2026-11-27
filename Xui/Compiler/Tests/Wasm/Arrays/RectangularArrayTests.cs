using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class RectangularArrayTests
{
    [Fact]
    public Task DimensionsGenericElementsAndRowMajorAccessMatchClr() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static T[,] Pair<T>(T left,T right) { var values=new T[2,3];values[0,2]=left;values[1,0]=right;return values; }
            public static int Main() {
                var values=Pair(20,22);
                Console.WriteLine(values.Length);Console.WriteLine(values.Rank);
                for(int d=0;d<2;d++) { Console.WriteLine(values.GetLength(d));Console.WriteLine(values.GetLowerBound(d));Console.WriteLine(values.GetUpperBound(d)); }
                int sum=0;for(int row=0;row<2;row++)for(int column=0;column<3;column++)sum+=values[row,column];
                var cube=new double[2,3,4];cube[1,2,3]=1.25;Console.WriteLine(cube[1,2,3]);Console.WriteLine(cube[0,0,0]);
                var empty=new int[3,0,4];Console.WriteLine(empty.Length);Console.WriteLine(empty.GetUpperBound(1));
                Console.WriteLine(Pair("a","b")[1,0]);return sum;
            }
        }
        ""","6\n2\n2\n0\n1\n3\n0\n2\n1.25\n0\n0\n-1\nb\n",42);

    [Fact]
    public Task StructElementCopiesPreserveOutstandingAddresses() => InlineArrayTests.AssertLinked("""
        using System;
        public struct Point { public double X,Y; }
        public struct Item { public string Name; public Point Position; }
        public static class Program { public static int Main() {
            var items=new Item[2,2];ref Item alias=ref items[1,1];
            items[1,1]=new Item {Name="original",Position=new Point {X=20,Y=22}};
            Item copy=items[1,1];items[0,0]=copy;alias.Name="updated";alias.Position.X=30;
            Console.WriteLine(items[1,1].Name);Console.WriteLine(items[0,0].Name);Console.WriteLine(copy.Position.X);
            var points=new Point[1,2];points[0,1]=new Point {X=20,Y=22};ref double x=ref points[0,1].X;x=21;
            Console.WriteLine(points[0,1].X);
            var strings=new string[1,1];ref string text=ref strings[0,0];text="native";Console.WriteLine(strings[0,0]);
            return (int)(copy.Position.X+copy.Position.Y);
        } }
        ""","updated\noriginal\n20\n21\nnative\n",42);

    [Fact]
    public Task NullAndEveryDimensionBoundsThrowManagedExceptions() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static int Negative()=>-1;
            public static int Main() {
                int caught=0;var a=new int[2,3];int[,] missing=null;
                try { Console.WriteLine(missing[0,0]); } catch(NullReferenceException) { caught++; }
                try { Console.WriteLine(missing.GetLength(0)); } catch(NullReferenceException) { caught++; }
                try { a[0,3]=7; } catch(IndexOutOfRangeException) { caught++; }
                try { a[2,0]=7; } catch(IndexOutOfRangeException) { caught++; }
                try { a[Negative(),0]=7; } catch(IndexOutOfRangeException) { caught++; }
                try { Console.WriteLine(a.GetLength(2)); } catch(IndexOutOfRangeException) { caught++; }
                try { Console.WriteLine(a.GetLowerBound(-1)); } catch(IndexOutOfRangeException) { caught++; }
                try { var bad=new int[Negative(),2]; } catch(OverflowException) { caught++; }
                Console.WriteLine(caught);return caught==8?42:1;
            }
        }
        ""","8\n",42);
}
