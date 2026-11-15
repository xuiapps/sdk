using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

/// <summary>
/// Hosts one child and presents it through an affine transform without changing
/// its layout rectangle. Input and render culling are mapped through the inverse
/// transform so a translated or scaled child remains interactive and cullable.
/// </summary>
public class TransformView : View
{
    private View? content;
    private AffineTransform transform = AffineTransform.Identity;

    /// <summary>Gets or sets the single child presented by this host.</summary>
    public View? Content
    {
        get => content;
        set => this.SetProtectedChild(ref content, value);
    }

    /// <summary>
    /// Gets or sets the transform from this view's layout coordinates into its
    /// parent presentation coordinates.
    /// </summary>
    public AffineTransform Transform
    {
        get => transform;
        set
        {
            if (SameTransform(transform, value))
                return;

            transform = value;
            this.InvalidateHitTest();
            this.InvalidateRender();
        }
    }

    /// <inheritdoc/>
    public override int Count => content is null ? 0 : 1;

    /// <inheritdoc/>
    public override View this[int index] => index == 0 && content is not null
        ? content : throw new IndexOutOfRangeException();

    /// <inheritdoc/>
    public override Rect CullingFrame => TransformBounds(transform, this.Frame);

    /// <inheritdoc/>
    public override Point TransformPoint(Point point) => transform.Inverse * point;

    /// <inheritdoc/>
    public override Point InverseTransformPoint(Point point) => transform * point;

    /// <inheritdoc/>
    public override Rect TransformRect(Rect rect) => TransformBounds(transform.Inverse, rect);

    /// <inheritdoc/>
    public override Rect InverseTransformRect(Rect rect) => TransformBounds(transform, rect);

    /// <inheritdoc/>
    public override bool HitTest(Point point) => content is not null && this.Frame.Contains(point);

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        context.Save();
        try
        {
            context.Transform(transform);
            this.RenderTransformedContent(context);
        }
        finally
        {
            context.Restore();
        }
    }

    /// <summary>Renders content after <see cref="Transform"/> has been applied.</summary>
    protected virtual void RenderTransformedContent(IContext context) => base.RenderCore(context);

    private static bool SameTransform(in AffineTransform left, in AffineTransform right) =>
        left.A == right.A && left.B == right.B && left.C == right.C &&
        left.D == right.D && left.Tx == right.Tx && left.Ty == right.Ty;

    private static Rect TransformBounds(in AffineTransform matrix, Rect rect)
    {
        var topLeft = matrix * rect.TopLeft;
        var topRight = matrix * rect.TopRight;
        var bottomRight = matrix * rect.BottomRight;
        var bottomLeft = matrix * rect.BottomLeft;

        var left = nfloat.Min(nfloat.Min(topLeft.X, topRight.X), nfloat.Min(bottomRight.X, bottomLeft.X));
        var top = nfloat.Min(nfloat.Min(topLeft.Y, topRight.Y), nfloat.Min(bottomRight.Y, bottomLeft.Y));
        var right = nfloat.Max(nfloat.Max(topLeft.X, topRight.X), nfloat.Max(bottomRight.X, bottomLeft.X));
        var bottom = nfloat.Max(nfloat.Max(topLeft.Y, topRight.Y), nfloat.Max(bottomRight.Y, bottomLeft.Y));
        return new Rect(left, top, right - left, bottom - top);
    }
}
