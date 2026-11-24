using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class ConcatenationTests
{
    [Fact]
    public Task FixedArityConcatenationKeepsOrderAndTreatsNullAsEmpty() => InlineArrayTests.AssertLinked("""
        using System;
        public class Example { public string Id; public Example() { Id=$"Example.{GetType().Name}"; } }
        public class CanvasExample : Example { }
        public static class Program {
            static string Part(string value) { Console.WriteLine(value); return value; }
            static string Join(string a, string b) => string.Concat(a,b);
            public static int Main() {
                Console.WriteLine(new CanvasExample().Id);
                Console.WriteLine(string.Concat(Part("left"),Part("right")));
                Console.WriteLine(string.Concat(null,"middle",null));
                Console.WriteLine(string.Concat("one",null,"three",null));
                Console.WriteLine(Join(null,null)==""); Console.WriteLine(Join("",null)=="");
                Console.WriteLine(Join("a\0","Ж😀"));
                Console.WriteLine(Join("\ud800", "\udfff")=="\ud800\udfff");
                return 42;
            }
        }
        """, "Example.CanvasExample\nleft\nright\nleftright\nmiddle\nonethree\nTrue\nTrue\na\0Ж😀\nTrue\n", 42);
}
