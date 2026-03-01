// File: Xui/Core/UI/Layers/Border.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// Draws a background + border around a single child. Padding is expressed by composing an <see cref="Inset{TChild}"/>
/// inside this border (and margin by composing Inset outside).
/// </summary>
public struct Border<TChild> : ILayer
    where TChild : struct, ILayer
{
    public Frame BorderThickness;
    public CornerRadius CornerRadius;

    public Color BackgroundColor;
    public Color BorderColor;

    public TChild Child;

    public LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsMeasure)
        {
            // Same behavior as UI.Border.MeasureCore, but without embedded Padding.
            var inner = guide;
            inner.AvailableSize = Size.Max(Size.Empty, guide.AvailableSize - this.BorderThickness);

            inner = this.Child.Update(inner);

            guide.DesiredSize = inner.DesiredSize + this.BorderThickness;
        }

        if (guide.IsArrange)
        {
            var inner = guide;
            inner.ArrangedRect = guide.ArrangedRect - this.BorderThickness;
            this.Child.Update(inner);
        }

        if (guide.IsRender)
        {
            var context = guide.RenderContext!;
            var frame = guide.ArrangedRect;

            // Background (same logic as UI.Border.RenderCore)
            if (!this.BackgroundColor.IsTransparent)
            {
                if (this.CornerRadius.IsZero)
                {
                    context.SetFill(this.BackgroundColor);
                    context.FillRect(frame - this.BorderThickness);
                }
                else if (this.BorderThickness.IsUniform)
                {
                    context.BeginPath();
                    var cornerRadius = CornerRadius.Max(CornerRadius.Zero, this.CornerRadius - this.BorderThickness.Left);
                    context.RoundRect(frame - this.BorderThickness, cornerRadius);
                    context.SetFill(this.BackgroundColor);
                    context.Fill();
                }
                else
                {
                    // TODO: Corners here are elliptical, calculate the shape...
                }
            }

            // Border (same logic as UI.Border.RenderCore)
            if (!this.BorderColor.IsTransparent)
            {
                if (this.BorderThickness.IsZero)
                {
                    // no-op
                }
                else if (this.BorderThickness.IsUniform)
                {
                    nfloat halfBorderThickness = this.BorderThickness.Left * (nfloat).5;
                    if (this.CornerRadius.IsZero)
                    {
                        context.SetStroke(this.BorderColor);
                        context.LineWidth = this.BorderThickness.Left;
                        context.SetStroke(this.BorderColor);
                        context.StrokeRect(frame - halfBorderThickness);
                    }
                    else
                    {
                        context.BeginPath();
                        context.RoundRect(frame - halfBorderThickness, this.CornerRadius - halfBorderThickness);
                        context.LineWidth = this.BorderThickness.Left;
                        context.SetStroke(this.BorderColor);
                        context.Stroke();
                    }
                }
                else
                {
                    if (this.CornerRadius.IsZero)
                    {
                        // TODO: Somewhat rectangular
                    }
                    else
                    {
                        // TODO: Outer RoundRect with the this.CornerRadius, inner edges are elliptical, or square...
                    }
                }
            }

            // Render child inside the border thickness
            var inner = guide;
            inner.ArrangedRect = frame - this.BorderThickness;
            this.Child.Update(inner);
        }

        if (guide.IsAnimate && !guide.IsRender && !guide.IsArrange && !guide.IsMeasure)
        {
            // If you're ever running a pure Animate pass, forward it.
            this.Child.Update(guide);
        }

        return guide;
    }
}