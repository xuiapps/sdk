using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet.Reachability;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Interfaces;

[Collection(nameof(ManagedConsoleCollection))]
public class DefaultInterfaceTests
{
    [Fact]
    public async Task DefaultLayoutMethodsExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/Interfaces/Defaults.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("panel\n11\n22\npanel\n33\n44\n", managed.Output);
            using var app = CanvasTests.Read(pe);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(app, new ConsoleWriteLineRewriter());
            Assert.Equal(managed, await Node.ExecuteAsync(wasm));
            Assert.DoesNotContain(emitter.Dependencies!.Methods, m => m.ToString() == "ILayout.Arrange");
            Assert.Contains(emitter.Dependencies.Methods, m => m.ToString() == "ILayout.Show");
            if (optimization == OptimizationLevel.Debug) continue;
            string dispatch = string.Join("\n", emitter.Types.Where(t => !t.IsInterface).SelectMany(t => t.InterfaceSlots.Select(slot =>
                $"| `{t.Name}` | `{slot.Declaration}` | `{slot.Implementation}` |")));
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Defaults", $"# Default interface layout methods\n\nSource: [Defaults.cs](Fixtures/Interfaces/Defaults.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{managed.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{managed.ExitCode}\n```\n\n## Dispatch\n\n| Receiver | Declaration | Selected implementation |\n| --- | --- | --- |\n{dispatch}\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;

    [Fact]
    public Task DefaultBodiesDispatchBackIntoClassImplementations() => GcTests.AssertExecution("""
        public interface I {
            int Value { get; }
            private int Twice() => Value * 2;
            private static int Extra() => 2;
            int Read() => Twice() + Extra();
        }
        public class A : I { public int Value => 20; }
        public static class Program { public static int Main() { I value = new A(); return value.Read(); } }
        """, "", 42);

    [Fact]
    public Task ClassImplementationsWinAndVirtualOverridesStayVirtual() => GcTests.AssertExecution("""
        public interface I { int Read() => 1; }
        public interface J : I { int I.Read() => 2; }
        public class Base : I { public virtual int Read() => 3; }
        public class Derived : Base, J { public override int Read() => 4; }
        public class Explicit : J { int I.Read() => 5; }
        public static class Program {
            static int Read(I value) => value.Read();
            public static int Main() => Read(new Derived()) * 10 + Read(new Explicit());
        }
        """, "", 45);

    [Fact]
    public Task MostSpecificDefaultWinsAcrossDiamondAndInheritedClass() => GcTests.AssertExecution("""
        public interface I { int Read() => 1; }
        public interface Left : I { int I.Read() => 2; }
        public interface Right : I { }
        public interface Diamond : Left, Right { int I.Read() => 3; }
        public class Base : I { }
        public class Derived : Base, Diamond { }
        public class Direct : Left, Right { }
        public static class Program {
            static int Read(I value) => value.Read();
            public static int Main() => Read(new Derived()) * 10 + Read(new Direct());
        }
        """, "", 32);

    [Fact]
    public Task NewInterfaceMemberIsASeparateSlot() => GcTests.AssertExecution("""
        public interface I { int Read() => 1; }
        public interface J : I { new int Read() => 2; }
        public class A : J { }
        public static class Program {
            public static int Main() { var a = new A(); return ((I)a).Read() * 10 + ((J)a).Read(); }
        }
        """, "", 12);

    [Fact]
    public Task GenericDefaultsUseClosedContextAndRefStructArguments() => GcTests.AssertExecution("""
        public struct Point { public int X; }
        public interface I<T> { T Read(T value) => value; Point Move(in Point p) => new Point { X = p.X + 1 }; }
        public interface J<T> : I<T> { T I<T>.Read(T value) => value; }
        public class A : J<int>, J<string> { }
        public static class Program {
            public static int Main() {
                var a = new A(); Point p = new Point { X = 40 };
                System.Console.WriteLine(((I<string>)a).Read("default"));
                return ((I<int>)a).Read(((I<int>)a).Move(in p).X) + 1;
            }
        }
        """, "default\n", 42);

    [Fact]
    public async Task OverriddenAndUncalledDefaultBodiesAreNotRetained()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public interface I {
                static I() { System.Console.WriteLine(System.Environment.TickCount); }
                int Read() => System.Environment.TickCount;
                int Uncalled() => System.Environment.TickCount;
            }
            public class A : I { public int Read() => 42; }
            public static class Program { public static int Main() { I a = new A(); return a.Read(); } }
            """);
        var managed = GcTests.RunManaged(image);
        Assert.Equal("", managed.Output);
        Assert.Equal(42, managed.ExitCode);
        using var app = CanvasTests.Read(image);
        var emitter = new WasmEmitter();
        Assert.Equal(42, (await Node.ExecuteAsync(emitter.Emit(app))).ExitCode);
        Assert.DoesNotContain(emitter.Dependencies!.Methods, m => m.DeclaringType.ToString() == "I");
        Assert.Single(emitter.Dependencies.Nodes.OfType<Dependency.Signature>());
    }

    [Fact]
    public async Task NullDefaultReceiverThrowsWithoutRetainingUnreachableBody()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            public interface I { int Read() => System.Environment.TickCount; }
            public static class Program { public static int Main() {
                I value = null;
                try { return value.Read(); } catch (System.NullReferenceException) { return 42; }
            } }
            """));
        var emitter = new WasmEmitter();
        Assert.Equal(42, (await Node.ExecuteAsync(emitter.Emit(app))).ExitCode);
        Assert.DoesNotContain(emitter.Dependencies!.Methods, m => m.DeclaringType.ToString() == "I");
    }

    [Fact]
    public Task ConcreteClassResolvesConflictingDefaultsAndReabstraction() => GcTests.AssertExecution("""
        public interface I { int Read() => 1; }
        public interface Left : I { int I.Read() => 2; }
        public interface Right : I { int I.Read() => 3; }
        public interface Required : I { abstract int I.Read(); }
        public class A : Left, Right, Required { public int Read() => 42; }
        public static class Program { public static int Main() { I value = new A(); return value.Read(); } }
        """, "", 42);

    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task DefaultImplementationsExecuteFromLinkedAssemblies(OptimizationLevel optimization)
    {
        byte[] library = Frontend.Roslyn.Compile("""
            public struct Point { public int X; }
            public interface I { Point Move(in Point p) => p; }
            public interface J : I { Point I.Move(in Point p) => new Point { X = p.X + 1 }; }
            """, OutputKind.DynamicallyLinkedLibrary, "DefaultContracts", optimization);
        byte[] program = Frontend.Roslyn.Compile("""
            public class A : J { }
            public static class Program { public static int Main() { I value = new A(); Point p = new Point { X = 41 }; return value.Move(in p).X; } }
            """, optimizationLevel: optimization, additionalReferences: [MetadataReference.CreateFromImage(library)]);
        using var app = CanvasTests.Read(program);
        using var contracts = CanvasTests.Read(library);
        _ = new Xui.Compiler.XuiNet.Compilation(app, contracts);
        var emitter = new WasmEmitter();
        Assert.Equal(42, (await Node.ExecuteAsync(emitter.Emit(app))).ExitCode);
        Assert.Contains(emitter.Dependencies!.Methods, m => m.DeclaringType.Module == contracts.ManifestModule && m.ToString() == "J.I.Move");
        Assert.DoesNotContain(emitter.Dependencies.Methods, m => m.ToString() == "I.Move");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AmbiguityAndReabstractionFailInsteadOfPickingAnArbitraryBody(bool reabstract)
    {
        const string original = """
            public interface I { int Read() => 1; }
            public interface Left : I { }
            public interface Right : I { }
            """;
        byte[] v1 = Frontend.Roslyn.Compile(original, OutputKind.DynamicallyLinkedLibrary, "Contracts");
        byte[] program = Frontend.Roslyn.Compile("""
            public class A : Left, Right { }
            public static class Program { public static int Main() { I a = new A(); return a.Read(); } }
            """, additionalReferences: [MetadataReference.CreateFromImage(v1)]);
        // Simulate a dependency update after the application was compiled. Both PE files
        // are valid C#, but the new interface hierarchy has no executable unique target.
        string updated = reabstract ? """
            public interface I { int Read() => 1; }
            public interface Left : I { abstract int I.Read(); }
            public interface Right : I { }
            """ : """
            public interface I { int Read() => 1; }
            public interface Left : I { int I.Read() => 2; }
            public interface Right : I { int I.Read() => 3; }
            """;
        using var app = CanvasTests.Read(program);
        using var contracts = CanvasTests.Read(Frontend.Roslyn.Compile(updated, OutputKind.DynamicallyLinkedLibrary, "Contracts"));
        _ = new Xui.Compiler.XuiNet.Compilation(app, contracts);
        var error = Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app));
        Assert.Contains(reabstract ? "is abstract" : "Ambiguous default", error.Message);
    }
}
