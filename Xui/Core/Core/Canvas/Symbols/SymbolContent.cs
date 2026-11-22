using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Symbols;

/// <summary>
/// Immutable, shareable drawing instructions. Recorded placements retain this exact definition,
/// even when its key is subsequently updated. Storage is reclaimed when the last reference dies.
/// </summary>
public sealed class SymbolContent
{
    private readonly CanvasCommandBuffer commands;
    /// <summary>The local clip and native recording bounds.</summary>
    public Rect Bounds { get; }
    /// <summary>Number of recorded instructions, useful for diagnostics.</summary>
    public int CommandCount => commands.Count;
    internal SymbolContent(CanvasCommandBuffer commands, Rect bounds) { this.commands = commands; Bounds = bounds; }
    // Definitions can outlive their source context in retained display lists. Returning the
    // pools here avoids invalidating live placements when a definition is replaced/released.
    /// <summary>Returns pooled storage after all retained placements are released.</summary>
    ~SymbolContent() => commands.Dispose();

    /// <summary>
    /// Portable vector replay. Paint defaults belong to the definition; transform, clip,
    /// and brightness come from the placement. Save/Restore isolates changes to drawing state.
    /// The current path is consumed, as with other path drawing operations.
    /// </summary>
    public void Replay(IContext context)
    {
        context.Save();
        try
        {
            context.BeginPath(); context.Rect(Bounds); context.Clip(); context.BeginPath();
            commands.Replay(context);
        }
        finally
        {
            try { context.Restore(); context.BeginPath(); }
            finally { GC.KeepAlive(this); }
        }
    }
}
