using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xunit;
using OpCode = Xui.Compiler.XuiNet.DotNet.Op.OpCode;

namespace Xui.Compiler.Tests.XuiNet.CodeFlow;

public class GraphTests
{
    [Fact]
    public void ArmsCanConsumeAnInputAndProduceMultipleResults()
    {
        var graph = Analyze(Program(il =>
        {
            var otherwise = il.DefineLabel();
            var join = il.DefineLabel();
            il.LoadConstantI4(1); il.LoadConstantI4(2); il.LoadConstantI4(0);
            il.Branch(ILOpCode.Brfalse_s, otherwise);
            il.OpCode(ILOpCode.Pop); il.LoadConstantI4(3); il.LoadConstantI4(4); il.Branch(ILOpCode.Br_s, join);
            il.MarkLabel(otherwise);
            il.OpCode(ILOpCode.Pop); il.LoadConstantI4(3); il.LoadConstantI4(4);
            il.MarkLabel(join);
            il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Ret);
        }));
        var branch = Assert.Single(graph.Instructions, node => node.Code == OpCode.Brfalse_s);
        Assert.Equal(3, branch.Before!.Depth);
        Assert.Equal(2, branch.After!.Depth);
        Assert.All(branch.Successors, next => Assert.Same(branch.After, next.Before));
        var pops = graph.Instructions.Where(node => node.Code == OpCode.Pop).ToArray();
        Assert.Equal(2, pops.Length);
        Assert.Same(pops[0].Before, pops[1].Before);
        Assert.Same(pops[0].After, pops[1].After);
        Assert.Same(branch.After.Parent, pops[0].After);
        var addition = graph.Instructions.First(node => node.Code == OpCode.Add);
        Assert.Equal(3, addition.Before!.Depth);
        Assert.Equal(2, addition.After!.Depth);
        Assert.Equal(1, addition.Lowest!.Depth);
        Assert.Same(branch.After, addition.After); // Same types, despite different runtime values.
        Assert.Equal(4, graph.Stacks.States.Count);
    }

    [Fact]
    public void OperandReadsAreRetainedWhenBeforeAndAfterAreIdentical()
    {
        var graph = Analyze(Program(il =>
        {
            il.LoadConstantI4(1); il.OpCode(ILOpCode.Neg);
            il.OpCode(ILOpCode.Dup); il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Ret);
        }));
        var neg = Assert.Single(graph.Instructions, node => node.Code == OpCode.Neg);
        Assert.Same(neg.Before, neg.After);
        Assert.Same(graph.Stacks.Empty, neg.Lowest);
        var dup = Assert.Single(graph.Instructions, node => node.Code == OpCode.Dup);
        Assert.Same(dup.Before, dup.After!.Parent);
        Assert.Same(graph.Stacks.Empty, dup.Lowest);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JoinsRejectDifferentTypesOrDepths(bool differentDepth)
    {
        byte[] image = Program(il =>
        {
            var otherwise = il.DefineLabel();
            var join = il.DefineLabel();
            il.LoadConstantI4(0); il.Branch(ILOpCode.Brfalse_s, otherwise);
            il.LoadConstantI4(1); il.Branch(ILOpCode.Br_s, join);
            il.MarkLabel(otherwise);
            if (differentDepth) { il.LoadConstantI4(1); il.LoadConstantI4(2); }
            else il.LoadConstantR8(1);
            il.MarkLabel(join); il.Branch(ILOpCode.Br_s, join);
        });
        Assert.Contains("Stack mismatch on edge", Assert.Throws<InvalidProgramException>(() => Analyze(image)).Message);
    }

    [Fact]
    public void StableBackEdgeReusesItsEntryState()
    {
        var graph = Analyze(Program(il =>
        {
            var repeat = il.DefineLabel();
            il.LoadConstantI4(1);
            il.MarkLabel(repeat);
            il.OpCode(ILOpCode.Dup); il.OpCode(ILOpCode.Pop); il.Branch(ILOpCode.Br_s, repeat);
        }));
        var backEdge = graph.Instructions[^1];
        Assert.Same(backEdge.After, Assert.Single(backEdge.Successors).Before);
        Assert.Equal(1, backEdge.After!.Depth);
        Assert.Equal(3, graph.Stacks.States.Count);
    }

    [Fact]
    public void GrowingBackEdgeFailsInsteadOfReanalyzingForever()
    {
        byte[] image = Program(il =>
        {
            var repeat = il.DefineLabel();
            il.MarkLabel(repeat); il.LoadConstantI4(1); il.Branch(ILOpCode.Br_s, repeat);
        });
        Assert.Contains("Stack mismatch on edge", Assert.Throws<InvalidProgramException>(() => Analyze(image)).Message);
    }

    [Fact]
    public void AnalysisFollowsJumpsInsteadOfAssumingFileOrder()
    {
        var graph = Analyze(Program(il =>
        {
            var earlier = il.DefineLabel();
            var later = il.DefineLabel();
            var exit = il.DefineLabel();
            il.Branch(ILOpCode.Br_s, later);
            il.MarkLabel(earlier); il.LoadConstantI4(5); il.Branch(ILOpCode.Br_s, exit);
            il.MarkLabel(later); il.Branch(ILOpCode.Br_s, earlier);
            il.MarkLabel(exit); il.OpCode(ILOpCode.Ret);
        }));
        Assert.All(graph.Instructions, node => Assert.NotNull(node.Before));
        Assert.Same(graph.Stacks.Empty, graph.Instructions[1].Before);
        Assert.Equal(StackType.Int32, graph.Instructions[^1].Before!.Type);
        Assert.Same(graph.Stacks.Empty, graph.Instructions[^1].After);
    }

    [Fact]
    public void UnreachableInstructionsHaveNoInventedStackState()
    {
        var graph = Analyze(Program(il =>
        {
            var live = il.DefineLabel();
            il.Branch(ILOpCode.Br_s, live);
            il.OpCode(ILOpCode.Ldtoken); il.Token(MetadataTokens.TypeDefinitionHandle(1)); il.OpCode(ILOpCode.Pop); // Unsupported when reachable.
            il.MarkLabel(live); il.LoadConstantI4(5); il.OpCode(ILOpCode.Ret);
        }));
        foreach (var node in graph.Instructions.Skip(1).Take(2))
        {
            Assert.False(node.IsReachable);
            Assert.Null(node.Before); Assert.Null(node.After); Assert.Null(node.Lowest);
        }
        Assert.True(graph.Entry.IsReachable);
        Assert.True(graph.Instructions[^1].IsReachable);
        Assert.Equal(2, graph.Stacks.States.Count);
    }

    [Fact]
    public void CallSignaturesNormalizeSingleAndDoubleToTheSameStackCategory()
    {
        var graph = Analyze(Frontend.Roslyn.Compile("""
            public static class Program
            {
                static float Identity(double value) => (float)value;
                public static int Main() { System.Console.WriteLine(Identity(1f)); return 0; }
            }
            """));
        var calls = graph.Instructions.Where(node => node.Code == OpCode.Call).ToArray();
        Assert.Equal(2, calls.Length);
        Assert.Equal(StackType.Float, calls[0].Before!.Type);
        Assert.Same(calls[0].Before, calls[0].After);
        Assert.Same(graph.Stacks.Empty, calls[0].Lowest);
        Assert.Same(graph.Stacks.Empty, calls[1].After);
    }

    [Fact]
    public void SwitchPropagatesThePoppedSelectorStateToEverySuccessor()
    {
        var graph = Analyze(Frontend.Roslyn.Compile("""
            public static class Program
            {
                public static int Main() => Run(2);
                static int Run(int value)
                {
                    switch (value)
                    {
                        case 0: return 10; case 1: return 20; case 2: return 30;
                        case 3: return 40; case 4: return 50; default: return 60;
                    }
                }
            }
            """), "Run");
        var branch = Assert.Single(graph.Instructions, node => node.Code == OpCode.Switch);
        Assert.True(branch.Successors.Length >= 5);
        Assert.Equal(StackType.Int32, branch.Before!.Type);
        Assert.Same(graph.Stacks.Empty, branch.After);
        Assert.All(branch.Successors, successor => Assert.Same(branch.After, successor.Before));
    }

    [Fact]
    public void JumpIntoAnOperandIsRejected()
    {
        byte[] image = Program(il =>
        {
            il.OpCode(ILOpCode.Br_s);
            il.CodeBuilder.WriteSByte(-1); // Targets its own displacement byte, not an instruction.
            il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret);
        });
        Assert.Contains("Invalid successor IL_0001", Assert.Throws<BadImageFormatException>(() => Analyze(image)).Message);
    }

    [Fact]
    public void UnderflowAndUnsupportedEffectsThrowAtTheirInstruction()
    {
        var underflow = Program(il => { il.OpCode(ILOpCode.Pop); il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret); });
        Assert.Contains("IL_0000", Assert.Throws<InvalidProgramException>(() => Analyze(underflow)).Message);
        var unsupported = Program(il => { il.OpCode(ILOpCode.Ldtoken); il.Token(MetadataTokens.MethodDefinitionHandle(1)); il.OpCode(ILOpCode.Pop); il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret); });
        Assert.Contains("method handles", Assert.Throws<NotSupportedException>(() => Analyze(unsupported)).Message);
    }

    [Fact]
    public void TypeTokensCarryDescriptorIdentityUntilConsumed()
    {
        var graph = Analyze(Program(il => { il.OpCode(ILOpCode.Ldtoken); il.Token(MetadataTokens.TypeDefinitionHandle(2)); il.OpCode(ILOpCode.Pop); il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret); }));
        var token = graph.Instructions[0];
        Assert.Equal(StackType.RuntimeTypeHandle, token.After!.Entry!.Value.Type);
        Assert.Equal(MetadataTokens.TypeDefinitionHandle(2), token.After.Entry.Value.TypeToken!.Value.Handle);
        Assert.Same(graph.Stacks.Empty, graph.Instructions[1].After);
    }

    private static InstructionGraph Analyze(byte[] image, string methodName = "Main")
    {
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        return Analyzer.Analyze(module, module.Methods.Single(handle => module.GetMethod(handle).Name == methodName));
    }

    private static byte[] Program(Action<InstructionEncoder> emit) => Frontend.Msil.Compile(emit);
}
