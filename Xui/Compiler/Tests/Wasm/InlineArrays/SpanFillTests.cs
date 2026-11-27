using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.InlineArrays;

[Collection(nameof(ManagedConsoleCollection))]
public class SpanFillTests
{
    [Fact]
    public Task FillAndClearRespectSlicesAndBorrowedReferences() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                Span<char> text=stackalloc char[6]; text.Fill('a'); text.Slice(1,3).Fill('Ж');
                Console.WriteLine(new string(text)); text.Slice(2,2).Clear();
                if(text[1]!='Ж' || text[2]!=0 || text[3]!=0 || text[4]!='a') return 1;
                string value="old"; var borrowed=new Span<string>(ref value); borrowed.Fill("new");
                if(value!="new") return 2; borrowed.Clear(); if(value!=null) return 3;
                var numbers=new double[4]; numbers.AsSpan().Fill(21); numbers.AsSpan(1,2).Clear();
                return (int)(numbers[0]+numbers[3]);
            }
        }
        """, "aЖЖЖaa\n",42);

    [Fact]
    public Task ClearResetsStructLeavesWithoutReplacingAddressedElements() => InlineArrayTests.AssertLinked("""
        using System;
        public struct Item { public string Name; public int Value; }
        public static class Program {
            public static int Main() {
                var items=new Item[3]; items[1].Name="held"; items[1].Value=42; items[2].Value=42;
                ref Item alias=ref items[1]; items.AsSpan(1,1).Clear();
                if(alias.Name!=null || alias.Value!=0) return -1;
                Span<int> empty=default; empty.Fill(12); empty.Clear();
                return items[2].Value;
            }
        }
        """, "",42);
}
