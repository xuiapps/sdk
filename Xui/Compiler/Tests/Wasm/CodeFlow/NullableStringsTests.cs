using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

[Collection(nameof(ManagedConsoleCollection))]
public class NullableStringsTests
{
    [Fact]
    public Task NullableStringsMergeAcrossConditionalsAndSharedTails() => InlineArrayTests.AssertLinked("""
        using System;
        class Item { public string Name; public int Value; }
        public static class Program {
            static string Select(bool b) => b ? null : "value";
            static string Name(Item item) => item?.Name ?? "missing";
            static int Use(ref int destination, string text, double number) { destination++; return text==null ? 1 : 2; }
            static string Choice(int i) => i switch { 0=>null, 1=>"one", 2=>null, _=>"other" };
            public static int Main() {
                var item=new Item {Name="named"};
                Console.WriteLine(Select(true)??"null"); Console.WriteLine(Select(false));
                Console.WriteLine(Name(null)); Console.WriteLine(Name(item));
                for(int i=0;i<4;i++) {
                    Console.WriteLine(Choice(i)??"empty");
                    Console.WriteLine(Use(ref item.Value,i%2==0?null:"yes",3.5));
                }
                return item.Value+38;
            }
        }
        """, "null\nvalue\nmissing\nnamed\nempty\n1\none\n2\nempty\n1\nother\n2\n",42);
}
