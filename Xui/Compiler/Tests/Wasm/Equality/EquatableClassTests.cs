using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class EquatableClassTests
{
    [Fact]
    public Task DefaultComparerUsesTypedVirtualEqualityAndItsNullContract() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        class Key : IEquatable<Key> {
            public int Value;
            public virtual bool Equals(Key other) => other!=null && Value==other.Value;
            public override bool Equals(object other) => false;
            public override int GetHashCode()=>Value;
        }
        class Derived : Key { public override bool Equals(Key other)=>other!=null && Value%10==other.Value%10; public override int GetHashCode()=>Value%10; }
        class Explicit : IEquatable<Explicit> {
            public static int Calls;
            bool IEquatable<Explicit>.Equals(Explicit other) { Calls++; return false; }
        }
        public static class Program {
            public static int Main() {
                IEqualityComparer<Key> c=EqualityComparer<Key>.Default;
                var a=new Derived {Value=12}; var b=new Derived {Value=22};
                Console.WriteLine(c.Equals(a,b)); Console.WriteLine(EqualityComparer<Derived>.Default.Equals(a,b)); Console.WriteLine(c.Equals(a,null)); Console.WriteLine(c.Equals(null,a)); Console.WriteLine(c.Equals(null,null));
                Console.WriteLine(Array.IndexOf<Key>(new Key[]{null,a},b));
                var e=new Explicit(); var ec=EqualityComparer<Explicit>.Default;
                Console.WriteLine(ec.Equals(e,e)); Console.WriteLine(ec.Equals(e,null)); Console.WriteLine(ec.Equals(null,e)); Console.WriteLine(ec.Equals(null,null)); Console.WriteLine(Explicit.Calls);
                var values=new Dictionary<Key,int>(); values.Add(a,42); return values[b];
            }
        }
        """, "True\nFalse\nFalse\nFalse\nTrue\n1\nFalse\nFalse\nFalse\nTrue\n1\n", 42, System.Reflection.Assembly.Load("System.Collections"));

    [Fact]
    public Task RecordKeysRetainValueEqualityAndRuntimeTypeIdentity() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public sealed record Key(string Name, int Id);
        public sealed record Other(string Name, int Id);
        public static class Program {
            public static int Main() {
                var values=new Dictionary<Key,int>(); values.Add(new Key("a",1),20); values.Add(new Key("b",2),22);
                Console.WriteLine(values.Count);
                Console.WriteLine(EqualityComparer<Key>.Default.Equals(new Key("a",1),new Key("b",2)));
                var types=new Dictionary<Type,int>(); types.Add(typeof(Key),1); types.Add(typeof(Other),2);
                Console.WriteLine(types[typeof(Key)]+types[typeof(Other)]);
                return values[new Key("a",1)]+values[new Key("b",2)];
            }
        }
        """, "2\nFalse\n3\n", 42, System.Reflection.Assembly.Load("System.Collections"));
}
