using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class ArrayIdentityTests
{
    [Fact]
    public Task DefaultComparersUseArrayIdentityIncludingNullAndInterfaceCalls() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public struct Item { public int Number; public string Name; }
        public static class Program {
            static void Check<T>(T a, T b) where T : class {
                var comparer = EqualityComparer<T>.Default;
                Console.WriteLine(comparer.Equals(a, a)); Console.WriteLine(comparer.Equals(a, b));
                Console.WriteLine(comparer.Equals(null, null)); Console.WriteLine(comparer.Equals(a, null));
                Console.WriteLine(comparer.GetHashCode(null));
                IEqualityComparer<T> contract = comparer;
                Console.WriteLine(contract.Equals(a, a)); Console.WriteLine(contract.Equals(a, b));
                Console.WriteLine(contract.GetHashCode(a) == comparer.GetHashCode(a));
            }
            public static int Main() {
                var first = new[] {1, 2}; var second = new[] {1, 2};
                Check(first, second); Check(new string[] {"x"}, new string[] {"x"});
                Check(new Item[1], new Item[1]); Check(new int[2,3], new int[2,3]);
                int hash = EqualityComparer<int[]>.Default.GetHashCode(first);
                first[0] = 9;
                Console.WriteLine(hash == EqualityComparer<int[]>.Default.GetHashCode(first));
                Console.WriteLine(Array.IndexOf(new[] { first, second }, second));
                Console.WriteLine(Array.IndexOf(new[] { first, second }, new[] {1, 2}));
                var keys = new Dictionary<int[], int>(); keys.Add(first, 20); keys.Add(second, 22);
                Console.WriteLine(keys.Count); Console.WriteLine(keys.ContainsKey(new[] {1, 2}));
                return keys[first] + keys[second];
            }
        }
        """, string.Concat(Enumerable.Repeat("True\nFalse\nTrue\nFalse\n0\nTrue\nFalse\nTrue\n", 4)) + "True\n1\n-1\n2\nFalse\n", 42,
        System.Reflection.Assembly.Load("System.Collections"));
}
