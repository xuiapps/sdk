namespace Xui.DevKit.UI.Design;

/// <summary>
/// Concrete spacing system with passive (layout) and active (interactive) scales.
/// On Desktop, active == passive (tight buttons). On Touch/Mobile, active shifts up
/// by a fractional amount so interactive elements get generous sizing.
/// </summary>
internal class XuiSpacingSystem : ISpacingSystem
{
    // Base scale values (4-pt grid)
    private static readonly nfloat[] Scale = [4, 8, 12, 16, 24, 32];
    //                                        S  M   L  XL XXL XXXL

    public XuiSpacingSystem(SizingPreset preset)
    {
        Passive = new SpacingScale
        {
            S    = Scale[0],  // 4
            M    = Scale[1],  // 8
            L    = Scale[2],  // 12
            XL   = Scale[3],  // 16
            XXL  = Scale[4],  // 24
            XXXL = Scale[5],  // 32
        };

        // Fractional shift: Desktop=0, Desktop+Touch=1, Mobile=1.5
        nfloat shift = preset switch
        {
            SizingPreset.Desktop      => 0,
            SizingPreset.TouchEnabled => 1,
            SizingPreset.Mobile       => 1.5f,
            _                         => 0,
        };

        nfloat minHit = preset switch
        {
            SizingPreset.Desktop      => 20,
            SizingPreset.TouchEnabled => 36,
            SizingPreset.Mobile       => 44,
            _                         => 20,
        };

        Active = new SpacingScale
        {
            S    = Lerp(0, shift),
            M    = Lerp(1, shift),
            L    = Lerp(2, shift),
            XL   = Lerp(3, shift),
            XXL  = Lerp(4, shift),
            XXXL = Lerp(5, shift),
            MinHitTarget = minHit,
        };
    }

    /// <summary>
    /// Interpolates the scale at a fractional index (base + shift), clamped to scale bounds.
    /// </summary>
    private static nfloat Lerp(int baseIndex, nfloat shift)
    {
        var idx = (nfloat)baseIndex + shift;
        if (idx <= 0) return Scale[0];
        if (idx >= Scale.Length - 1) return Scale[Scale.Length - 1];

        int lo = (int)idx;
        int hi = lo + 1;
        nfloat t = idx - lo;
        return Scale[lo] * (1 - t) + Scale[hi] * t;
    }

    /// <inheritdoc/>
    public SpacingScale Passive { get; }

    /// <inheritdoc/>
    public SpacingScale Active { get; }

    /// <inheritdoc/>
    public nfloat None => 0;
}
