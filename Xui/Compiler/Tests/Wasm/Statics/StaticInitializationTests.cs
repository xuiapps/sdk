using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Statics;

[Collection(nameof(ManagedConsoleCollection))]
public class StaticInitializationTests
{
    [Fact]
    public async Task RealFontWeightStaticStructInitializes()
    {
        foreach (var optimization in new[] { Microsoft.CodeAnalysis.OptimizationLevel.Release, Microsoft.CodeAnalysis.OptimizationLevel.Debug })
        {
            var coreAssembly = typeof(Xui.Core.Canvas.FontWeight).Assembly;
            byte[] image = Frontend.Roslyn.Compile("""
                using Xui.Core.Canvas;
                public static class Program { public static int Main() {
                    System.Console.WriteLine((double)FontWeight.SemiBold.Value);
                    return FontWeight.SemiBold.Value==600 ? 42 : 1;
                } }
                """, optimizationLevel: optimization, additionalReferences: [
                    Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(coreAssembly.Location),
                    Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll"))]);
            var managed = Gc.GcTests.RunManaged(image);
            using var app = Canvas.CanvasTests.Read(image);
            using var core = Canvas.CanvasTests.Read(File.ReadAllBytes(coreAssembly.Location));
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation([app, core, .. framework.Assemblies]);
            byte[] wasm = new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app, new Xui.Compiler.Playground.ConsoleWriteLineRewriter());
            Assert.Equal("600\n", managed.Output); Assert.Equal(42, managed.ExitCode);
            Assert.Equal(managed, await Repl.Node.ExecuteAsync(wasm));
        }
    }

    [Fact]
    public Task StaticFieldsAndBorrowedAddressesKeepPersistentStorage() => InlineArrayTests.AssertLinked("""
        using System;
        public struct Point { public int X,Y; }
        public struct Label { public Point Position; public string Text; }
        public class Box { public int Value; }
        public static class Values {
            public static int Number;
            public static string Text;
            public static Point Point;
            public static Label Label;
            public static Box Box;
            public static float Single;
            public static long Wide;
            public static readonly Point Origin=new Point { X=2,Y=3 };
        }
        public static class Program {
            static void Set(ref int value) { value=42; }
            static void Text(ref string value) { value="hello"; }
            public static int Main() {
                Set(ref Values.Number); Text(ref Values.Text);
                Values.Point=Values.Origin; ref int alias=ref Values.Point.X;
                Values.Point=new Point { X=7,Y=8 }; Set(ref alias);
                Values.Label=new Label { Position=Values.Point, Text=Values.Text };
                var copy=Values.Label; copy.Text="copy"; copy.Position.X=9;
                Values.Box=new Box { Value=Values.Number }; Values.Single=1.25f; Values.Wide=long.MinValue;
                return Values.Number==42 && Values.Label.Position.X==42 && Values.Label.Text=="hello" &&
                    Values.Point.X==42 && Values.Origin.X==2 && Values.Box.Value==42 &&
                    Values.Single==1.25f && Values.Wide==long.MinValue ? 42 : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task StrictInitializersRunOnCallsAndConstructionOnce() => InlineArrayTests.AssertLinked("""
        using System;
        public class A {
            static A() { Console.WriteLine("A"); }
            public static int Read() { Console.WriteLine("call"); return 21; }
        }
        public class B {
            static B() { Console.WriteLine("B"); }
            public B() { Console.WriteLine("new"); }
        }
        public static class Program { public static int Main() {
            Console.WriteLine("start"); int result=A.Read()+A.Read(); new B(); new B(); return result;
        } }
        """, "start\nA\ncall\ncall\nB\nnew\nnew\n", 42);

    [Fact]
    public Task ClosedGenericsAndInheritedStaticsHaveIndependentIdentities() => InlineArrayTests.AssertLinked("""
        public static class Count { public static int Value; }
        public class Cache<T> { public static int Value; static Cache() { Value=++Count.Value; } }
        public class Derived : Cache<int> { }
        public static class Program { public static int Main() {
            int first=Cache<int>.Value; int second=Cache<string>.Value;
            Derived.Value=40;
            return first==1 && second==2 && Count.Value==2 ? Cache<int>.Value+Cache<string>.Value : 1;
        } }
        """, "", 42);

    [Fact]
    public Task RecursiveInitializersObservePartialState() => InlineArrayTests.AssertLinked("""
        public class A { public static int Value; static A() { Value=B.Value+1; } }
        public class B { public static int Value; static B() { Value=A.Value+2; } }
        public static class Program { public static int Main() => A.Value==3 && B.Value==2 ? 42 : 1; }
        """, "", 42);

    [Fact]
    public Task InitializationFailureIsWrappedAndCached() => InlineArrayTests.AssertLinked("""
        using System;
        public class Count { public static int Value; }
        public class Broken {
            static Broken() { Count.Value++; throw new InvalidOperationException("broken"); }
            public static int Read() => 1;
        }
        public static class Program { public static int Main() {
            Exception cause=null; bool same=true;
            for(int i=0;i<2;i++) {
                try { Broken.Read(); }
                catch(TypeInitializationException e) {
                    Console.WriteLine(e.TypeName); Console.WriteLine(e.Message); Console.WriteLine(e.InnerException.Message);
                    if(cause!=null && cause!=e.InnerException) same=false;
                    cause=e.InnerException;
                }
            }
            return Count.Value==1 && same && cause!=null ? 42 : 1;
        } }
        """, string.Concat(Enumerable.Repeat("Broken\nThe type initializer for 'Broken' threw an exception.\nbroken\n", 2)), 42);

    [Fact]
    public Task ModuleInitializerRunsBeforeEntry() => InlineArrayTests.AssertLinked("""
        using System.Runtime.CompilerServices;
        public static class Program {
            static int value;
            [ModuleInitializer] public static void Initialize() { value=42; }
            public static int Main() => value;
        }
        """, "", 42);

    [Fact]
    public Task ValueTypeInstanceMethodTriggersStrictInitialization() => InlineArrayTests.AssertLinked("""
        public static class Count { public static int Value; }
        public struct Value { static Value() { Count.Value=42; } public int Read() => Count.Value; }
        public static class Program { public static int Main() { Value value=default; return value.Read(); } }
        """, "", 42);

    [Fact]
    public Task FailedGenericSpecializationDoesNotPoisonOtherTypes() => InlineArrayTests.AssertLinked("""
        using System;
        public class Cache<T> {
            public static int Value;
            static Cache() { if(typeof(T).IsValueType) throw new InvalidOperationException("value"); Value=42; }
        }
        public static class Program { public static int Main() {
            bool failed=false;
            try { int value=Cache<int>.Value; }
            catch(TypeInitializationException e) { failed=true; Console.WriteLine(e.TypeName); }
            return failed ? Cache<string>.Value : 1;
        } }
        """, "Cache`1\n", 42);
}
