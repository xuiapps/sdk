using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

[Collection(nameof(ManagedConsoleCollection))]
public class UnusedGenericMethodsTests
{
    [Fact]
    public Task UnusedGenericVirtualsDoNotBlockOrdinaryDispatch() => GcTests.AssertExecution("""
        public class Parent { public virtual T Unused<T>(T value) => value; public virtual int Read() => 1; }
        public class Child : Parent { public override T Unused<T>(T value) => value; public override int Read() => 42; }
        public static class Program { public static int Main() { Parent value = new Child(); return value.Read(); } }
        """, "", 42);
}
