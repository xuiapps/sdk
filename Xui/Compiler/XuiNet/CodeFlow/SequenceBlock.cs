namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Structured operations executed in order.</summary>
public sealed class SequenceBlock(IReadOnlyList<FlowNode> children) : FlowNode
{
    /// <summary>Instructions and nested conditionals in execution order.</summary>
    public IReadOnlyList<FlowNode> Children { get; } = children;
}
