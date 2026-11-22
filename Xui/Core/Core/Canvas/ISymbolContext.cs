using Xui.Core.Canvas.Symbols;

namespace Xui.Core.Canvas;

/// <summary>Defines reusable vector drawings independently of their placements.</summary>
/// <example>
/// <code>
/// using (var symbol = context.Symbol(new SymbolOptions { Key = chair, Bounds = (0, 0, 40, 40) }))
/// {
///     if (symbol.NeedsDrawing)
///     {
///         symbol.Context.SetFill(Colors.Gray);
///         symbol.Context.FillRect((4, 4, 32, 32));
///     }
/// }
/// context.Save();
/// context.Translate((100, 50));
/// context.UseSymbol(chair);
/// context.Restore();
/// </code>
/// </example>
/// <remarks>
/// Definitions start with default pen, alpha and text state. Placements inherit the current
/// transform, clip and brightness, and restore enclosing paint/text state afterwards.
/// Use relative transforms inside definitions. Native view groups, GPU surfaces, absolute
/// SetTransform and nested symbols are not supported inside a symbol. On recording failure,
/// call the scope's Cancel method before disposing it. Cache storage follows reference lifetime;
/// ReleaseSymbol drops a key without invalidating already-recorded placements.
/// </remarks>
public interface ISymbolContext
{
    /// <summary>
    /// Opens a definition without drawing it. When NeedsDrawing is true, draw into the
    /// returned scope's Context, then dispose the scope before calling UseSymbol.
    /// Keys use reference identity. A new context may request the definition again.
    /// </summary>
    SymbolScope Symbol(scoped in SymbolOptions options) => SymbolCache.For(this).Begin(in options);

    /// <summary>Draws a completed definition under the current transform and clip.</summary>
    void UseSymbol(object key) => DrawSymbol(SymbolCache.For(this).Get(key));

    /// <summary>Forgets a definition. Previously recorded placements retain their immutable content.</summary>
    void ReleaseSymbol(object key) => SymbolCache.For(this).Remove(key);

    /// <summary>
    /// Backend playback entry point. Retained contexts record a reference; native contexts
    /// may compile it. The default implements vector command replay for any IContext.
    /// </summary>
    void DrawSymbol(SymbolContent content) => content.Replay((IContext)this);
}
