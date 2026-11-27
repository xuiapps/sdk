using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class BuilderTests
{
    [Fact]
    public void EditingAndGrowthMatchClrCodeUnits()
    {
        var actual = new Xui.Compiler.Runtime.Text.StringBuilder(1);
        var expected = new System.Text.StringBuilder(1);
        var random = new Random(815);
        for (int i = 0; i < 2000; i++)
        {
            switch (random.Next(5))
            {
                case 0:
                    char unit = (char)random.Next(65536);
                    int count = random.Next(10);
                    expected.Append(unit, count); actual.Append(unit, count); break;
                case 1:
                    int length = random.Next(50);
                    expected.Length = length; actual.Length = length; break;
                case 2:
                    expected.Append("🙂a\0"); actual.Append("🙂a\0"); break;
                case 3 when expected.Length > 0:
                    int at = random.Next(expected.Length);
                    expected[at] = 'X'; actual[at] = 'X'; break;
                default:
                    expected.Clear(); actual.Clear(); break;
            }
            Assert.Equal(expected.ToString(), actual.ToString());
            Assert.Equal(expected.Length, actual.Length);
            Assert.True(actual.Capacity >= actual.Length);
        }
        Assert.Equal(new System.Text.StringBuilder("abcdefghijklmnopqrst", 1).Capacity,
            new Xui.Compiler.Runtime.Text.StringBuilder("abcdefghijklmnopqrst", 1).Capacity);
    }

    [Fact]
    public async Task MutableBufferMaterializesIndependentNativeStrings()
    {
        const string source="""
            using System;
            using System.Text;
            public record Label(string Name);
            public static class Program {
                public static int Main() {
                    var b=new StringBuilder(2);
                    b.Append("ab").Append('c').Append('!',3).Append((string)null).Append((object)null);
                    string before=b.ToString();
                    b[0]='Z'; Console.WriteLine(before); Console.WriteLine(b.ToString());
                    b.Length=2; b.Length=5;
                    for(int i=0;i<b.Length;i++) Console.WriteLine((int)b[i]);
                    b.Clear().Append(new char[]{'a','b','c'},1,2).Append("01234",1,3);
                    b.Append("🙂Ж\ud800".AsSpan());
                    Console.WriteLine(b.ToString(0,5));
                    Console.WriteLine((int)b[b.Length-1]);
                    b.Clear().Append(new Label("record")); Console.WriteLine(b.ToString());
                    b.Clear().AppendLine("line"); Console.WriteLine(b.Length);
                    b.Length=0; b.Capacity=0; b.Append('x'); Console.WriteLine(b.ToString());
                    try { new StringBuilder(-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { b.Length=-1; } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { b.Append('x',-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { b.ToString(2,0); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { b.ToString(0,2); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { b.Append((string)null,1,0); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                    return 42;
                }
            }
            """;
        foreach(var optimization in new[]{OptimizationLevel.Release,OptimizationLevel.Debug})
        {
            byte[] image=Frontend.Roslyn.Compile(source,optimizationLevel:optimization);
            var expected=GcTests.RunManaged(image);
            Assert.Equal(42,expected.ExitCode);
            using var app=CanvasTests.Read(image);
            using var runtime=CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Text.StringBuilder).Assembly.Location));
            using var framework=new Frontend.Framework();
            _=new Xui.Compiler.XuiNet.Compilation([app,runtime,..framework.Assemblies],BrowserStringBuilder.Replacements(framework.Assemblies[0],runtime));
            byte[] wasm;
            try { wasm=new WasmEmitter().Emit(app,new ConsoleWriteLineRewriter()); }
            catch(Exception e) when(e.Data["Xui.DependencyPath"] is {} path) { throw new Exception(e.Message+"\n"+path,e); }
            Assert.Equal(expected,await Node.ExecuteAsync(wasm));
        }
    }
}
