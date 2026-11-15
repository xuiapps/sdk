namespace Xui.Core.Animation;

/// <summary>Bounded rubber-band displacement for dragging beyond a scrollable range.</summary>
public static class ElasticMotion
{
    /// <summary>Maps an unconstrained position to a progressively resisted position outside the bounds.</summary>
    public static nfloat Resist(nfloat position, nfloat min, nfloat max, nfloat viewport) =>
        Resist(position, min, max, viewport, 0.55f);

    /// <summary>Maps a position with a platform-specific resistance factor (smaller values feel firmer).</summary>
    public static nfloat Resist(nfloat position, nfloat min, nfloat max, nfloat viewport, nfloat resistance)
    {
        var bound = nfloat.Clamp(position, min, max);
        var distance = position - bound;
        var size = nfloat.Max(1, viewport);
        var factor = resistance > 0 ? resistance : (nfloat)0.55f;
        return bound + distance * factor * size / (size + factor * nfloat.Abs(distance));
    }

    /// <summary>Recovers the drag position so a new delta or a grabbed rebound does not jump.</summary>
    public static nfloat Unresist(nfloat position, nfloat min, nfloat max, nfloat viewport) =>
        Unresist(position, min, max, viewport, 0.55f);

    /// <summary>Inverts resistance using the same factor as the drag that produced the position.</summary>
    public static nfloat Unresist(nfloat position, nfloat min, nfloat max, nfloat viewport, nfloat resistance)
    {
        var bound = nfloat.Clamp(position, min, max);
        var distance = position - bound;
        var size = nfloat.Max(1, viewport);
        var factor = resistance > 0 ? resistance : (nfloat)0.55f;
        return bound + distance * size / (factor * nfloat.Max(0.001f, size - nfloat.Abs(distance)));
    }
}
