using System.IO;
using System.Runtime.InteropServices;
using Xui.Core;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Canvas.Tests;

/// <summary>
/// Demonstrates bitmap image loading via IBitmapContext.LoadBitmap, then:
///  - draws the raw image in the top-left corner via DrawImage
///  - fills a 5-pointed star path with the image as a repeating tile via SetFill(Bitmap)
/// </summary>
public class BitmapFillTest : View
{
    private Bitmap? bitmap;

    private static string ImagePath =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "test.png");

    protected override void OnAttach(ref AttachEventRef e)
    {
        this.TryFindParent<RootView>(out var root);
        this.bitmap = root?.Window?.BitmapContext?.LoadBitmap(ImagePath);
    }

    protected override void OnDetach(ref DetachEventRef e)
    {
        this.bitmap = null;
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        if (this.bitmap is null)
        {
            // Fallback: show a placeholder when running on a platform without IBitmapContext
            context.SetFill(LightGray);
            context.FillRect(new Rect(this.Frame.X + 10, this.Frame.Y + 10, 280, 130));
            return;
        }

        // ── 1. DrawImage – show the loaded image at natural size, top-left corner ────
        var imgW = (NFloat)System.Math.Min(this.bitmap.Width, 140);
        var imgH = (NFloat)System.Math.Min(this.bitmap.Height, 140);
        context.DrawImage(this.bitmap, new Rect(this.Frame.X + 10, this.Frame.Y + 10, imgW, imgH));

        // ── 2. Bitmap-filled star – centered in the lower half of the test area ──────
        NFloat cx = this.Frame.X + 150;
        NFloat cy = this.Frame.Y + 210;

        NFloat outerR = 100f;
        NFloat innerR = 40f;

        context.Save();
        context.Translate((cx, cy));

        // Build the 5-pointed star path
        context.BeginPath();
        NFloat tenthPi = NFloat.Pi / 5f;          // 36°
        for (int i = 0; i < 5; i++)
        {
            NFloat outerAngle = i * NFloat.Pi * 2f / 5f - NFloat.Pi / 2f;   // 72° steps, top-up
            NFloat innerAngle = outerAngle + tenthPi;                         // halfway between spikes

            var outerPt = new Point(NFloat.Cos(outerAngle) * outerR, NFloat.Sin(outerAngle) * outerR);
            var innerPt = new Point(NFloat.Cos(innerAngle) * innerR, NFloat.Sin(innerAngle) * innerR);

            if (i == 0)
                context.MoveTo(outerPt);
            else
                context.LineTo(outerPt);

            context.LineTo(innerPt);
        }
        context.ClosePath();

        // Fill with the bitmap as a repeating tile
        context.SetFill(this.bitmap);
        context.Fill(FillRule.NonZero);

        // Stroke the outline on top
        context.SetStroke(new Color(0x00, 0x00, 0x00, 0xCC));
        context.LineWidth = 2f;
        context.Stroke();

        context.Restore();
    }
}
