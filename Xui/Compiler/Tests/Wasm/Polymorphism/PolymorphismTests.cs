using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Rewrite;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Polymorphism;

[Collection(nameof(ManagedConsoleCollection))]
public class PolymorphismTests
{
    [Fact]
    public async Task UnitsExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Polymorphism", "Units.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("Ada\nscout\n14\n4\nTrue\n3\nFalse\nTrue\n", managed.Output);
            Assert.Equal(14, managed.ExitCode);
            using var stream = new MemoryStream(pe);
            using var assembly = AssemblyCompiler.Read(stream);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());
            Assert.Equal(managed, await Node.ExecuteAsync(wasm));
            if (optimization == OptimizationLevel.Debug) continue;
            var metadata = new StringBuilder();
            foreach (var type in emitter.Types)
            {
                metadata.AppendLine($"### {type.Name}\n");
                metadata.AppendLine($"- ID: `{type.Id}`\n- Base: `{type.BaseType?.Name ?? "System.Object"}`\n- WASM type: `{type.WasmType.Value}`\n- Table start: `{type.TableStart}`\n");
                foreach (var field in type.Fields)
                    metadata.AppendLine($"- Field `{field.DeclaringType}.{field.Name}`: first `{field.First.Value}`, count `{field.Count}`");
                metadata.AppendLine();
                foreach (var slot in type.VirtualSlots)
                {
                    var method = assembly.ManifestModule.GetMethod(slot.Implementation.Handle);
                    string owner = assembly.ManifestModule.GetType(method.DeclaringType).Name;
                    metadata.AppendLine($"- Slot {slot.Index}: `{owner}.{method.Name}`");
                }
                metadata.AppendLine();
            }
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Units", $"# Class inheritance and virtual dispatch\n\nSource: [Units.cs](Fixtures/Polymorphism/Units.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{managed.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{managed.ExitCode}\n```\n\n## Runtime Types\n\n{metadata}## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Fact]
    public Task NewSlotsOverridesAndBaseCallsRemainDistinct() => GcTests.AssertExecution("""
        public class A { public virtual int F() => 1; public virtual int F(int x) => x + 2; }
        public class B : A { public new virtual int F() => 3; public override int F(int x) => base.F(x) + 4; }
        public class C : B { public override int F() => 5; }
        public static class Program {
            static int ReadA(A a) => a.F() + a.F(10);
            static int ReadB(B b) => b.F();
            public static int Main() { var c = new C(); return ReadA(c) * 10 + ReadB(c); }
        }
        """, "", 175);

    [Fact]
    public Task AbstractSlotsResolveToConcreteOverrides() => GcTests.AssertExecution("""
        public abstract class A { public abstract int F(); public virtual int G() => F() + 2; }
        public class B : A { public override int F() => 7; }
        public static class Program { static int Read(A a) => a.G(); public static int Main() => Read(new B()); }
        """, "", 9);

    [Fact]
    public Task NominalTestsDistinguishIdenticalSiblingLayouts() => GcTests.AssertExecution("""
        public class A { public int Value; }
        public class B : A { }
        public class C : A { }
        public static class Program {
            static int Read(A a) {
                System.Console.WriteLine(a is A);
                System.Console.WriteLine(a is B);
                System.Console.WriteLine(a is C);
                System.Console.WriteLine((a as C) == null);
                var b = (B)a; b.Value = 7; return a.Value;
            }
            public static int Main() => Read(new B());
        }
        """, "True\nTrue\nFalse\nTrue\n", 7);

    [Fact]
    public Task NullTypeTestsAndCastsPreserveNull() => GcTests.AssertExecution("""
        public class A { }
        public class B : A { }
        public static class Program {
            static int Read(A a) {
                System.Console.WriteLine(a is B);
                System.Console.WriteLine((a as B) == null);
                System.Console.WriteLine((B)a == null);
                return 0;
            }
            public static int Main() => Read(null);
        }
        """, "False\nTrue\nTrue\n", 0);

    [Fact]
    public Task VirtualDispatchInsideBaseConstructorUsesDerivedType() => GcTests.AssertExecution("""
        public class A { public int Value; public A() { Value = F(); } public virtual int F() => 1; }
        public class B : A { public override int F() => 8; }
        public static class Program { public static int Main() => new B().Value; }
        """, "", 8);

    [Theory]
    [InlineData("return ((C)a).F();", typeof(InvalidCastException))]
    [InlineData("a = null; return a.F();", typeof(NullReferenceException))]
    public async Task InvalidCastsAndNullVirtualReceiversTrap(string body, System.Type exception)
    {
        string source = $$"""
            public class A { public virtual int F() => 1; }
            public class B : A { }
            public class C : A { }
            public static class Program {
                static int Read(A a) { {{body}} }
                public static int Main() => Read(new B());
            }
            """;
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var error = Assert.Throws<TargetInvocationException>(() => Assembly.Load(pe).EntryPoint!.Invoke(null, null));
            Assert.IsType(exception, error.InnerException);
            await Node.AssertTrapsAsync(GcTests.Emit(pe));
        }
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(1, 2)]
    public Task ConditionalReferencesJoinAtTheirCommonBase(int choice, int result) => GcTests.AssertExecution($$"""
        public class A { public virtual int F() => 1; }
        public class B : A { public override int F() => 2; }
        public class C : A { public override int F() => 3; }
        public static class Program {
            static A Choose(int condition) => condition > 0 ? new B() : new C();
            static A OrNull(int condition) => condition > 0 ? new B() : null;
            public static int Main() {
                System.Console.WriteLine(OrNull(0) == null);
                System.Console.WriteLine(OrNull(1) is B);
                return Choose({{choice}}).F();
            }
        }
        """, "True\nTrue\n", result);

    [Fact]
    public void TypeMetadataExposesInheritedLayoutAndOverrideSlots()
    {
        const string source = """
            public struct Point { public int X; public int Y; }
            public class A { public Point Position; public virtual int F() => 1; }
            public class B : A { public string Name; public override int F() => 2; }
            public static class Program { public static int Main() => new B().F(); }
            """;
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source));
        using var assembly = AssemblyCompiler.Read(stream);
        var emitter = new WasmEmitter();
        emitter.Emit(assembly);
        var a = Assert.Single(emitter.Types, type => type.Name == "A");
        var b = Assert.Single(emitter.Types, type => type.Name == "B");
        Assert.Same(a, b.BaseType);
        Assert.NotEqual(a.Id, b.Id);
        Assert.NotEqual(a.Descriptor, b.Descriptor);
        Assert.Equal(4u, b.FieldCount); // Descriptor, Point.X, Point.Y, string.
        Assert.Collection(b.Fields,
            field => { Assert.Equal("Position", field.Name); Assert.Equal(1u, field.First.Value); Assert.Equal(2u, field.Count); },
            field => { Assert.Equal("Name", field.Name); Assert.Equal(3u, field.First.Value); Assert.Equal(1u, field.Count); });
        var original = Assert.Single(a.VirtualSlots);
        var overridden = Assert.Single(b.VirtualSlots);
        Assert.Equal(original.Index, overridden.Index);
        Assert.Equal(original.Declaration, overridden.Declaration);
        Assert.NotEqual(original.Implementation, overridden.Implementation);
    }

    [Fact]
    public Task VirtualCallsPreserveArgumentWidthsAndGcValueCopies() => GcTests.AssertExecution("""
        public struct Label { public string Text; public int Value; }
        public abstract class A { public abstract float Change(Label label, byte b, float f); }
        public class B : A {
            public override float Change(Label label, byte b, float f) {
                label.Text = "changed"; label.Value = 50; return f + b;
            }
        }
        public static class Program {
            static float Read(A a, Label label) => a.Change(label, 250, 1.25f);
            public static int Main() {
                Label label = default; label.Text = "original"; label.Value = 3;
                System.Console.WriteLine(Read(new B(), label));
                System.Console.WriteLine(label.Text); return label.Value;
            }
        }
        """, "251.25\noriginal\n", 3);

    [Theory]
    [InlineData("public interface I { int F(); } public class A : I { int I.F() => 1; }", "new A()", "Explicit method")]
    [InlineData("public class A : System.Exception { }", "new A()", "External class inheritance")]
    public void UnsupportedRuntimeTypeModelsFailExplicitly(string declarations, string expression, string reason)
    {
        var pe = Frontend.Roslyn.Compile(declarations + " public static class Program { public static int Main() { var value = " + expression + "; return 0; } }");
        Assert.Contains(reason, Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
