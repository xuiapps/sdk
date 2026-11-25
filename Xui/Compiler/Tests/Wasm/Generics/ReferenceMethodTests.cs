using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

[Collection(nameof(ManagedConsoleCollection))]
public class ReferenceMethodTests
{
    [Fact]
    public Task GenericRefChildSetterReadsReplacesAndClearsTheActualField() => GcTests.AssertExecution("""
        public class View { public int Value; }
        public class Child : View { }
        public class Container {
            public Child Content;
            public int Changes;
            public void Set(Child value) { SetChild<Child>(ref Content, value); }
            void SetChild<T>(ref T field, T value) where T : View {
                if (field == value) return;
                if (field is not null) { Changes += field.Value; field = null; }
                if (value is not null) { Changes++; field = value; }
            }
        }
        public static class Program {
            public static int Main() {
                var container = new Container();
                var child = new Child { Value = 7 };
                container.Set(child); container.Set(child); container.Set(null);
                System.Console.WriteLine(container.Content == null);
                return container.Changes;
            }
        }
        """, "True\n", 8);

    [Fact]
    public Task ReferencePairsPreserveAliasingAcrossOwnersFieldsAndLoopIterations() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public class Base { public Item First; }
        public class Holder : Base { public Item Second; }
        public static class Program {
            static void Both(ref Item a, ref Item b, Item value) { a = value; b.Value += 1; }
            static void Forward(ref Item a, Item value) { Both(ref a, ref a, value); }
            public static int Main() {
                var original = new Holder(); var owner = original;
                ref Item saved = ref owner.First;
                owner = new Holder();
                for (int i = 0; i < 3; i++) {
                    ref Item current = ref owner.First;
                    if (i == 0) saved = ref current;
                    Forward(ref current, new Item { Value = i });
                    owner = new Holder();
                }
                saved.Value += 10;
                Forward(ref original.Second, saved);
                System.Console.WriteLine(original.First == null);
                return original.Second.Value;
            }
        }
        """, "True\n", 12);

    [Fact]
    public Task AddressTakenLocalsArgumentsAndStringsUseCallerOwnedReferenceSlots() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public class Holder {
            public string Text;
            public Holder(ref Item item) { item = new Item { Value = 5 }; }
        }
        public static class Program {
            static void Replace<T>(ref T location, T value) { location = value; }
            static Item Argument(Item value) { Replace(ref value, new Item { Value = 9 }); return value; }
            public static int Main() {
                Item item = null;
                var holder = new Holder(ref item);
                Replace(ref holder.Text, "field");
                string text = "before";
                Replace(ref text, holder.Text);
                System.Console.WriteLine(text);
                return item.Value + Argument(null).Value;
            }
        }
        """, "field\n", 14);

    [Fact]
    public Task ConditionalByrefsSelectTheActualField() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public class Holder { public Item First; public Item Second; }
        public static class Program {
            static void Set(Holder holder, bool first) {
                ref Item location = ref (first ? ref holder.First : ref holder.Second);
                location = new Item { Value = first ? 4 : 8 };
            }
            public static int Main() {
                var holder = new Holder(); Set(holder, true); Set(holder, false);
                return holder.First.Value + holder.Second.Value;
            }
        }
        """, "", 12);
    [Fact]
    public Task VirtualRefCallsAndStructResultsUseTheSameAbi() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public struct Point { public int X; public int Y; }
        public interface IChange { Point Change(ref Item item, int value); }
        public class Change : IChange {
            public virtual Point ChangeValue(ref Item item, int value) {
                item = new Item { Value = value }; return new Point { X = value, Y = 2 };
            }
            Point IChange.Change(ref Item item, int value) => ChangeValue(ref item, value);
        }
        public class Derived : Change {
            public override Point ChangeValue(ref Item item, int value) => base.ChangeValue(ref item, value + 1);
        }
        public static class Program {
            public static int Main() {
                Item item = null; IChange change = new Derived();
                Point result = change.Change(ref item, 7);
                return item.Value + result.X + result.Y;
            }
        }
        """, "", 18);

    [Fact]
    public Task ReferenceSlotsAreReclaimedOnNormalAndExceptionalReturn() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public static class Program {
            static void Replace<T>(ref T slot, T value) { slot = value; }
            static int Invoke(Item item, bool fail) {
                Item local = null; Replace(ref local, item);
                if (fail) throw new System.InvalidOperationException();
                return local.Value;
            }
            public static int Main() {
                var item = new Item { Value = 1 }; int total = 0;
                for (int i = 0; i < 66000; i++) {
                    try { total += Invoke(item, (i & 1) == 0); }
                    catch (System.InvalidOperationException) { }
                }
                return total;
            }
        }
        """, "", 33000);

    [Fact]
    public Task DefaultReferenceAndNullFieldAddressHaveManagedSemantics() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public class Holder { public Item Item; }
        public static class Program {
            static void Clear<T>(ref T value) where T : class { value = default; }
            public static int Main() {
                var holder = new Holder { Item = new Item() }; Clear(ref holder.Item);
                System.Console.WriteLine(holder.Item == null);
                holder = null; int result = 0;
                try { Clear(ref holder.Item); }
                catch (System.NullReferenceException) { result = 7; }
                return result;
            }
        }
        """, "True\n", 7);

}
