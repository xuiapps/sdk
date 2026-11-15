using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Graphics;

/// <summary>Portable surface approximation for contexts without native backdrop effects.</summary>
public static class SurfaceEffects
{
    /// <summary>Draws an outline shadow and translucent material without filtering foreground text.</summary>
    public static void Draw(IContext context, in GroupOptions options)
    {
        if (options.Bounds is not { } bounds) return;
        context.Save();
        try
        {
            if (options.Shadow is { } shadow && shadow.Color.Alpha > 0)
            {
                const int bands = 12;
                for (int i = bands; i >= 1; i--)
                {
                    var spread = shadow.Radius * i / bands;
                    context.BeginPath();
                    context.RoundRect(new Rect(bounds.X + shadow.Offset.X - spread, bounds.Y + shadow.Offset.Y - spread,
                        bounds.Width + 2 * spread, bounds.Height + 2 * spread), options.CornerRadius + spread);
                    context.SetFill(new Color(shadow.Color.Red, shadow.Color.Green, shadow.Color.Blue, shadow.Color.Alpha / bands));
                    context.Fill();
                }
            }
            if (options.Backdrop != BackdropMaterial.None)
            {
                context.BeginPath(); context.RoundRect(bounds, options.CornerRadius);
                context.SetFill(new LinearGradient(bounds.TopLeft, bounds.BottomRight,
                    [new(0, new Color(1, 1, 1, (nfloat).88)), new(1, new Color((nfloat).86, (nfloat).9, 1, (nfloat).72))]));
                context.Fill();
                context.SetStroke(new Color(1, 1, 1, (nfloat).7)); context.LineWidth = 1; context.Stroke();
            }
            if (options.Background is { } background)
            {
                context.BeginPath(); context.RoundRect(bounds, options.CornerRadius);
                context.SetFill(background); context.Fill();
            }
            if (options.Border is { } border && border.Width > 0)
            {
                var width = nfloat.Min(border.Width, nfloat.Min(bounds.Width, bounds.Height));
                context.BeginPath();
                context.RoundRect(new Rect(bounds.X + width / 2, bounds.Y + width / 2,
                    bounds.Width - width, bounds.Height - width), nfloat.Max(0, options.CornerRadius - width / 2));
                context.SetStroke(border.Color); context.LineWidth = width; context.Stroke();
            }
        }
        finally { context.Restore(); }
    }
}
