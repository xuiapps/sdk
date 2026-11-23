using System.Runtime.CompilerServices;

namespace Xui.Core.Canvas.Symbols;

internal sealed class SymbolCache(ISymbolContext context)
{
    private static readonly ConditionalWeakTable<ISymbolContext, SymbolCache> caches = new();
    private readonly ConditionalWeakTable<object, Entry> entries = new();
    internal sealed class Entry { public SymbolContent? Content; public bool Recording; }
    internal static SymbolCache For(ISymbolContext context) =>
        caches.TryGetValue(context, out var cache) ? cache : caches.GetValue(context, static c => new(c));
    internal SymbolScope Begin(scoped in SymbolOptions options)
    {
        options.Validate();
        if (!entries.TryGetValue(options.Key, out var entry))
            entry = entries.GetValue(options.Key, static _ => new());
        if (entry.Recording) throw new InvalidOperationException("This symbol definition is already open.");
        if (!options.HasUpdates && entry.Content is { } content && content.Bounds == options.Bounds)
            return new(entry, null);
        var recorder = new SymbolRecorder((IContext)context, options.Bounds);
        entry.Recording = true;
        return new(entry, recorder);
    }
    internal SymbolContent Get(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (!entries.TryGetValue(key, out var entry) || entry.Content is null || entry.Recording)
            throw new InvalidOperationException("Define and close the symbol before using it in this context.");
        return entry.Content;
    }
    internal void Remove(object key)
    {
        if (entries.TryGetValue(key, out var entry) && entry.Recording)
            throw new InvalidOperationException("Cannot release an open symbol definition.");
        entries.Remove(key);
    }
}
