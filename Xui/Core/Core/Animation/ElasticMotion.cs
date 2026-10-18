namespace Xui.Core.Animation;

/// <summary>Bounded rubber-band displacement for dragging beyond a scrollable range.</summary>
public static class ElasticMotion
{
    /// <summary>Maps an unconstrained position to a progressively resisted position outside the bounds.</summary>
    public static nfloat Resist(nfloat position, nfloat min, nfloat max, nfloat viewport)
    {
        var bound = nfloat.Clamp(position, min, max);
        var distance = position - bound;
        var size = nfloat.Max(1, viewport);
        return bound + distance * 0.55f * size / (size + 0.55f * nfloat.Abs(distance));
    }

    /// <summary>Recovers the drag position so a new delta or a grabbed rebound does not jump.</summary>
    public static nfloat Unresist(nfloat position, nfloat min, nfloat max, nfloat viewport)
    {
        var bound = nfloat.Clamp(position, min, max);
        var distance = position - bound;
        var size = nfloat.Max(1, viewport);
        return bound + distance * size / (0.55f * nfloat.Max(0.001f, size - nfloat.Abs(distance)));
    }
}
