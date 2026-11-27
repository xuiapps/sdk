using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Classes;

[Collection(nameof(ManagedConsoleCollection))]
public class ManagedTypeTests
{
    [Fact]
    public Task DynamicValueTypeChecksUseClosedDescriptors() => InlineArrayTests.AssertLinked("""
        using System;
        public class Item { }
        public enum Kind : byte { One }
        public static class Program {
            static bool Value(Type type) => type.IsValueType;
            static bool Key(object value) => value.GetType().IsValueType;
            public static int Main() {
                Console.WriteLine(Key(new Item())); Console.WriteLine(Key(42));
                Console.WriteLine(Key(Kind.One)); Console.WriteLine(Key("text"));
                Console.WriteLine(Value(typeof(Item))); Console.WriteLine(Value(typeof(double)));
                try { Value(null); } catch(NullReferenceException) { Console.WriteLine("null"); }
                return 42;
            }
        }
        """, "False\nTrue\nTrue\nFalse\nFalse\nTrue\nnull\n", 42);

    [Fact]
    public Task TypeOfSharesDescriptorsWithInstancesAndGenericSpecializations() => InlineArrayTests.AssertLinked("""
        using System;
        public interface IItem { }
        public class Item : IItem { }
        public class Box<T> { }
        public class MetadataOnly { public MetadataOnly() { throw new Exception("must not run"); } }
        namespace One { public class Item { } }
        namespace Two { public class Item { } }
        public static class Program {
            static Type Kind<T>() where T : class => typeof(T);
            public static int Main() {
                Console.WriteLine(typeof(Item)==new Item().GetType());
                Console.WriteLine(Kind<Box<int>>()==typeof(Box<int>));
                Console.WriteLine(Kind<Box<int>>()!=typeof(Box<string>));
                Console.WriteLine(typeof(IItem).Name); Console.WriteLine(typeof(MetadataOnly).Name);
                Console.WriteLine(typeof(One.Item).Name==typeof(Two.Item).Name);
                Console.WriteLine(typeof(One.Item)!=typeof(Two.Item));
                return 42;
            }
        }
        """, "True\nTrue\nTrue\nIItem\nMetadataOnly\nTrue\nTrue\n", 42);

    [Fact]
    public Task TypeOfDrivesInterfaceServiceLookup() => InlineArrayTests.AssertLinked("""
        using System;
        public interface ITheme { string Name { get; } }
        public interface IMissing { }
        public class Theme : ITheme { public string Name => "Canvas"; }
        public class Services : IServiceProvider {
            private ITheme theme=new Theme();
            public object GetService(Type serviceType) => serviceType==typeof(ITheme) ? theme : null;
        }
        public static class Program {
            public static int Main() {
                IServiceProvider services=new Services();
                Console.WriteLine(((ITheme)services.GetService(typeof(ITheme))).Name);
                Console.WriteLine(services.GetService(typeof(IMissing))==null);
                Console.WriteLine(services.GetService(null)==null);
                return 42;
            }
        }
        """, "Canvas\nTrue\nTrue\n", 42, typeof(IServiceProvider).Assembly);

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
