using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Interfaces;

[Collection(nameof(ManagedConsoleCollection))]
public class VarianceTests
{
    [Fact]
    public async Task VarianceExecutesAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/Interfaces/Variance.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("True\nTrue\nTrue\nTrue\nTrue\nTrue\nFalse\nFalse\nFalse\nFalse\n42\n", managed.Output);
            Assert.Equal(42, managed.ExitCode);
            using var stream = new MemoryStream(pe);
            using var assembly = AssemblyCompiler.Read(stream);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(managed, result);
            if (optimization == OptimizationLevel.Debug) continue;
            string slots = string.Join("\n", emitter.Types.SelectMany(type => type.InterfaceSlots.Select(slot =>
                $"- `{type.Name}`: `{slot.Declaration}` → `{slot.Implementation}`")));
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Variance", $"# Covariant sources and contravariant sinks\n\nSource: [Variance.cs](Fixtures/Interfaces/Variance.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## Interface Dispatch\n\n{slots}\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Fact]
    public Task CovariantCallsAndContravariantCallsShareClassReferences() => GcTests.AssertExecution("""
        public class Animal { public int Value; }
        public class Dog : Animal { }
        public interface ISource<out T> { T Get(); }
        public interface ISink<in T> { int Put(T value); }
        public class Source<T> : ISource<T> { public T Item; public Source(T item) { Item = item; } public T Get() => Item; }
        public class Sink : ISink<Animal> { public int Put(Animal value) => value.Value; }
        public static class Program {
            static Animal Get(ISource<Animal> source) => source.Get();
            static int Put(ISink<Dog> sink, Dog dog) => sink.Put(dog);
            public static int Main() {
                var dog = new Dog(); dog.Value = 42;
                System.Console.WriteLine(Get(new Source<Dog>(dog)) == dog);
                return Put(new Sink(), dog);
            }
        }
        """, "True\n", 42);

    [Fact]
    public Task VariantCastsRespectDirectionNullAndValueTypeInvariance() => GcTests.AssertExecution("""
        public class Animal { } public class Dog : Animal { }
        public interface ISource<out T> { }
        public interface ISink<in T> { }
        public interface IInvariant<T> { }
        public class Source<T> : ISource<T>, IInvariant<T> { }
        public class Sink<T> : ISink<T> { }
        public static class Program {
            static void Check(object source) {
                System.Console.WriteLine(source is ISource<Animal>);
                System.Console.WriteLine(source is ISource<object>);
                System.Console.WriteLine(source is IInvariant<Animal>);
                System.Console.WriteLine((source as ISource<Animal>) != null);
            }
            static int Cast(object source) {
                try { var value = (ISource<Dog>)source; return value == null ? 1 : 2; }
                catch (System.InvalidCastException) { return 3; }
            }
            static bool Sink(object value) => value is ISink<Dog>;
            public static int Main() {
                Check(new Source<Dog>()); Check(new Source<int>()); Check(null);
                System.Console.WriteLine(Sink(new Sink<Animal>()));
                System.Console.WriteLine(Sink(new Sink<object>()));
                return Cast(new Source<Animal>()) * 10 + Cast(null);
            }
        }
        """, "True\nTrue\nFalse\nTrue\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\nTrue\nTrue\n", 31);

    [Fact]
    public Task NestedVarianceAndInheritedInterfacesRetainSelectedBodies() => GcTests.AssertExecution("""
        public class Animal { public int Value; } public class Dog : Animal { }
        public interface ISource<out T> { T Get(); }
        public interface IDerived<out T> : ISource<T> { }
        public class Source<T> : IDerived<T> { public T Item; public Source(T item) { Item = item; } public virtual T Get() => Item; }
        public class DogSource : Source<Dog> { public DogSource(Dog dog) : base(dog) { } public override Dog Get() { Item.Value += 2; return Item; } }
        public static class Program {
            static int Read(ISource<ISource<Animal>> source) => source.Get().Get().Value;
            public static int Main() {
                var dog = new Dog(); dog.Value = 40;
                return Read(new Source<ISource<Dog>>(new DogSource(dog)));
            }
        }
        """, "", 42);

    [Fact]
    public Task DefaultBodyRunsInItsOriginalGenericSpecialization() => GcTests.AssertExecution("""
        public class Animal { public int Value; } public class Dog : Animal { }
        public interface ISource<out T> { T Get(); T Read() => Get(); }
        public interface IDerived<out T> : ISource<T> { T ISource<T>.Read() => Get(); }
        public class Source : IDerived<Dog> { public Dog Get() { var dog = new Dog(); dog.Value = 42; return dog; } }
        public static class Program { static Animal Read(ISource<Animal> source) => source.Read(); public static int Main() => Read(new Source()).Value; }
        """, "", 42);

    [Fact]
    public Task ExactMappingPrecedesVariantMappingOnTheSameClass() => GcTests.AssertExecution("""
        public class Animal { public int Value; } public class Dog : Animal { }
        public interface ISource<out T> { T Get(); }
        public class Source : ISource<Dog>, ISource<Animal> {
            Dog ISource<Dog>.Get() { var value = new Dog(); value.Value = 1; return value; }
            Animal ISource<Animal>.Get() { var value = new Animal(); value.Value = 2; return value; }
        }
        public static class Program { static int Read(ISource<Animal> source) => source.Get().Value; public static int Main() => Read(new Source()); }
        """, "", 2);

    [Fact]
    public Task DerivedVariantMappingPrecedesBaseExactMapping() => GcTests.AssertExecution("""
        public class Animal { public int Value; } public class Dog : Animal { }
        public interface ISource<out T> { T Get(); }
        public class Base : ISource<Animal> { public Animal Get() { var value = new Animal(); value.Value = 1; return value; } }
        public class Derived : Base, ISource<Dog> { Dog ISource<Dog>.Get() { var value = new Dog(); value.Value = 2; return value; } }
        public static class Program { static int Read(ISource<Animal> source) => source.Get().Value; public static int Main() => Read(new Derived()); }
        """, "", 2);

    [Theory]
    [InlineData("ISource<Dog>, ISource<Cat>", 1)]
    [InlineData("ISource<Cat>, ISource<Dog>", 2)]
    public Task MultipleCompatibleMappingsFollowDeclaredInterfaceOrder(string contracts, int expected) => GcTests.AssertExecution($$"""
        public class Animal { public int Value; } public class Dog : Animal { } public class Cat : Animal { }
        public interface ISource<out T> { T Get(); }
        public class Source : {{contracts}} {
            Dog ISource<Dog>.Get() { var value = new Dog(); value.Value = 1; return value; }
            Cat ISource<Cat>.Get() { var value = new Cat(); value.Value = 2; return value; }
        }
        public static class Program { static int Read(ISource<Animal> source) => source.Get().Value; public static int Main() => Read(new Source()); }
        """, "", expected);

    [Fact]
    public Task CombinedVarianceAndInterfaceToObjectArgumentsExecute() => GcTests.AssertExecution("""
        public interface ITag { } public class Animal { public int Value; } public class Dog : Animal, ITag { }
        public interface IMap<in T, out U> { U Map(T value); }
        public class Mapper : IMap<object, Dog> { public Dog Map(object value) => (Dog)value; }
        public static class Program {
            static Animal Map(IMap<ITag, Animal> mapper, ITag value) => mapper.Map(value);
            public static int Main() { var dog = new Dog(); dog.Value = 42; return Map(new Mapper(), dog).Value; }
        }
        """, "", 42);

    [Fact]
    public Task UnusedVariantStringContractDoesNotRequireAnAdapter() => GcTests.AssertExecution("""
        public interface ISource<out T> { T Get(); }
        public class Source : ISource<string> { public string Get() => "hello"; }
        public static class Program { public static int Main() { var source = new Source(); return source == null ? 1 : 0; } }
        """, "", 0);

    [Fact]
    public Task StringAndArrayArgumentsAllowMembershipWithoutRepresentationAdapters() => GcTests.AssertExecution("""
        public class Animal { } public class Dog : Animal { }
        public interface ISource<out T> { T Get(); }
        public class Source<T> : ISource<T> { public T Get() => default; }
        public static class Program {
            static void Check(object value) {
                System.Console.WriteLine(value is ISource<object>);
                System.Console.WriteLine(value is ISource<Animal[]>);
            }
            public static int Main() { Check(new Source<string>()); Check(new Source<Dog[]>()); Check(new Source<int[]>()); return 0; }
        }
        """, "True\nFalse\nTrue\nTrue\nTrue\nFalse\n", 0);

    [Fact]
    public void MultipleVariantDefaultCandidatesFailExplicitly()
    {
        var pe = Frontend.Roslyn.Compile("""
            public class Animal { } public class Dog : Animal { } public class Cat : Animal { }
            public interface ISource<out T> { T Get() => default; }
            public class Source : ISource<Dog>, ISource<Cat> { }
            public static class Program { static Animal Read(ISource<Animal> source) => source.Get(); public static int Main() => Read(new Source()) == null ? 0 : 1; }
            """);
        Assert.Contains("Multiple variant default interface candidates", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    [Theory]
    [InlineData("string", "\"hello\"")]
    [InlineData("int[]", "new int[1]")]
    public void RepresentationsThatNeedAdaptersFailBeforeWasmExecution(string argument, string expression)
    {
        var pe = Frontend.Roslyn.Compile($$"""
            public interface ISource<out T> { T Get(); }
            public class Source : ISource<{{argument}}> { public {{argument}} Get() => {{expression}}; }
            public static class Program { static object Read(ISource<object> source) => source.Get(); public static int Main() => Read(new Source()) == null ? 1 : 0; }
            """);
        Assert.Contains("representation adapter", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    [Fact]
    public async Task CovariantContractCanComeFromALinkedAssembly()
    {
        var contracts = Frontend.Roslyn.Compile("public interface ISource<out T> { T Get(); } public class Animal { public int Value; } public class Dog : Animal { }", OutputKind.DynamicallyLinkedLibrary, "Contracts");
        var application = Frontend.Roslyn.Compile("""
            public class Source : ISource<Dog> { public Dog Get() { var dog = new Dog(); dog.Value = 42; return dog; } }
            public static class Program { static Animal Read(ISource<Animal> value) => value.Get(); public static int Main() => Read(new Source()).Value; }
            """, additionalReferences: [MetadataReference.CreateFromImage(contracts)]);
        using var contractStream = new MemoryStream(contracts);
        using var applicationStream = new MemoryStream(application);
        using var contract = AssemblyCompiler.Read(contractStream);
        using var app = AssemblyCompiler.Read(applicationStream);
        _ = new Xui.Compiler.XuiNet.Compilation(app, contract);
        var result = await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal(42, result.ExitCode);
    }
    private static string SourceDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}
