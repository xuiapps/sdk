using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.ReferenceSlots;

[Collection(nameof(ManagedConsoleCollection))]
public class ReturnAddressTests
{
    [Fact]
    public Task ReturnedFieldReferenceKeepsItsOwnerAndAliasesOtherReferences() => GcTests.AssertExecution("""
        using System;
        public class Item { public int Value; }
        public class Holder { public Item Item; public string Text; }
        public static class Program {
            static ref T Forward<T>(ref T value) => ref value;
            static ref Item Field(Holder holder) => ref Forward(ref holder.Item);
            static ref string Text(Holder holder) => ref Forward(ref holder.Text);
            public static int Main() {
                var original = new Holder(); var owner = original;
                ref Item first = ref Field(owner);
                ref Item second = ref Field(owner);
                owner = new Holder();
                first = new Item { Value = 7 }; second.Value += 3;
                Text(original) = "saved";
                Console.WriteLine(original.Text);
                Console.WriteLine(owner.Item == null);
                return original.Item.Value;
            }
        }
        """, "saved\nTrue\n", 10);

    [Fact]
    public Task CallerFrameSlotsSurviveCalleeFrameCleanupAndFinally() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static void Set(ref string slot, string value) { slot = value; }
            static ref string Forward(ref string caller) {
                string temporary = "temporary"; Set(ref temporary, "callee");
                try { return ref caller; }
                finally { Set(ref temporary, "cleared"); }
            }
            public static int Main() {
                string text = "before";
                for (int i = 0; i < 66000; i++) Forward(ref text) = "after";
                Console.WriteLine(text);
                return 42;
            }
        }
        """, "after\n", 42);

    [Fact]
    public Task LinearResultsRemainAddressesRatherThanNarrowedOrCopiedValues() => GcTests.AssertExecution("""
        using System;
        public struct Point { public int X; public int Y; }
        public static class Program {
            static ref T Forward<T>(ref T value) => ref value;
            static ref int Select(bool first, ref int a, ref int b) => ref (first ? ref a : ref b);
            public static int Main() {
                byte small = 1; float number = 2; Point point = default;
                Forward(ref small) = 255;
                Forward(ref number) = 3.5f;
                Forward(ref point).X = 7;
                Select(false, ref point.X, ref point.Y) = 11;
                Console.WriteLine(small); Console.WriteLine(number);
                return point.X + point.Y;
            }
        }
        """, "255\n3.5\n", 18);

    [Fact]
    public Task ReturnedStackStructAddressSharesSlotsButValueCopiesDoNot() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public struct Event { public string Text; public Item Target; public int Count; }
        public static class Program {
            static ref Event Forward(ref Event value) => ref value;
            public static int Main() {
                Event value = new Event { Text = "initial", Target = new Item { Value = 4 }, Count = 2 };
                ref Event alias = ref Forward(ref value);
                Event copy = alias;
                alias.Text = "changed"; alias.Count = 9; alias.Target.Value = 6;
                System.Console.WriteLine(copy.Text); System.Console.WriteLine(value.Text);
                return copy.Count + copy.Target.Value + value.Count;
            }
        }
        """, "initial\nchanged\n", 17);

    [Fact]
    public Task InlineGcValueFieldReturnsPreserveTheirOwner() => GcTests.AssertExecution("""
        public struct Point { public int X; }
        public class Holder { public Point Point; public ref Point Get() => ref Point; }
        public static class Program { public static int Main() { var holder = new Holder(); holder.Get().X = 7; return holder.Point.X; } }
        """, "", 7);

    [Fact]
    public Task InterfaceAndVirtualReturnsKeepTheSelectedLocation() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public interface IHolder { ref Item Get(); }
        public class Holder : IHolder {
            public Item First;
            public virtual ref Item Get() => ref First;
        }
        public class Derived : Holder {
            public Item Second;
            public override ref Item Get() => ref Second;
        }
        public static class Program {
            static void Set(IHolder holder) { holder.Get() = new Item { Value = 8 }; }
            public static int Main() {
                var holder = new Derived(); Set(holder);
                Holder basis = holder; basis.Get().Value += 2;
                System.Console.WriteLine(holder.First == null);
                return holder.Second.Value;
            }
        }
        """, "True\n", 10);

    [Fact]
    public Task MulticastReturnsTheLastHandlersAddressAndExecutesEveryHandler() => GcTests.AssertExecution("""
        public delegate ref string Getter();
        public class Holder {
            public string First = "first";
            public string Second = "second";
            public int Calls;
            public ref string GetFirst() { Calls++; return ref First; }
            public ref string GetSecond() { Calls++; return ref Second; }
        }
        public static class Program {
            public static int Main() {
                var holder = new Holder();
                Getter getter = new Getter(holder.GetFirst);
                getter += new Getter(holder.GetSecond);
                getter() = "changed";
                System.Console.WriteLine(holder.First);
                System.Console.WriteLine(holder.Second);
                return holder.Calls;
            }
        }
        """, "first\nchanged\n", 2);
}
