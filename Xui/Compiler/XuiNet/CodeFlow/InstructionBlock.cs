namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>An instruction whose stack annotations belong to the source graph.</summary>
public sealed class InstructionBlock(InstructionNode instruction) : FlowNode
{
    /// <summary>The original instruction, including its before, after, and lowest states.</summary>
    public InstructionNode Instruction { get; } = instruction;
}
