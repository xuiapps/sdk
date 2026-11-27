using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class ReplaceTests
{
    [Fact]
    public Task OrdinalReplacementPreservesLiteralTextUtf16AndManagedFailures() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                Console.WriteLine("12px".Replace("px", ""));
                Console.WriteLine("ababa".Replace("aba", "x"));
                Console.WriteLine("a.a".Replace("a", "$&$1$$"));
                Console.WriteLine("aabb".Replace("a", (string)null));
                Console.WriteLine("ab".Replace("a", "aa"));
                Console.WriteLine("".Replace("x", "y"));
                Console.WriteLine("ABC".Replace("a", "z"));
                Console.WriteLine("🙂🙂".Replace("🙂", "Ж"));
                Console.WriteLine("a\0b\0".Replace('\0','x'));
                string partial="🙂".Replace('\ud83d','x');
                Console.WriteLine(partial.Length); Console.WriteLine((int)partial[1]);
                try { string missing=null; missing.Replace((string)null, "x"); } catch(NullReferenceException) { Console.WriteLine("receiver"); }
                try { "".Replace((string)null, "x"); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                try { "".Replace("", "x"); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                return 42;
            }
        }
        """, "12\nxba\n$&$1$$.$&$1$$\nbb\naab\n\nABC\nЖЖ\naxbx\n2\n56898\nreceiver\noldValue\noldValue\n",42);
}
