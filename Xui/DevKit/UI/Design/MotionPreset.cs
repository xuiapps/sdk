namespace Xui.DevKit.UI.Design;

/// <summary>
/// Controls animation timing and whether animations are enabled.
/// </summary>
public enum MotionPreset
{
    /// <summary>No animations — all transitions snap instantly.</summary>
    None,

    /// <summary>Short/snappy animations (50% of normal duration).</summary>
    Short,

    /// <summary>Normal animation timing (default).</summary>
    Normal,

    /// <summary>Slow/deliberate animations (200% of normal duration). Useful for review/accessibility.</summary>
    Long,
}
