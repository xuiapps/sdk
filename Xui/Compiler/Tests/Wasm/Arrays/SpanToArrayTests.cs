using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class SpanToArrayTests
{
    [Fact]
    public Task LinearManagedAndBorrowedSlicesProduceIndependentArrays() => InlineArrayTests.AssertLinked("""
        using System;
        public struct Point { public double X,Y; }
        public struct Item { public string Name; public Point Position; }
        public static class Program {
            static Point[] Make() {
                Span<Point> points=stackalloc Point[3]; points[1].X=42;points[1].Y=7;
                return points.Slice(1,1).ToArray();
            }
            public static int Main() {
                var points=Make(); Console.WriteLine(points[0].X); Console.WriteLine(points[0].Y);
                int[] numbers=[1,2,3,4];ReadOnlySpan<int> slice=numbers.AsSpan(1,2);
                var copy=slice.ToArray();numbers[1]=99;Console.WriteLine(copy[0]);Console.WriteLine(copy[1]);
                Item[] items=[new Item {Name="first",Position=new Point {X=20,Y=22}},default];
                var cloned=items.AsSpan().ToArray();ref Item alias=ref cloned[0];
                items[0].Name="changed";items[0].Position.X=0;Console.WriteLine(alias.Name);
                Console.WriteLine(alias.Position.X+alias.Position.Y);
                string[] texts=["a","b"];var textsCopy=texts.AsSpan().ToArray();texts[0]="c";Console.WriteLine(textsCopy[0]);
                Console.WriteLine(new string("xЖ🙂y".AsSpan(1,3).ToArray().AsSpan()));
                Console.WriteLine(default(Span<int>).ToArray().Length);
                return 42;
            }
        }
        """, "42\n7\n2\n3\nfirst\n42\na\nЖ🙂\n0\n",42);
}
