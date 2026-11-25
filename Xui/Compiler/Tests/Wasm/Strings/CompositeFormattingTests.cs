using System.Globalization;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class CompositeFormattingTests(Xunit.Abstractions.ITestOutputHelper log)
{
    [Fact]
    public Task FixedArgumentsAlignmentEscapesAndNativeStrings() => AssertInvariant("""
        using System;
        public static class Program { public static int Main() {
            Console.WriteLine(string.Format("{{{1}}} |{0,5}|{2,-5}|", -42, true, 'λ'));
            Console.WriteLine(string.Format("{0}/{0}/{1}", "Hello 🌍", (object)null));
            Console.WriteLine(string.Format("{0}", 42));
            Console.WriteLine(string.Format(null, "{0}", 7));
            return 42;
        } }
        """, "{True} |  -42|λ    |\nHello 🌍/Hello 🌍/\n42\n7\n", 42);

    [Theory]
    [InlineData("sbyte", "-128", "-128", "80")]
    [InlineData("byte", "255", "255", "FF")]
    [InlineData("short", "-32768", "-32768", "8000")]
    [InlineData("ushort", "65535", "65535", "FFFF")]
    [InlineData("int", "int.MinValue", "-2147483648", "80000000")]
    [InlineData("uint", "uint.MaxValue", "4294967295", "FFFFFFFF")]
    [InlineData("long", "long.MinValue", "-9223372036854775808", "8000000000000000")]
    [InlineData("ulong", "ulong.MaxValue", "18446744073709551615", "FFFFFFFFFFFFFFFF")]
    public Task IntegerFormattingPreservesWidthAndPrecision(string type, string value, string decimalText, string hex) => AssertInvariant($$"""
        public static class Program { public static int Main() {
            {{type}} value={{value}};
            System.Console.WriteLine(string.Format("{0} {0:X}", value));
            return 42;
        } }
        """, decimalText + " " + hex + "\n", 42);

    [Fact]
    public Task ArraysRepeatedArgumentsAndPadding() => AssertInvariant("""
        using System;
        public static class Program { public static int Main() {
            object[] values=new object[] { -42, 18446744073709551615UL, true, null };
            Console.WriteLine(string.Format("{0:D5}/{1}/{2}/{3}{{}}", values));
            Console.WriteLine(string.Format("{1:x4}/{0}", 1, (byte)15));
            Console.WriteLine(string.Format("empty", new object[0]));
            return 42;
        } }
        """, "-00042/18446744073709551615/True/{}\n000f/1\nempty\n", 42);

    [Theory]
    [InlineData("1.2f", "1.2")]
    [InlineData("float.Epsilon", "1E-45")]
    [InlineData("float.MaxValue", "3.4028235E+38")]
    [InlineData("double.Epsilon", "5E-324")]
    [InlineData("double.MaxValue", "1.7976931348623157E+308")]
    [InlineData("1e-5", "1E-05")]
    [InlineData("1e16", "10000000000000000")]
    [InlineData("1e17", "1E+17")]
    [InlineData("9.999999999999998e16", "99999999999999980")]
    [InlineData("double.NaN", "NaN")]
    [InlineData("double.NegativeInfinity", "-Infinity")]
    [InlineData("-0.0", "-0")]
    public Task FloatingDefaultsMatchInvariantFormatting(string value, string output) => AssertInvariant($$"""
        public static class Program { public static int Main() {
            System.Console.WriteLine(string.Format("{0}", {{value}})); return 42;
        } }
        """, output + "\n", 42);

    [Theory]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("{1}")]
    [InlineData("{0,+3}")]
    [InlineData("{0:{{}")]
    [InlineData("{0:Q}")]
    [InlineData("{ 0}")]
    public Task InvalidFormatsThrowCatchableManagedExceptions(string format) => AssertInvariant($$"""
        public static class Program { public static int Main() {
            try { string.Format("{{format}}", 42); return 1; }
            catch(System.FormatException) { return 42; }
        } }
        """, "", 42);

    [Fact]
    public Task NullChecksKeepParameterNames() => AssertInvariant("""
        using System;
        public static class Program { public static int Main() {
            try { string.Format(null, 1); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            try { string.Format("{0}", (object[])null); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            try { string.Format(null, (object[])null); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
            return 42;
        } }
        """, "format\nargs\nformat\n", 42);

    [Fact]
    public Task UnusedUnsupportedObjectsAreNotFormatted() => AssertInvariant("""
        public class Value { }
        public static class Program { public static int Main() {
            System.Console.WriteLine(string.Format("{1}", new Value(), 42)); return 42;
        } }
        """, "42\n", 42);

    [Theory]
    [InlineData("string.Format(\"{0:N2}\", 42)")]
    [InlineData("string.Format(\"{0}\", new Value())")]
    [InlineData("string.Format(new Provider(), \"{0}\", 42)")]
    public async Task UnsupportedFormattingThrowsManagedNotSupported(string expression)
    {
        byte[] image = Frontend.Roslyn.Compile($$"""
            public class Value { }
            public class Provider : System.IFormatProvider { public object GetFormat(System.Type type) => null; }
            public static class Program { public static int Main() {
                try { {{expression}}; return 1; }
                catch(System.NotSupportedException) { return 42; }
            } }
            """);
        using var app = Canvas.CanvasTests.Read(image);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        byte[] wasm = new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app);
        Assert.Equal(42, (await Repl.Node.ExecuteAsync(wasm)).ExitCode);
    }

    [Fact]
    public Task RealDictionaryInsertReplaceLookupAndDuplicateMessage() => AssertInvariant("""
        using System;
        using System.Collections.Generic;
        public static class Program { public static int Main() {
            var values=new Dictionary<int,int>();
            values.Add(7, 21); values[7]=42; values.Add(9, 99);
            Console.WriteLine(values[7]); Console.WriteLine(values.TryGetValue(9, out int found)); Console.WriteLine(found);
            try { values.Add(7, 100); } catch(ArgumentException e) { Console.WriteLine(e.Message); }
            return values[7];
        } }
        """, "42\nTrue\n99\nAn item with the same key has already been added. Key: 7\n", 42);

    [Fact]
    public Task RealDictionaryResizesAndCopiesStructValuesWithReferences() => AssertInvariant("""
        using System;
        using System.Collections.Generic;
        public struct Value { public int Number; public string Name; }
        public static class Program { public static int Main() {
            var values=new Dictionary<int,Value>();
            for(int i=0;i<100;i++) values.Add(i*7, new Value { Number=i, Name="original" });
            var copy=values[49]; copy.Number=1000; copy.Name="changed";
            if(values[49].Number!=7 || values[49].Name!="original") return 1;
            values[49]=copy;
            for(int i=0;i<100;i++) if(values[i*7].Number!=(i==7 ? 1000 : i)) return 2;
            Console.WriteLine(values.TryGetValue(1, out Value absent));
            Console.WriteLine(absent.Number); Console.WriteLine(absent.Name==null);
            try { var missing=values[12345]; } catch(KeyNotFoundException e) { Console.WriteLine(e.Message); }
            return values[49].Name=="changed" ? 42 : 3;
        } }
        """, "False\n0\nTrue\nThe given key '12345' was not present in the dictionary.\n", 42);

    [Fact]
    public async Task CompositeFormattingSnapshot()
    {
        const string source = """
            public static class Program {
                public static int Main() {
                    System.Console.WriteLine(string.Format("{{key {0:D4}}}: {1,-5} 0x{2:X}", 42, true, 18446744073709551615UL));
                    return 42;
                }
            }
            """;
        using var app = Canvas.CanvasTests.Read(Frontend.Roslyn.Compile(source));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var emitter = new Xui.Compiler.WASM.Emitter.WasmEmitter();
        byte[] wasm = emitter.Emit(app, new Xui.Compiler.Playground.ConsoleWriteLineRewriter());
        var result = await Repl.Node.ExecuteAsync(wasm);
        Assert.Equal("{key 0042}: True  0xFFFFFFFFFFFFFFFF\n", result.Output);
        Assert.Equal(42, result.ExitCode);
        string wat = await WasmTools.ToWatAsync(wasm);
        Snapshot.AssertMatches("CompositeFormatting", $"# Composite formatting\n\n## Source\n\n```csharp\n{source}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
    }

    private async Task AssertInvariant(string source, string output, int exitCode)
    {
        var culture=CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture=CultureInfo.InvariantCulture; await InlineArrayTests.AssertLinked(source, output, exitCode); }
        catch(NotSupportedException error)
        {
            foreach(System.Collections.DictionaryEntry entry in error.Data) log.WriteLine($"{entry.Key}: {entry.Value}");
            throw;
        }
        finally { CultureInfo.CurrentCulture=culture; }
    }
}
