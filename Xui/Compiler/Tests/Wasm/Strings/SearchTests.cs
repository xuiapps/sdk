using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class SearchTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task UnsupportedComparisonsDoNotSilentlyUseOrdinalSemantics(int comparison)
    {
        using var app=Canvas.CanvasTests.Read(Frontend.Roslyn.Compile($$"""
            using System;
            public static class Program {
                public static int Main() {
                    try { "a".StartsWith("A",(StringComparison){{comparison}}); return 1; }
                    catch(NotSupportedException) { return 42; }
                }
            }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Equal(42,(await Repl.Node.ExecuteAsync(new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app))).ExitCode);
    }

    [Fact]
    public Task OrdinalSearchPreservesUtf16AndEmptyPatterns() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                string text="a\0🙂a";
                Console.WriteLine(text.StartsWith("a\0",StringComparison.Ordinal));
                Console.WriteLine(text.EndsWith("\uDE42a",StringComparison.Ordinal));
                Console.WriteLine(text.IndexOf("\uDE42",StringComparison.Ordinal));
                Console.WriteLine(text.Contains("\0🙂"));
                Console.WriteLine(text.Contains("A",StringComparison.Ordinal));
                Console.WriteLine(text.StartsWith('a')); Console.WriteLine(text.EndsWith('a'));
                Console.WriteLine("".StartsWith('a')); Console.WriteLine("".EndsWith('a'));
                Console.WriteLine("".StartsWith("",StringComparison.Ordinal));
                Console.WriteLine("".EndsWith("",StringComparison.Ordinal));
                Console.WriteLine("".Contains(""));
                Console.WriteLine("".IndexOf("",StringComparison.Ordinal));
                return 42;
            }
        }
        """, "True\nTrue\n3\nTrue\nFalse\nTrue\nTrue\nFalse\nFalse\nTrue\nTrue\nTrue\n0\n",42);

    [Fact]
    public Task NullAndInvalidComparisonFailuresAreManaged() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                string missing=null;
                try { missing.StartsWith("a",StringComparison.Ordinal); } catch(NullReferenceException) { Console.WriteLine("receiver"); }
                try { "a".StartsWith(missing,StringComparison.Ordinal); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                try { "a".EndsWith(missing,StringComparison.Ordinal); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                try { "a".Contains(missing); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                try { "a".IndexOf(missing,StringComparison.Ordinal); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                try { "a".StartsWith("b",(StringComparison)(-1)); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                try { "a".IndexOf("b",(StringComparison)6); } catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                return 42;
            }
        }
        """, "receiver\nvalue\nvalue\nvalue\nvalue\ncomparisonType\ncomparisonType\n",42);
}
