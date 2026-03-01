// File: Xui/Core/UI/Layers/Tick.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A leaf layer that draws a checkmark path within its arranged rect.
/// Set <see cref="Color"/> to a transparent color to render nothing.
/// </summary>
public struct Tick : ILeaf
{
    public Color Color;
    public nfloat StrokeWidth;

    public LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsMeasure)
        {
            // Square: use the smaller available dimension
            var side = guide.AvailableSize.Width < guide.AvailableSize.Height
                ? guide.AvailableSize.Width
                : guide.AvailableSize.Height;
            guide.DesiredSize = new Size(side, side);
        }

        if (guide.IsRender && !this.Color.IsTransparent)
        {
            var ctx = guide.RenderContext!;
            var r = guide.ArrangedRect;

            // Checkmark: bottom-left bend at ~42% x, 76% y; tip at ~82% x, 24% y
            ctx.BeginPath();
            ctx.MoveTo(new Point(r.X + r.Width * 0.18f, r.Y + r.Height * 0.52f));
            ctx.LineTo(new Point(r.X + r.Width * 0.42f, r.Y + r.Height * 0.76f));
            ctx.LineTo(new Point(r.X + r.Width * 0.82f, r.Y + r.Height * 0.24f));
            ctx.LineWidth = this.StrokeWidth;
            ctx.LineCap = LineCap.Round;
            ctx.LineJoin = LineJoin.Round;
            ctx.SetStroke(this.Color);
            ctx.Stroke();
        }

        return guide;
    }
}
