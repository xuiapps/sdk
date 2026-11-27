using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class ObjectHashCallTests
{
    [Fact]
    public Task ConstrainedReferenceReceiversPreserveOverridesIdentityAndNullChecks() => InlineArrayTests.AssertLinked("""
        using System;
        public class Base { public override int GetHashCode()=>17; }
        public class Derived : Base { public override int GetHashCode()=>42; }
        public class Hidden : Base { public new virtual int GetHashCode()=>99; }
        public class Holder { public Base Value; }
        public static class Program {
            static int Hash<T>(ref T value)=>value.GetHashCode();
            public static int Main() {
                Base value=new Derived(); Console.WriteLine(Hash(ref value));
                var hidden=new Hidden(); Console.WriteLine(Hash(ref hidden));
                var holder=new Holder { Value=value }; Console.WriteLine(Hash(ref holder.Value));
                var plain=new object(); Console.WriteLine(Hash(ref plain)==Hash(ref plain));
                var array=new int[2]; Console.WriteLine(Hash(ref array)==Hash(ref array));
                var tuple=(value,7); Console.WriteLine(Hash(ref tuple)==Hash(ref tuple));
                value=null;
                try { Hash(ref value); } catch(NullReferenceException) { return 42; }
                return 1;
            }
        }
        """, "42\n17\n42\nTrue\nTrue\nTrue\n", 42);

    [Fact]
    public Task VirtualOverridesHiddenSlotsAndBaseIdentityStayDistinct() => InlineArrayTests.AssertLinked("""
        using System;
        public class Base { public override int GetHashCode()=>17; }
        public class Derived : Base { public override int GetHashCode()=>42; }
        public class Hidden : Base { public new virtual int GetHashCode()=>99; }
        public class Counted {
            public int Calls;
            public override int GetHashCode() { Calls++; return 30; }
            public int Identity()=>base.GetHashCode();
        }
        public static class Program {
            static int Hash(object value)=>value.GetHashCode();
            public static int Main() {
                Console.WriteLine(Hash(new Base())); Console.WriteLine(Hash(new Derived()));
                var hidden=new Hidden(); Console.WriteLine(Hash(hidden)); Console.WriteLine(hidden.GetHashCode());
                var counted=new Counted(); int a=counted.Identity(),b=counted.Identity();
                Console.WriteLine(a==b); Console.WriteLine(counted.Calls);
                Console.WriteLine(Hash(counted)); Console.WriteLine(counted.Calls);
                var plain=new object(); Console.WriteLine(Hash(plain)==Hash(plain));
                var array=new int[1]; Console.WriteLine(Hash(array)==Hash(array));
                try { Hash(null); } catch(NullReferenceException) { return 42; }
                return 1;
            }
        }
        ""","17\n42\n17\n99\nTrue\n0\n30\n1\nTrue\nTrue\n",42);

    [Fact]
    public Task BoxedPrimitiveEnumAndStringHashesAgreeWithTypedOperations() => InlineArrayTests.AssertLinked("""
        using System;
        public enum Number : long { Answer=9007199254740993L }
        public static class Program {
            static int Hash(object value)=>value.GetHashCode();
            public static int Main() {
                Console.WriteLine(Hash(42)==42.GetHashCode()); Console.WriteLine(Hash(true)==true.GetHashCode());
                Console.WriteLine(Hash('Ж')=='Ж'.GetHashCode()); Console.WriteLine(Hash(Number.Answer)==Number.Answer.GetHashCode());
                Console.WriteLine(Hash(1.5f)==1.5f.GetHashCode()); Console.WriteLine(Hash(1.5)==1.5.GetHashCode());
                Console.WriteLine(Hash(double.NaN)==double.NaN.GetHashCode()); Console.WriteLine(Hash(-0.0)==0.0.GetHashCode());
                Console.WriteLine(Hash("hello")=="hello".GetHashCode());
                return 42;
            }
        }
        ""","True\nTrue\nTrue\nTrue\nTrue\nTrue\nTrue\nTrue\nTrue\n",42);
}
