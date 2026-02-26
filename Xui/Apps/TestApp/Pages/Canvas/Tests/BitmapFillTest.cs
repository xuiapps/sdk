using System.IO;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Canvas.Tests;

/// <summary>
/// Demonstrates image loading via <see cref="IImageFactory.Load"/> and drawing via DrawImage.
/// </summary>
public class BitmapFillTest : View
{
    private IImage? image;

    private static string ImagePath =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "test.png");

    protected override void OnAttach(ref AttachEventRef e)
    {
        this.TryFindParent<RootView>(out var root);
        this.image = root?.Window?.ImageFactory?.Load(ImagePath);
    }

    protected override void OnDetach(ref DetachEventRef e)
    {
        this.image = null;
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        if (this.image is null)
        {
            // Fallback: show a placeholder when running on a platform without IImageFactory
            context.SetFill(LightGray);
            context.FillRect(new Rect(this.Frame.X + 10, this.Frame.Y + 10, 280, 130));
            return;
        }

        var imgW = (NFloat)System.Math.Min((double)this.image.Size.Width, 280);
        var imgH = (NFloat)System.Math.Min((double)this.image.Size.Height, 280);
        context.DrawImage(this.image, new Rect(this.Frame.X + 10, this.Frame.Y + 10, imgW, imgH));
    }
}
