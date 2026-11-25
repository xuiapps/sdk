using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Gc;

[Collection(nameof(ManagedConsoleCollection))]
public class GcTests
{
    [Theory]
    [InlineData("Objects", "Classes and GC struct values", "4\n6\nfirst\ncopy\nother\n2\n7\n", 20)]
    [InlineData("Unit", "Unit with a string and inline Point", "Scout\n4\n6\n20\n20\nTrue\n", 0)]
    public async Task ClassesAndInlineStructsExecuteAndSnapshot(string fixture, string title, string output, int exitCode)
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Gc", fixture + ".cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = RunManaged(pe);
            Assert.Equal(output, managed.Output);
            Assert.Equal(exitCode, managed.ExitCode);
            byte[] wasm = Emit(pe);
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(managed, result);
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches(fixture, $"# {title}\n\nSource: [{fixture}.cs](Fixtures/Gc/{fixture}.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Fact]
    public Task GcStructConstructorsArgumentsAndReturnsCopyValues() => AssertExecution("""
        public struct Label {
            public string Text; public int Value;
            public Label(string text, int value) { Text = text; Value = value; }
        }
        public static class Program {
            static Label Change(Label value) { value.Text = "changed"; value.Value = 9; return value; }
            public static int Main() {
                var original = new Label("original", 2);
                var copy = Change(original);
                System.Console.WriteLine(original.Text);
                System.Console.WriteLine(copy.Text);
                copy.Value = 20;
                return original.Value + copy.Value;
            }
        }
        """, "original\nchanged\n", 22);

    [Fact]
    public Task ClassParametersAndReturnsShareIdentity() => AssertExecution("""
        public class Box { public int Value; public string Text; }
        public static class Program {
            static Box Change(Box value) { value.Value = 7; value.Text = "same"; return value; }
            public static int Main() {
                var original = new Box(); var alias = Change(original);
                alias.Value = 12; System.Console.WriteLine(original.Text);
                return original.Value;
            }
        }
        """, "same\n", 12);

    [Fact]
    public Task ResetInlineStructPreservesOtherFieldsAndAddresses() => AssertExecution("""
        public struct Inner { public string Text; public int Value; }
        public struct Outer { public Inner Inner; public int Tail; }
        public class Box { public Outer Data; public int Tail; }
        public static class Program {
            public static int Main() {
                var box = new Box(); box.Data.Inner.Text = "before"; box.Data.Inner.Value = 9;
                box.Data.Tail = 4; box.Tail = 8;
                box.Data.Inner = default;
                System.Console.WriteLine(box.Data.Inner.Text);
                return box.Data.Inner.Value + box.Data.Tail + box.Tail;
            }
        }
        """, "\n", 12);

    [Fact]
    public Task GcStructDupAndAssignmentKeepIndependentCopies() => AssertExecution("""
        public struct Label { public string Text; public int Value; }
        public static class Program {
            public static int Main() {
                Label a = default; Label b = default;
                a.Text = "a"; a.Value = 7;
                int first = (b = a).Value;
                a.Text = "new"; a.Value = 90;
                System.Console.WriteLine(b.Text);
                return first + b.Value;
            }
        }
        """, "a\n", 14);

    [Fact]
    public Task ConditionalGcStructResultsRetainValueSemantics() => AssertExecution("""
        public struct Label { public string Text; public int Value; }
        public static class Program {
            static Label Choose(int condition, Label a, Label b) => condition > 0 ? a : b;
            public static int Main() {
                Label a = default; Label b = default; a.Value = 3; b.Value = 5;
                var copy = Choose(1, a, b); copy.Value = 9;
                return a.Value + b.Value + copy.Value;
            }
        }
        """, "", 17);

    [Fact]
    public Task GcStructLoopAssignmentsCopyRatherThanAlias() => AssertExecution("""
        public struct Label { public string Text; public int Value; }
        public static class Program {
            public static int Main() {
                Label value = default; value.Text = "loop";
                for (int i = 0; i < 10; i++) {
                    var copy = value; copy.Value = copy.Value + 1; value = copy;
                }
                System.Console.WriteLine(value.Text); return value.Value;
            }
        }
        """, "loop\n", 10);

    [Fact]
    public Task ReferencesSupportIdentityAndNullChecks() => AssertExecution("""
        public class Box { public string Text; public Box Next; public int Value; }
        public static class Program {
            static Box Empty() => null;
            public static int Main() {
                var a = new Box(); var b = a; var other = new Box();
                if (a == other) return 99;
                if (a != b) return 98;
                if (a.Next != null) return 97;
                a.Next = b;
                if (a.Next == null) return 96;
                a.Text = "old"; a.Text = null;
                System.Console.WriteLine(a.Text);
                b = Empty(); if (b != null) return 95;
                bool same = a == a.Next; System.Console.WriteLine(same);
                a.Next = null;
                return a.Next == null ? 7 : 94;
            }
        }
        """, "\nTrue\n", 7);

    [Fact]
    public Task NonvirtualInstanceCallsMutateTheSameClass() => AssertExecution("""
        public class Box {
            public string Text; public int Value;
            public Box(string text, int value) { Text = text; Value = value; }
            public int Change(int value) { Value = value; return Value; }
        }
        public static class Program {
            public static int Main() {
                var box = new Box("box", 2); var alias = box;
                int result = alias.Change(8);
                System.Console.WriteLine(box.Text);
                return result + box.Value;
            }
        }
        """, "box\n", 16);

    [Fact]
    public Task RootGcStructInstanceMethodMutatesItsLocal() => AssertExecution("""
        public struct Label {
            public string Text; public int Value;
            public void Change(int value) { Value = value; }
        }
        public static class Program {
            public static int Main() {
                Label value = default; value.Text = "struct"; value.Change(4);
                var copy = value; copy.Change(9);
                System.Console.WriteLine(value.Text);
                return value.Value + copy.Value;
            }
        }
        """, "struct\n", 13);

    [Fact]
    public Task InlineNumericStoragePreservesWidthsAndFloatRounding() => AssertExecution("""
        public struct Numbers { public byte Small; public short Signed; public float Fraction; public long Wide; }
        public class Box { public Numbers Data; public string Text; }
        public static class Program {
            public static int Main() {
                Numbers value = default; value.Small = 250; value.Signed = -30000;
                value.Fraction = 1.25f; value.Wide = 1234567890123L;
                var box = new Box(); box.Data = value;
                value.Small = 1;
                var copy = box.Data; copy.Signed = 4;
                System.Console.WriteLine((int)box.Data.Small);
                System.Console.WriteLine((int)box.Data.Signed);
                System.Console.WriteLine(box.Data.Fraction);
                System.Console.WriteLine(box.Data.Wide);
                return copy.Signed;
            }
        }
        """, "250\n-30000\n1.25\n1234567890123\n", 4);

    [Theory]
    [InlineData("value.Read()")]
    [InlineData("value.Field")]
    public async Task NullInstanceAccessThrowsEvenWhenMethodDoesNotReadFields(string access)
    {
        string source = $$"""
            public class Box { public int Field; public int Read() => 7; }
            public static class Program {
                static int Access(Box value) => {{access}};
                public static int Main() => Access(null);
            }
            """;
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var error = Assert.Throws<TargetInvocationException>(() => Assembly.Load(pe).EntryPoint!.Invoke(null, null));
            Assert.IsType<NullReferenceException>(error.InnerException);
            await Node.AssertThrowsAsync(Emit(pe));
        }
    }

    [Fact]
    public Task StringNullComparisonsUseTheHostReferenceRepresentation() => AssertExecution("""
        public class Box { public string Text; }
        public static class Program {
            public static int Main() {
                string value = null;
                System.Console.WriteLine(value == null);
                System.Console.WriteLine(null == value);
                return 0;
            }
        }
        """, "True\nTrue\n", 0);

    [Theory]
    [InlineData(0, "right\n")]
    [InlineData(1, "left\n")]
    public Task ConditionalFieldAssignmentRetainsItsInlineAddress(int choice, string expected) => AssertExecution($$"""
        public struct Label { public string Text; }
        public class Box { public int Prefix; public Label Label; }
        public static class Program {
            static void Set(Box value, int choice) { value.Label.Text = choice > 0 ? "left" : "right"; }
            public static int Main() {
                var value = new Box(); Set(value, {{choice}});
                System.Console.WriteLine(value.Label.Text); return value.Prefix;
            }
        }
        """, expected, 0);

    [Fact]
    public Task AssignmentUpdatesExistingStructStorageWhileAnAddressIsLive() => AssertExecution("""
        public struct Label { public string Text; public int Value; }
        public static class Program {
            public static int Main() {
                Label a = default; Label b = default; a.Text = "old"; b.Text = "new"; b.Value = 5;
                a.Value = (a = b).Value + 1;
                System.Console.WriteLine(a.Text);
                return a.Value + b.Value;
            }
        }
        """, "new\n", 11);

    [Fact]
    public Task ManagedAddressLocalsForGcFieldsKeepTheirFieldPath() => AssertExecution("""
        public struct Label { public string Text; }
        public class Box { public Label Value; }
        public static class Program {
            public static int Main() { var box = new Box(); ref Label alias = ref box.Value; alias.Text = "x"; System.Console.WriteLine(box.Value.Text); return 0; }
        }
        """, "x\n", 0);

    internal static async Task AssertExecution(string source, string output, int exitCode)
    {
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = RunManaged(pe);
            Assert.Equal(output, managed.Output); Assert.Equal(exitCode, managed.ExitCode);
            Assert.Equal(managed, await Node.ExecuteAsync(Emit(pe)));
        }
    }

    internal static NodeResult RunManaged(byte[] pe)
    {
        var original = Console.Out;
        using var capture = new StringWriter(CultureInfo.InvariantCulture);
        try {
            Console.SetOut(capture);
            int result = (int)Assembly.Load(pe).EntryPoint!.Invoke(null, null)!;
            return new(capture.ToString().ReplaceLineEndings("\n"), result);
        }
        finally { Console.SetOut(original); }
    }

    internal static byte[] Emit(byte[] pe)
    {
        using var stream = new MemoryStream(pe);
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
