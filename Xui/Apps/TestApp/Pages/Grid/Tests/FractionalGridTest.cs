using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Apps.TestApp.Pages.Grid.Tests;

/// <summary>
/// Grid with mixed track sizes: 80px fixed, 1fr, and 2fr columns.
/// The grid fills the available width so fractional units are visible.
/// Two rows with 1fr and 2fr heights.
/// </summary>
public class FractionalGridTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid grid;

    public override int Count => 1;
    public override View this[int index] => grid;

    public FractionalGridTest()
    {
        grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [80, Fr(1), Fr(2)],
            TemplateRows = [Fr(1), Fr(2)],
            ColumnGap = 8,
            RowGap = 8,
            Content =
            [
                Cell("80px\nfixed", new Color(0x5D, 0xAD, 0xE8, 0xFF), 1, 1),
                Cell("1fr", new Color(0x52, 0xBE, 0x80, 0xFF), 1, 2),
                Cell("2fr", new Color(0xAF, 0x7A, 0xC5, 0xFF), 1, 3),
                Cell("80px\n1fr row", new Color(0x85, 0xC1, 0xE9, 0xFF), 2, 1),
                Cell("1fr\n2fr row", new Color(0x82, 0xE0, 0xAA, 0xFF), 2, 2),
                Cell("2fr\n2fr row", new Color(0xF1, 0x94, 0x8A, 0xFF), 2, 3),
            ]
        };
        AddProtectedChild(grid);
    }

    static Border Cell(string text, Color bg, int row, int col) => new Border
    {
        [RowStart] = row,
        [ColumnStart] = col,
        BackgroundColor = bg,
        CornerRadius = 4,
        BorderThickness = 1,
        BorderColor = new Color(0x00, 0x00, 0x00, 0x30),
        Padding = 8,
        Content = new Label
        {
            Text = text,
            FontFamily = "Inter",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Middle,
        }
    };

    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
    {
        grid.Measure(availableSize, context);
        return availableSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat padding = 30;
        grid.Arrange(new Rect(
            rect.X + padding,
            rect.Y + padding,
            rect.Width - padding * 2,
            rect.Height - padding * 2), context);
    }
}
