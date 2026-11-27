using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class AggregateTests
{
    [Fact]
    public Task NestedInnerMessagesIncludeActualValuesAndCatchHierarchyMatchesClr() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                object actual = 42;
                var inner = new ArgumentOutOfRangeException("number", actual, "range");
                var error = new AggregateException("outer", new AggregateException("middle", inner));
                Console.WriteLine(error.Message);
                try { throw error; }
                catch (SystemException) { return -1; }
                catch (AggregateException e) { Console.WriteLine(ReferenceEquals(e.InnerException.InnerException, inner)); }
                try { _ = new AggregateException("outer", (Exception)null); }
                catch (ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                return 42;
            }
        }
        """, "outer (middle (range (Parameter 'number')\nActual value was 42.))\nTrue\ninnerException\n", 42);
}
