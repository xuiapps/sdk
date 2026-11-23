using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

[Collection(nameof(ManagedConsoleCollection))]
public class GenericPolymorphismTests
{
    [Fact]
    public async Task DerivedBoxesExecuteAndSnapshot()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Generics", "DerivedBoxes.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(pe);
            Assert.Equal("42\n7\nTrue\nTrue\nnumber\nHello\nWorld\nTrue\nTrue\n52\n", managed.Output);
            Assert.Equal(42, managed.ExitCode);
            using var stream = new MemoryStream(pe);
            using var assembly = AssemblyCompiler.Read(stream);
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(assembly, new ConsoleWriteLineRewriter());
            Assert.Equal(managed, await Node.ExecuteAsync(wasm));
            var root = Assert.Single(emitter.Types, type => type.Name == "Box<Int32>");
            var middle = Assert.Single(emitter.Types, type => type.Name == "TaggedBox<String, Int32>");
            var leaf = Assert.Single(emitter.Types, type => type.Name == "IntBox");
            Assert.Same(root, middle.BaseType);
            Assert.Same(middle, leaf.BaseType);
            Assert.Equal(4u, leaf.FieldCount);
            Assert.Equal(root.Fields[0], middle.Fields[0]);
            Assert.Equal(middle.Fields, leaf.Fields.Take(2));
            Assert.Equal(root.VirtualSlots.Select(slot => slot.Declaration), leaf.VirtualSlots.Select(slot => slot.Declaration));
            Assert.Equal(leaf.Identity, leaf.VirtualSlots[0].Implementation.DeclaringType);
            Assert.Equal(middle.Identity, leaf.VirtualSlots[1].Implementation.DeclaringType);
            if (optimization == OptimizationLevel.Debug) continue;
            var metadata = new StringBuilder();
            foreach (var type in emitter.Types)
            {
                metadata.AppendLine($"### `{type.Name}`\n");
                metadata.AppendLine($"- Base: `{type.BaseType?.Name ?? "System.Object"}`\n- WASM type: `{type.WasmType.Value}`\n- Table start: `{type.TableStart}`\n");
                foreach (var field in type.Fields)
                    metadata.AppendLine($"- Field `{field.DeclaringType}.{field.Name}`: `{field.DeclaredType}`, first `{field.First.Value}`, count `{field.Count}`");
                metadata.AppendLine();
                foreach (var slot in type.VirtualSlots)
                    metadata.AppendLine($"- Slot {slot.Index}: `{slot.Implementation.DeclaringType}.{assembly.ManifestModule.GetMethod(slot.Implementation.Handle).Name}`");
                metadata.AppendLine();
            }
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("DerivedBoxes", $"# Generic inheritance and virtual dispatch\n\nSource: [DerivedBoxes.cs](Fixtures/Generics/DerivedBoxes.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{managed.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{managed.ExitCode}\n```\n\n## Runtime Types\n\n{metadata}## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Fact]
    public Task NewSlotsRemainDistinctThroughGenericBases() => GcTests.AssertExecution("""
        public class A<T> { public T Value; public virtual T Read() => Value; public virtual int Score() => 1; }
        public class B<T> : A<T> { public T Other; public new virtual T Read() => Other; public override int Score() => base.Score() + 2; }
        public class C<T> : B<T> { public override T Read() => base.Read(); }
        public static class Program {
            static int ReadA(A<int> a) => a.Read();
            static int ReadB(B<int> b) => b.Read();
            public static int Main() {
                var c = new C<int>(); c.Value = 4; c.Other = 7;
                return ReadA(c) * 100 + ReadB(c) * 10 + c.Score();
            }
        }
        """, "", 473);

    [Fact]
    public Task GenericOverridesOfNongenericBaseDispatchInsideConstructor() => GcTests.AssertExecution("""
        public abstract class A { public int Value; protected A() { Value = Read(); } public abstract int Read(); }
        public class B<T> : A { public T Tag; public B(T tag) { Tag = tag; } public override int Read() => 8; }
        public static class Program {
            static int Read(A a) => a.Read() + a.Value;
            public static int Main() { var b = new B<string>("tag"); System.Console.WriteLine(b.Tag); return Read(b); }
        }
        """, "tag\n", 16);

    [Fact]
    public Task VirtualTemplateOverloadsDoNotCollapseAfterSpecialization() => GcTests.AssertExecution("""
        public class A<T> {
            public virtual int Read(T value) => 1;
            public virtual int Read(int value) => 2;
            public int ThroughT(T value) => Read(value);
        }
        public class B<T> : A<T> { public override int Read(T value) => 3; }
        public static class Program {
            public static int Main() { A<int> a = new B<int>(); return a.ThroughT(0) * 10 + a.Read(0); }
        }
        """, "", 32);

    [Theory]
    [InlineData("float", "1.25f", "1.25\n")]
    [InlineData("long", "1234567890123L", "1234567890123\n")]
    [InlineData("byte", "250", "250\n")]
    [InlineData("string", "\"hello\"", "hello\n")]
    public Task VirtualCallsUseTheSpecializedParameterAndResultTypes(string type, string value, string output) => GcTests.AssertExecution($$"""
        public class A<T> { public virtual T Echo(T value) => value; }
        public class B<T> : A<T> { public override T Echo(T value) => base.Echo(value); }
        public static class Program {
            static void Print(A<{{type}}> a) => System.Console.WriteLine(a.Echo({{value}}));
            public static int Main() { Print(new A<{{type}}>()); Print(new B<{{type}}>()); return 0; }
        }
        """, output + output, 0);

    [Fact]
    public Task NestedBaseArgumentsAndInheritedMethodsKeepTheirContext() => GcTests.AssertExecution("""
        public class Box<T> { public T Value; }
        public class A<T> { public T Value; public virtual T Read() => Value; public T Inherited() => Read(); }
        public class B<T> : A<Box<T>> { public override Box<T> Read() => base.Read(); }
        public class C<T> : B<T> { }
        public static class Program {
            static Box<int> Read(A<Box<int>> a) => a.Inherited();
            public static int Main() {
                var c = new C<int>(); c.Value = new Box<int>(); c.Value.Value = 7;
                var result = Read(c); result.Value = 9;
                return c.Inherited().Value;
            }
        }
        """, "", 9);

    [Fact]
    public Task VirtualGcStructArgumentsAndResultsKeepValueSemantics() => GcTests.AssertExecution("""
        public struct Label { public string Text; public int Value; }
        public abstract class A<T> { public abstract T Change(T value); }
        public class B : A<Label> {
            public override Label Change(Label value) { value.Text = "changed"; value.Value = 9; return value; }
        }
        public static class Program {
            static Label Change(A<Label> a, Label value) => a.Change(value);
            public static int Main() {
                Label original = default; original.Text = "original"; original.Value = 3;
                var changed = Change(new B(), original);
                System.Console.WriteLine(original.Text); System.Console.WriteLine(changed.Text);
                return original.Value + changed.Value;
            }
        }
        """, "original\nchanged\n", 12);

    [Fact]
    public Task GenericBaseFieldsKeepTheirFlattenedPrefixAndHiddenFieldsStaySeparate() => GcTests.AssertExecution("""
        public struct Point { public int X; public int Y; }
        public class A<T> { public T Value; public virtual int Read() => 1; }
        public class B<T> : A<T> { public new string Value; public int Tail; }
        public class C : B<Point> { public override int Read() => base.Read() + Tail; }
        public static class Program {
            static int Read(A<Point> a) => a.Value.X + a.Value.Y + a.Read();
            public static int Main() {
                var c = new C(); A<Point> a = c; a.Value.X = 4; a.Value.Y = 6;
                c.Value = "hidden"; c.Tail = 8; System.Console.WriteLine(c.Value);
                return Read(c);
            }
        }
        """, "hidden\n", 19);

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    public Task GenericSiblingJoinsAndNominalTestsUseTheConstructedBase(int choice, int result) => GcTests.AssertExecution($$"""
        public class Root { }
        public abstract class A<T> : Root { public abstract int Read(); }
        public class B<T> : A<T> { public override int Read() => 1; }
        public class C<T> : A<T> { public override int Read() => 2; }
        public static class Program {
            static A<int> Choose(int x) => x > 0 ? new B<int>() : new C<int>();
            static void Check(Root value) {
                System.Console.WriteLine(value is A<int>);
                System.Console.WriteLine(value is A<string>);
                System.Console.WriteLine((value as B<string>) == null);
            }
            public static int Main() { Check(new B<int>()); Check(new B<string>()); return Choose({{choice}}).Read(); }
        }
        """, "True\nFalse\nTrue\nFalse\nTrue\nFalse\n", result);

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
