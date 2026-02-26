using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

/// <summary>
/// A view that displays an image. Works like <c>HTMLImageElement</c>: it holds an
/// <see cref="IImage"/> handle (acquired from the service chain) and sets its source
/// via <see cref="IImage.Load"/>. The platform catalog handles caching and device-lost
/// rehydration transparently.
/// </summary>
public class ImageView : View
{
    private string? source;
    private IImage? image;

    /// <summary>
    /// Gets or sets the URI of the image to display.
    /// Setting this property triggers a load if the view is already active.
    /// </summary>
    public string? Source
    {
        get => source;
        set
        {
            source = value;
            if (image != null && source != null)
            {
                image.Load(source);
                InvalidateRender();
                InvalidateMeasure();
            }
        }
    }

    protected override void OnActivate()
    {
        image = this.GetService<IImage>();
        if (source != null)
        {
            image?.Load(source);
            InvalidateRender();
            InvalidateMeasure();
        }
    }

    protected override void OnDeactivate()
    {
        image = null;
    }

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        if (image == null)
            return Size.Empty;

        var intrinsic = image.Size;
        if (intrinsic == Size.Empty)
            return Size.Empty;

        if (availableBorderEdgeSize.Width <= 0 || availableBorderEdgeSize.Height <= 0)
            return intrinsic;

        nfloat scaleX = availableBorderEdgeSize.Width / intrinsic.Width;
        nfloat scaleY = availableBorderEdgeSize.Height / intrinsic.Height;
        nfloat scale = nfloat.Min(nfloat.Min(scaleX, scaleY), 1);
        return new Size(intrinsic.Width * scale, intrinsic.Height * scale);
    }

    protected override void RenderCore(IContext context)
    {
        if (image != null && image.Size != Size.Empty)
            context.DrawImage(image, this.Frame);

        base.RenderCore(context);
    }
}
