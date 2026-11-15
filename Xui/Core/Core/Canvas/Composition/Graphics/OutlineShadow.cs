using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Graphics;

/// <summary>A drop shadow cast by the group's rounded bounds, not by its glyphs or transparent pixels.</summary>
public readonly record struct OutlineShadow(Color Color, nfloat Radius, Vector Offset)
{
    /// <summary>Checks finite, nonnegative blur and finite offsets.</summary>
    public void Validate()
    {
        if (!nfloat.IsFinite(Radius) || Radius < 0 || !nfloat.IsFinite(Offset.X) || !nfloat.IsFinite(Offset.Y))
            throw new ArgumentOutOfRangeException(nameof(Radius));
    }
}
