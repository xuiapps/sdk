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
/// Grid demonstrating Auto track sizing.
/// Columns are Auto — each sizes to its content.
/// Rows are a mix of fixed and Auto.
/// </summary>
public class AutoTracksGridTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid grid;

    public override int Count => 1;
    public override View this[int index] => grid;

    public AutoTracksGridTest()
    {
        grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [Auto, Auto, Auto],
            TemplateRows = [Auto, 50, Auto],
            ColumnGap = 10,
            RowGap = 10,
            Content =
            [
                Cell("Short", new Color(0x5D, 0xAD, 0xE8, 0xFF), 1, 1),
                Cell("A longer label", new Color(0x52, 0xBE, 0x80, 0xFF), 1, 2),
                Cell("M", new Color(0xAF, 0x7A, 0xC5, 0xFF), 1, 3),
                Cell("Auto width", new Color(0xF0, 0xB2, 0x7A, 0xFF), 2, 1),
                Cell("50px row", new Color(0x85, 0xC1, 0xE9, 0xFF), 2, 2),
                Cell("Row 2", new Color(0x82, 0xE0, 0xAA, 0xFF), 2, 3),
                Cell("Last row", new Color(0xF1, 0x94, 0x8A, 0xFF), 3, 1),
                Cell("Auto row height", new Color(0xFA, 0xD7, 0xA0, 0xFF), 3, 2),
                Cell("End", new Color(0xA9, 0xCC, 0xE3, 0xFF), 3, 3),
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
        Padding = 10,
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
