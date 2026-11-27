using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class EmptyTests
{
    [Fact]
    public Task RuntimeInitializedEmptyFieldIsNeverNull() => InlineArrayTests.AssertLinked("""
        using System;
        public class Document { public string Text=string.Empty; public void Clear() { Text=string.Empty; } }
        public static class Program {
            static int Length(string value) => value.Length;
            public static int Main() {
                var document=new Document();
                if(document.Text==null || Length(string.Empty)!=0 || document.Text!="") return 1;
                document.Text="filled"; document.Clear();
                Console.WriteLine(string.Empty==null); Console.WriteLine(document.Text.Length);
                Console.WriteLine(string.Concat("a",string.Empty));
                return 42;
            }
        }
        """, "False\n0\na\n",42);
}
