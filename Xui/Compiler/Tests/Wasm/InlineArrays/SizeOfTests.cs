using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.InlineArrays;

[Collection(nameof(ManagedConsoleCollection))]
public class SizeOfTests
{
    [Fact]
    public Task ClosedStructSizesDriveStackAllocatedSpans() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public struct Point { public double X,Y; }
        [StructLayout(LayoutKind.Sequential,Pack=2)] public struct Packed { public byte Tag; public int Value; }
        [StructLayout(LayoutKind.Explicit,Size=16)] public struct Union { [FieldOffset(4)]public int Value; [FieldOffset(4)]public float Other; }
        public static class Program {
            static unsafe int Size<T>() where T:unmanaged => sizeof(T);
            public static int Main() {
                Console.WriteLine(Size<Point>()); Console.WriteLine(Size<Packed>()); Console.WriteLine(Size<Union>());
                Span<Point> points=stackalloc Point[3]; points[0].X=20;points[2].Y=22;
                Span<Packed> packed=stackalloc Packed[2];packed[1].Tag=7;packed[1].Value=42;
                if(packed[0].Value!=0 || packed[1].Tag!=7) return -1;
                return (int)(points[0].X+points[2].Y);
            }
        }
        """, "16\n6\n16\n",42, allowUnsafe: true);
}
