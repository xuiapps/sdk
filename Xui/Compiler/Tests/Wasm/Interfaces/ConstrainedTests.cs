using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Interfaces;

[Collection(nameof(ManagedConsoleCollection))]
public class ConstrainedTests
{
    [Fact]
    public Task ConstrainedCallsMutateTheOriginalStruct() => InlineArrayTests.AssertLinked("""
        using System;
        interface IChange { void Change(int value); }
        struct Counter : IChange, IDisposable {
            public int Value;
            void IChange.Change(int value) { Value += value; }
            public void Dispose() { Value += 2; }
        }
        struct Named : IChange { public string Name; public void Change(int value) { Name="changed"; } }
        class Unrelated : IDisposable { public void Dispose() { throw new NotSupportedException(); } }
        public static class Program {
            static void Change<T>(ref T value) where T:struct,IChange { value.Change(40); }
            static void Close<T>(ref T value) where T:struct,IDisposable { value.Dispose(); }
            public static int Main() {
                var unrelated = new Unrelated();
                var counter=new Counter(); Change(ref counter); Close(ref counter);
                var named=new Named { Name="original" }; Change(ref named); Console.WriteLine(named.Name);
                return counter.Value;
            }
        }
        """, "changed\n", 42);

    [Fact]
    public Task ConstrainedClassReceiversLoadTheReferenceAndDispatchVirtually() => InlineArrayTests.AssertLinked("""
        using System;
        interface IValue { int Read(int add); }
        class Base : IValue { public virtual int Read(int add) => add; }
        class Derived : Base { public override int Read(int add) => add+40; }
        class Holder { public Base Value; }
        public static class Program {
            static int Read<T>(ref T value) where T:IValue => value.Read(2);
            public static int Main() {
                Base value=new Derived(); Console.WriteLine(Read(ref value));
                var holder=new Holder { Value=value }; Console.WriteLine(Read(ref holder.Value));
                var values=new Base[1]; values[0]=value; Console.WriteLine(Read(ref values[0]));
                value=null;
                try { Read(ref value); } catch(NullReferenceException) { Console.WriteLine("null"); }
                return 42;
            }
        }
        """, "42\n42\n42\nnull\n", 42);

    [Fact]
    public Task ForeachDisposesTheSpecializedListEnumerator() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public static class Program {
            public static int Main() {
                var numbers = new List<int>(); numbers.Add(12); numbers.Add(30);
                int sum=0; foreach(var number in numbers) sum+=number;
                Console.WriteLine(sum); return sum;
            }
        }
        """, "42\n", 42, System.Reflection.Assembly.Load("System.Collections"));
}
