using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Collections;

[Collection(nameof(ManagedConsoleCollection))]
public class ArrayPoolTests
{
    [Fact]
    public Task CharacterLeasesRemainIndependentAndSupportSpans() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Buffers;
        public static class Program {
            public static int Main() {
                var pool=ArrayPool<char>.Shared;
                char[] first=pool.Rent(8); first[0]='Ж';
                char[] second=pool.Rent(8); second[0]='A';
                if(first[0]!='Ж') return 1;
                "hello".AsSpan().CopyTo(second.AsSpan());
                Console.WriteLine(new string(second.AsSpan(0,5)));
                pool.Return(first,true); pool.Return(second,true);
                char[] third=pool.Rent(8);
                for(int i=0;i<third.Length;i++) if(third[i]!=0) return 2;
                pool.Return(third);
                if(pool.Rent(100).Length<100 || pool.Rent(0).Length<0) return 3;
                return 42;
            }
        }
        """, "hello\n",42);

    [Fact]
    public Task ReferenceAndStructArraysClearThroughExistingAliases() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Buffers;
        public struct Item { public string Name; public int Count; }
        public static class Program {
            public static int Main() {
                var strings=ArrayPool<string>.Shared; var a=strings.Rent(4); a[0]="held";
                strings.Return(a,true); if(a[0]!=null) return 1;
                var items=ArrayPool<Item>.Shared; var b=items.Rent(4);
                b[0].Name="held"; b[0].Count=42; ref Item alias=ref b[0];
                items.Return(b,true); if(alias.Name!=null || alias.Count!=0) return 2;
                var c=items.Rent(4); c[0].Count=42; items.Return(c);
                return 42;
            }
        }
        """, "",42);

    [Fact]
    public Task InvalidArgumentsAndNullReceiversThrowManagedExceptions() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Buffers;
        public static class Program {
            public static int Main() {
                int caught=0; var pool=ArrayPool<int>.Shared;
                try { pool.Rent(-1); } catch(ArgumentOutOfRangeException) { caught++; }
                try { pool.Return(null); } catch(ArgumentNullException) { caught++; }
                ArrayPool<int> missing=null;
                try { missing.Rent(1); } catch(NullReferenceException) { caught++; }
                try { missing.Return(new int[1]); } catch(NullReferenceException) { caught++; }
                return caught==4 ? 42 : -1;
            }
        }
        """, "",42);
}
