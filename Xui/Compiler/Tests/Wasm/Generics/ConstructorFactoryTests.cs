using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

[Collection(nameof(ManagedConsoleCollection))]
public class ConstructorFactoryTests
{
    [Fact]
    public Task GenericFactoriesSelectClosedConstructorsAndVirtualReceivers() => InlineArrayTests.AssertLinked("""
        using System;
        public class Base { public virtual int Number()=>1; }
        public class Page : Base { public int Value; public Page(){Value=42;} public override int Number()=>Value; }
        public class Box<T> { public int Value; public Box(){Value=7;} }
        public static class Program {
            static T Create<T>() where T : new() => new T();
            static int Invoke<T>() where T : Base,new() => Create<T>().Number();
            public static int Main() {
                Console.WriteLine(Invoke<Page>()); Console.WriteLine(Create<Box<int>>().Value);
                Console.WriteLine(Create<Box<string>>().Value); Console.WriteLine(Activator.CreateInstance<Page>().Value);
                Console.WriteLine(Create<Page>()==Create<Page>()); return 42;
            }
        }
        """, "42\n7\n7\n42\nFalse\n", 42);

    [Fact]
    public Task GenericConstructorFailuresRetainTheirInnerExceptionAndHierarchy() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Reflection;
        public class Broken { public Broken() { throw new ArgumentNullException("page"); } }
        public static class Program {
            static T Create<T>() where T:new() => new T();
            public static int Main() {
                try { Create<Broken>(); } catch(SystemException) { Console.WriteLine("wrong hierarchy"); }
                catch(TargetInvocationException e) {
                    Console.WriteLine(e.Message); Console.WriteLine(e.InnerException.Message);
                    Console.WriteLine(((ArgumentNullException)e.InnerException).ParamName);
                    Console.WriteLine(e.InnerException.InnerException==null);
                }
                try { Activator.CreateInstance<Broken>(); } catch(ApplicationException) { Console.WriteLine("application exception"); }
                try { new Broken(); } catch(ArgumentNullException) { Console.WriteLine("direct constructor"); }
                return 42;
            }
        }
        """, "Exception has been thrown by the target of an invocation.\nValue cannot be null. (Parameter 'page')\npage\nTrue\napplication exception\ndirect constructor\n", 42);
}
