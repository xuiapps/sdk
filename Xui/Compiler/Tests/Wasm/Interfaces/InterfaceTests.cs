using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Interfaces;

[Collection(nameof(ManagedConsoleCollection))]
public class InterfaceTests
{
    [Fact]
    public async Task InterfaceDispatchAndCastsExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Interfaces", "Units.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("True\nTrue\nFalse\nTrue\nScout\n14\n4\n", managed.Output);
            Assert.Equal(14, managed.ExitCode);
            using var stream = new MemoryStream(pe);
            using var assembly = AssemblyCompiler.Read(stream);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(managed, result);
            var scout = Assert.Single(emitter.Types, type => type.Name == "Scout");
            Assert.Contains(scout.Interfaces, type => type.Name == "IPosition" && type.IsInterface);
            Assert.Contains(scout.InterfaceSlots, slot => slot.Implementation.ToString() == "Scout.get_X");
            if (optimization == OptimizationLevel.Debug) continue;
            string metadata = string.Join("\n\n", emitter.Types.Select(type => $"### {type.Name}\n\n- Interface: `{type.IsInterface}`\n- Implements: {(type.Interfaces.Count == 0 ? "—" : string.Join(", ", type.Interfaces.Select(i => $"`{i.Name}`")))}\n" +
                string.Join("\n", type.InterfaceSlots.Select(slot => $"- Slot {slot.Index}: `{slot.Declaration}` → `{slot.Implementation}`"))));
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Units", $"# Interface dispatch and casts\n\nSource: [Units.cs](Fixtures/Interfaces/Units.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## Runtime Types\n\n{metadata}\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Fact]
    public Task TwoInterfacesCanMapTheSameSignatureToDifferentExplicitMethods() => GcTests.AssertExecution("""
        public interface ILeft { int Read(); }
        public interface IRight { int Read(); }
        public class Both : ILeft, IRight { int ILeft.Read() => 3; int IRight.Read() => 7; }
        public static class Program {
            static int Read(ILeft left, IRight right) => left.Read() * 10 + right.Read();
            public static int Main() { var both = new Both(); return Read(both, both); }
        }
        """, "", 37);

    [Fact]
    public Task InheritedMappingIgnoresHidingButReimplementationSelectsNewMember() => GcTests.AssertExecution("""
        public interface I { int Read(); }
        public class Base : I { public int Read() => 1; }
        public class Hidden : Base { public new int Read() => 2; }
        public class Reimplemented : Base, I { public new int Read() => 3; }
        public static class Program {
            static int Read(I value) => value.Read();
            public static int Main() => Read(new Hidden()) * 10 + Read(new Reimplemented());
        }
        """, "", 13);

    [Fact]
    public Task ClosedGenericInterfacesKeepTheirIdentityAndSubstitutedSignatures() => GcTests.AssertExecution("""
        public interface IValue<T> { T Get(); }
        public class Value<T> : IValue<T> { public T Item; public Value(T item) { Item = item; } public T Get() => Item; }
        public static class Program {
            static int Read(IValue<int> value) {
                System.Console.WriteLine(value is IValue<string>);
                System.Console.WriteLine((value as IValue<string>) == null);
                return value.Get();
            }
            static string ReadText(IValue<string> value) => value.Get();
            public static int Main() { System.Console.WriteLine(ReadText(new Value<string>("hello"))); return Read(new Value<int>(42)); }
        }
        """, "hello\nFalse\nTrue\n", 42);

    [Fact]
    public Task NullInterfaceTestsAndCastsPreserveNull() => GcTests.AssertExecution("""
        public interface I { int Read(); } public interface J { }
        public static class Program {
            static int Read(I value) {
                System.Console.WriteLine(value is J);
                System.Console.WriteLine((value as J) == null);
                System.Console.WriteLine((J)value == null);
                return 0;
            }
            public static int Main() => Read(null);
        }
        """, "False\nTrue\nTrue\n", 0);

    [Theory]
    [InlineData("return ((J)value).Read();", typeof(InvalidCastException))]
    [InlineData("value = null; return value.Read();", typeof(NullReferenceException))]
    public async Task FailedInterfaceCastsAndNullReceiversThrow(string body, System.Type expected)
    {
        string source = $$"""
            public interface I { int Read(); } public interface J { int Read(); }
            public class A : I { public int Read() => 1; }
            public static class Program { static int Run(I value) { {{body}} } public static int Main() => Run(new A()); }
            """;
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var error = Assert.Throws<TargetInvocationException>(() => Assembly.Load(pe).EntryPoint!.Invoke(null, null));
            Assert.IsType(expected, error.InnerException);
            await Node.AssertThrowsAsync(GcTests.Emit(pe));
        }
    }

    [Theory]
    [InlineData("public interface I { int Read() => 1; } public class A : I { }", "default interface")]
    [InlineData("public interface I<out T> { T Read(); } public class A : I<string> { public string Read() => null; }", "Variant interface")]
    public void UnsupportedInterfaceModelsFailExplicitly(string declarations, string reason)
    {
        var pe = Frontend.Roslyn.Compile(declarations + " public static class Program { public static int Main() { var a = new A(); return 0; } }");
        Assert.Contains(reason, Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    [Fact]
    public async Task NullOnlyInterfaceDispatchStillEmitsAValidModule()
    {
        var pe = Frontend.Roslyn.Compile("""
            public interface I { int Read(); }
            public static class Program { static int Read(I value) => value.Read(); public static int Main() => Read(null); }
            """);
        await Node.AssertThrowsAsync(GcTests.Emit(pe));
    }

    [Fact]
    public Task InterfaceReferencesInFieldsAndConditionalPathsKeepIdentity() => GcTests.AssertExecution("""
        public interface I { int Read(); }
        public class A : I { public int Read() => 7; }
        public class Holder { public I Value; }
        public static class Program {
            static I Choose(bool take, I existing) => take ? new A() : existing;
            public static int Main() { var holder = new Holder(); holder.Value = Choose(true, null); return holder.Value.Read(); }
        }
        """, "", 7);

    [Fact]
    public Task GenericInterfaceMappingPreservesTemplateOverloads() => GcTests.AssertExecution("""
        public interface I<T> { int Read(T value); }
        public class A<T> : I<T> { public int Read(int value) => 2; public int Read(T value) => 1; }
        public static class Program { static int Read(I<int> value) => value.Read(0); public static int Main() => Read(new A<int>()); }
        """, "", 1);

    [Fact]
    public async Task InterfaceContractsCanComeFromASeparateAssembly()
    {
        var contractPe = Frontend.Roslyn.Compile("public interface I { int Read(); }", OutputKind.DynamicallyLinkedLibrary, "Contracts");
        var applicationPe = Frontend.Roslyn.Compile("""
            public class A : I { int I.Read() => 42; }
            public static class Program {
                static int Read(I value) { System.Console.WriteLine(value is A); return value.Read(); }
                public static int Main() => Read(new A());
            }
            """, additionalReferences: [MetadataReference.CreateFromImage(contractPe)]);
        using var contractStream = new MemoryStream(contractPe);
        using var applicationStream = new MemoryStream(applicationPe);
        using var contract = AssemblyCompiler.Read(contractStream);
        using var application = AssemblyCompiler.Read(applicationStream);
        _ = new Xui.Compiler.XuiNet.Compilation(application, contract);
        var emitter = new WasmEmitter();
        var result = await Node.ExecuteAsync(emitter.Emit(application, new ConsoleWriteLineRewriter()));
        Assert.Equal("True\n", result.Output);
        Assert.Equal(42, result.ExitCode);
    }

    [Fact]
    public Task ObjectReferencesCanBeTestedAndCastToInterfaces() => GcTests.AssertExecution("""
        public interface I { int Read(); }
        public class A : I { public int Read() => 42; }
        public class Other { }
        public class Holder { public object Value; }
        public static class Program {
            static object Choose(bool take, object other) => take ? new A() : other;
            static int Read(object value) {
                System.Console.WriteLine(value is I);
                var contract = value as I;
                if (contract == null) return 0;
                return ((I)value).Read();
            }
            public static int Main() {
                var holder = new Holder(); holder.Value = Choose(true, new Other());
                System.Console.WriteLine(Read(new Other()));
                System.Console.WriteLine(Read(null));
                return Read(holder.Value);
            }
        }
        """, "False\n0\nFalse\n0\nTrue\n", 42);

    [Fact]
    public Task ReimplementationCanRetainAnInheritedExplicitImplementation() => GcTests.AssertExecution("""
        public interface I { int Read(); }
        public class Base : I { int I.Read() => 7; }
        public class Derived : Base, I { }
        public static class Program { static int Read(I value) => value.Read(); public static int Main() => Read(new Derived()); }
        """, "", 7);

    [Fact]
    public Task UncalledInterfaceBodiesAreNotRetained() => GcTests.AssertExecution("""
        public interface I { int Read(); int Unused(); }
        public class A : I { public int Read() => 42; public int Unused() => throw new System.NotSupportedException(); }
        public static class Program { static int Read(I value) => value.Read(); public static int Main() => Read(new A()); }
        """, "", 42);

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
