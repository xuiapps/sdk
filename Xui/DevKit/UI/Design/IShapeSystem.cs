using Xui.Core.Canvas;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// Provides corner-radius tokens scaled by <see cref="RoundnessFactor"/>.
/// </summary>
public interface IShapeSystem
{
    /// <summary>Global multiplier for all corner radii (default 1.0).</summary>
    nfloat RoundnessFactor { get; }

    /// <summary>0 pt — no rounding.</summary>
    CornerRadius None { get; }

    /// <summary>2 pt base, scaled by RoundnessFactor.</summary>
    CornerRadius ExtraSmall { get; }

    /// <summary>4 pt base, scaled by RoundnessFactor.</summary>
    CornerRadius Small { get; }

    /// <summary>8 pt base, scaled by RoundnessFactor.</summary>
    CornerRadius Medium { get; }

    /// <summary>12 pt base, scaled by RoundnessFactor.</summary>
    CornerRadius Large { get; }

    /// <summary>16 pt base, scaled by RoundnessFactor.</summary>
    CornerRadius ExtraLarge { get; }

    /// <summary>9999 pt — fully round (pill).</summary>
    CornerRadius Full { get; }
}
