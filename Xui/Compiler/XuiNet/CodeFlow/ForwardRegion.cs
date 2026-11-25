namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A topologically ordered acyclic region with shared forward destinations.</summary>
/// <remarks>Each segment emits once. Nested WASM result blocks can represent its edges without a dispatcher.</remarks>
public sealed class ForwardRegion : FlowNode
{
    internal ForwardRegion(IReadOnlyList<ForwardSegment> segments) => Segments = segments;
    /// <summary>Basic instruction runs in predecessor-before-successor order.</summary>
    public IReadOnlyList<ForwardSegment> Segments { get; }
}
