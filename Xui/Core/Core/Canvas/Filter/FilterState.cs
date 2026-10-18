namespace Xui.Core.Canvas.Filter;

/// <summary>The paint color adjustment stored in a canvas state.</summary>
public enum FilterKind
{
    /// <summary>No color adjustment.</summary>
    None,
    /// <summary>Multiply RGB channels by the brightness amount.</summary>
    Brightness,
}

/// <summary>Owned, allocation-free filter state. The default value leaves colors unchanged.</summary>
public readonly struct FilterState
{
    /// <summary>The active adjustment.</summary>
    public FilterKind Kind { get; }

    /// <summary>The brightness multiplier, or one when no adjustment is active.</summary>
    public nfloat Amount => Kind == FilterKind.None ? 1 : amount;

    private readonly nfloat amount;

    /// <summary>Creates state for a brightness adjustment.</summary>
    public FilterState(Brightness brightness)
    {
        Kind = FilterKind.Brightness;
        amount = brightness.Amount;
    }

    /// <summary>Adjusts RGB, clamps each channel to [0,1], and preserves alpha.</summary>
    public Color Apply(Color color) => Kind == FilterKind.None ? color : new(
        nfloat.Clamp(color.Red * amount, 0, 1),
        nfloat.Clamp(color.Green * amount, 0, 1),
        nfloat.Clamp(color.Blue * amount, 0, 1),
        color.Alpha);
}
