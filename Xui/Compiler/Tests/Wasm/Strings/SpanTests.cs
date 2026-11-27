using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class SpanTests
{
    [Fact]
    public Task StringSpansBorrowUtf16AndSupportSlicingCopyAndReturn() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static ReadOnlySpan<char> Make(string text) => text.AsSpan(1,4);
            public static int Main() {
                string text="A🙂ЖZ!"; ReadOnlySpan<char> span=Make(text);
                Console.WriteLine(span.Length); Console.WriteLine((int)span[0]); Console.WriteLine((int)span[1]);
                Console.WriteLine(span.ToString()); Console.WriteLine(span.Slice(2).ToString());
                char[] chars=new char[4]; span.CopyTo(chars); chars[3]='?';
                Console.WriteLine(((Span<char>)chars).ToString()); Console.WriteLine(new string(span));
                ReadOnlySpan<char> implicitSpan=text; Console.WriteLine(implicitSpan.Slice(5).ToString());
                string missing=null; Console.WriteLine(missing.AsSpan().IsEmpty); Console.WriteLine(missing.AsSpan(0,0).ToString());
                Console.WriteLine(text.AsSpan(text.Length).IsEmpty);
                try { _=missing.AsSpan(1); } catch(ArgumentOutOfRangeException) { Console.WriteLine("null range"); }
                try { _=text.AsSpan(-1); } catch(ArgumentOutOfRangeException) { Console.WriteLine("start"); }
                try { _=text.AsSpan(1,int.MaxValue); } catch(ArgumentOutOfRangeException) { Console.WriteLine("length"); }
                try { _=span[4]; } catch(IndexOutOfRangeException) { Console.WriteLine("index"); }
                return 42;
            }
        }
        """, "4\n55357\n56898\n🙂ЖZ\nЖZ\n🙂Ж?\n🙂ЖZ\n!\nTrue\n\nTrue\nnull range\nstart\nlength\nindex\n",42);
}
