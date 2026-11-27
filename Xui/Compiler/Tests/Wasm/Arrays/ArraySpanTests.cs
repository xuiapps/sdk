using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class ArraySpanTests
{
    [Fact]
    public Task ArraySpansAliasTheirStorageAndSupportSlices() => InlineArrayTests.AssertLinked("""
        using System;
        class Item { public int Value; }
        public static class Program {
            public static int Main() {
                int[] values={1,2,3,4}; Span<int> span=values;
                span.Slice(0,3).CopyTo(span.Slice(1));
                if(values[1]!=1 || values[2]!=2 || values[3]!=3) return 1;
                ReadOnlySpan<int> read=new ReadOnlySpan<int>(values,1,2);
                var items=new Item[2]; Span<Item> references=new Span<Item>(items,1,1);
                references[0]=new Item {Value=42};
                string[] texts={"before"}; Span<string> strings=texts; strings[0]="after";
                return read[1]==2 && texts[0]=="after" ? items[1].Value : 2;
            }
        }
        """, "", 42);

    [Fact]
    public Task StructArraySpansProjectFieldsAndCopyValues() => InlineArrayTests.AssertLinked("""
        using System;
        struct Item { public int Value; public string Name; }
        class Holder { public int Value; }
        public static class Program {
            public static int Main() {
                Item[] values=new Item[3]; Span<Item> span=values;
                span[0].Value=42; span[0].Name="first";
                ref Item held=ref span[1];
                span.Slice(0,2).CopyTo(span.Slice(1));
                if(held.Value!=42 || held.Name!="first") return 1;
                held.Value=20;
                // Both arrays have eqref elements but different managed identities.
                Holder[] classes=new Holder[1]; Span<Holder> refs=classes; refs[0]=new Holder {Value=22};
                return span[0].Value==42 ? held.Value+classes[0].Value : 2;
            }
        }
        """, "", 42);

    [Fact]
    public Task NullArraysAndInvalidRangesFollowManagedBounds() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                int[] missing=null; Span<int> empty=missing;
                ReadOnlySpan<int> zero=new ReadOnlySpan<int>(missing,0,0);
                int caught=0;
                try { var s=new Span<int>(missing,0,1); Console.WriteLine(s.Length); } catch(ArgumentOutOfRangeException) { caught++; }
                try { var s=new Span<int>(new int[2],-1,1); Console.WriteLine(s.Length); } catch(ArgumentOutOfRangeException) { caught++; }
                try { var s=new ReadOnlySpan<int>(new int[2],1,int.MaxValue); Console.WriteLine(s.Length); } catch(ArgumentOutOfRangeException) { caught++; }
                try { Console.WriteLine(zero[0]); } catch(IndexOutOfRangeException) { caught++; }
                Console.WriteLine(empty.IsEmpty); Console.WriteLine(zero.IsEmpty); Console.WriteLine(caught);
                return empty.IsEmpty && zero.IsEmpty && caught==4 ? 42 : 1;
            }
        }
        """, "True\nTrue\n4\n", 42);
}
