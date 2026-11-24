using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.Reachability;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class ArrayTests
{
    [Fact]
    public async Task PreservedArrayAccessDefinesElementStorageWithoutAnAllocation()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            public struct Point { public int X, Y; }
            public static class Program {
                public static Point Read(Point[] values) => values[0];
                public static int Main() => 42;
            }
            """));
        var module = app.ManifestModule;
        var read = module.Methods.Single(h => module.GetMethod(h).Name == "Read");
        byte[] wasm = new WasmEmitter().Emit(app, additionalRoots: [new Root.Method(MethodIdentity.FromDefinition(module, read))]);
        Assert.Equal(42, (await Node.ExecuteAsync(wasm)).ExitCode);
    }

    [Fact]
    public Task PrimitiveVectorsExecuteLoopsAndCalls() => GcTests.AssertExecution("""
        public class Holder { public int[] Values; }
        public static class Program {
            static int[] Make(int n) { var a = new int[n]; for (int i=0;i<n;i++) a[i]=i+1; return a; }
            static int Sum(int[] a) { int sum=0; for(int i=0;i<a.Length;i++) sum+=a[i]; return sum; }
            public static int Main() {
                var h = new Holder(); h.Values=Make(8);
                var alias=h.Values; alias[0]=7;
                return Sum(h.Values);
            }
        }
        """, "", 42);

    [Fact]
    public Task PrimitiveStoragePromotesAndNarrows() => GcTests.AssertExecution("""
        public static class Program { public static int Main() {
            var bytes=new byte[1]; var signed=new sbyte[1]; var shorts=new short[1]; var chars=new char[1];
            var longs=new long[1]; var floats=new float[1]; var doubles=new double[1]; var bools=new bool[1];
            bytes[0]=255; signed[0]=-2; shorts[0]=-300; chars[0]='A'; longs[0]=1234567890123;
            floats[0]=1.25f; doubles[0]=2.5; bools[0]=true;
            System.Console.WriteLine(bytes[0]); System.Console.WriteLine(signed[0]);
            System.Console.WriteLine(shorts[0]); System.Console.WriteLine((int)chars[0]);
            System.Console.WriteLine(longs[0]); System.Console.WriteLine(floats[0]);
            System.Console.WriteLine(doubles[0]); System.Console.WriteLine(bools[0]); return 0;
        } }
        """, "255\n-2\n-300\n65\n1234567890123\n1.25\n2.5\nTrue\n", 0);

    [Fact]
    public Task ReferencesAndJaggedArraysPreserveAliases() => GcTests.AssertExecution("""
        public class Box { public int Value; }
        public class Holder<T> { public T[] Values; }
        public static class Program { public static int Main() {
            var text=new string[2]; text[1]="hello";
            System.Console.WriteLine(text[0]); System.Console.WriteLine(text[1]);
            var boxes=new Box[2]; var b=new Box(); b.Value=40; boxes[0]=b; boxes[1]=b;
            boxes[1].Value=41;
            var holder=new Holder<int[]>(); holder.Values=new int[2][]; holder.Values[1]=new int[1]; holder.Values[1][0]=1;
            return boxes[0].Value+holder.Values[1][0];
        } }
        """, "\nhello\n", 42);

    [Fact]
    public Task StructElementsHaveIndependentValues() => GcTests.AssertExecution("""
        public struct Point { public int X, Y; }
        public static class Program { public static int Main() {
            var points=new Point[3]; points[1].X=10; points[1].Y=20;
            Point copy=points[1]; copy.X=99; points[2]=points[1]; points[2].Y=12;
            System.Console.WriteLine(points[0].X); System.Console.WriteLine(points[1].X);
            return points[1].Y+points[2].X+points[2].Y;
        } }
        """, "0\n10\n", 42);

    [Fact]
    public Task ReferenceStructElementsCopyReferencesAndValues() => GcTests.AssertExecution("""
        public struct Label { public string Name; public int Value; }
        public static class Program { public static int Main() {
            var a=new Label[2]; a[0].Name="first"; a[0].Value=40;
            a[1]=a[0]; a[1].Name="second"; a[1].Value=2;
            System.Console.WriteLine(a[0].Name); System.Console.WriteLine(a[1].Name);
            return a[0].Value+a[1].Value;
        } }
        """, "first\nsecond\n", 42);

    [Fact]
    public Task ArrayFailuresAreManagedExceptions() => GcTests.AssertExecution("""
        public static class Program { public static int Main() {
            var a=new int[1]; int score=0; int index=-1;
            try { a[index]=1; } catch(System.IndexOutOfRangeException) { score+=1; }
            try { score+=a[1]; } catch(System.IndexOutOfRangeException) { score+=2; }
            a=null;
            try { score+=a.Length; } catch(System.NullReferenceException) { score+=4; }
            try { a=new int[index]; } catch(System.OverflowException) { score+=8; }
            return score;
        } }
        """, "", 15);

    [Fact]
    public Task NullArrayJoinsAndIdentityComparisons() => GcTests.AssertExecution("""
        public static class Program {
            static int[] Pick(bool b, int[] a) => b ? a : null;
            public static int Main() { var a=new int[1]; a[0]=42;
                if(Pick(false,a)!=null) return 1;
                if(Pick(true,a)==a) return a[0]; return 2;
            }
        }
        """, "", 42);
}
