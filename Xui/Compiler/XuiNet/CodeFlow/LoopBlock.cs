namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A single-entry loop with one continuation and typed back-edge and exit values.</summary>
public sealed class LoopBlock : FlowNode
{
    internal LoopBlock(InstructionNode header, InstructionNode exit, StackState preserved)
    {
        Header = header;
        Exit = exit;
        Preserved = preserved;
        Parameters = StackSignature.Suffix(header.Before!, preserved);
        Results = StackSignature.Suffix(exit.Before!, preserved);
    }

    /// <summary>The first instruction executed on entry and on every back edge.</summary>
    public InstructionNode Header { get; }
    /// <summary>The common continuation outside this loop.</summary>
    public InstructionNode Exit { get; }
    /// <summary>The stack prefix untouched by any iteration.</summary>
    public StackState Preserved { get; }
    /// <summary>Values passed on entry and on every continue, ordered bottom to top.</summary>
    public IReadOnlyList<StackEntry> Parameters { get; }
    /// <summary>Values passed to the continuation on break, ordered bottom to top.</summary>
    public IReadOnlyList<StackEntry> Results { get; }
    /// <summary>The structured iteration, ending in a transfer or return.</summary>
    public SequenceBlock Body { get; internal set; } = new([]);
}
