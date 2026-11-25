using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

[Collection(nameof(ManagedConsoleCollection))]
public class ReferenceContentTests
{
    [Fact]
    public Task ClosedQueriesUseActualInstanceReferenceContent() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        public struct Numeric { public long Value; public static string Shared; }
        public struct Value<T> { public T Item; }
        public enum Code : byte { Zero }
        public static class Program {
            static bool Refs<T>() => RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            public static int Main() {
                Console.WriteLine(Refs<int>()); Console.WriteLine(Refs<Code>());
                Console.WriteLine(Refs<Numeric>()); Console.WriteLine(Refs<int?>());
                Console.WriteLine(Refs<Value<Value<Numeric>>>());
                Console.WriteLine(Refs<string>()); Console.WriteLine(Refs<object>());
                Console.WriteLine(Refs<int[]>()); Console.WriteLine(Refs<Value<Value<string>>>());
                return 42;
            }
        }
        """, "False\nFalse\nFalse\nFalse\nFalse\nTrue\nTrue\nTrue\nTrue\n", 42);
}
