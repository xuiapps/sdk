using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class MultipleExitTests
{
    [Fact]
    public Task LeavesSelectTheirDestinationAfterNestedFinally() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static int cleanup;
            static int Select(int value) {
                int result;
                try {
                    try {
                        if (value == 0) goto First;
                        if (value == 1) goto Second;
                        throw new InvalidOperationException();
                    } finally { cleanup += 1; }
                } catch (InvalidOperationException) { goto Second; }
                finally { cleanup += 10; }
            First: result = 10; goto End;
            Second: result = 20;
            End: return result;
            }
            public static int Main() {
                Console.WriteLine(Select(0)); Console.WriteLine(Select(1)); Console.WriteLine(Select(2));
                return cleanup;
            }
        }
        """, "10\n20\n20\n", 33);
}
