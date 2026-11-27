using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class TextElementTests
{
    [Fact]
    public Task HostGraphemeBoundariesMatchManagedEditingCases() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Globalization;
        public static class Program {
            static void Scan(string text) {
                int offset=0;
                while(offset<text.Length) {
                    int length=StringInfo.GetNextTextElementLength(text.AsSpan(offset));
                    Console.WriteLine(length); offset+=length;
                }
            }
            public static int Main() {
                Scan("a\u0301🙂👩‍👩‍👧‍👦🇧🇬\r\n");
                Scan("\uD800x\uDC00");
                Span<char> buffer=stackalloc char[3]; buffer[0]='a';buffer[1]='\u0301';buffer[2]='b';
                Console.WriteLine(StringInfo.GetNextTextElementLength(buffer));
                Console.WriteLine(StringInfo.GetNextTextElementLength(ReadOnlySpan<char>.Empty));
                return 42;
            }
        }
        """, "2\n2\n11\n4\n2\n1\n1\n1\n2\n0\n",42);
}
