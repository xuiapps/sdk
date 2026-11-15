namespace Xui.Core.Canvas.Filter;

/// <summary>Multiplies paint RGB channels, preserving alpha. One is unchanged; zero is black.</summary>
public readonly struct Brightness
{
    /// <summary>The nonnegative, finite RGB multiplier.</summary>
    public nfloat Amount { get; }

    /// <summary>Creates a brightness adjustment.</summary>
    public Brightness(nfloat amount)
    {
        if (!nfloat.IsFinite(amount) || amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        Amount = amount;
    }
}
