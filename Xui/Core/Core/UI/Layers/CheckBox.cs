// File: Xui/Core/UI/Layers/CheckBox.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A leaf layer that draws a checkbox control: a rounded square border with an optional
/// checkmark. Vertically centers itself within the allocated rect.
/// </summary>
public struct CheckBox : ILeaf
{
    /// <summary>Whether the checkbox is checked (draws the tick when true).</summary>
    public bool Checked;

    /// <summary>The side length of the checkbox square.</summary>
    public nfloat Size;

    public Color BackgroundColor;
    public Color BorderColor;
    public Color CheckColor;
    public nfloat CornerRadius;
    public nfloat StrokeWidth;

    public LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsMeasure)
        {
            guide.DesiredSize = new Size(this.Size, this.Size);
        }

        if (guide.IsRender)
        {
            var ctx = guide.RenderContext!;
            var r = guide.ArrangedRect;

            // Center the box vertically within the allocated rect
            var boxY = r.Y + (r.Height - this.Size) / 2;
            var box = new Rect(r.X, boxY, this.Size, this.Size);

            // Background
            if (!this.BackgroundColor.IsTransparent)
            {
                ctx.BeginPath();
                ctx.RoundRect(box, this.CornerRadius);
                ctx.SetFill(this.BackgroundColor);
                ctx.Fill();
            }

            // Border
            if (!this.BorderColor.IsTransparent && this.StrokeWidth > 0)
            {
                nfloat half = this.StrokeWidth / 2;
                ctx.BeginPath();
                ctx.RoundRect(box - half, this.CornerRadius - half);
                ctx.LineWidth = this.StrokeWidth;
                ctx.SetStroke(this.BorderColor);
                ctx.Stroke();
            }

            // Checkmark
            if (this.Checked && !this.CheckColor.IsTransparent)
            {
                var s = this.Size;
                var ox = r.X;
                var oy = boxY;
                ctx.BeginPath();
                ctx.MoveTo(new Point(ox + s * 0.18f, oy + s * 0.52f));
                ctx.LineTo(new Point(ox + s * 0.42f, oy + s * 0.76f));
                ctx.LineTo(new Point(ox + s * 0.82f, oy + s * 0.24f));
                ctx.LineWidth = this.StrokeWidth;
                ctx.LineCap = LineCap.Round;
                ctx.LineJoin = LineJoin.Round;
                ctx.SetStroke(this.CheckColor);
                ctx.Stroke();
            }
        }

        return guide;
    }
}
