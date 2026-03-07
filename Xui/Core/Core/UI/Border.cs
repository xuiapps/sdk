using Xui.Core.Math2D;
using Xui.Core.Canvas;
using Xui.Core.UI.Layer;

namespace Xui.Core.UI;

/// <summary>
/// A view that draws a background, border, and padding around a single child content view.
/// All rendering is owned by <see cref="BorderLayer{TChild}"/> composed with
/// <see cref="ContentLayer"/>. The child view lives in <c>Layer.Child.Child</c> and is
/// managed via the standard <see cref="View.SetProtectedChild{T}"/> lifecycle.
/// </summary>
public class Border : LayerView<View, BorderLayer<View, ContentLayer>>
{
    /// <summary>Gets or sets the content view displayed inside the border.</summary>
    public View? Content
    {
        get => Layer.Child.Child;
        set => SetProtectedChild(ref Layer.Child.Child, value);
    }

    /// <summary>Gets or sets the per-side border thickness.</summary>
    public Frame BorderThickness
    {
        get => Layer.BorderThickness;
        set => Layer.BorderThickness = value;
    }

    /// <summary>Gets or sets the corner radius.</summary>
    public CornerRadius CornerRadius
    {
        get => Layer.CornerRadius;
        set => Layer.CornerRadius = value;
    }

    /// <summary>Gets or sets the background fill color.</summary>
    public Color BackgroundColor
    {
        get => Layer.BackgroundColor;
        set => Layer.BackgroundColor = value;
    }

    /// <summary>Gets or sets the border stroke color.</summary>
    public Color BorderColor
    {
        get => Layer.BorderColor;
        set => Layer.BorderColor = value;
    }

    /// <summary>Gets or sets the padding between the border edge and the content.</summary>
    public Frame Padding
    {
        get => Layer.Padding;
        set => Layer.Padding = value;
    }

    /// <inheritdoc/>
    public override int Count => Layer.Child.Child is null ? 0 : 1;

    /// <inheritdoc/>
    public override View this[int index]
    {
        get
        {
            if (Layer.Child.Child is not null && index == 0)
                return Layer.Child.Child;
            throw new IndexOutOfRangeException();
        }
    }
}
