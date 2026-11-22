namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>All unique stack-type prefixes encountered while analyzing one method.</summary>
public sealed class StackTree
{
    private readonly List<StackState> states = new();

    public StackTree()
    {
        Empty = new StackState(this, 0, null, null);
        states.Add(Empty);
        States = states.AsReadOnly();
    }

    /// <summary>The shared empty root for this method.</summary>
    public StackState Empty { get; }
    /// <summary>States in creation order; IDs index this list.</summary>
    public IReadOnlyList<StackState> States { get; }

    internal StackState Create(StackState parent, StackEntry type)
    {
        var state = new StackState(this, states.Count, parent, type);
        states.Add(state);
        return state;
    }
}
