using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class InvariantCasingTests
{
    [Fact]
    public Task LowercaseUsesSimpleUnicodeMappingsAndPreservesUtf16()
    {
        const string value="MARKET ЖЯİIıΟΣΣ Kẞ ᾈ𐐀🙂\ud800";
        string expected=string.Concat(value.ToLowerInvariant().Select(c=>((int)c)+"\n"));
        return InlineArrayTests.AssertLinked("""
            using System;
            public static class Program { public static int Main() {
                string value="MARKET ЖЯİIıΟΣΣ Kẞ ᾈ𐐀🙂\ud800";
                foreach(char c in value.ToLowerInvariant()) Console.WriteLine((int)c);
                return 42;
            } }
            """,expected,42);
    }

    [Fact]
    public Task NullReceiverThrowsManagedExceptionAndEmptyStringStaysEmpty() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            int result=0;string missing=null;
            try { Console.WriteLine(missing.ToLowerInvariant()); } catch(NullReferenceException) { result=42; }
            Console.WriteLine("".ToLowerInvariant().Length);return result;
        } }
        ""","0\n",42);
}
