using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class RepeatTests
{
    [Fact]
    public Task RepeatedCharacterConstructionPreservesUtf16AndRejectsNegativeCounts() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static string Repeat(char value,int count)=>new string(value,count);
            public static int Main() {
                Console.WriteLine(Repeat('a',4)); Console.WriteLine(Repeat('x',0)=="");
                foreach(char unit in new char[] {'\0','\ud800','\udfff','\uffff'}) {
                    string value=Repeat(unit,3); Console.WriteLine(value.Length);
                    Console.WriteLine(value[0]==unit && value[1]==unit && value[2]==unit);
                }
                try { Repeat('x',-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { Repeat('x',int.MinValue); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                return 42;
            }
        }
        ""","aaaa\nTrue\n3\nTrue\n3\nTrue\n3\nTrue\n3\nTrue\ncount\ncount\n",42);
}
