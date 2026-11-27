using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Configuration;

[Collection(nameof(ManagedConsoleCollection))]
public class AppContextTests
{
    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public Task SwitchesCanBeReadChangedAndValidated(OptimizationLevel optimization) => Run($$"""
        using System;
        public static class Program {
            public static int Main() {
                string key="Xui.Compiler.Configuration.{{optimization}}";
                bool value=true; Console.WriteLine(AppContext.TryGetSwitch(key,out value)); Console.WriteLine(value);
                AppContext.SetSwitch(key,true); Console.WriteLine(AppContext.TryGetSwitch(key,out value)); Console.WriteLine(value);
                AppContext.SetSwitch(key,false); Console.WriteLine(AppContext.TryGetSwitch(key,out value)); Console.WriteLine(value);
                try { AppContext.TryGetSwitch(null,out value); } catch(ArgumentNullException e) { Console.WriteLine(e.Message); }
                try { AppContext.SetSwitch("",true); } catch(ArgumentException e) { Console.WriteLine(e.Message); }
                return 42;
            }
        }
        """, optimization);

    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public Task LinqMaterializesAnIteratorOfReferenceContainingTuples(OptimizationLevel optimization) => Run("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Program {
            static IEnumerable<(int Number,string Text)> Values() {
                for(int i=0;i<4;i++) yield return (i+9,"value");
            }
            public static int Main() {
                var values=Values().ToArray();
                int sum=0; foreach(var value in values) { sum+=value.Number; Console.WriteLine(value.Text); }
                return sum;
            }
        }
        """, optimization);

    private static async Task Run(string source, OptimizationLevel optimization)
    {
        byte[] image=Frontend.Roslyn.Compile(source,optimizationLevel:optimization,
            additionalReferences:[MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Linq").Location)]);
        var expected=GcTests.RunManaged(image);
        Assert.Equal(42,expected.ExitCode);
        using var app=CanvasTests.Read(image);
        using var runtime=CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.AppContext).Assembly.Location));
        using var framework=new Frontend.Framework("System.Linq.dll","System.Collections.dll");
        _=new Xui.Compiler.XuiNet.Compilation([app,runtime,..framework.Assemblies],
            BrowserConfiguration.Replacements(framework.Assemblies.Single(a=>a.Identity.Name=="System.Private.CoreLib"),runtime));
        byte[] wasm;
        try { wasm=new WasmEmitter().Emit(app,new Xui.Compiler.Playground.ConsoleWriteLineRewriter()); }
        catch(NotSupportedException e) { throw new NotSupportedException(e.Message+"\n"+e.Data["Xui.DependencyPath"],e); }
        Assert.Equal(expected,await Node.ExecuteAsync(wasm));
    }
}
