using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.UI.Layout.Grid;

namespace Xui.Apps.TestApp.Pages.Grid.Tests;

/// <summary>
/// 3×3 grid with 12px column gaps and 8px row gaps between cells.
/// </summary>
public class GapsGridTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid grid;

    public override int Count => 1;
    public override View this[int index] => grid;

    public GapsGridTest()
    {
        grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [90, 90, 90],
            TemplateRows = [55, 55, 55],
            ColumnGap = 12,
            RowGap = 8,
            Content =
            [
                Cell("A", new Color(0x52, 0xBE, 0x80, 0xFF), 1, 1),
                Cell("B", new Color(0x5D, 0xAD, 0xE8, 0xFF), 1, 2),
                Cell("C", new Color(0xAF, 0x7A, 0xC5, 0xFF), 1, 3),
                Cell("D", new Color(0xF0, 0xB2, 0x7A, 0xFF), 2, 1),
                Cell("E", new Color(0x85, 0xC1, 0xE9, 0xFF), 2, 2),
                Cell("F", new Color(0x82, 0xE0, 0xAA, 0xFF), 2, 3),
                Cell("G", new Color(0xF1, 0x94, 0x8A, 0xFF), 3, 1),
                Cell("H", new Color(0xFA, 0xD7, 0xA0, 0xFF), 3, 2),
                Cell("I", new Color(0xA9, 0xCC, 0xE3, 0xFF), 3, 3),
            ]
        };
        AddProtectedChild(grid);
    }

    static Border Cell(string text, Color bg, int row, int col) => new Border
    {
        [RowStart] = row,
        [ColumnStart] = col,
        BackgroundColor = bg,
        CornerRadius = 6,
        Padding = 4,
        Content = new Label
        {
            Text = text,
            FontFamily = "Inter",
            FontSize = 14,
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
        // 3 cols × 90 + 2 gaps × 12 = 294; 3 rows × 55 + 2 gaps × 8 = 181
        NFloat gridW = 294;
        NFloat gridH = 181;
        NFloat x = rect.X + (rect.Width - gridW) / 2;
        NFloat y = rect.Y + (rect.Height - gridH) / 2;
        grid.Arrange(new Rect(x, y, gridW, gridH), context);
    }
}
