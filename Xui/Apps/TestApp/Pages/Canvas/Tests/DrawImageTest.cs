using System.IO;
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
public class DrawImageTest : View
{
    private IImage? image;

    private static string ImagePath =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "test.png");

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
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat margin = 10f;
        var dest = new Rect(
            this.Frame.X + margin,
            this.Frame.Y + margin,
            this.Frame.Width  - margin * 2,
            this.Frame.Height - margin * 2);

        if (image is not null && image.Size != Size.Empty)
        {
            context.DrawImage(image, dest);
        }
        else
        {
            context.SetFill(LightGray);
            context.FillRect(dest);
        }
    }
}
