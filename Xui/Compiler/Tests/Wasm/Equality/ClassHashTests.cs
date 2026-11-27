using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class ClassHashTests
{
    [Fact]
    public Task DefaultClassKeysUseStableIdentityHashes() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        class Key { public int Value; }
        public static class Program {
            public static int Main() {
                var a=new Key {Value=1}; var b=new Key {Value=1};
                var comparer=EqualityComparer<Key>.Default;
                int hash=comparer.GetHashCode(a); a.Value=8;
                Console.WriteLine(hash==comparer.GetHashCode(a));
                Console.WriteLine(comparer.GetHashCode(null)==0);
                var values=new Dictionary<Key,int>(); values.Add(a,20); values.Add(b,22);
                Console.WriteLine(values.Count); Console.WriteLine(values[a]); Console.WriteLine(values[b]);
                return values[a]+values[b];
            }
        }
        """, "True\nTrue\n2\n20\n22\n", 42, System.Reflection.Assembly.Load("System.Collections"));

    [Fact]
    public Task ClassComparerInterfacePreservesVirtualEqualityAndHashing() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        class Key {
            public int Value;
            public override bool Equals(object other) => other is Key key && key.Value==Value;
            public override int GetHashCode() => Value;
        }
        class Derived : Key { public override int GetHashCode() => Value+100; }
        public static class Program {
            public static int Main() {
                IEqualityComparer<Key> comparer=EqualityComparer<Key>.Default;
                var a=new Derived {Value=42}; var b=new Derived {Value=42};
                Console.WriteLine(comparer.Equals(a,b)); Console.WriteLine(comparer.GetHashCode(a));
                Console.WriteLine(comparer.GetHashCode(null));
                var values=new Dictionary<Key,int>(); values.Add(a,42);
                return values[b];
            }
        }
        """, "True\n142\n0\n", 42, System.Reflection.Assembly.Load("System.Collections"));
}
