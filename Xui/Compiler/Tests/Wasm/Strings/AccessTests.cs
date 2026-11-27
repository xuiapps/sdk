using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class AccessTests
{
    [Fact]
    public Task CharacterSearchPreservesUtf16IndicesAndChecksRanges() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                string text="a🙂a\0Ж";
                Console.WriteLine(text.IndexOf('a')); Console.WriteLine(text.IndexOf('a',1));
                Console.WriteLine(text.IndexOf('\uDE42')); Console.WriteLine(text.IndexOf('\0',0,5));
                Console.WriteLine(text.IndexOf('a',1,2)); Console.WriteLine(text.IndexOf('Ж',5,1));
                Console.WriteLine(text.IndexOf('a',text.Length,0)); Console.WriteLine("".IndexOf('a'));
                Console.WriteLine(text.Contains('\uDE42')); Console.WriteLine(text.Contains('z')); Console.WriteLine("".Contains('\0'));
                try { string missing=null; missing.IndexOf('x'); } catch(NullReferenceException) { Console.WriteLine("null"); }
                try { text.IndexOf('x',-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { text.IndexOf('x',7,0); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { text.IndexOf('x',1,int.MaxValue); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { text.IndexOf('x',1,-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                return 42;
            }
        }
        """, "0\n3\n2\n4\n-1\n5\n-1\n-1\nTrue\nFalse\nFalse\nnull\nstartIndex\nstartIndex\ncount\ncount\n",42);

    [Fact]
    public Task CharacterFormattingPreservesEveryUtf16CodeUnit() => InlineArrayTests.AssertLinked("""
        public static class Program {
            public static int Main() {
                for(int i=0;i<=65535;i++) {
                    char c=(char)i;
                    string instance=c.ToString(), direct=char.ToString(c);
                    if(instance.Length!=1 || direct.Length!=1 || instance[0]!=c || direct[0]!=c) return 1;
                }
                return 42;
            }
        }
        """, "", 42);

    [Fact]
    public Task AccessUsesUtf16CodeUnits() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                string text="a😀\0Ж";
                Console.WriteLine(text.Length);
                for(int i=0;i<text.Length;i++) Console.WriteLine((int)text[i]);
                Console.WriteLine(text.Substring(1,2));
                Console.WriteLine(text.Substring(4));
                Console.WriteLine(text.Substring(text.Length)=="");
                Console.WriteLine(text.Substring(2,1)[0]==text[2]);
                return 42;
            }
        }
        """, "5\n97\n55357\n56832\n0\n1046\n😀\nЖ\nTrue\nTrue\n", 42);

    [Fact]
    public Task InvalidAccessThrowsManagedExceptions() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static int Index() => -1;
            public static int Main() {
                try { string text=null; Console.WriteLine(text.Length); } catch(NullReferenceException) { Console.WriteLine("null"); }
                try { Console.WriteLine((int)"a"[Index()]); } catch(IndexOutOfRangeException) { Console.WriteLine("index"); }
                try { Console.WriteLine((int)"a"[1]); } catch(IndexOutOfRangeException) { Console.WriteLine("end"); }
                try { Console.WriteLine("abc".Substring(-1)); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { Console.WriteLine("abc".Substring(4,0)); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { Console.WriteLine("abc".Substring(1,-1)); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { Console.WriteLine("abc".Substring(1,int.MaxValue)); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                return 42;
            }
        }
        """, "null\nindex\nend\nstartIndex\nstartIndex\nlength\nlength\n", 42);
}
