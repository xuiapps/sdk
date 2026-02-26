using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

/// <summary>
/// A view that displays an image loaded from a file path via the window's <see cref="IImageFactory"/>.
/// The factory caches images and handles device-lost rehydration transparently — this view
/// holds its <see cref="IImage"/> reference for the lifetime of the activation.
/// </summary>
public class ImageView : View
{
    private string? source;
    private IImage? image;

    /// <summary>
    /// Gets or sets the file path of the image to display.
    /// Setting this property triggers an immediate load if the view is already active.
    /// </summary>
    public string? Source
    {
        get => source;
        set
        {
            source = value;
            LoadImage();
        }
    }

    protected override void OnActivate()
    {
        LoadImage();
    }

    protected override void OnDeactivate()
    {
        image = null;
    }

    private void LoadImage()
    {
        if (source == null || (this.Flags & ViewFlags.Active) == 0)
            return;

        if (!this.TryFindParent<RootView>(out var root))
            return;

        var factory = root.Window.ImageFactory;
        if (factory == null)
            return;

        image = factory.Load(source);
        InvalidateRender();
        InvalidateMeasure();
    }

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        if (image == null)
            LoadImage();

        if (image == null)
            return Size.Empty;

        var intrinsic = image.Size;
        if (availableBorderEdgeSize.Width <= 0 || availableBorderEdgeSize.Height <= 0)
            return intrinsic;

        nfloat scaleX = availableBorderEdgeSize.Width / intrinsic.Width;
        nfloat scaleY = availableBorderEdgeSize.Height / intrinsic.Height;
        nfloat scale = nfloat.Min(nfloat.Min(scaleX, scaleY), 1);
        return new Size(intrinsic.Width * scale, intrinsic.Height * scale);
    }

    protected override void RenderCore(IContext context)
    {
        if (image != null)
            context.DrawImage(image, this.Frame);

        base.RenderCore(context);
    }
}
