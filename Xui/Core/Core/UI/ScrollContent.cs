using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

/// <summary>
/// Internal <see cref="TransformView"/> specialization for <see cref="ScrollView"/>.
/// It owns the scroll-coordinate transform so normal views need only understand
/// their own coordinate mapping.
/// </summary>
internal sealed class ScrollContent : TransformView
{
    private readonly ScrollView owner;

    public ScrollContent(ScrollView owner, View content)
    {
        this.owner = owner;
        this.Content = content;
    }

    public void UpdateTransform()
    {
        var offset = this.owner.ScrollOffset;
        this.Transform = AffineTransform.Translate(new Vector(-offset.X, -offset.Y));
    }

    public void ClearContent() => this.Content = null;
}
