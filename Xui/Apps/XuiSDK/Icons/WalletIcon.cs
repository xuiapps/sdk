using System.Runtime.InteropServices;
using Xui.Core.Canvas;

namespace Xui.Apps.XuiSDK.Icons;

public class WalletIcon : INavIcon
{
    public nfloat Selected { get; set; }
    public Color Color { get; set; }
    public Color SelectedColor { get; set; }

    public void Render(IContext context)
    {
        context.Save();
        context.BeginPath();

        context.LineWidth = NFloat.Lerp(1.25f, 2.25f, this.Selected);

        var stroke = new Xui.Core.Canvas.Color(
            NFloat.Lerp(this.Color.Red, this.SelectedColor.Red, this.Selected),
            NFloat.Lerp(this.Color.Green, this.SelectedColor.Green, this.Selected),
            NFloat.Lerp(this.Color.Blue, this.SelectedColor.Blue, this.Selected),
            NFloat.Lerp(this.Color.Alpha, this.SelectedColor.Alpha, this.Selected)
        );
        var fill = new Xui.Core.Canvas.Color(stroke.Red, stroke.Green, stroke.Blue, NFloat.Lerp(0f, stroke.Alpha, this.Selected));

        NFloat seven = NFloat.Lerp(7, 9, this.Selected);
        NFloat five = NFloat.Lerp(5, 6, this.Selected);

        NFloat oneTop = NFloat.Lerp(-2f, -4f, this.Selected);
        NFloat oneBottom = NFloat.Lerp(-2f, -0.5f, this.Selected);

        context.MoveTo((-seven, oneTop));
        context.ArcTo((-seven, -five), (seven, -five), 2);
        context.ArcTo((seven, -five), (seven, oneTop), 2);
        context.LineTo((seven, oneTop));
        context.ClosePath();

        context.MoveTo((-seven, oneBottom));
        context.LineTo((seven, oneBottom));
        context.ArcTo((seven, five), (-seven, five), 2);
        context.ArcTo((-seven, five), (-seven, oneBottom), 2);
        context.LineTo((-seven, oneBottom));
        context.ClosePath();

        context.Rect((NFloat.Lerp(-5, -7, this.Selected), NFloat.Lerp(2, 1, this.Selected), NFloat.Lerp(4, 8, this.Selected), NFloat.Lerp(0, 4, this.Selected)));
        context.ClosePath();

        context.SetStroke(stroke);
        context.Stroke();

        context.BeginPath();

        context.MoveTo((-seven, oneTop));
        context.ArcTo((-seven, -five), (seven, -five), 2);
        context.ArcTo((seven, -five), (seven, oneTop), 2);
        context.LineTo((seven, oneTop));
        context.ClosePath();

        context.MoveTo((-seven, oneBottom));
        context.LineTo((seven, oneBottom));
        context.ArcTo((seven, five), (-seven, five), 2);
        context.ArcTo((-seven, five), (-seven, oneBottom), 2);
        context.LineTo((-seven, oneBottom));
        context.ClosePath();

        context.Rect((NFloat.Lerp(-5, -7, this.Selected), NFloat.Lerp(2, 1, this.Selected), NFloat.Lerp(4, 8, this.Selected), NFloat.Lerp(0, 4, this.Selected)));
        context.ClosePath();

        context.SetFill(fill);
        context.Fill(FillRule.EvenOdd);

        context.Restore();
    }
}
