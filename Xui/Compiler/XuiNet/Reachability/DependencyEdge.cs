namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>An explanatory dependency, optionally located at an IL offset in the source method.</summary>
/// <remarks>Condition records the second prerequisite of a virtual-dispatch dependency.</remarks>
public readonly record struct DependencyEdge(
    Dependency Source,
    Dependency Target,
    DependencyReason Reason,
    int? Offset = null,
    Dependency? Condition = null);
