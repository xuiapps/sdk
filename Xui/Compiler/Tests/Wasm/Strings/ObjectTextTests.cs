using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class ObjectTextTests
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task ObjectCallsPreserveOverridesBoxesEnumsAndArrayIdentity(OptimizationLevel optimization)
    {
        byte[] pe = Frontend.Roslyn.Compile("""
            using System;
            namespace Demo {
                [Flags] public enum Flags : byte { A=1, B=2 }
                public class Plain<T> { public class Nested<U> { } }
                public class Base { public override string ToString() => "base"; }
                public class Derived : Base { public override string ToString() => "derived"; }
                public class Hidden : Base { public new virtual string ToString() => "hidden"; }
                public class FromObject { public override string ToString() => base.ToString()+"!"; }
                public class NullText { public override string ToString() => null; }
                public static class Program {
                    static void Print(object value) => Console.WriteLine(value.ToString());
                    public static int Main() {
                        Print("native"); Print(true); Print('x'); Print((sbyte)-2); Print((byte)255);
                        Print((short)-300); Print((ushort)65535); Print(-42); Print(uint.MaxValue);
                        Print(long.MinValue); Print(ulong.MaxValue); Print(1.5f); Print(2.25d);
                        Print(Flags.A|Flags.B); Print(new object()); Print(new Plain<int>.Nested<string>());
                        Print(new Base()); Print(new Derived()); Print(new Hidden()); Print(new FromObject()); Print(new NullText());
                        Print(new int[0]); Print(new uint[0]); Print(new string[0]); Print(new int[1,2]); Print(new int[0][]);
                        try { Print(null); return -1; } catch(NullReferenceException) { return 42; }
                    }
                }
            }
            """, optimizationLevel: optimization);
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        NodeResult expected;
        try { System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture; expected = GcTests.RunManaged(pe); }
        finally { System.Globalization.CultureInfo.CurrentCulture = culture; }
        using var app = CanvasTests.Read(pe);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        byte[] wasm = new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter());
        Assert.Equal(expected, await Node.ExecuteAsync(wasm));
    }
}
