using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class ProviderInterpolationTests
{
    [Fact]
    public async Task CustomProvidersFailExplicitlyInsteadOfBeingIgnored()
    {
        using var app=Canvas.CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System;
            using System.Runtime.CompilerServices;
            public class CustomProvider:IFormatProvider { public object GetFormat(Type type) => null; }
            public static class Program {
                public static int Main() {
                    int result=-1;
                    try { var handler=new DefaultInterpolatedStringHandler(0,0,new CustomProvider()); }
                    catch(NotSupportedException) { result=42; }
                    return result;
                }
            }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app,..framework.Assemblies]);
        byte[] wasm=new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app);
        Assert.Equal(42,(await Repl.Node.ExecuteAsync(wasm)).ExitCode);
    }

    [Fact]
    public Task InvariantProviderSurvivesLocalsAndStringCreateFinishesTheHandler() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Globalization;
        using System.Runtime.CompilerServices;
        public static class Program {
            static string Format(IFormatProvider provider,double strength,double frequency) =>
                string.Create(provider,$"Strength {strength:0.00}   Frequency {frequency:0.00}");
            public static int Main() {
                IFormatProvider provider=CultureInfo.InvariantCulture;
                Console.WriteLine(Format(provider,.75,1.25));
                var handler=new DefaultInterpolatedStringHandler(0,1,provider);
                handler.AppendFormatted(12.5,"0.0");Console.WriteLine(string.Create(provider,ref handler));
                Console.WriteLine(handler.ToStringAndClear()=="");
                return 42;
            }
        }
        """, "Strength 0.75   Frequency 1.25\n12.5\nTrue\n",42);
}
