using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Canvas.Tests;

/// <summary>
/// Demonstrates <see cref="IImageDrawingContext.DrawImage"/> — draws a loaded image
/// scaled to fit the canvas area with a small margin.
/// </summary>
public class DrawImageTest : View, ICanvasDrawingExample
{
    Rect ICanvasDrawingExample.GetArtworkBounds(IMeasureContext context)
    {
        if (image is not null && image.Size != Size.Empty)
        {
            var scale = NFloat.Min(580 / image.Size.Width, 280 / image.Size.Height);
            var size = new Size(image.Size.Width * scale, image.Size.Height * scale);
            return new Rect(300 - size.Width / 2, 150 - size.Height / 2, size.Width, size.Height);
        }
        return new Rect(160, 10, 280, 280);
    }

    private IImage? image;

    private const string ImagePath = "asset://test-512.png";

    protected override void OnAttach(ref AttachEventRef e)
    {
        image = this.GetService<IImage>();
        image?.Load(ImagePath);
    }

    protected override void OnDetach(ref DetachEventRef e)
    {
        image = null;
    }

    protected override void RenderCore(IContext context)
    {
        NFloat margin = 10f;

        if (image is not null && image.Size != Size.Empty)
        {
            var scale = NFloat.Min(
                (this.Frame.Width - margin * 2) / image.Size.Width,
                (this.Frame.Height - margin * 2) / image.Size.Height);
            var size = new Size(image.Size.Width * scale, image.Size.Height * scale);
            var dest = new Rect(
                this.Frame.Center.X - size.Width / 2,
                this.Frame.Center.Y - size.Height / 2,
                size.Width,
                size.Height);
            context.DrawImage(image, dest);
        }
        else
        {
            var side = NFloat.Min(this.Frame.Width, this.Frame.Height) - margin * 2;
            var dest = new Rect(
                this.Frame.Center.X - side / 2,
                this.Frame.Center.Y - side / 2,
                side,
                side);
            context.SetFill(LightGray);
            context.FillRect(dest);
        }
    }
}
