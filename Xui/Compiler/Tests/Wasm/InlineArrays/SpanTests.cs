using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.InlineArrays;

[Collection(nameof(ManagedConsoleCollection))]
public class SpanTests
{
    [Fact]
    public Task SingleElementSpansAliasReferencesAndSurviveHelperReturns() => InlineArrayTests.AssertLinked("""
        using System;
        class Item { public string Name; public int Value; }
        struct Entry { public string Name; public int Value; }
        public static class Program {
            static Span<string> Borrow(ref string text) => new Span<string>(ref text);
            static Span<Entry> BorrowEntry(ref Entry value) => new Span<Entry>(ref value);
            static ReadOnlySpan<Item> One(Item value) => new Item[] { value };
            public static int Main() {
                string text="before"; Span<string> names=Borrow(ref text);
                names[0]="after"; Console.WriteLine(text);
                text="alias"; Console.WriteLine(names.Slice(0)[0]);
                Span<string> copy=new string[1]; names.CopyTo(copy); copy[0]="copy"; copy.CopyTo(names);
                Console.WriteLine(text); Console.WriteLine(names.Slice(1).IsEmpty);
                var item=new Item { Name="field", Value=10 };
                var field=Borrow(ref item.Name); field[0]="changed"; Console.WriteLine(item.Name);
                Item held=item; Span<Item> refs=new Span<Item>(ref held);
                refs[0]=new Item { Value=20 }; Console.WriteLine(held.Value);
                Entry entry=new Entry {Name="struct",Value=1};
                var entries=BorrowEntry(ref entry); entries[0].Name="updated"; entries[0].Value=42;
                Console.WriteLine(entry.Name);
                int number=5; Span<int> numbers=new Span<int>(ref number); numbers[0]=6;
                ReadOnlySpan<int> read=new ReadOnlySpan<int>(in number); Console.WriteLine(read[0]);
                ReadOnlySpan<Item> single=[item]; Console.WriteLine(single[0].Name);
                try { _=names[1]; } catch(IndexOutOfRangeException) { Console.WriteLine("bounds"); }
                return entry.Value;
            }
        }
        """, "after\nalias\ncopy\nTrue\nchanged\n20\nupdated\n6\nchanged\nbounds\n",42);

    [Fact]
    public Task ManagedSpansPreserveOwnersAcrossReturnsSlicesAndOverlappingCopies() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        public struct Entry { public string Name; public int Value; }
        [InlineArray(4)] public struct Buffer { private Entry first; }
        public class Owner { public Buffer Items; }
        public static class Program {
            static Span<Entry> Create() {
                var owner=new Owner();
                for(int i=0;i<4;i++) owner.Items[i]=new Entry { Name="item", Value=i+1 };
                return owner.Items;
            }
            static void Copy(ReadOnlySpan<Entry> a, Span<Entry> b) => a.CopyTo(b);
            public static int Main() {
                Span<Entry> span=Create(); ref Entry alias=ref span[1];
                Copy(span[..3],span[1..]); Console.WriteLine(alias.Value);
                Copy(span[1..],span[..3]); Console.WriteLine(alias.Value);
                Buffer local=default; Copy(span,local);
                local[0].Name="local"; local[0].Value=42;
                Console.WriteLine(span[0].Name);
                var second=new Owner(); Copy(local,second.Items);
                for(int i=0;i<70000;i++) Copy(second.Items,span);
                Console.WriteLine(span[0].Name); Console.WriteLine(span.TryCopyTo(span[..1]));
                Console.WriteLine(span.Slice(4).IsEmpty);
                try { _=span[4]; } catch(IndexOutOfRangeException) { Console.WriteLine("bounds"); }
                return span[0].Value;
            }
        }
        """, "1\n2\nitem\nlocal\nFalse\nTrue\nbounds\n", 42);

    [Fact]
    public Task NumericSpanCopiesCrossManagedAndLinearBacking() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        [InlineArray(4)] public struct Buffer { private int first; }
        public class Owner { public Buffer Items; }
        public static class Program { public static int Main() {
            var owner=new Owner(); Span<int> span=owner.Items;
            for(int i=0;i<4;i++) span[i]=i+10;
            span[..3].CopyTo(span[1..]); Console.WriteLine(span[3]);
            span[1..].CopyTo(span[..3]); Console.WriteLine(span[0]);
            Buffer local=default; span.CopyTo(local); local[0]=42;
            ((ReadOnlySpan<int>)local).CopyTo(span);
            var other=new Owner(); span.CopyTo(other.Items);
            return other.Items[0];
        } }
        """, "12\n10\n", 42);

    [Fact]
    public Task SlicesBorrowNumericStorageAndCopiesHandleOverlap() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        [InlineArray(4)] public struct Numbers { private int first; }
        public static class Program {
            static void Copy(ReadOnlySpan<int> source, Span<int> destination) => source.CopyTo(destination);
            public static int Main() {
                Numbers buffer = default; for (int i=0; i<4; i++) buffer[i]=i+1;
                Span<int> span=buffer; var middle=span.Slice(1,2); middle[0]=20;
                Copy(span[..3], span[1..]);
                for (int i=0; i<4; i++) Console.WriteLine(span[i]);
                Copy(span[1..], span[..3]);
                for (int i=0; i<4; i++) Console.WriteLine(span[i]);
                ReadOnlySpan<int> read=span;
                Console.WriteLine(read.Slice(4).IsEmpty); Console.WriteLine(span.Slice(4,0).Length);
                Span<int> empty=default; Console.WriteLine(empty.IsEmpty); empty.CopyTo(span);
                Console.WriteLine(read.TryCopyTo(span[..1]));
                Console.WriteLine(read[..1].TryCopyTo(span[1..]));
                try { _=span.Slice(-1); } catch(ArgumentOutOfRangeException) { Console.WriteLine("start"); }
                try { _=span.Slice(4,1); } catch(ArgumentOutOfRangeException) { Console.WriteLine("end"); }
                try { _=read.Slice(1,int.MaxValue); } catch(ArgumentOutOfRangeException) { Console.WriteLine("overflow"); }
                try { _=read.Slice(1,-1); } catch(ArgumentOutOfRangeException) { Console.WriteLine("count"); }
                try { read.CopyTo(span[..1]); } catch(ArgumentException) { Console.WriteLine("short"); }
                return 42;
            }
        }
        """, "1\n1\n20\n3\n1\n20\n3\n3\nTrue\n0\nTrue\nFalse\nTrue\nstart\nend\noverflow\ncount\nshort\n", 42);

    [Fact]
    public Task ReferenceCopiesPreserveDestinationSlotsAndLiveAliases() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        public class Box { public int Value; }
        public struct Entry { public string Name; public Box Target; public int Number; }
        [InlineArray(4)] public struct Entries { private Entry first; }
        [InlineArray(4)] public struct Names { private string first; }
        public static class Program {
            // Each span roots its borrowed owner independently of the element reference slots.
            static void Copy(ReadOnlySpan<Entry> source, Span<Entry> destination) => source.CopyTo(destination);
            static void NamesCopy(ReadOnlySpan<string> source, Span<string> destination) => source.CopyTo(destination);
            public static int Main() {
                Entries buffer=default; var shared=new Box { Value=7 };
                buffer[0]=new Entry { Name="A", Target=shared, Number=1 };
                buffer[1]=new Entry { Name="B", Target=shared, Number=2 };
                buffer[2]=new Entry { Name="C", Target=shared, Number=3 };
                Span<Entry> span=buffer; ref Entry live=ref span[1];
                Copy(span[..3],span[1..]);
                Console.WriteLine(live.Name); Console.WriteLine(live.Number);
                span[0].Name="changed"; Console.WriteLine(live.Name);
                live.Target.Value=9; Console.WriteLine(span[0].Target.Value);
                Copy(span[1..],span[..3]);
                Console.WriteLine(live.Name); Console.WriteLine(span[0].Name); Console.WriteLine(span[2].Name);
                span[3].Name="tail"; Console.WriteLine(span[2].Name);
                Entries destination=default; Copy(span,destination);
                destination[0].Name="separate"; Console.WriteLine(span[0].Name);
                Names names=default; names[0]="one"; names[1]="two"; names[2]="three";
                Span<string> text=names; ref string slot=ref text[1];
                NamesCopy(text[..3],text[1..]); Console.WriteLine(slot);
                text[0]="new"; Console.WriteLine(slot);
                NamesCopy(text[1..],text[..3]); Console.WriteLine(slot);
                return 42;
            }
        }
        """, "A\n1\nA\n9\nB\nA\nC\nC\nA\none\none\ntwo\n", 42);
}
