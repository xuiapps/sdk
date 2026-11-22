using System.Reflection.Metadata;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

public class LoopTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task BackEdgesCarryTypedValuesAndPreserveTheLowerStack(int initial)
    {
        byte[] image = Frontend.Msil.Compile(il =>
        {
            var header = il.DefineLabel(); var exit = il.DefineLabel();
            il.LoadConstantI4(7); il.LoadConstantI4(initial);
            il.MarkLabel(header); il.OpCode(ILOpCode.Dup); il.Branch(ILOpCode.Brfalse_s, exit);
            il.LoadConstantI4(1); il.OpCode(ILOpCode.Sub); il.Branch(ILOpCode.Br_s, header);
            il.MarkLabel(exit); il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Ret);
        });
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        var graph = Analyzer.Analyze(assembly.ManifestModule, assembly.ManifestModule.EntryPoint);
        var loop = Assert.Single(StructuredBlocks.Build(graph).Children.OfType<LoopBlock>());
        Assert.Equal(1, loop.Preserved.Depth);
        Assert.Equal(new StackEntry[] { StackType.Int32 }, loop.Parameters);
        Assert.Equal(new StackEntry[] { StackType.Int32 }, loop.Results);
        byte[] wasm = new WasmEmitter().Emit(assembly);
        Assert.Equal(7, (await Node.ExecuteAsync(wasm)).ExitCode);
        Assert.Contains("(param i32) (result i32)", await WasmTools.ToWatAsync(wasm));
    }

    [Fact]
    public async Task LoopInputsAndResultsCanHaveDifferentLengths()
    {
        byte[] image = Frontend.Msil.Compile(il =>
        {
            var header = il.DefineLabel(); var exit = il.DefineLabel();
            il.LoadConstantI4(1);
            il.MarkLabel(header); il.Branch(ILOpCode.Brfalse_s, exit);
            il.LoadConstantI4(0); il.Branch(ILOpCode.Br_s, header);
            il.MarkLabel(exit); il.LoadConstantI4(9); il.OpCode(ILOpCode.Ret);
        });
        Assert.Equal(9, (await Node.ExecuteAsync(Emit(image))).ExitCode);
    }

    [Fact]
    public async Task LoopCanProduceAResultWithoutAnInput()
    {
        byte[] image = Frontend.Msil.Compile(il =>
        {
            var header = il.DefineLabel(); var exit = il.DefineLabel();
            il.MarkLabel(header); il.LoadConstantI4(0); il.OpCode(ILOpCode.Dup); il.Branch(ILOpCode.Brfalse_s, exit);
            il.OpCode(ILOpCode.Pop); il.Branch(ILOpCode.Br_s, header);
            il.MarkLabel(exit); il.OpCode(ILOpCode.Ret);
        });
        Assert.Equal(0, (await Node.ExecuteAsync(Emit(image))).ExitCode);
    }

    [Fact]
    public void MultipleEntryCycleRequiresADispatcher()
    {
        byte[] image = Frontend.Msil.Compile(il =>
        {
            var first = il.DefineLabel(); var second = il.DefineLabel(); var exit = il.DefineLabel();
            il.LoadConstantI4(0); il.Branch(ILOpCode.Brtrue_s, second);
            il.MarkLabel(first); il.LoadConstantI4(0); il.Branch(ILOpCode.Brtrue_s, exit);
            il.MarkLabel(second); il.Branch(ILOpCode.Br_s, first);
            il.MarkLabel(exit); il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret);
        });
        Assert.Contains("multiple-entry cycle", Assert.Throws<NotSupportedException>(() => Emit(image)).Message);
    }

    [Fact]
    public void AStoreOnTheContinuePathDoesNotInitializeTheExit()
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var header = il.DefineLabel(); var exit = il.DefineLabel();
            il.MarkLabel(header); il.LoadConstantI4(0); il.Branch(ILOpCode.Brfalse_s, exit);
            il.LoadConstantI4(4); il.StoreLocal(0); il.Branch(ILOpCode.Br_s, header);
            il.MarkLabel(exit); il.LoadLocal(0); il.OpCode(ILOpCode.Ret);
        }, [SignatureTypeCode.Int32], initLocals: false);
        Assert.Contains("Uninitialized local 0", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public async Task AStoreBeforeEveryExitInitializesTheContinuation()
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var header = il.DefineLabel();
            il.MarkLabel(header); il.LoadConstantI4(4); il.StoreLocal(0);
            il.LoadConstantI4(0); il.Branch(ILOpCode.Brtrue_s, header);
            il.LoadLocal(0); il.OpCode(ILOpCode.Ret);
        }, [SignatureTypeCode.Int32], initLocals: false);
        Assert.Equal(4, (await Node.ExecuteAsync(Emit(image))).ExitCode);
    }

    private static byte[] Emit(byte[] image)
    {
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly);
    }
}
