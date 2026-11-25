using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Collections;

[Collection(nameof(ManagedConsoleCollection))]
public class ListTests
{
    [Fact]
    public Task ActualListRemovalInitializesGenericReferenceTemporaries() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public class Item { public string Name; public Item(string name) { Name=name; } }
        public static class Program {
            public static int Main() {
                var first=new Item("first"); var second=new Item("second"); var third=new Item("third");
                var list=new List<Item>(); list.Add(first); list.Add(second); list.Add(third);
                list.RemoveAt(0);
                Console.WriteLine(list[0].Name); Console.WriteLine(list[1].Name);
                list.RemoveAt(1); list.RemoveAt(0);
                Console.WriteLine(list.Count);
                list.Add(third); Console.WriteLine(list[0].Name);
                return 42;
            }
        }
        """, "second\nthird\n0\nthird\n", 42);
}
