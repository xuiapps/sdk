using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Delegates;

[Collection(nameof(ManagedConsoleCollection))]
public class DelegateTests
{
    [Fact]
    public Task ExplicitStaticAndInstanceDelegatesReturnValues() => GcTests.AssertExecution("""
        public delegate int Callback(int value);
        public class Counter { public int Count; public int Add(int value) { Count += value; return Count; } }
        public static class Program {
            static int Twice(int value) => value * 2;
            public static int Main() {
                var counter = new Counter { Count = 3 };
                var first = new Callback(Twice);
                var second = new Callback(counter.Add);
                System.Console.WriteLine(first(4));
                System.Console.WriteLine(second(5));
                return counter.Count;
            }
        }
        """, "8\n8\n", 8);

    [Fact]
    public Task EscapingClosuresShareCapturedVariablesAndKeepTheirReceiverAlive() => GcTests.AssertExecution("""
        public delegate int Read();
        public delegate void Add(int amount);
        public class Callbacks { public Read Read; public Add Add; }
        public class Factory {
            public int Factor;
            public Callbacks Create(int seed) {
                int count = seed;
                return new Callbacks { Read = () => count, Add = amount => count += amount * Factor };
            }
        }
        public static class Program {
            public static int Main() {
                var factory = new Factory { Factor = 2 };
                var a = factory.Create(1); var b = factory.Create(10);
                factory.Factor = 3; factory = null;
                a.Add(4); b.Add(2); a.Add(1);
                System.Console.WriteLine(a.Read());
                return b.Read();
            }
        }
        """, "16\n", 16);

    [Fact]
    public Task VirtualAndInterfaceMethodGroupsCaptureTheSelectedImplementation() => GcTests.AssertExecution("""
        public delegate int Callback(int value);
        public interface ICounter { int Add(int value); }
        public class Base { public virtual int Add(int value) => value + 1; }
        public class Derived : Base, ICounter { public override int Add(int value) => value + 7; }
        public static class Program {
            public static int Main() {
                Base counter = new Derived(); ICounter contract = (ICounter)counter;
                var first = new Callback(counter.Add); var second = new Callback(contract.Add);
                counter = new Base();
                var third = new Callback(Twice);
                return first(1) + second(2) + third(3);
            }
            static int Twice(int value) => value * 2;
        }
        """, "", 23);

    [Fact]
    public Task ReferenceArgumentsAndLinearStructResultsUseTheCallbackAbi() => GcTests.AssertExecution("""
        public class Item { public int Value; }
        public struct Point { public int X; public int Y; }
        public delegate Point Callback(ref Item item, int value);
        public static class Program {
            static Point Change(ref Item item, int value) {
                item = new Item { Value = value }; return new Point { X = value, Y = 2 };
            }
            public static int Main() {
                var callback = new Callback(Change); Item item = null;
                Point point = callback(ref item, 9);
                return item.Value + point.X + point.Y;
            }
        }
        """, "", 20);

    [Fact]
    public Task DelegateNullAndCallbackExceptionsRemainCatchable() => GcTests.AssertExecution("""
        public delegate void Callback();
        public static class Program {
            static void Fail() { throw new System.InvalidOperationException(); }
            public static int Main() {
                int count = 0; Callback callback = null;
                try { callback(); } catch (System.NullReferenceException) { count++; }
                callback = new Callback(Fail);
                try { callback(); } catch (System.InvalidOperationException) { count += 2; }
                return count;
            }
        }
        """, "", 3);
    [Fact]
    public Task NominalDelegateTypesStayDistinctWhenStoredAsObject() => GcTests.AssertExecution("""
        public delegate int First(int value);
        public delegate int Second(int value);
        public static class Program {
            static int Echo(int value) => value;
            public static int Main() {
                object callback = new First(Echo);
                System.Console.WriteLine(callback is First);
                System.Console.WriteLine(callback is Second);
                return ((First)callback)(7);
            }
        }
        """, "True\nFalse\n", 7);

    [Fact]
    public Task MethodBindingSupportsReferenceParameterAndResultConversions() => GcTests.AssertExecution("""
        public class Base { public int Value; }
        public class Derived : Base { }
        public delegate Base Callback(Derived value);
        public static class Program {
            static Derived Convert(Base value) => new Derived { Value = value.Value + 1 };
            public static int Main() {
                var callback = new Callback(Convert);
                return callback(new Derived { Value = 9 }).Value;
            }
        }
        """, "", 10);

    [Fact]
    public Task NullInstanceBindingsMatchClrExceptions() => GcTests.AssertExecution("""
        public delegate void Callback();
        public class Receiver { public void Direct() { } public virtual void Virtual() { } }
        public static class Program {
            public static int Main() {
                Receiver receiver = null; int count = 0;
                try { var callback = new Callback(receiver.Direct); callback(); }
                catch (System.ArgumentException) { count++; }
                try { var callback = new Callback(receiver.Virtual); callback(); }
                catch (System.NullReferenceException) { count += 2; }
                return count;
            }
        }
        """, "", 3);

}
