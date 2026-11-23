namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>An edge to a loop's next iteration or its continuation.</summary>
public sealed class LoopTransfer(LoopBlock target, LoopTransferKind kind) : FlowNode
{
    /// <summary>The enclosing loop whose label is targeted.</summary>
    public LoopBlock Target { get; } = target;
    /// <summary>Whether this edge repeats or exits the loop.</summary>
    public LoopTransferKind Kind { get; } = kind;
}
