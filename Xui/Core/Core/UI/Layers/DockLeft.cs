// File: Xui/Core/UI/Layers/DockLeft.cs
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A two-child horizontal layout layer. The left child is measured at its natural (desired) width;
/// the right child fills the remaining horizontal space. Both children receive the full allocated height.
/// Use <see cref="Gap"/> to insert horizontal space between the two children.
/// </summary>
public struct DockLeft<TLeft, TRight> : ILayer
    where TLeft : struct, ILayer
    where TRight : struct, ILayer
{
    public TLeft Left;
    public TRight Right;

    /// <summary>Horizontal gap in pixels between the left and right children.</summary>
    public nfloat Gap;

    // Stored from the last Measure pass so Arrange/Render can split the rect correctly.
    private nfloat _leftWidth;

    public LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsMeasure)
        {
            var leftGuide = guide;
            leftGuide = Left.Update(leftGuide);
            _leftWidth = leftGuide.DesiredSize.Width;

            var rightGuide = guide;
            rightGuide.AvailableSize = new Size(
                guide.AvailableSize.Width - _leftWidth - Gap,
                guide.AvailableSize.Height);
            rightGuide = Right.Update(rightGuide);

            var height = leftGuide.DesiredSize.Height >= rightGuide.DesiredSize.Height
                ? leftGuide.DesiredSize.Height
                : rightGuide.DesiredSize.Height;

            guide.DesiredSize = new Size(_leftWidth + Gap + rightGuide.DesiredSize.Width, height);
        }

        if (guide.IsArrange)
        {
            var rect = guide.ArrangedRect;

            var leftGuide = guide;
            leftGuide.ArrangedRect = new Rect(rect.X, rect.Y, _leftWidth, rect.Height);
            Left.Update(leftGuide);

            var rightGuide = guide;
            rightGuide.ArrangedRect = new Rect(rect.X + _leftWidth + Gap, rect.Y, rect.Width - _leftWidth - Gap, rect.Height);
            Right.Update(rightGuide);
        }

        if (guide.IsRender)
        {
            var rect = guide.ArrangedRect;

            var leftGuide = guide;
            leftGuide.ArrangedRect = new Rect(rect.X, rect.Y, _leftWidth, rect.Height);
            Left.Update(leftGuide);

            var rightGuide = guide;
            rightGuide.ArrangedRect = new Rect(rect.X + _leftWidth + Gap, rect.Y, rect.Width - _leftWidth - Gap, rect.Height);
            Right.Update(rightGuide);
        }

        if (guide.IsAnimate && !guide.IsRender && !guide.IsArrange && !guide.IsMeasure)
        {
            Left.Update(guide);
            Right.Update(guide);
        }

        return guide;
    }
}
