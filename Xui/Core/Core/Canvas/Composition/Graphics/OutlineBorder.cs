namespace Xui.Core.Canvas.Composition.Graphics;

/// <summary>A solid border following the inside of a surface's rounded bounds.</summary>
public readonly record struct OutlineBorder(Color Color, nfloat Width)
{
    /// <summary>Checks a finite, nonnegative width.</summary>
    public void Validate()
    {
        if (!nfloat.IsFinite(Width) || Width < 0) throw new ArgumentOutOfRangeException(nameof(Width));
    }
}
