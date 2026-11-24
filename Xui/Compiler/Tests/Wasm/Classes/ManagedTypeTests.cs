using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Classes;

[Collection(nameof(ManagedConsoleCollection))]
public class ManagedTypeTests
{
    [Fact]
    public Task DescriptorsKeepDynamicIdentityAndMetadataNames() => InlineArrayTests.AssertLinked("""
        using System;
        namespace Demo {
            public interface IItem { }
            public class Base : IItem { public string Name; public Base() { Name=GetType().Name; } }
            public class Derived : Base { }
            public class Box<T> : Base { }
            public class Outer { public class Nested : Base { } }
            public class Holder { public Type Type; }
            public static class Program {
                static Type Read(Base value) => value.GetType();
                static string Name(Type value) => value.Name;
                public static int Main() {
                    Base a=new Derived(); Base b=new Derived(); IItem c=a;
                    Console.WriteLine(a.Name); Console.WriteLine(c.GetType().Name);
                    var holder=new Holder { Type=Read(a) };
                    Console.WriteLine(Name(holder.Type));
                    Console.WriteLine(holder.Type == b.GetType()); Console.WriteLine(holder.Type != new Base().GetType());
                    Console.WriteLine(new Box<int>().GetType().Name);
                    Console.WriteLine(new Box<int>().GetType() != new Box<string>().GetType());
                    Console.WriteLine(new Outer.Nested().GetType().Name);
                    Type empty=null; Console.WriteLine(empty==null); Console.WriteLine(holder.Type==empty);
                    object opaque=holder.Type; Console.WriteLine(opaque is Type); Console.WriteLine(((Type)opaque).Name);
                    Console.WriteLine(a is Type);
                    return 42;
                }
            }
        }
        """, "Derived\nDerived\nDerived\nTrue\nTrue\nBox`1\nTrue\nNested\nTrue\nFalse\nTrue\nDerived\nFalse\n", 42);

    [Fact]
    public Task NullReceiversThrowManagedExceptions() => InlineArrayTests.AssertLinked("""
        using System;
        public class Item { }
        public static class Program {
            static string Name(Type value) => value.Name;
            static Type Read(Item value) => value.GetType();
            public static int Main() {
                try { Name(null); } catch (NullReferenceException) { Console.WriteLine("name"); }
                try { Read(null); } catch (NullReferenceException) { Console.WriteLine("type"); }
                return 42;
            }
        }
        """, "name\ntype\n", 42);
}
