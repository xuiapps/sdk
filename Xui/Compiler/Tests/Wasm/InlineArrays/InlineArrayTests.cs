using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;

namespace Xui.Compiler.Tests.Wasm.InlineArrays;

[Collection(nameof(ManagedConsoleCollection))]
public class InlineArrayTests
{
    [Fact]
    public Task InlineArraysInGcOwnersKeepAliasingAndStructCopies() => AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        public struct Item { public int X, Y; public string Text; }
        [InlineArray(3)] public struct Buffer { private Item element; }
        public struct State { public Buffer Items; public string Name; }
        public class Owner { public State State; }
        public static class Program {
            static void Write(ref Item item,int value) { item.X=value; item.Y=value+1; item.Text="item"; }
            public static int Main() {
                var owner=new Owner(); owner.State.Name="state";
                for(int i=0;i<3;i++) Write(ref owner.State.Items[i],i+10);
                State copy=owner.State;
                copy.Items[0].X=99;
                int sum=0;
                for(int i=0;i<3;i++) { sum+=owner.State.Items[i].Y; Console.WriteLine(owner.State.Items[i].Text); }
                Console.WriteLine(copy.Items[0].X);
                Console.WriteLine(owner.State.Items[0].X);
                return sum+6;
            }
        }
        """, "item\nitem\nitem\n99\n10\n", 42);

    [Fact]
    public Task NativeFloatAddressesUseTargetStorageInInlineArraysAndSpans() => AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        [System.Runtime.CompilerServices.InlineArray(3)] public struct Buffer { private NFloat element; }
        public static class Program {
            static double Sum(ReadOnlySpan<NFloat> values) {
                double sum=0; for(int i=0;i<values.Length;i++) sum+=(double)values[i]; return sum;
            }
            public static int Main() {
                Buffer values=default; values[0]=(NFloat)10; values[1]=(NFloat)12;
                Span<NFloat> span=values; span[2]=(NFloat)20;
                return Sum(span)==42 ? 42 : -1;
            }
        }
        """, "", 42, typeof(System.Runtime.InteropServices.NFloat).Assembly);

    [Fact]
    public Task InlineValuesAreCopiedIndependently() => GcTests.AssertExecution("""
        [System.Runtime.CompilerServices.InlineArray(4)]
        public struct Buffer { private int element; }
        public static class Program { public static int Main() {
            Buffer a=default; a[0]=20; a[3]=22; Buffer b=a; b[0]=99;
            return a[0]+a[3];
        } }
        """, "", 42);
    [Fact]
    public Task DynamicIndexingAndReadonlySpanCalls() => AssertLinked("""
        using System;
        [System.Runtime.CompilerServices.InlineArray(4)] public struct Buffer { private int element; }
        public static class Program {
            static int Sum(ReadOnlySpan<int> values) { int sum=0; for(int i=0;i<values.Length;i++) sum+=values[i]; return sum; }
            public static int Main() { Buffer a=default; for(int i=0;i<4;i++) a[i]=i+9; return Sum(a); }
        }
        """, "", 42);

    [Fact]
    public Task ReferenceInlineArraysKeepIndependentFrameHandles() => AssertLinked("""
        using System;
        [System.Runtime.CompilerServices.InlineArray(3)] public struct Buffer { private string element; }
        public static class Program {
            static void Change(ref Buffer value, int index) { value[index]="changed"; }
            public static int Main() {
                Buffer a=default; a[0]="original"; a[2]="last"; Buffer b=a;
                for(int i=0;i<100000;i++) Change(ref b,0);
                Console.WriteLine(a[0]); Console.WriteLine(b[0]); Console.WriteLine(a[1]); Console.WriteLine(b[2]);
                return 42;
            }
        }
        """, "original\nchanged\n\nlast\n", 42);

    [Fact]
    public Task InlineBuffersCanContainManagedArrayReferences() => AssertLinked("""
        [System.Runtime.CompilerServices.InlineArray(2)] public struct Buffer { private int[] element; }
        public static class Program { public static int Main() {
            Buffer a=default; a[1]=new int[1]; a[1][0]=42; Buffer b=a;
            a[1]=new int[1]; return b[1][0];
        } }
        """, "", 42);

    [Fact]
    public Task DynamicInlineBoundsAreCatchable() => AssertLinked("""
        [System.Runtime.CompilerServices.InlineArray(3)] public struct Buffer { private int element; }
        public static class Program {
            static int BadIndex() => 3;
            public static int Main() {
                Buffer a=default; int result=0;
                try { a[BadIndex()]=10; } catch(System.IndexOutOfRangeException) { result=42; }
                return result;
            }
        }
        """, "", 42);

    internal static async Task AssertLinked(string source, string output, int exitCode, params System.Reflection.Assembly[] additionalAssemblies)
    {
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] image = Frontend.Roslyn.Compile(source, optimizationLevel: optimization,
                additionalReferences: additionalAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location)));
            var managed = GcTests.RunManaged(image);
            Assert.Equal(output, managed.Output); Assert.Equal(exitCode, managed.ExitCode);
            using var app = CanvasTests.Read(image);
            using var framework = new Frontend.Framework(additionalAssemblies.Select(a => a.GetName().Name + ".dll").ToArray());
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
            Assert.Equal(managed, await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter())));
        }
    }
}
