using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

[Collection(nameof(ManagedConsoleCollection))]
public class MethodTests
{
    [Fact]
    public Task ClosedMethodsHaveIndependentTypeAndMethodArguments() => GcTests.AssertExecution("""
        public class Pair<T> {
            public T Value;
            public U Choose<U>(U item) { return Forward<U>(item); }
            static U Forward<U>(U item) { U copy = item; return copy; }
        }
        public static class Program {
            static T Identity<T>(T item) => item;
            static int Identity(int item) => item + 1;
            public static int Main() {
                var pair = new Pair<int> { Value = 4 };
                System.Console.WriteLine(pair.Choose<string>("generic"));
                System.Console.WriteLine(Identity<double>(2.5));
                return Identity<int>(pair.Value) + Identity(3) + pair.Choose<int>(5);
            }
        }
        """, "generic\n2.5\n", 13);

    [Fact]
    public void ExpandingGenericMethodRecursionFailsWithABoundedDiagnostic()
    {
        byte[] pe = Frontend.Roslyn.Compile("""
            public class Box<T> { }
            public static class Program {
                static int Expand<T>() => Expand<Box<T>>();
                public static int Main() => Expand<int>();
            }
            """);
        Assert.Contains("specialization depth", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

}
