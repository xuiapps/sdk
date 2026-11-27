using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Statics;

[Collection(nameof(ManagedConsoleCollection))]
public class VolatileFieldTests
{
    [Fact]
    public Task SingleAgentFieldsPreserveValuesReferencesAndCallOrdering() => InlineArrayTests.AssertLinked("""
        using System;
        public class Box<T> where T : class { public volatile T Value; public volatile bool Ready; }
        public struct State { public volatile int Count; }
        public class Item { public int Value; }
        public static class Program {
            static volatile int sequence;
            static volatile uint bits;
            static volatile Item shared;
            static void Publish(Item item) { item.Value=21; shared=item; sequence=1; }
            public static int Main() {
                var item=new Item(); var box=new Box<Item>();
                Publish(item); box.Value=shared; box.Ready=true;
                State state=default; state.Count=box.Value.Value;
                bits=uint.MaxValue;
                Console.WriteLine(sequence); Console.WriteLine(bits==uint.MaxValue); Console.WriteLine(box.Ready);
                Console.WriteLine(box.Value==item); Console.WriteLine(state.Count);
                shared=null; Console.WriteLine(shared==null); Console.WriteLine(box.Value.Value);
                return box.Value.Value + state.Count;
            }
        }
        """, "1\nTrue\nTrue\nTrue\n21\nTrue\n21\n", 42);

    [Fact]
    public Task VolatileLoadsStillThrowManagedNullReference() => InlineArrayTests.AssertLinked("""
        public class Box { public volatile int Value; }
        public static class Program {
            static int Read(Box box) => box.Value;
            static void Write(Box box) { box.Value=9; }
            public static int Main() {
                int result=0;
                try { result=Read(null); } catch(System.NullReferenceException) { result+=21; }
                try { Write(null); } catch(System.NullReferenceException) { result+=21; }
                return result;
            }
        }
        """, "", 42);
}
