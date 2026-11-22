namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>An interned stack prefix. Reference identity identifies the entire stack within one method.</summary>
/// <remarks>The parent removes the top slot. Children append one type; there is at most one child per type.
/// The empty state has no parent or type. States use reference equality, not record equality.</remarks>
public sealed class StackState
{
    private readonly StackTree tree;
    private Dictionary<StackEntry, StackState>? children;

    internal StackState(StackTree tree, int id, StackState? parent, StackEntry? entry)
    {
        this.tree = tree;
        Id = id;
        Parent = parent;
        Entry = entry;
        Depth = parent is null ? 0 : parent.Depth + 1;
    }

    /// <summary>A method-local diagnostic identifier, assigned on first creation.</summary>
    public int Id { get; }
    /// <summary>The same stack without its top slot, or null for the empty state.</summary>
    public StackState? Parent { get; }
    /// <summary>The top slot's type, or null for the empty state.</summary>
    public StackType? Type => Entry?.Type;
    /// <summary>The full top-slot type, including struct identity or managed-address element type.</summary>
    public StackEntry? Entry { get; }
    /// <summary>The number of slots represented by this state.</summary>
    public int Depth { get; }
    /// <summary>Existing states obtained by pushing one type onto this state.</summary>
    public IReadOnlyCollection<StackState> Children => children is null ? Array.Empty<StackState>() : children.Values;

    /// <summary>Returns the existing child for this type, creating it only on the first push.</summary>
    public StackState Push(StackEntry type)
    {
        children ??= new();
        if (children.TryGetValue(type, out var child)) return child;
        child = tree.Create(this, type);
        children.Add(type, child);
        return child;
    }

    /// <summary>Returns the parent state without mutating this state.</summary>
    public StackState Pop() => Parent ?? throw new InvalidProgramException("Evaluation stack underflow.");
}
