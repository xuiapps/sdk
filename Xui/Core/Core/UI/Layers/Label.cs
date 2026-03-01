// File: Xui/Core/UI/Layers/Label.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A single-line text leaf, mirroring UI.Label behavior.
/// </summary>
public struct Label : ILayer
{
    public string Text;

    public Color TextColor;

    public string[] FontFamily;
    public nfloat FontSize;
    public FontStyle FontStyle;
    public FontWeight FontWeight;
    public FontStretch FontStretch;
    public nfloat LineHeight;

    public LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsMeasure)
        {
            var context = guide.MeasureContext!;

            context.SetFont(new Font()
            {
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontStyle = this.FontStyle,
                FontWeight = this.FontWeight,
                LineHeight = this.LineHeight,
                FontStretch = this.FontStretch
            });

            var textSize = context.MeasureText(this.Text ?? string.Empty);
            guide.DesiredSize = textSize.Size;
        }

        if (guide.IsRender)
        {
            var context = guide.RenderContext!;

            context.SetFont(new Font()
            {
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontStyle = this.FontStyle,
                FontWeight = this.FontWeight,
                LineHeight = this.LineHeight,
                FontStretch = this.FontStretch
            });

            context.TextBaseline = TextBaseline.Top;
            context.TextAlign = TextAlign.Left;
            context.SetFill(this.TextColor);

            context.FillText(this.Text ?? string.Empty, guide.ArrangedRect.TopLeft);
        }

        return guide;
    }
}