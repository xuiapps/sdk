namespace Xui.Core.Canvas.Symbols;

/// <summary>A stack-only definition scope. Do not copy a live scope. Dispose before placing the symbol.</summary>
public ref struct SymbolScope
{
    private SymbolCache.Entry? entry;
    private readonly SymbolRecorder? recorder;
    /// <summary>Whether the caller must supply drawing instructions.</summary>
    public bool NeedsDrawing => recorder is not null;
    /// <summary>The definition canvas; drawing here never draws into the surrounding scene.</summary>
    public IContext Context => recorder ?? throw new InvalidOperationException("The symbol is cached; check NeedsDrawing.");
    internal SymbolScope(SymbolCache.Entry entry, SymbolRecorder? recorder) { this.entry = entry; this.recorder = recorder; }
    /// <summary>Abandons a failed definition, preserving the previous completed content.</summary>
    public void Cancel()
    {
        if (entry is null || recorder is null) return;
        recorder.Dispose(); entry.Recording = false; entry = null;
    }
    /// <summary>Publishes balanced drawing instructions as immutable reusable content.</summary>
    public void Dispose()
    {
        if (entry is null || recorder is null) return;
        var target = entry; entry = null;
        try { target.Content = recorder.Complete(); }
        finally { target.Recording = false; recorder.Dispose(); }
    }
}
