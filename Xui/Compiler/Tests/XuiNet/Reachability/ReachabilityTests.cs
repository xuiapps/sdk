using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.Reachability;
using Xui.Compiler.WASM.Emitter;
using Xunit;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.Tests.XuiNet.Reachability;

public class ReachabilityTests
{
    [Theory]
    [InlineData("EntryPoint")]
    [InlineData("Type")]
    [InlineData("PublicApi")]
    [InlineData("Assembly")]
    public void RootsPreserveOnlyTheirDependencyClosure(string policy)
    {
        using var assembly = Compile(Fixture("Collections"));
        var module = assembly.ManifestModule;
        Root root = policy switch
        {
            "EntryPoint" => Root.EntryPoint(assembly),
            "Type" => new Root.Type(Type(module, "Program")),
            "PublicApi" => new Root.Assembly(assembly, MemberPolicy.PublicApi),
            _ => new Root.Assembly(assembly)
        };
        var graph = Analyzer.Analyze([root]);
        Assert.True(graph.Contains(new Dependency.Type(Type(module, "Dictionary"))));
        Assert.Equal(policy != "EntryPoint", graph.Contains(new Dependency.Type(Type(module, "HashSet"))));
        Assert.Equal(policy is "Type" or "Assembly", graph.Contains(new Dependency.Type(Type(module, "InternalCollection"))));
        Assert.Equal(policy is "PublicApi" or "Assembly", graph.Methods.Any(method => method.ToString() == "Dictionary.Unused"));
        Assert.Equal(graph.Nodes.Count, graph.Nodes.Distinct().Count());
        foreach (var edge in graph.Edges)
        {
            Assert.Contains(edge.Source, graph.Nodes);
            Assert.Contains(edge.Target, graph.Nodes);
            if (edge.Condition is not null) Assert.Contains(edge.Condition, graph.Nodes);
        }
        Snapshot.AssertMatches(policy, Dump("Collections", graph));
    }

    [Fact]
    public async Task UnusedClassesDoNotChangeTheWasmBlob()
    {
        const string source = """
            public class Used { public int Value; public int Read() => Value; }
            public static class Program { public static int Main() { var x = new Used(); x.Value = 42; return x.Read(); } }
            """;
        using var baseline = Compile(source);
        // Unsupported bodies and layouts must not even be planned when the containing type is unused.
        using var extended = Compile(source + """
            public class Unused { public System.Random Random; public int[] Data; public int Run() => Random.Next(); }
            """);
        var first = new WasmEmitter().Emit(baseline);
        var emitter = new WasmEmitter();
        var second = emitter.Emit(extended);
        Assert.Equal(first, second);
        Assert.DoesNotContain(emitter.Types, type => type.Name == "Unused");
        Assert.DoesNotContain(emitter.Dependencies!.Nodes.OfType<Dependency.Type>(), type => type.Identity.ToString() == "Unused");
        Assert.Equal(42, (await Node.ExecuteAsync(second)).ExitCode);
    }

    [Fact]
    public async Task ExplicitTypeRootsPreserveMethodsWithoutExportingThem()
    {
        using var assembly = Compile(Fixture("Collections"));
        var emitter = new WasmEmitter();
        var wasm = emitter.Emit(assembly, additionalRoots: [new Root.Type(Type(assembly.ManifestModule, "HashSet"))]);
        Assert.Contains(emitter.Dependencies!.Methods, method => method.ToString() == "HashSet.Count");
        Assert.Contains(emitter.Types, type => type.Name == "HashSet");
        Assert.DoesNotContain(emitter.Types, type => type.Name == "InternalCollection");
        Assert.Equal(42, (await Node.ExecuteAsync(wasm)).ExitCode);
    }

    [Fact]
    public async Task VirtualTargetsRequireBothAnInvokedSlotAndAnInstantiatedType()
    {
        using var assembly = Compile(Fixture("Dispatch"));
        var graph = Analyzer.Analyze([Root.EntryPoint(assembly)]);
        var edge = Assert.Single(graph.Edges, edge => edge.Reason == DependencyReason.VirtualImplementation);
        Assert.Equal("IntBox.Read", Assert.IsType<Dependency.Method>(edge.Target).Identity.ToString());
        Assert.Equal("IntBox", Assert.IsType<Dependency.Instantiation>(edge.Condition).Identity.ToString());
        Assert.DoesNotContain(graph.Nodes.OfType<Dependency.Type>(), type => type.Identity.ToString() == "UnusedBox");
        Assert.DoesNotContain(graph.Methods, method => method.ToString() == "Box<Int32>.Unused");
        Snapshot.AssertMatches("Dispatch", Dump("Dispatch", graph));
        Assert.Equal(42, (await Node.ExecuteAsync(new WasmEmitter().Emit(assembly))).ExitCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void VirtualDiscoveryWorksInEitherOrder(bool allocationFirst)
    {
        using var assembly = Compile("""
            public class A { public virtual int Read() => 1; }
            public class B : A { public override int Read() => 2; }
            public static class Api {
                public static A Create() => new B();
                public static int Read(A value) => value.Read();
            }
            """, OutputKind.DynamicallyLinkedLibrary);
        var module = assembly.ManifestModule;
        var create = new Root.Method(Method(module, "Api", "Create"));
        var read = new Root.Method(Method(module, "Api", "Read"));
        var graph = Analyzer.Analyze(allocationFirst ? [create, read] : [read, create]);
        Assert.Contains(Method(module, "B", "Read"), graph.Methods);
    }

    [Fact]
    public void OpenTypeRootsApplyOnlyToDiscoveredSpecializations()
    {
        using var assembly = Compile("""
            public class Box<T> { public T Value; public T Read() => Value; public int Unused() => 7; }
            public class Set<T> { public int Unused() => 8; }
            public static class Program { public static int Main() { var box = new Box<int>(); return box.Read(); } }
            """);
        var graph = Analyzer.Analyze([new Root.Type(Type(assembly.ManifestModule, "Box`1")), Root.EntryPoint(assembly)]);
        Assert.Contains(graph.Methods, method => method.ToString() == "Box<Int32>.Unused");
        Assert.DoesNotContain(graph.Methods, method => method.DeclaringType.Arguments is null && method.DeclaringType.ToString() == "Box`1");
        Assert.DoesNotContain(graph.Nodes.OfType<Dependency.Type>(), type => type.Identity.ToString().StartsWith("Set"));
        Assert.Single(graph.Nodes.OfType<Dependency.Layout>(), type => type.Identity.ToString().StartsWith("Box"));
    }

    [Fact]
    public void RecursiveCallsAndReferenceFieldsTerminateWithoutRootingTheirMethods()
    {
        using var assembly = Compile("""
            public class Link { public Link Next; public int Unused() => 9; }
            public static class Program {
                static int A(int n) => n > 0 ? B(n - 1) : 0;
                static int B(int n) => A(n);
                public static int Main() { var link = new Link(); return A(4); }
            }
            """);
        var graph = Analyzer.Analyze([Root.EntryPoint(assembly)]);
        Assert.Single(graph.Methods, method => method.ToString() == "Program.A");
        Assert.Single(graph.Methods, method => method.ToString() == "Program.B");
        Assert.Single(graph.Nodes.OfType<Dependency.Layout>(), type => type.Identity.ToString() == "Link");
        Assert.DoesNotContain(graph.Methods, method => method.ToString() == "Link.Unused");
    }

    [Fact]
    public void StaticFieldsRetainInitializersAndTheirSideEffects()
    {
        using var assembly = Compile("""
            public static class State { public static int Value = Initialize(); static int Initialize() => Helper.Read(); }
            public static class Helper { public static int Read() => 42; }
            public static class Unused { public static int Value = 99; }
            public static class Program { public static int Main() => State.Value; }
            """);
        var graph = Analyzer.Analyze([Root.EntryPoint(assembly)]);
        Assert.Contains(graph.Methods, method => method.ToString() == "State..cctor");
        Assert.Contains(graph.Methods, method => method.ToString() == "Helper.Read");
        Assert.Contains(graph.Edges, edge => edge.Reason == DependencyReason.FieldRead);
        Assert.DoesNotContain(graph.Nodes.OfType<Dependency.Type>(), type => type.Identity.ToString() == "Unused");
        Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(assembly));
    }

    [Fact]
    public async Task HostReplacementsCollectOnceAndExcludeOriginalBodies()
    {
        using var assembly = Compile("System.Console.WriteLine(42);");
        var rewriter = new CountingConsoleRewriter();
        var emitter = new WasmEmitter();
        var wasm = emitter.Emit(assembly, rewriter);
        Assert.Equal(1, rewriter.Replacements);
        Assert.Single(emitter.Dependencies!.Nodes.OfType<Dependency.Rewrite>());
        Assert.DoesNotContain(emitter.Dependencies.Nodes.OfType<Dependency.Type>(), type => type.Identity.ToString() == "System.Console");
        Assert.Equal("42\n", (await Node.ExecuteAsync(wasm)).Output);
    }

    private sealed class CountingConsoleRewriter : ConsoleWriteLineRewriter
    {
        public int Replacements;
        public override bool Collect(Xui.Compiler.XuiNet.DotNet.Op.Instruction instruction, ref ILReader reader)
        {
            bool replaced = base.Collect(instruction, ref reader);
            if (replaced) Replacements++;
            return replaced;
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ForeignRootsAreRejectedBeforeBackendPlanning(bool methodRoot)
    {
        using var application = Compile("return 0;");
        using var library = Compile("public abstract class Base { public abstract int Read(); } public struct Point { public int X; }",
            OutputKind.DynamicallyLinkedLibrary);
        Root root = methodRoot ? new Root.Method(Method(library.ManifestModule, "Base", "Read")) :
            new Root.Type(Type(library.ManifestModule, "Point"));
        Assert.Contains("same-module", Assert.Throws<NotSupportedException>(() =>
            new WasmEmitter().Emit(application, additionalRoots: [root])).Message);
    }

    [Fact]
    public async Task UnreachableInstructionsDoNotRetainTheirDependencies()
    {
        using var stream = new MemoryStream(Frontend.Msil.Compile(il =>
        {
            var done = il.DefineLabel();
            il.Branch(ILOpCode.Br_s, done);
            il.Call(System.Reflection.Metadata.Ecma335.MetadataTokens.MethodDefinitionHandle(1));
            il.OpCode(ILOpCode.Pop);
            il.MarkLabel(done);
            il.LoadConstantI4(42);
            il.OpCode(ILOpCode.Ret);
        }));
        using var assembly = AssemblyCompiler.Read(stream);
        var graph = Analyzer.Analyze([Root.EntryPoint(assembly)]);
        Assert.DoesNotContain(graph.Edges, edge => edge.Reason == DependencyReason.Call);
        Assert.Equal(42, (await Node.ExecuteAsync(new WasmEmitter().Emit(assembly))).ExitCode);
    }

    [Fact]
    public void TypeReferencesPreserveFieldsWithoutInventingInstancesOrMethodRoots()
    {
        using var assembly = Compile("""
            public class Payload { public int Value; public int Unused() => 99; }
            public class Owner { public Payload Payload; public int Unread; }
            public static class Program { public static int Main() { var owner = new Owner(); return 0; } }
            """);
        var graph = Analyzer.Analyze([Root.EntryPoint(assembly)]);
        var payload = Type(assembly.ManifestModule, "Payload");
        Assert.True(graph.Contains(new Dependency.Layout(payload)));
        Assert.False(graph.Contains(new Dependency.Instantiation(payload)));
        Assert.DoesNotContain(graph.Methods, method => method.DeclaringType == payload);
        Assert.Equal(3, graph.Nodes.OfType<Dependency.Field>().Count());
    }

    [Fact]
    public async Task VirtualImplementationCanDiscoverAnotherAllocationAndDispatchTarget()
    {
        using var assembly = Compile("""
            public abstract class A { public abstract int Read(); }
            public sealed class B : A { public override int Read() => new C().Read(); }
            public sealed class C : A { public override int Read() => 42; }
            public static class Program { public static int Main() { A a = new B(); return a.Read(); } }
            """);
        var emitter = new WasmEmitter();
        var wasm = emitter.Emit(assembly);
        Assert.Contains(emitter.Dependencies!.Methods, method => method.ToString() == "C.Read");
        Assert.Contains(emitter.Dependencies.Edges, edge => edge.Reason == DependencyReason.VirtualImplementation &&
            edge.Target is Dependency.Method method && method.Identity.ToString() == "C.Read");
        Assert.Equal(42, (await Node.ExecuteAsync(wasm)).ExitCode);
    }

    private static Model.Assembly Compile(string source, OutputKind kind = OutputKind.ConsoleApplication)
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source, kind));
        return AssemblyCompiler.Read(stream);
    }

    private static TypeIdentity Type(Model.Module module, string name) => new(module,
        module.Types.Single(handle => module.GetType(handle).Name == name));

    private static MethodIdentity Method(Model.Module module, string type, string name)
    {
        var identity = Type(module, type);
        return new(identity, module.GetType((TypeDefinitionHandle)identity.Handle).Methods.Single(handle => module.GetMethod(handle).Name == name));
    }

    private static string Fixture(string name) => File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Reachability", name + ".cs"));
    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;

    private static string Dump(string fixture, DependencyGraph graph)
    {
        var text = new StringBuilder($"# {fixture} reachability\n\nSource: [{fixture}.cs](Fixtures/Reachability/{fixture}.cs)\n\n");
        text.AppendLine("## Retained requirements\n\n| Node | Kind | Identity |\n| --- | --- | --- |");
        var ids = graph.Nodes.Select((node, index) => (node, index)).ToDictionary(pair => pair.node, pair => pair.index);
        foreach (var node in graph.Nodes) text.AppendLine($"| N{ids[node]} | {node.GetType().Name} | `{Name(node)}` |");
        text.AppendLine("\n## Dependencies\n\n| From | To | Reason | IL offset | Condition |\n| --- | --- | --- | --- | --- |");
        foreach (var edge in graph.Edges)
            text.AppendLine($"| N{ids[edge.Source]} | N{ids[edge.Target]} | {edge.Reason} | {(edge.Offset is { } offset ? $"IL_{offset:x4}" : "—")} | {(edge.Condition is { } condition ? $"N{ids[condition]}" : "—")} |");
        return text.ToString();
    }

    private static string Name(Dependency node) => node switch
    {
        Dependency.Root { Value: Root.Method method } => "Method: " + method.Identity,
        Dependency.Root { Value: Root.Type type } => $"Type: {type.Identity} ({type.Policy})",
        Dependency.Root { Value: Root.Assembly assembly } => $"Assembly: {assembly.Value.Identity.Name} ({assembly.Policy})",
        Dependency.Type type => type.Identity.ToString(),
        Dependency.Layout layout => layout.Identity.ToString(),
        Dependency.Instantiation allocation => allocation.Identity.ToString(),
        Dependency.Method method => method.Identity.ToString(),
        Dependency.Signature signature => signature.Identity.ToString(),
        Dependency.VirtualCall call => call.Identity.ToString(),
        Dependency.Initialization initialization => initialization.Identity.ToString(),
        Dependency.Field field => field.Identity.DeclaringType + "." + field.Identity.DeclaringType.Module.GetField(field.Identity.Handle).Name,
        Dependency.Literal literal => literal.Value,
        Dependency.Intrinsic intrinsic => intrinsic.Name,
        Dependency.Rewrite rewrite => $"{rewrite.Identity} IL_{rewrite.Offset:x4}",
        _ => throw new NotSupportedException()
    };
}
