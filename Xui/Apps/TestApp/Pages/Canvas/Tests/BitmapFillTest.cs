using System.IO;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Canvas.Tests;

/// <summary>
/// Demonstrates image loading via <see cref="IImage.Load"/> and drawing via DrawImage.
/// </summary>
public class BitmapFillTest : View
{
    private IImage? image;

    private static string ImagePath =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "test.png");

    protected override void OnAttach(ref AttachEventRef e)
    {
        image = this.GetService(typeof(IImage)) as IImage;
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

        if (image is null || image.Size == Size.Empty)
        {
            // Fallback: placeholder when image not loaded or platform without IImage service
            context.SetFill(LightGray);
            context.FillRect(new Rect(this.Frame.X + 10, this.Frame.Y + 10, 280, 130));
            return;
        }

        var imgW = (NFloat)System.Math.Min((double)image.Size.Width, 280);
        var imgH = (NFloat)System.Math.Min((double)image.Size.Height, 280);
        context.DrawImage(image, new Rect(this.Frame.X + 10, this.Frame.Y + 10, imgW, imgH));
    }
}
