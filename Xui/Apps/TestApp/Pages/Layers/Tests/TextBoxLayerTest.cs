using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Layers.Tests;

/// <summary>
/// Demonstrates <see cref="TextBox"/> rewritten as
/// <c>LayerView&lt;FocusBorderLayer&lt;TextInputLayer&gt;&gt;</c>.
/// Shows standard text input, password masking, and a digit-only filtered box.
/// </summary>
public class TextBoxLayerTest : View
{
    private readonly Label label1, label2, label3;
    private readonly TextBox box1, box2, box3;

    public override int Count => 6;
    public override View this[int index] => index switch
    {
        0 => label1,
        1 => box1,
        2 => label2,
        3 => box2,
        4 => label3,
        5 => box3,
        _ => throw new IndexOutOfRangeException(),
    };

    public TextBoxLayerTest()
    {
        label1 = new Label { Text = "Text", FontFamily = ["Inter"], FontWeight = FontWeight.Normal };
        box1   = new TextBox { Text = "Hello, layers!", FontWeight = FontWeight.Normal };

        label2 = new Label { Text = "Password", FontFamily = ["Inter"], FontWeight = FontWeight.Normal };
        box2   = new TextBox { IsPassword = true, FontWeight = FontWeight.Normal };

        label3 = new Label { Text = "Digits only", FontFamily = ["Inter"], FontWeight = FontWeight.Normal };
        box3   = new TextBox
        {
            Text        = "42",
            InputFilter = char.IsAsciiDigit,
            FontWeight = FontWeight.Normal
        };

        AddProtectedChild(label1);
        AddProtectedChild(box1);
        AddProtectedChild(label2);
        AddProtectedChild(box2);
        AddProtectedChild(label3);
        AddProtectedChild(box3);
    }

    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
    {
        NFloat labelH = 20;
        NFloat boxW   = availableSize.Width - 40;

        label1.Measure(new Size(boxW, labelH), context);
        box1.Measure(new Size(boxW, availableSize.Height), context);
        label2.Measure(new Size(boxW, labelH), context);
        box2.Measure(new Size(boxW, availableSize.Height), context);
        label3.Measure(new Size(boxW, labelH), context);
        box3.Measure(new Size(boxW, availableSize.Height), context);

        return availableSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat x    = rect.X + 20;
        NFloat w    = rect.Width - 40;
        NFloat y    = rect.Y + 20;
        NFloat gap  = 8;

        void Place(View v, NFloat h) { v.Arrange(new Rect(x, y, w, h), context); y += h + gap; }

        Place(label1, 20);
        Place(box1,   box1.Frame.Height > 0 ? box1.Frame.Height : 26);
        y += 8;
        Place(label2, 20);
        Place(box2,   box2.Frame.Height > 0 ? box2.Frame.Height : 26);
        y += 8;
        Place(label3, 20);
        Place(box3,   box3.Frame.Height > 0 ? box3.Frame.Height : 26);
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFill(new Color(0xF8, 0xF9, 0xFA, 0xFF));
        context.FillRect(Frame);
        base.RenderCore(context);
    }
}
