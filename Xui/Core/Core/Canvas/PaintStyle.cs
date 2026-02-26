namespace Xui.Core.Canvas;

/// <summary>
/// Specifies the type of brush or fill style used for painting shapes or strokes.
/// Helps distinguish between solid colors and gradient fills.
/// </summary>
public enum PaintStyle
{
    /// <summary>
    /// A single, uniform color.
    /// </summary>
    SolidColor = 0,

    /// <summary>
    /// A linear gradient that transitions colors along a straight line.
    /// </summary>
    LinearGradient = 1,

    /// <summary>
    /// A radial gradient that transitions colors outward in a circular or elliptical shape.
    /// </summary>
    RadialGradient = 2,

    /// <summary>
    /// A repeating bitmap pattern (equivalent to <c>createPattern(image, "repeat")</c>).
    /// </summary>
    BitmapBrush = 3
}
