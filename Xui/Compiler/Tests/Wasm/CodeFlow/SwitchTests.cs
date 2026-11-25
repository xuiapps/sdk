using System.Reflection.Metadata;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

[Collection(nameof(ManagedConsoleCollection))]
public class SwitchTests
{
    [Theory]
    [InlineData(int.MinValue, 15)]
    [InlineData(-1, 15)]
    [InlineData(0, 22)]
    [InlineData(1, 24)]
    [InlineData(2, 15)]
    [InlineData(int.MaxValue, 15)]
    public async Task ArmsConsumeInputsAndReturnMultipleValues(int selector, int expected)
    {
        byte[] image = Frontend.Msil.Compile(il =>
        {
            var a = il.DefineLabel(); var b = il.DefineLabel(); var join = il.DefineLabel();
            il.LoadConstantI4(7); il.LoadConstantI4(4); il.LoadConstantI4(selector);
            var cases = il.Switch(2); cases.Branch(a); cases.Branch(b);
            il.OpCode(ILOpCode.Pop); il.LoadConstantI4(3); il.LoadConstantI4(5); il.Branch(ILOpCode.Br, join);
            il.MarkLabel(a); il.LoadConstantI4(6); il.OpCode(ILOpCode.Add); il.LoadConstantI4(5); il.Branch(ILOpCode.Br, join);
            il.MarkLabel(b); il.LoadConstantI4(2); il.OpCode(ILOpCode.Mul); il.LoadConstantI4(9);
            il.MarkLabel(join); il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Ret);
        });
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        var graph = Analyzer.Analyze(assembly.ManifestModule, assembly.ManifestModule.EntryPoint);
        var selection = Assert.Single(StructuredBlocks.Build(graph).Children.OfType<SwitchBlock>());
        Assert.Equal(1, selection.Preserved.Depth);
        Assert.Equal(new StackEntry[] { StackType.Int32 }, selection.Parameters);
        Assert.Equal(new StackEntry[] { StackType.Int32, StackType.Int32 }, selection.Results);
        byte[] wasm = new WasmEmitter().Emit(assembly);
        Assert.Equal(expected, GcTests.RunManaged(image).ExitCode);
        Assert.Equal(expected, (await Node.ExecuteAsync(wasm)).ExitCode);
        Assert.Contains("br_table", await WasmTools.ToWatAsync(wasm));
    }

    [Fact]
    public async Task EmptySwitchConsumesOnlyItsSelector()
    {
        byte[] image = Frontend.Msil.Compile(il =>
        {
            il.LoadConstantI4(42); il.LoadConstantI4(-1);
            il.OpCode(ILOpCode.Switch); il.CodeBuilder.WriteInt32(0); il.OpCode(ILOpCode.Ret);
        });
        Assert.Equal(42, GcTests.RunManaged(image).ExitCode);
        Assert.Equal(42, (await Node.ExecuteAsync(GcTests.Emit(image))).ExitCode);
    }

    [Fact]
    public Task DenseCasesShareTargetsAndProduceStrings() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static string Name(int value) => value switch { 0=>"zero", 1 or 2=>"pair", 3=>"three", 4=>"four", _=>"other" };
            static int Number(int value) { int result; switch(value) {
                case 0: result=10; break; case 1: case 2: result=20; break;
                case 3: result=30; break; case 4: result=40; break; default: result=50; break;
            } return result+1; }
            public static int Main() {
                for(int i=-1;i<6;i++) { Console.WriteLine(Name(i)); Console.WriteLine(Number(i)); }
                return 42;
            }
        }
        """, "other\n51\nzero\n11\npair\n21\npair\n21\nthree\n31\nfour\n41\nother\n51\n", 42);

    [Fact]
    public Task LoopContinueKeepsItsEnclosingLabel() => GcTests.AssertExecution("""
        using System;
        public static class Program { public static int Main() {
            int sum=0;
            for(int i=0;i<6;i++) {
                switch(i) { case 0: sum+=1; break; case 1: sum+=2; break; case 2: continue; case 3: sum+=4; break; default: sum+=10; break; }
                sum+=100;
            }
            Console.WriteLine(sum); return 42;
        } }
        """, "527\n", 42);

    [Fact]
    public Task NestedSwitchReturnsStillRunFinally() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static int Run(int x,int y) {
                try {
                    switch(x) {
                        case 0: return 10;
                        case 1: switch(y) { case 0:return 20; case 1:return 21; case 2:return 22; case 3:return 23; default:return 24; }
                        case 2: throw new InvalidOperationException("selected");
                        case 3: return 30;
                        default: return 40;
                    }
                } finally { Console.WriteLine("cleanup"); }
            }
            public static int Main() {
                Console.WriteLine(Run(0,0)); Console.WriteLine(Run(1,2));
                try { Run(2,0); } catch(InvalidOperationException e) { Console.WriteLine(e.Message); }
                Console.WriteLine(Run(-1,0)); return 42;
            }
        }
        """, "cleanup\n10\ncleanup\n22\ncleanup\nselected\ncleanup\n40\n", 42);
}
