using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp.Pages.Canvas.Tests;

/// <summary>Font weight, italic style, color, and alignment using fixed drawing positions.</summary>
public sealed class TextTest : View, ICanvasDrawingExample
{
    // Fixed composition bounds: neither layout nor drawing needs MeasureText.
    Rect ICanvasDrawingExample.GetArtworkBounds(IMeasureContext context) => new(140, 0, 320, 300);

    protected override void RenderCore(IContext context)
    {
        context.Save();
        try
        {
            context.Translate(Frame.Center);
            context.TextAlign = TextAlign.Left;
            context.TextBaseline = TextBaseline.Top;

            context.SetFill(new Color(0x218B80FF));
            context.SetFont(new Font(12, "Inter", FontWeight.SemiBold));
            context.FillText("T Y P E  /  C O L O R", (-140, -133));

            context.SetFill(new Color(0x242529FF));
            context.SetFont(new Font(56, "Inter", FontWeight.Bold));
            context.FillText("Hello,", (-140, -106));
            context.SetFill(new Color(0x5065DFFF));
            context.FillText("canvas.", (-140, -45));

            context.SetFill(new Color(0x626875FF));
            context.SetFont(new Font(17, "Inter", FontWeight.Normal, FontStyle.Italic));
            context.FillText("Make every word count.", (-140, 32));

            context.SetFill(new Color(0xDCE0E9FF));
            context.FillRect(new Rect(-140, 68, 280, 1));

            context.SetFill(new Color(0x242529FF));
            context.SetFont(new Font(17, "Inter", FontWeight.Light));
            context.FillText("Light", (-140, 90));
            context.TextAlign = TextAlign.Center;
            context.SetFont(new Font(17, "Inter", FontWeight.Normal));
            context.FillText("Regular", (0, 90));
            context.TextAlign = TextAlign.Right;
            context.SetFont(new Font(17, "Inter", FontWeight.Bold));
            context.FillText("Bold", (140, 90));

            context.SetFill(new Color(0x7A808CFF));
            context.SetFont(new Font(10, "Inter", FontWeight.Normal));
            context.TextAlign = TextAlign.Left;
            context.FillText("LEFT", (-140, 116));
            context.TextAlign = TextAlign.Center;
            context.FillText("CENTER", (0, 116));
            context.TextAlign = TextAlign.Right;
            context.FillText("RIGHT", (140, 116));
        }
        finally { context.Restore(); }
    }
}
