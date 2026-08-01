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
/// Grid demonstrating MinMax and FitContent track sizes.
/// Column 1: minmax(60, 1fr) — at least 60px, grows to fill
/// Column 2: minmax(80, 160) — between 80 and 160px
/// Column 3: fit-content(120) — like max-content but capped at 120px
/// </summary>
public class MinMaxTracksTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid grid;

    public override int Count => 1;
    public override View this[int index] => grid;

    public MinMaxTracksTest()
    {
        grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [MinMax(60, 200), MinMax(80, 160), FitContent(120)],
            TemplateRows = [60, 60, 60],
            ColumnGap = 10,
            RowGap = 10,
            Content =
            [
                Cell("minmax\n(60,200)\n→ fills", new Color(0x4A, 0x90, 0xD9, 0xFF), 1, 1),
                Cell("minmax\n(80,160)", new Color(0x52, 0xBE, 0x80, 0xFF), 1, 2),
                Cell("fit-content\n(120)", new Color(0xAF, 0x7A, 0xC5, 0xFF), 1, 3),
                Cell("Row 2, Col 1", new Color(0x85, 0xC1, 0xE9, 0xFF), 2, 1),
                Cell("Row 2, Col 2", new Color(0x82, 0xE0, 0xAA, 0xFF), 2, 2),
                Cell("Row 2, Col 3", new Color(0xF1, 0x94, 0x8A, 0xFF), 2, 3),
                Cell("Row 3, Col 1", new Color(0xF0, 0xB2, 0x7A, 0xFF), 3, 1),
                Cell("Row 3, Col 2", new Color(0xFA, 0xD7, 0xA0, 0xFF), 3, 2),
                Cell("Row 3, Col 3", new Color(0xA9, 0xCC, 0xE3, 0xFF), 3, 3),
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
        BorderColor = new Color(0x00, 0x00, 0x00, 0x25),
        Padding = 6,
        Content = new GridCellLabel(text, 10)
    };

    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
    {
        grid.Measure(availableSize, context);
        return availableSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat padding = 24;
        grid.Arrange(new Rect(
            rect.X + padding,
            rect.Y + padding,
            rect.Width - padding * 2,
            rect.Height - padding * 2), context);
    }
}
