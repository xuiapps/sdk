namespace Xui.Core.UI.Layers;

public struct Empty : ILayer
{
    /// <inheritdoc/>
    public bool IsEmpty => true;

    public LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsMeasure)
            guide.DesiredSize = default;

        return guide;
    }
}
