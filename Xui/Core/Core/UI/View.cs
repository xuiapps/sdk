using Xui.Core.Math2D;

namespace Xui.Core.UI;

/// <summary>
/// Base class for all UI elements in the Xui layout engine.
/// A view participates in layout, rendering, and input hit testing, and may contain child views.
/// </summary>
public partial class View
{
    /// <summary>
    /// An optional unique identifier for this view, used for lookup via
    /// <see cref="ViewExtensions.FindViewById"/>.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The set of class names assigned to this view, used for lookup via
    /// <see cref="ViewExtensions.FindViewsByClass"/>.
    /// </summary>
    public ClassNameCollection ClassName;

    /// <summary>
    /// The parent view in the visual hierarchy. This is set automatically when the view is added to a container.
    /// </summary>
    public View? Parent { get; internal set; }

    /// <summary>
    /// The border edge of this view in global coordinates relative to the top-left of the window.
    /// </summary>
    public Rect Frame { get; protected set; }

    /// <summary>
    /// The margin around this view. Margins participate in collapsed margin logic during layout,
    /// and are external spacing relative to the parent or surrounding siblings.
    /// </summary>
    public Frame Margin { get; set; } = (0, 0);

    /// <summary>
    /// The horizontal alignment of this view inside its layout anchor region.
    /// Used during layout when the view has remaining space within its container.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;

    /// <summary>
    /// The vertical alignment of this view inside its layout anchor region.
    /// Used during layout when the view has remaining space within its container.
    /// </summary>
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Stretch;

    /// <summary>
    /// The writing direction of this view, which determines the block or inline flow direction.
    /// Inherited from the parent flow context if set to <see cref="Direction.Inherit"/>.
    /// </summary>
    public Direction Direction { get; set; } = Direction.Inherit;

    /// <summary>
    /// The writing mode of this view (e.g. horizontal top-to-bottom or vertical right-to-left).
    /// Inherited from the parent if set to <see cref="WritingMode.Inherit"/>.
    /// </summary>
    public WritingMode WritingMode { get; set; } = WritingMode.HorizontalTB;

    /// <summary>
    /// Controls how the layout system treats this view's children.
    /// Can be inherited or explicitly overridden for advanced layout containers.
    /// </summary>
    public Flow Flow { get; set; } = Flow.Aware;

    /// <summary>
    /// The minimum width of the border edge box.
    /// </summary>
    public nfloat MinimumWidth { get; set; } = 0;

    /// <summary>
    /// The minimum height of the border edge box.
    /// </summary>
    public nfloat MinimumHeight { get; set; } = 0;

    /// <summary>
    /// The maximum width of the border edge box.
    /// </summary>
    public nfloat MaximumWidth { get; set; } = nfloat.PositiveInfinity;

    /// <summary>
    /// The maximum height of the border edge box.
    /// </summary>
    public nfloat MaximumHeight { get; set; } = nfloat.PositiveInfinity;

    /// <summary>
    /// Determines whether the given point (in local coordinates) hits this view's visual bounds.
    /// Used for input dispatch and hit testing.
    /// </summary>
    /// <param name="point">The point to test, relative to this view's coordinate space.</param>
    /// <returns><c>true</c> if the point is inside the view's frame; otherwise <c>false</c>.</returns>
    public virtual bool HitTest(Point point)
    {
        for (int i = this.Count - 1; i >= 0; i--)
            if (this[i].HitTest(point))
                return true;

        return this.Frame.Contains(point);
    }
}
