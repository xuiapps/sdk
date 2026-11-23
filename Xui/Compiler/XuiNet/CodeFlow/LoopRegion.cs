namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Instruction membership used while reconstructing a natural loop.</summary>
internal sealed record LoopRegion(
    LoopBlock Block,
    HashSet<InstructionNode> Members);
