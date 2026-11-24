using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Delegates;

[Collection(nameof(ManagedConsoleCollection))]
public class MulticastTests
{
    [Fact]
    public Task DelegateBaseTypeTestsPreserveNominalConcreteTypes() => GcTests.AssertExecution("""
        public delegate int Callback();
        public delegate int Other();
        public class Plain { }
        public static class Program {
            static int A() => 7;
            public static int Main() {
                object value = new Callback(A) + new Callback(A);
                System.Console.WriteLine(value is System.Delegate);
                System.Console.WriteLine(value is System.MulticastDelegate);
                System.Console.WriteLine(new Plain() is System.Delegate);
                System.Console.WriteLine(value is Other);
                System.Console.WriteLine(((object)null as System.Delegate) == null);
                System.MulticastDelegate multi = (System.MulticastDelegate)value;
                System.Delegate common = multi;
                System.Console.WriteLine(common == (System.Delegate)value);
                var callback = (Callback)common;
                return callback();
            }
        }
        """, "True\nTrue\nFalse\nFalse\nTrue\nTrue\n", 7);

    [Fact]
    public Task LinearStructResultsComeFromTheLastHandler() => GcTests.AssertExecution("""
        public struct Point { public int X; public int Y; }
        public delegate Point Callback(Point value);
        public static class Program {
            static Point A(Point value) { value.X = 99; return value; }
            static Point B(Point value) { System.Console.WriteLine(value.X); value.Y = 5; return value; }
            public static int Main() {
                var callback = new Callback(A) + new Callback(B);
                var result = callback(new Point { X = 3, Y = 4 });
                return result.X + result.Y;
            }
        }
        """, "3\n", 8);

    [Fact]
    public Task RemovalUsesLastMatchingSequenceAndPreservesOldLists() => GcTests.AssertExecution("""
        public delegate int Callback();
        public static class Program {
            static int A() { System.Console.WriteLine("A"); return 1; }
            static int B() { System.Console.WriteLine("B"); return 2; }
            static int C() { System.Console.WriteLine("C"); return 3; }
            public static int Main() {
                var a = new Callback(A); var b = new Callback(B); var c = new Callback(C);
                var pair = a + b; var original = pair + c + pair;
                var reduced = original - pair;
                reduced(); original();
                var missing = reduced - (b + a);
                System.Console.WriteLine(object.ReferenceEquals(missing, reduced));
                System.Console.WriteLine((pair - pair) == null);
                System.Console.WriteLine(object.ReferenceEquals(pair - a, b));
                System.Console.WriteLine(object.ReferenceEquals(pair - b, a));
                System.Console.WriteLine(object.ReferenceEquals(a + null, a));
                System.Console.WriteLine(object.ReferenceEquals(null + a, a));
                System.Console.WriteLine((a + a + a - (a + a)) == a);
                return (a + b + c)();
            }
        }
        """, "A\nB\nC\nA\nB\nC\nA\nB\nTrue\nTrue\nTrue\nTrue\nTrue\nTrue\nTrue\nA\nB\nC\n", 3);

    [Fact]
    public Task EqualityUsesActualMethodAndReceiverAcrossDispatchPaths() => GcTests.AssertExecution("""
        public delegate int Callback();
        public interface IValue { int Get(); }
        public class Base { public virtual int Get() => 1; }
        public class Derived : Base, IValue { public override int Get() => 2; }
        public static class Program {
            public static int Main() {
                var value = new Derived(); Base parent = value; IValue contract = value;
                var a = new Callback(value.Get); var b = new Callback(parent.Get); var c = new Callback(contract.Get);
                System.Console.WriteLine(a == b); System.Console.WriteLine(b == c);
                System.Console.WriteLine((a + b) == (b + c));
                System.Console.WriteLine(a != new Callback(new Derived().Get));
                System.Console.WriteLine((a + b - c) == a);
                return (a + b)();
            }
        }
        """, "True\nTrue\nTrue\nTrue\nTrue\n", 2);

    [Fact]
    public Task InvocationRetainsItsSnapshotWhenAHandlerChangesSubscriptions() => GcTests.AssertExecution("""
        public delegate void Callback();
        public class Owner {
            public Callback Handlers;
            public void A() { System.Console.WriteLine("A"); Handlers -= new Callback(B); }
            public void B() { System.Console.WriteLine("B"); }
        }
        public static class Program {
            public static int Main() {
                var owner = new Owner(); owner.Handlers = new Callback(owner.A) + new Callback(owner.B);
                owner.Handlers(); owner.Handlers(); return 0;
            }
        }
        """, "A\nB\nA\n", 0);

    [Fact]
    public Task ReferenceArgumentsAliasButGcValueArgumentsAreCopiedPerHandler() => GcTests.AssertExecution("""
        public struct Value { public string Text; public int Number; }
        public struct Total { public int Number; }
        public delegate Value Callback(Value value, ref Total total, ref string text);
        public static class Program {
            static Value A(Value value, ref Total total, ref string text) {
                value.Number = 99; total.Number += 2; text = "changed"; return value;
            }
            static Value B(Value value, ref Total total, ref string text) {
                System.Console.WriteLine(value.Number); System.Console.WriteLine(text);
                total.Number += 3; value.Text = "last"; return value;
            }
            public static int Main() {
                var callback = new Callback(A) + new Callback(B);
                var total = new Total { Number = 1 }; string text = "initial";
                var result = callback(new Value { Text = "first", Number = 7 }, ref total, ref text);
                System.Console.WriteLine(result.Text); return total.Number + result.Number;
            }
        }
        """, "7\nchanged\nlast\n", 13);

    [Fact]
    public Task ExceptionsStopInvocationAndDifferentDelegateTypesCannotCombine() => GcTests.AssertExecution("""
        public delegate void Callback();
        public delegate void Other();
        public static class Program {
            static void A() { System.Console.WriteLine("A"); throw new System.InvalidOperationException(); }
            static void B() { System.Console.WriteLine("B"); }
            public static int Main() {
                var a = new Callback(A); var b = new Callback(B);
                try { (a + b)(); } catch (System.InvalidOperationException) { System.Console.WriteLine("caught"); }
                try { System.Delegate.Combine(a, new Other(B)); }
                catch (System.ArgumentException) { System.Console.WriteLine("combine"); }
                try { System.Delegate.Remove(a, new Other(B)); }
                catch (System.ArgumentException) { System.Console.WriteLine("remove"); }
                System.Console.WriteLine((System.Delegate)b == new Other(B));
                return 0;
            }
        }
        """, "A\ncaught\ncombine\nremove\nFalse\n", 0);
}
