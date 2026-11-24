using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Interfaces;

[Collection(nameof(ManagedConsoleCollection))]
public class GenericInterfaceTests
{
    [Fact]
    public Task ClosedMethodArgumentsHaveSeparateSlots() => GcTests.AssertExecution("""
        public interface I { T Echo<T>(T value); }
        public class A : I { public T Echo<T>(T value) => value; }
        public static class Program { public static int Main() {
            I a=new A(); System.Console.WriteLine(a.Echo("hello"));
            System.Console.WriteLine(a.Echo(1234567890123456789L)); return a.Echo(42);
        } }
        """, "hello\n1234567890123456789\n", 42);

    [Fact]
    public Task ExplicitImplementationsAndClosedOwnerArguments() => GcTests.AssertExecution("""
        public interface I<T> { U Echo<U>(T key, U value); }
        public class A<T> : I<T> { U I<T>.Echo<U>(T key, U value) => value; }
        public static class Program { public static int Main() {
            I<string> a=new A<string>(); I<int> b=new A<int>();
            System.Console.WriteLine(b.Echo(1,"explicit")); return a.Echo("key",42);
        } }
        """, "explicit\n", 42);

    [Fact]
    public Task VirtualOverridesWinWithoutCollapsingNewSlots() => GcTests.AssertExecution("""
        public interface I { int Read<T>(T value); }
        public class A : I { public virtual int Read<T>(T value) => 1; }
        public class B : A { public override int Read<T>(T value) => 4; }
        public class C : B { public new virtual int Read<T>(T value) => 9; }
        public class D : C { public override int Read<T>(T value) => 8; }
        public class E : I { public int Read<T>(T value) => 2; }
        public static class Program { public static int Main() {
            I a=new D(); I b=new E(); return a.Read("ignored")*10+b.Read(1);
        } }
        """, "", 42);

    [Fact]
    public Task MostSpecificGenericDefaultAndReabstractedImplementation() => GcTests.AssertExecution("""
        public interface I { T Echo<T>(T value) => value; int Read<T>(T value) => 1; }
        public interface J : I { int I.Read<T>(T value) => 4; }
        public interface K : I { abstract T I.Echo<T>(T value); }
        public class A : J, K { public T Echo<T>(T value) => value; }
        public static class Program { public static int Main() {
            I a=new A(); return a.Read("a")*10+a.Echo(2);
        } }
        """, "", 42);

    [Fact]
    public Task GenericDefaultsReturnStructValuesAndAcceptByrefs() => GcTests.AssertExecution("""
        public struct Point { public int X; }
        public interface I { T Copy<T>(ref T value) => value; }
        public class A : I { }
        public static class Program { public static int Main() {
            I a=new A(); Point p=new Point { X=42 }; var copy=a.Copy(ref p); p.X=1; return copy.X;
        } }
        """, "", 42);

    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task GenericInterfaceCallsLinkAcrossAssemblies(OptimizationLevel optimization)
    {
        byte[] contract = Frontend.Roslyn.Compile("public interface I { T Echo<T>(T value); }", OutputKind.DynamicallyLinkedLibrary, "GenericContract", optimization);
        byte[] library = Frontend.Roslyn.Compile("public class A:I { public T Echo<T>(T value)=>value; }", OutputKind.DynamicallyLinkedLibrary,
            "GenericImplementation", optimization, additionalReferences: [MetadataReference.CreateFromImage(contract)]);
        byte[] program = Frontend.Roslyn.Compile("public static class Program { public static int Main(){ I a=new A(); return a.Echo(42); } }",
            optimizationLevel: optimization, additionalReferences: [MetadataReference.CreateFromImage(contract), MetadataReference.CreateFromImage(library)]);
        using var app=CanvasTests.Read(program); using var contracts=CanvasTests.Read(contract); using var implementation=CanvasTests.Read(library);
        _=new Xui.Compiler.XuiNet.Compilation(app,contracts,implementation);
        var emitter=new WasmEmitter();
        Assert.Equal(42,(await Node.ExecuteAsync(emitter.Emit(app))).ExitCode);
        var slot=Assert.Single(Assert.Single(emitter.Types,t=>t.Name=="A").InterfaceSlots);
        Assert.NotNull(slot.Declaration.Arguments); Assert.Equal(slot.Declaration.Arguments,slot.Implementation.Arguments);
    }
}
