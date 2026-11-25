using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Classes;

[Collection(nameof(ManagedConsoleCollection))]
public class DefaultHashTests
{
    [Fact]
    public Task Int32HashingPreservesBitsAndChecksComparerReceiver() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        enum Signed : int { Min=int.MinValue }
        enum Unsigned : uint { Max=uint.MaxValue }
        public static class Program {
            static int Hash(EqualityComparer<int> comparer,int value) => comparer.GetHashCode(value);
            public static int Main() {
                Console.WriteLine(Hash(EqualityComparer<int>.Default,0));
                Console.WriteLine(Hash(EqualityComparer<int>.Default,-1));
                Console.WriteLine(Hash(EqualityComparer<int>.Default,int.MinValue));
                Console.WriteLine(Hash(EqualityComparer<int>.Default,int.MaxValue));
                Console.WriteLine(EqualityComparer<uint>.Default.GetHashCode(uint.MaxValue));
                Console.WriteLine(EqualityComparer<Signed>.Default.GetHashCode(Signed.Min));
                Console.WriteLine(EqualityComparer<Unsigned>.Default.GetHashCode(Unsigned.Max));
                try { Hash(null,42); } catch(NullReferenceException) { Console.WriteLine("null comparer"); }
                return 42;
            }
        }
        """, "0\n-1\n-2147483648\n2147483647\n-1\n-2147483648\n-1\nnull comparer\n", 42);
}
