using Xui.Core.Math2D;

namespace Xui.Core.Canvas;

/// <summary>
/// Utility for clamping corner radii to fit within a rectangle, following the
/// CSS Backgrounds and Borders Module Level 3, Section 5.5 "Overlapping Curves".
/// When the sum of adjacent radii exceeds a side's length, all radii are scaled
/// down proportionally so that curves never overlap.
/// </summary>
public static class RoundedRect
{
    /// <summary>
    /// Returns a <see cref="CornerRadius"/> whose values are clamped so that adjacent
    /// corners do not overlap on any side of a rectangle of the given <paramref name="size"/>.
    /// <para>
    /// Algorithm: compute <c>f = min(1, L_i / S_i)</c> over all four sides, where
    /// <c>L_i</c> is the side length and <c>S_i</c> is the sum of the two corner radii
    /// on that side. Then multiply every radius by <c>f</c>.
    /// </para>
    /// </summary>
    /// <param name="size">The dimensions of the rectangle.</param>
    /// <param name="radius">The desired corner radii.</param>
    /// <returns>Corner radii scaled to fit within the rectangle.</returns>
    public static CornerRadius Clamp(Size size, CornerRadius radius)
    {
        nfloat f = 1;

        // Top edge: TopLeft + TopRight ≤ width
        var topSum = radius.TopLeft + radius.TopRight;
        if (topSum > 0)
            f = nfloat.Min(f, size.Width / topSum);

        // Right edge: TopRight + BottomRight ≤ height
        var rightSum = radius.TopRight + radius.BottomRight;
        if (rightSum > 0)
            f = nfloat.Min(f, size.Height / rightSum);

        // Bottom edge: BottomRight + BottomLeft ≤ width
        var bottomSum = radius.BottomRight + radius.BottomLeft;
        if (bottomSum > 0)
            f = nfloat.Min(f, size.Width / bottomSum);

        // Left edge: BottomLeft + TopLeft ≤ height
        var leftSum = radius.BottomLeft + radius.TopLeft;
        if (leftSum > 0)
            f = nfloat.Min(f, size.Height / leftSum);

        if (f >= 1)
            return radius;

        return new CornerRadius(
            radius.TopLeft * f,
            radius.TopRight * f,
            radius.BottomRight * f,
            radius.BottomLeft * f
        );
    }

    /// <summary>
    /// Returns a uniform radius clamped to fit within the given <paramref name="size"/>.
    /// Equivalent to <c>min(radius, width / 2, height / 2)</c>.
    /// </summary>
    /// <param name="size">The dimensions of the rectangle.</param>
    /// <param name="radius">The desired uniform corner radius.</param>
    /// <returns>The clamped radius value.</returns>
    public static nfloat Clamp(Size size, nfloat radius)
    {
        var max = nfloat.Min(size.Width, size.Height) / 2;
        return nfloat.Min(radius, max);
    }
}
