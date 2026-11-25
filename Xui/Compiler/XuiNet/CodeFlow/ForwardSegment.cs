namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A straight instruction run ending at a branch, join, or method exit.</summary>
public sealed class ForwardSegment : FlowNode
{
    internal ForwardSegment(IReadOnlyList<InstructionNode> instructions) => Instructions = instructions;
    /// <summary>Instructions executed consecutively, including the final control transfer.</summary>
    public IReadOnlyList<InstructionNode> Instructions { get; }
    /// <summary>The analyzed stack supplied by every incoming edge.</summary>
    public StackState Input => Instructions[0].Before!;
    /// <summary>All values returned by this segment's enclosing destination label.</summary>
    public IReadOnlyList<StackEntry> Results => StackSignature.Suffix(Input, Root(Input));
    private static StackState Root(StackState state) { while (state.Parent is not null) state = state.Parent; return state; }
}
