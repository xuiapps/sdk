using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

[Collection(nameof(ManagedConsoleCollection))]
public class VirtualMethodTests
{
    [Fact]
    public async Task LinkedGenericVirtualsCoexistWithInterfaceAndOrdinaryVirtualSlots()
    {
        byte[] library = Frontend.Roslyn.Compile("""
            public interface IValue { int Plain(); U Map<U>(U value); }
            public class Base<T> : IValue {
                public virtual int Plain() => 1;
                public virtual U Map<U>(U value) => value;
                public virtual U Map<U>(T ignored, U value) => value;
            }
            public static class Library {
                public static int Call(Base<int> value) => value.Map<int>(4) + value.Map<int>(0, 6);
                public static int Call(IValue value) => value.Map<int>(5) + value.Plain();
            }
            """, Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary, "VirtualLibrary");
        byte[] source = Frontend.Roslyn.Compile("""
            public class Derived : Base<int> {
                public int Calls;
                public override int Plain() => 7;
                public override U Map<U>(U value) { Calls += 10; return value; }
                public override U Map<U>(int ignored, U value) { Calls += 20; return value; }
            }
            public static class Program {
                public static int Main() {
                    var item = new Derived();
                    int result = Library.Call((Base<int>)item) + Library.Call((IValue)item);
                    return result + item.Calls;
                }
            }
            """, additionalReferences: [Microsoft.CodeAnalysis.MetadataReference.CreateFromImage(library)]);
        using var dependency = Canvas.CanvasTests.Read(library);
        using var app = Canvas.CanvasTests.Read(source);
        _ = new Xui.Compiler.XuiNet.Compilation(app, dependency);
        var emitter = new Xui.Compiler.WASM.Emitter.WasmEmitter();
        Assert.Equal(62, (await Repl.Node.ExecuteAsync(emitter.Emit(app))).ExitCode);
        var type = Assert.Single(emitter.Types, type => type.Name == "Derived");
        Assert.NotEmpty(type.GenericVirtualSlots);
        Assert.NotEmpty(type.InterfaceSlots);
        Assert.NotEmpty(type.VirtualSlots);
    }

    [Fact]
    public Task ClosedArgumentsUseIndependentSlotsAndPreserveNewslotChains() => GcTests.AssertExecution("""
        public class Base<T> {
            public int Calls;
            public virtual U Echo<U>(U value) { Calls += 1; return value; }
        }
        public class Override<T> : Base<T> {
            public override U Echo<U>(U value) { Calls += 10; return value; }
        }
        public class Hidden<T> : Override<T> {
            public new virtual U Echo<U>(U value) { Calls += 100; return value; }
        }
        public class Final : Hidden<string> {
            public override U Echo<U>(U value) { Calls += 1000; return value; }
        }
        public struct Point { public double X; public double Y; }
        public struct Label { public string Text; public int Value; }
        public static class Program {
            public static int Main() {
                var item = new Final(); Base<string> root = item; Hidden<string> hidden = item;
                System.Console.WriteLine(root.Echo<int>(42));
                System.Console.WriteLine(root.Echo<string>("base chain"));
                System.Console.WriteLine(hidden.Echo<double>(2.5));
                System.Console.WriteLine(hidden.Echo<Point>(new Point { X=3,Y=4 }).Y);
                System.Console.WriteLine(hidden.Echo<Label>(new Label { Text="gc",Value=5 }).Text);
                return item.Calls;
            }
        }
        """, "42\nbase chain\n2.5\n4\ngc\n", 3020);

    [Fact]
    public Task AbstractGenericMethodsAndByrefCallsDispatchToTheConcreteImplementation() => GcTests.AssertExecution("""
        public abstract class Base { public abstract T Read<T>(ref T value); }
        public class Derived : Base { public override T Read<T>(ref T value) => value; }
        public struct Point { public int X; public int Y; }
        public static class Program {
            public static int Main() {
                Base item = new Derived(); Point p = new Point { X=20,Y=22 };
                int scalar = 42; string text = "ref";
                System.Console.WriteLine(item.Read<int>(ref scalar));
                System.Console.WriteLine(item.Read<string>(ref text));
                var copy = item.Read<Point>(ref p); return copy.X+copy.Y;
            }
        }
        """, "42\nref\n", 42);

    [Fact]
    public Task GenericVirtualDelegatesKeepOverrideIdentity() => GcTests.AssertExecution("""
        public delegate int Callback(int value);
        public class Base { public virtual T Read<T>(T value) => value; }
        public class Derived : Base { public int Calls; public override T Read<T>(T value) { Calls++; return value; } }
        public static class Program {
            public static int Main() {
                var item = new Derived(); Base parent = item;
                Callback first = parent.Read<int>, second = item.Read<int>;
                System.Console.WriteLine(first == second);
                return first(20) + second(20) + item.Calls;
            }
        }
        """, "True\n", 42);
}
