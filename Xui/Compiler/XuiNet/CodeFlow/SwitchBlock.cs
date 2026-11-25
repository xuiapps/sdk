namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A switch whose arms share a typed input stack and optional common continuation.</summary>
public sealed class SwitchBlock : FlowNode
{
    internal SwitchBlock(InstructionNode selector, IReadOnlyList<SwitchArm> arms, InstructionNode? join, StackState preserved)
    {
        Selector = selector; Arms = arms; Join = join; Preserved = preserved;
        var indices = arms.Select((arm, index) => (arm.Entry, index)).ToDictionary(pair => pair.Entry, pair => pair.index);
        DefaultArm = indices[selector.Successors[0]];
        CaseArms = selector.Successors.Skip(1).Select(node => indices[node]).ToArray();
        Parameters = StackSignature.Suffix(selector.After!, preserved);
        Results = join is null ? [] : StackSignature.Suffix(join.Before!, preserved);
    }

    /// <summary>The switch instruction, which consumes an Int32 selector.</summary>
    public InstructionNode Selector { get; }
    /// <summary>Distinct destinations; multiple case values can share an arm.</summary>
    public IReadOnlyList<SwitchArm> Arms { get; }
    /// <summary>The arm index for each zero-based case value.</summary>
    public IReadOnlyList<int> CaseArms { get; }
    /// <summary>The arm selected by negative or out-of-range values.</summary>
    public int DefaultArm { get; }
    /// <summary>The common continuation, or null when all arms transfer out.</summary>
    public InstructionNode? Join { get; }
    /// <summary>The untouched prefix below each arm's inputs and results.</summary>
    public StackState Preserved { get; }
    /// <summary>Consumed stack suffix, excluding the selector.</summary>
    public IReadOnlyList<StackEntry> Parameters { get; }
    /// <summary>Produced stack suffix at the continuation.</summary>
    public IReadOnlyList<StackEntry> Results { get; }
}
