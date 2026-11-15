namespace Xui.Core.Canvas.Composition.Graphics;

/// <summary>An allocation-free, stack-only lifetime for a canvas composition group.</summary>
/// <remarks>
/// Use with a using declaration or statement. Scopes must close in reverse opening order;
/// do not copy a live scope. Balance all Save/Restore calls inside the scope before closing it.
/// </remarks>
public ref struct CompositionGroup
{
    private ICompositionContext? owner;

    /// <summary>Whether the caller must emit this group's content into the context.</summary>
    public bool NeedsRedraw { get; }

    /// <summary>Creates a scope for an already-opened group. Intended for context implementations.</summary>
    public CompositionGroup(ICompositionContext owner, bool needsRedraw)
    {
        ArgumentNullException.ThrowIfNull(owner);
        this.owner = owner;
        NeedsRedraw = needsRedraw;
    }

    /// <summary>Closes the group and restores its enclosing drawing state.</summary>
    public void Dispose()
    {
        if (owner is not { } context)
            return;
        context.EndGroup();
        owner = null;
    }
}
