using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Playground;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class SplitTests
{
    [Fact]
    public Task SeparatorsCountsAndOptionsMatchCoreLib() => Compare("""
        using System;
        public static class Program {
            static void Print(string[] values) {
                Console.WriteLine(values.Length);
                foreach(var value in values) Console.WriteLine("["+value+"]");
            }
            public static int Main() {
                foreach(var text in new[] { "", "a", ",", ",,", "a,,b,,,c,", " , a , , b , c , ", "Ж😀,🧡\0" })
                    for(int options=0;options<4;options++)
                        for(int count=0;count<6;count++) {
                            Print(text.Split(',', count, (StringSplitOptions)options));
                            Print(text.Split(",", count, (StringSplitOptions)options));
                            Print(text.Split(new[] { ',', ';' }, count, (StringSplitOptions)options));
                            Print(text.Split(new[] { ",", ";" }, count, (StringSplitOptions)options));
                        }
                Print("a::b:c:::d".Split(new[] { ":", "::" }, StringSplitOptions.None));
                Print("a::b:c:::d".Split(new[] { "::", ":", null, "" }, StringSplitOptions.None));
                Print("abc".Split((string)null, StringSplitOptions.None));
                Print("abc".Split("", StringSplitOptions.None));
                Print("a\ud800b\ud800c".Split('\ud800'));
                Print("line one\nline two\n".Split('\n'));
                var values="a,b".Split(','); values[0]="changed"; Print(values);
                return 42;
            }
        }
        """);

    [Fact]
    public Task WhitespaceAndTrimUseDotNetCodeUnits() => Compare("""
        using System;
        public static class Program {
            static void Print(string[] values) {
                Console.WriteLine(values.Length);
                foreach(var value in values) Console.WriteLine("["+value+"]");
            }
            public static int Main() {
                string text=" \tA\u0085B\u00a0C\u1680D\u2003E\u2028F\u2029G\u202fH\u205fI\u3000J\ufeffK ";
                for(int options=0;options<4;options++) for(int count=0;count<15;count++) {
                    Print(text.Split((char[])null, count, (StringSplitOptions)options));
                    Print(text.Split(new char[0], count, (StringSplitOptions)options));
                    Print(text.Split((string[])null, count, (StringSplitOptions)options));
                    Print(text.Split(new string[0], count, (StringSplitOptions)options));
                    Print(text.Split(new[] { (string)null, "" }, count, (StringSplitOptions)options));
                    Print("\u0085a\u0085, \ufeffb\ufeff ".Split(',', count, (StringSplitOptions)options));
                }
                return 42;
            }
        }
        """);

    [Fact]
    public Task InvalidArgumentsAreManagedExceptions() => Compare("""
        using System;
        public static class Program {
            public static int Main() {
                try { string value=null; value.Split(',', -1, (StringSplitOptions)4); }
                catch(NullReferenceException) { Console.WriteLine("null receiver"); }
                try { "a".Split(',', -1, (StringSplitOptions)4); }
                catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { "a".Split(',', 0, (StringSplitOptions)4); }
                catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                try { "a".Split(',', (StringSplitOptions)(-1)); }
                catch(ArgumentException e) { Console.WriteLine(e.ParamName); }
                return 42;
            }
        }
        """);

    private static async Task Compare(string source)
    {
        foreach(var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var image = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            var managed = GcTests.RunManaged(image);
            using var app = CanvasTests.Read(image);
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
            var emitter = new WasmEmitter();
            var wasm = emitter.Emit(app, new ConsoleWriteLineRewriter());
            var actual = await Node.ExecuteAsync(wasm);
            Assert.Equal(managed.ExitCode, actual.ExitCode);
            // The managed capture normalizes Unicode line separators as well as CR/LF.
            Assert.Equal(managed.Output, actual.Output.ReplaceLineEndings("\n"));
            Assert.DoesNotContain(emitter.Dependencies!.Methods, method => method.ToString().Contains("System.String.Split"));
        }
    }
}
