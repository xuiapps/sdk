using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.UI.Layout.Grid;

namespace Xui.Apps.TestApp.Pages.Grid.Tests;

/// <summary>
/// A simple 3×3 grid with all fixed pixel tracks and no gaps.
/// All nine cells are explicitly positioned by row and column.
/// </summary>
public class BasicFixedGridTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid grid;

    public override int Count => 1;
    public override View this[int index] => grid;

    static readonly Color[] palette =
    [
        new Color(0x4A, 0x90, 0xD9, 0xFF),
        new Color(0x5C, 0xC8, 0x5A, 0xFF),
        new Color(0xE8, 0x5D, 0x5D, 0xFF),
        new Color(0xF5, 0xA6, 0x23, 0xFF),
        new Color(0x9B, 0x59, 0xB6, 0xFF),
        new Color(0x1A, 0xBC, 0x9C, 0xFF),
        new Color(0xF3, 0x9C, 0x12, 0xFF),
        new Color(0x34, 0x98, 0xDB, 0xFF),
        new Color(0xE7, 0x4C, 0x3C, 0xFF),
    ];

    public BasicFixedGridTest()
    {
        grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [100, 100, 100],
            TemplateRows = [60, 60, 60],
            Content =
            [
                Cell("1, 1", palette[0], 1, 1),
                Cell("1, 2", palette[1], 1, 2),
                Cell("1, 3", palette[2], 1, 3),
                Cell("2, 1", palette[3], 2, 1),
                Cell("2, 2", palette[4], 2, 2),
                Cell("2, 3", palette[5], 2, 3),
                Cell("3, 1", palette[6], 3, 1),
                Cell("3, 2", palette[7], 3, 2),
                Cell("3, 3", palette[8], 3, 3),
            ]
        };
        AddProtectedChild(grid);
    }

    static Border Cell(string text, Color bg, int row, int col) => new Border
    {
        [RowStart] = row,
        [ColumnStart] = col,
        BackgroundColor = bg,
        BorderThickness = 1,
        BorderColor = new Color(0x00, 0x00, 0x00, 0x40),
        Padding = 4,
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
        // Center the fixed-size grid
        NFloat gridW = 300;
        NFloat gridH = 180;
        NFloat x = rect.X + (rect.Width - gridW) / 2;
        NFloat y = rect.Y + (rect.Height - gridH) / 2;
        grid.Arrange(new Rect(x, y, gridW, gridH), context);
    }
}
