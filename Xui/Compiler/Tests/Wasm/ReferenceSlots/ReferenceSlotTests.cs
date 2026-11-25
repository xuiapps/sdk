using System.Runtime.CompilerServices;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.ReferenceSlots;

[Collection(nameof(ManagedConsoleCollection))]
public class ReferenceSlotTests
{
    [Fact]
    public async Task EventCopiesAndByrefDispatch()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "../../Fixtures/ReferenceSlots/Events.cs"));
        await GcTests.AssertExecution(source, "handled\noriginal\nTrue\n", 42);
        byte[] wasm = GcTests.Emit(Frontend.Roslyn.Compile(source));
        var result = await Node.ExecuteAsync(wasm);
        string wat = await WasmTools.ToWatAsync(wasm);
        Snapshot.AssertMatches("Events", $"# Stack events with reference slots\n\nSource: [Events.cs](Fixtures/ReferenceSlots/Events.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
    }

    [Fact]
    public Task AliasesObserveImmediateMutations() => GcTests.AssertExecution("""
        using System;
        public struct Event { public string Name; public int Value; }
        public static class Program {
            static void Change(ref Event a, ref Event b) { a.Name = "shared"; b.Value = a.Name == null ? 0 : 42; a = b; }
            public static int Main() { Event e = default; Change(ref e, ref e); Console.WriteLine(e.Name); return e.Value; }
        }
        """, "shared\n", 42);

    [Fact]
    public Task NestedAddressesAndResetPreserveSlots() => GcTests.AssertExecution("""
        using System;
        public struct Inner { public string Name; public int Value; }
        public struct Outer { public Inner Inner; public object Owner; }
        public class Box { }
        public static class Program {
            static void Change(ref Inner e) { e.Name = "nested"; e.Value = 42; }
            static void Clear(ref Outer e) { e = default; }
            public static int Main() {
                Outer e = default; e.Owner = new Box(); Change(ref e.Inner);
                Outer copy = e; Clear(ref e); Change(ref e.Inner);
                Console.WriteLine(copy.Inner.Name); Console.WriteLine(e.Owner == null); Console.WriteLine(copy.Owner != null);
                return e.Inner.Value;
            }
        }
        """, "nested\nTrue\nTrue\n", 42);

    [Fact]
    public Task NumericStructsCanBePassedByRef() => GcTests.AssertExecution("""
        public struct Point { public int X; public int Y; }
        public static class Program {
            static void Move(ref Point p) { p.X += 20; p.Y += 22; }
            public static int Main() { Point p = default; Move(ref p); return p.X + p.Y; }
        }
        """, "", 42);

    [Fact]
    public Task NestedCallsKeepCallerReferencesAliveDuringUnwind() => GcTests.AssertExecution("""
        using System;
        public struct Event { public string Name; public object Owner; public int Value; }
        public class Box { }
        public static class Program {
            static void Keep(ref Event e) { }
            static void Fail(ref Event caller) {
                Event inner = default; inner.Name = "inner"; inner.Owner = new Box(); Keep(ref inner);
                caller.Value += 1; throw new Exception("failed");
            }
            public static int Main() {
                Event e = default; e.Name = "outer"; e.Owner = new Box();
                for (int i = 0; i < 80000; i++) { try { Fail(ref e); } catch (Exception) { } }
                Console.WriteLine(e.Name); Console.WriteLine(e.Owner != null); return e.Value;
            }
        }
        """, "outer\nTrue\n", 80000);

    [Fact]
    public Task CopiesTransferReferencesBetweenStackAndClassFields() => GcTests.AssertExecution("""
        using System;
        public class Owner { public int Value; }
        public struct Event { public Owner First; public Owner Second; public string Name; public int Value; }
        public class Holder { public Event Value; }
        public static class Program {
            static void Keep(ref Event e) { }
            public static int Main() {
                Event e = default; Keep(ref e); e.First = new Owner { Value = 10 }; e.Second = new Owner { Value = 20 }; e.Name = "saved"; e.Value = 12;
                var holder = new Holder(); holder.Value = e;
                e = default; e = holder.Value; holder.Value = default;
                Console.WriteLine(e.Name); return e.First.Value + e.Second.Value + e.Value;
            }
        }
        """, "saved\n", 42);

    [Fact]
    public Task RefLocalsAndOutParametersShareStorage() => GcTests.AssertExecution("""
        using System;
        public struct Event { public string Name; public int Value; }
        public static class Program {
            static void Create(out Event e) { e = default; e.Name = "out"; e.Value = 42; }
            static int Read(in Event e) => e.Value;
            public static int Main() { Event e = default; ref Event alias = ref e; Create(out alias); Console.WriteLine(e.Name); return Read(in e); }
        }
        """, "out\n", 42);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FrameSlotsAreClearedAtTheHostBoundary(bool throws)
    {
        string source = $$"""
            using System;
            public struct Event { public string Name; public object Owner; }
            public class Box { }
            public static class Program {
                static void Keep(ref Event e) { }
                public static int Main() {
                    Event e = default; e.Name = "held"; e.Owner = new Box(); Keep(ref e);
                    {{(throws ? "throw new Exception();" : "return 42;")}}
                }
            }
            """;
        await ReferenceSlotHost.AssertCleared(GcTests.Emit(Frontend.Roslyn.Compile(source)), throws);
    }

    [Fact]
    public Task InterfaceDispatchSharesTheStackEvent() => GcTests.AssertExecution("""
        using System;
        public ref struct Event { public string Name; public IHandler Handler; public int Value; }
        public interface IHandler { void Handle(ref Event e); }
        public class Handler : IHandler { public void Handle(ref Event e) { e.Name = "interface"; e.Value = 42; } }
        public static class Program {
            public static int Main() { Event e = default; e.Handler = new Handler(); e.Handler.Handle(ref e); Console.WriteLine(e.Name); return e.Value; }
        }
        """, "interface\n", 42);

    [Fact]
    public Task ConstructorResultsUseFrameOwnedSlots() => GcTests.AssertExecution("""
        using System;
        public struct Event {
            public string Name; public int Value;
            public Event(string name, int value) { Name = name; Value = value; }
            public void Add(int value) { Value += value; }
        }
        public static class Program {
            static void Change(ref Event e) { e = new Event("created", 40); e.Add(2); }
            public static int Main() { Event e = default; Change(ref e); Console.WriteLine(e.Name); return e.Value; }
        }
        """, "created\n", 42);

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
