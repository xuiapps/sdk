using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Symbols;

/// <summary>Identity and local drawing bounds of a reusable symbol.</summary>
public readonly struct SymbolOptions
{
    /// <summary>A stable reference, never a boxed value or a serialized identifier.</summary>
    public required object Key { get; init; }
    /// <summary>Local drawing bounds, including strokes. Content is clipped to these bounds.</summary>
    public required Rect Bounds { get; init; }
    /// <summary>True replaces the definition; false (the default) reuses it when available.</summary>
    public bool HasUpdates { get; init; }

    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(Key);
        if (Key.GetType().IsValueType) throw new ArgumentException("Use a stable reference as the symbol key.", nameof(Key));
        if (!nfloat.IsFinite(Bounds.X) || !nfloat.IsFinite(Bounds.Y) ||
            !nfloat.IsFinite(Bounds.Width) || !nfloat.IsFinite(Bounds.Height) || Bounds.Width <= 0 || Bounds.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(Bounds));
    }
}
