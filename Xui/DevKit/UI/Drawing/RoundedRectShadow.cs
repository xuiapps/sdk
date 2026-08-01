using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.DevKit.UI.Drawing;

/// <summary>Draws a soft rounded-rectangle outline shadow using only gradient-filled geometry.</summary>
public static class RoundedRectShadow
{
    /// <summary>Draws a shadow outside the supplied rounded rectangle.</summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="bounds">The rectangle whose interior remains unpainted.</param>
    /// <param name="radius">Corner radii, clamped to fit the rectangle.</param>
    /// <param name="extent">Distance from the outline to the transparent outer edge.</param>
    /// <param name="color">Shadow color and opacity at the outline.</param>
    /// <remarks>
    /// Uses up to four strips and four quarter rings, without a bitmap, blur pass, or extra clip.
    /// Uniform corners share one radial gradient; all strips share one linear gradient.
    /// This is a geometric falloff, not a Gaussian blur. Canvas state is restored, but the current
    /// path is replaced. The caller must include the extent in its culling bounds.
    /// </remarks>
    public static void Draw(IContext context, Rect bounds, CornerRadius radius, nfloat extent, Color color)
    {
        if (!nfloat.IsFinite(extent) || extent < 0)
            throw new ArgumentOutOfRangeException(nameof(extent));
        if (extent == 0 || color.Alpha <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        radius = RoundedRect.Clamp(bounds.Size, CornerRadius.Max(radius, CornerRadius.Zero));
        var middle = new Color(color.Red, color.Green, color.Blue, color.Alpha * 0.25f);
        var transparent = new Color(color.Red, color.Green, color.Blue, 0);
        context.Save();
        try
        {
            // Local X points outwards; local Y runs along the straight edge.
            context.SetFill(new LinearGradient((0, 0), (extent, 0),
                [new(0, color), new(0.5f, middle), new(1, transparent)]));
            DrawStrip(context, (bounds.Left + radius.TopLeft, bounds.Top), -nfloat.Pi / 2,
                bounds.Width - radius.TopLeft - radius.TopRight, extent);
            DrawStrip(context, (bounds.Right, bounds.Top + radius.TopRight), 0,
                bounds.Height - radius.TopRight - radius.BottomRight, extent);
            DrawStrip(context, (bounds.Right - radius.BottomRight, bounds.Bottom), nfloat.Pi / 2,
                bounds.Width - radius.BottomLeft - radius.BottomRight, extent);
            DrawStrip(context, (bounds.Left, bounds.Bottom - radius.BottomLeft), nfloat.Pi,
                bounds.Height - radius.TopLeft - radius.BottomLeft, extent);

            nfloat lastRadius = -1;
            DrawCorner(context, (bounds.Left + radius.TopLeft, bounds.Top + radius.TopLeft),
                radius.TopLeft, extent, color, middle, transparent, nfloat.Pi, (0, -1), ref lastRadius);
            DrawCorner(context, (bounds.Right - radius.TopRight, bounds.Top + radius.TopRight),
                radius.TopRight, extent, color, middle, transparent, 3 * nfloat.Pi / 2, (1, 0), ref lastRadius);
            DrawCorner(context, (bounds.Right - radius.BottomRight, bounds.Bottom - radius.BottomRight),
                radius.BottomRight, extent, color, middle, transparent, 0, (0, 1), ref lastRadius);
            DrawCorner(context, (bounds.Left + radius.BottomLeft, bounds.Bottom - radius.BottomLeft),
                radius.BottomLeft, extent, color, middle, transparent, nfloat.Pi / 2, (-1, 0), ref lastRadius);
        }
        finally
        {
            context.Restore();
        }
    }

    private static void DrawStrip(IContext context, Point origin, nfloat angle, nfloat length, nfloat extent)
    {
        if (length <= 0) return;
        context.Save();
        try
        {
            context.Translate((origin.X, origin.Y));
            if (angle != 0) context.Rotate(angle);
            context.FillRect(new Rect(0, 0, extent, length));
        }
        finally { context.Restore(); }
    }

    private static void DrawCorner(IContext context, Point center, nfloat radius, nfloat extent,
        Color color, Color middle, Color transparent, nfloat startAngle, Point endDirection, ref nfloat lastRadius)
    {
        var outerRadius = radius + extent;
        if (lastRadius != radius)
        {
            context.SetFill(new RadialGradient((0, 0), outerRadius,
                [new(0, color), new(radius / outerRadius, color),
                 new((radius + extent * 0.5f) / outerRadius, middle), new(1, transparent)]));
            lastRadius = radius;
        }
        context.Save();
        try
        {
            context.Translate((center.X, center.Y));
            var endAngle = startAngle + nfloat.Pi / 2;
            context.BeginPath();
            context.Arc((0, 0), outerRadius, startAngle, endAngle);
            context.LineTo((endDirection.X * radius, endDirection.Y * radius));
            if (radius > 0)
                context.Arc((0, 0), radius, endAngle, startAngle, Winding.CounterClockWise);
            context.ClosePath();
            context.Fill();
        }
        finally { context.Restore(); }
    }
}
