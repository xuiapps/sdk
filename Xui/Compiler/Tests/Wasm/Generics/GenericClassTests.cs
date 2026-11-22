using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Rewrite;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

[Collection(nameof(ManagedConsoleCollection))]
public class GenericClassTests
{
    [Fact]
    public async Task ClosedBoxesExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Generics", "Boxes.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("42\nHello\n42\n7\nHello\nWorld\n", managed.Output);
            Assert.Equal(0, managed.ExitCode);
            using var stream = new MemoryStream(pe);
            using var assembly = AssemblyCompiler.Read(stream);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());
            Assert.Equal(managed, await Node.ExecuteAsync(wasm));
            Assert.Equal(2, emitter.Types.Count);
            var number = Assert.Single(emitter.Types, type => type.GenericArguments.Single().Code == SignatureTypeCode.Int32);
            var text = Assert.Single(emitter.Types, type => type.GenericArguments.Single().Code == SignatureTypeCode.String);
            Assert.Equal(number.GenericDefinition, text.GenericDefinition);
            Assert.NotEqual(number.Identity, text.Identity);
            Assert.NotEqual(number.Descriptor, text.Descriptor);
            Assert.Equal(SignatureTypeCode.Int32, Assert.Single(number.Fields).DeclaredType.Code);
            Assert.Equal(SignatureTypeCode.String, Assert.Single(text.Fields).DeclaredType.Code);
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Boxes", $"# Specialized generic classes\n\nSource: [Boxes.cs](Fixtures/Generics/Boxes.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{managed.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{managed.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Fact]
    public Task MemberResolutionKeepsTemplateOverloadsDistinct() => GcTests.AssertExecution("""
        public class Box<T> {
            public int Read(T value) => 1;
            public int Read(int value) => 2;
            public int ThroughT(T value) => Read(value);
        }
        public static class Program {
            public static int Main() { var box = new Box<int>(); return box.ThroughT(1) * 10 + box.Read(1); }
        }
        """, "", 12);

    [Fact]
    public Task NestedAndRecursiveGenericReferencesPreserveIdentity() => GcTests.AssertExecution("""
        public class Box<T> { public T Value; }
        public class Node<T> { public T Value; public Node<T> Next; }
        public static class Program {
            public static int Main() {
                var number = new Box<int>(); number.Value = 3;
                var nested = new Box<Box<int>>(); nested.Value = number; nested.Value.Value = 7;
                var first = new Node<string>(); first.Value = "node"; first.Next = first;
                System.Console.WriteLine(first.Next.Value); System.Console.WriteLine(first == first.Next);
                return number.Value;
            }
        }
        """, "node\nTrue\n", 7);

    [Fact]
    public Task SpecializedMethodsRoundFloatAndNarrowSmallIntegers() => GcTests.AssertExecution("""
        public class Box<T> { public T Value; public Box(T value) { Value = value; } public T Read() => Value; }
        public static class Program {
            public static int Main() {
                var f = new Box<float>(1.25f); var b = new Box<byte>(250); var l = new Box<long>(1234567890123L);
                System.Console.WriteLine(f.Read()); System.Console.WriteLine((int)b.Read()); System.Console.WriteLine(l.Read()); return 0;
            }
        }
        """, "1.25\n250\n1234567890123\n", 0);

    [Fact]
    public Task GcStructArgumentsKeepValueSemanticsAfterSubstitution() => GcTests.AssertExecution("""
        public struct Label { public string Text; public int Value; }
        public class Box<T> { public T Value; public Box(T value) { Value = value; } public T Read() => Value; }
        public static class Program {
            public static int Main() {
                Label label = default; label.Text = "original"; label.Value = 3;
                var box = new Box<Label>(label); label.Text = "changed";
                var copy = box.Read(); copy.Value = 9;
                System.Console.WriteLine(box.Value.Text); return box.Value.Value + copy.Value;
            }
        }
        """, "original\n", 12);

    [Fact]
    public void GenericMethodsRemainExplicitlyUnsupported()
    {
        var pe = Frontend.Roslyn.Compile("public static class Program { static T Id<T>(T x) => x; public static int Main() => Id(3); }");
        Assert.Contains("MethodSpecification", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    [Fact]
    public void ExpandingGenericFieldGraphsStopWithADiagnostic()
    {
        var pe = Frontend.Roslyn.Compile("public class Growing<T> { public Growing<Growing<T>> Next; } public static class Program { public static int Main() { var x = new Growing<int>(); return 0; } }");
        Assert.Contains("specialization depth", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    [Fact]
    public void PrimitiveGenericDefaultFailsExplicitly()
    {
        var pe = Frontend.Roslyn.Compile("public class Box<T> { public T Value; public void Clear() { Value = default; } } public static class Program { public static int Main() { var box = new Box<int>(); box.Clear(); return 0; } }");
        Assert.Contains("initobj", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    [Fact]
    public void GenericStructLayoutRemainsExplicitlyUnsupported()
    {
        var pe = Frontend.Roslyn.Compile("public struct Pair<T> { public T First; } public static class Program { public static int Main() { Pair<int> pair = default; pair.First = 3; return pair.First; } }");
        Assert.Contains("Generic struct", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
