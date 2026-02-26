namespace Xui.Core.Canvas;

/// <summary>
/// Controls how an image pattern tiles, following the same values as the
/// <c>repetition</c> argument to the browser's <c>createPattern(image, repetition)</c>.
/// </summary>
public enum PatternRepeat
{
    /// <summary>Tiles in both directions.</summary>
    Repeat,

    /// <summary>Tiles horizontally only; clamped vertically.</summary>
    RepeatX,

    /// <summary>Tiles vertically only; clamped horizontally.</summary>
    RepeatY,

    /// <summary>Drawn once — no tiling in either direction.</summary>
    NoRepeat,
}
