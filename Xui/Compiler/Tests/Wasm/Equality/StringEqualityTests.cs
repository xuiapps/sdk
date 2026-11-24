using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class StringEqualityTests
{
    [Fact]
    public Task OperatorsAndTypedEqualsUseOrdinalUtf16Semantics() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Compare(string a, string b) {
                Console.WriteLine(a == b); Console.WriteLine(a != b); Console.WriteLine(string.Equals(a,b));
                try { Console.WriteLine(a.Equals(b)); } catch(NullReferenceException) { Console.WriteLine("null receiver"); }
            }
            public static int Main() {
                Compare(null,null); Compare(null,""); Compare("",null); Compare("","");
                Compare("A","a"); Compare("🙂Ж","🙂Ж"); Compare("\u00e9","e\u0301");
                Compare("\ud800", "\ud800"); Compare("a\0b", "a\0c");
                int value=42; Compare($"value:{value}","value:42");
                return 42;
            }
        }
        """, "True\nFalse\nTrue\nnull receiver\nFalse\nTrue\nFalse\nnull receiver\nFalse\nTrue\nFalse\nFalse\nTrue\nFalse\nTrue\nTrue\nFalse\nTrue\nFalse\nFalse\nTrue\nFalse\nTrue\nTrue\nFalse\nTrue\nFalse\nFalse\nTrue\nFalse\nTrue\nTrue\nFalse\nTrue\nFalse\nFalse\nTrue\nFalse\nTrue\nTrue\n", 42);
}
