using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Interfaces;

[Collection(nameof(ManagedConsoleCollection))]
public class StaticInterfaceTests
{
    [Fact]
    public Task ClosedTypesSelectPublicExplicitAndInheritedStaticImplementations() => InlineArrayTests.AssertLinked("""
        using System;
        public interface IOperation<T> { static abstract T Apply(T a,T b); }
        public struct Add:IOperation<int> { public static int Apply(int a,int b)=>a+b; }
        public struct Multiply:IOperation<int> { static int IOperation<int>.Apply(int a,int b)=>a*b; }
        public class Base:IOperation<int> { public static int Apply(int a,int b)=>a-b; }
        public class Derived:Base {}
        public static class Program {
            static T Execute<T,TOperation>(T a,T b) where TOperation:IOperation<T> => TOperation.Apply(a,b);
            public static int Main() {
                Console.WriteLine(Execute<int,Add>(20,22));Console.WriteLine(Execute<int,Multiply>(6,7));
                Console.WriteLine(Execute<int,Derived>(50,8));return 42;
            }
        }
        ""","42\n42\n42\n",42);

    [Fact]
    public Task StaticDefaultsAndGenericMethodsSpecializeWithoutAnInstance() => InlineArrayTests.AssertLinked("""
        using System;
        public interface IFactory {
            static virtual int Count()=>20;
            static abstract T Echo<T>(T value);
        }
        public interface IOverride:IFactory { static int IFactory.Count()=>22; }
        public struct Default:IFactory { public static T Echo<T>(T value)=>value; }
        public struct Override:IOverride { static T IFactory.Echo<T>(T value)=>value; }
        public static class Program {
            static int Count<T>() where T:IFactory => T.Count();
            static U Echo<T,U>(U value) where T:IFactory => T.Echo<U>(value);
            public static int Main() {
                Console.WriteLine(Echo<Default,string>("default"));Console.WriteLine(Echo<Override,int>(42));
                return Count<Default>()+Count<Override>();
            }
        }
        ""","default\n42\n",42);
}
