namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Two arms with a shared input stack and an optional common continuation.</summary>
public sealed class ConditionalBlock : FlowNode
{
    internal ConditionalBlock(InstructionNode condition, SequenceBlock taken, SequenceBlock notTaken,
        InstructionNode? join, StackState preserved, bool takenCanContinue, bool notTakenCanContinue)
    {
        Condition = condition;
        Taken = taken;
        NotTaken = notTaken;
        Join = join;
        Preserved = preserved;
        TakenCanContinue = takenCanContinue;
        NotTakenCanContinue = notTakenCanContinue;
        Parameters = StackSignature.Suffix(condition.After!, preserved);
        Results = join is null ? [] : StackSignature.Suffix(join.Before!, preserved);
    }

    /// <summary>The conditional branch; its operands are consumed before entering either arm.</summary>
    public InstructionNode Condition { get; }
    /// <summary>The arm executed when the source branch is taken.</summary>
    public SequenceBlock Taken { get; }
    /// <summary>The source branch's fallthrough arm.</summary>
    public SequenceBlock NotTaken { get; }
    /// <summary>Whether the taken arm can reach the common continuation.</summary>
    public bool TakenCanContinue { get; }
    /// <summary>Whether the fallthrough arm can reach the common continuation.</summary>
    public bool NotTakenCanContinue { get; }
    /// <summary>The common continuation, or null when both arms transfer control out of the conditional.</summary>
    public InstructionNode? Join { get; }
    /// <summary>The untouched stack prefix below this block's inputs and outputs.</summary>
    public StackState Preserved { get; }
    /// <summary>Consumed stack suffix, ordered bottom to top, excluding the condition.</summary>
    public IReadOnlyList<StackEntry> Parameters { get; }
    /// <summary>Produced stack suffix, ordered bottom to top.</summary>
    public IReadOnlyList<StackEntry> Results { get; }

}
