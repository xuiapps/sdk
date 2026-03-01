using System.IO;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

/// <summary>
/// Demonstrates <see cref="ImagePattern"/> fill: a 5-pointed star drawn with
/// <c>SetFill(new ImagePattern(image))</c> — the Canvas API equivalent of
/// <c>ctx.fillStyle = ctx.createPattern(img, "repeat")</c>.
/// </summary>
public class BitmapFillTest : View
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

        NFloat cx = this.Frame.X + 150;
        NFloat cy = this.Frame.Y + 150;

        context.Save();
        context.Translate((cx, cy));

        BuildStar(context, outerRadius: 120f, innerRadius: 50f, points: 5);

        if (image is not null && image.Size != Size.Empty)
        {
            context.SetFill(new ImagePattern(image));
            context.Fill();
        }
        else
        {
            // Fallback while image is loading or service unavailable
            context.SetFill(LightGray);
            context.Fill();
        }

        // Stroke outline on top
        BuildStar(context, outerRadius: 120f, innerRadius: 50f, points: 5);
        context.SetStroke(Black);
        context.LineWidth = 2f;
        context.Stroke();

        context.Restore();
    }

    private static void BuildStar(IContext context, NFloat outerRadius, NFloat innerRadius, int points)
    {
        context.BeginPath();
        NFloat step = NFloat.Pi * 2f / points;
        NFloat half = step / 2f;

        for (int i = 0; i < points; i++)
        {
            NFloat outerAngle = i * step - NFloat.Pi / 2f;
            NFloat innerAngle = outerAngle + half;

            var outer = new Point(NFloat.Cos(outerAngle) * outerRadius, NFloat.Sin(outerAngle) * outerRadius);
            var inner = new Point(NFloat.Cos(innerAngle) * innerRadius, NFloat.Sin(innerAngle) * innerRadius);

            if (i == 0)
                context.MoveTo(outer);
            else
                context.LineTo(outer);

            context.LineTo(inner);
        }

        context.ClosePath();
    }
}
