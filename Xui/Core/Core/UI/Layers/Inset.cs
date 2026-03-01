// File: Xui/Core/UI/Layers/Inset.cs
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

public struct Inset<TChild> : IContainer<TChild>
    where TChild : struct, ILayer
{
    public Frame Value;

    public TChild Child;

    public Inset(Frame value)
    {
        this.Value = value;
        this.Child = default;
    }

    public LayoutGuide Update(LayoutGuide guide)
    {
        // Measure: shrink available size, expand desired size
        if (guide.IsMeasure)
        {
            var inner = guide;
            inner.AvailableSize = Size.Max((0, 0), guide.AvailableSize - this.Value);

            inner = this.Child.Update(inner);

            guide.DesiredSize = inner.DesiredSize + this.Value;
        }

        // Arrange: assign child rect to inner rect
        if (guide.IsArrange)
        {
            var inner = guide;
            inner.ArrangedRect = guide.ArrangedRect - this.Value;
            this.Child.Update(inner);
        }

        // Render/Animate: forward with same inner rect
        if (guide.IsRender || guide.IsAnimate)
        {
            var inner = guide;
            inner.ArrangedRect = guide.ArrangedRect - this.Value;
            this.Child.Update(inner);
        }

        return guide;
    }
}