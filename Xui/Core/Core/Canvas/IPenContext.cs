namespace Xui.Core.Canvas;

/// <summary>
/// Defines properties and methods for controlling stroke and fill styles in a 2D drawing context,
/// including line caps, joins, width, dashes, and brush settings for fill and stroke.
/// </summary>
public interface IPenContext
{
    /// <summary>
    /// Sets the global alpha value for all drawing operations.
    /// Range: 0.0 (fully transparent) to 1.0 (fully opaque).
    /// </summary>
    nfloat GlobalAlpha { set; }

    /// <summary>
    /// Sets the style of line caps used for strokes.
    /// </summary>
    LineCap LineCap { set; }

    /// <summary>
    /// Sets the style of line joins between segments.
    /// </summary>
    LineJoin LineJoin { set; }

    /// <summary>
    /// Sets the width of stroked lines, in user space units.
    /// </summary>
    nfloat LineWidth { set; }

    /// <summary>
    /// Sets the miter limit ratio for miter joins.
    /// If the ratio of miter length to line width exceeds this value, a bevel join is used instead.
    /// </summary>
    nfloat MiterLimit { set; }

    /// <summary>
    /// Sets the phase offset for the start of the dash pattern.
    /// </summary>
    nfloat LineDashOffset { set; }

    /// <summary>
    /// Sets the dash pattern used for stroking lines.
    /// Each element in the span represents a dash or gap length, alternating.
    /// </summary>
    /// <param name="segments">A sequence of dash and gap lengths.</param>
    void SetLineDash(ReadOnlySpan<nfloat> segments);

    /// <summary>
    /// Sets the stroke style to a solid color.
    /// </summary>
    /// <param name="color">The stroke color.</param>
    void SetStroke(Color color);

    /// <summary>
    /// Sets the stroke style to a linear gradient.
    /// </summary>
    /// <param name="linearGradient">The gradient to use for stroking paths.</param>
    void SetStroke(LinearGradient linearGradient);

    /// <summary>
    /// Sets the stroke style to a radial gradient.
    /// </summary>
    /// <param name="radialGradient">The gradient to use for stroking paths.</param>
    void SetStroke(RadialGradient radialGradient);

    /// <summary>
    /// Sets the fill style to a solid color.
    /// </summary>
    /// <param name="color">The fill color.</param>
    void SetFill(Color color);

    /// <summary>
    /// Sets the fill style to a linear gradient.
    /// </summary>
    /// <param name="linearGradient">The gradient to use for filling shapes.</param>
    void SetFill(LinearGradient linearGradient);

    /// <summary>
    /// Sets the fill style to a radial gradient.
    /// </summary>
    /// <param name="radialGradient">The gradient to use for filling shapes.</param>
    void SetFill(RadialGradient radialGradient);

    /// <summary>
    /// Sets the fill style to a tiled image pattern, equivalent to
    /// <c>ctx.fillStyle = ctx.createPattern(image, repetition)</c> in the browser.
    /// </summary>
    void SetFill(ImagePattern pattern);

    /// <summary>
    /// Sets the stroke style to a tiled image pattern.
    /// </summary>
    void SetStroke(ImagePattern pattern);

}
