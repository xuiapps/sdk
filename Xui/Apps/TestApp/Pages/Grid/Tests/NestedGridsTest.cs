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
/// An outer 2×2 grid where two of the cells contain inner grids.
/// Demonstrates that Grid can be nested as both container and child.
/// </summary>
public class NestedGridsTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid outerGrid;

    public override int Count => 1;
    public override View this[int index] => outerGrid;

    public NestedGridsTest()
    {
        // Top-left: plain colored cell
        var topLeft = new Border
        {
            [RowStart] = 1,
            [ColumnStart] = 1,
            BackgroundColor = new Color(0x4A, 0x90, 0xD9, 0xFF),
            CornerRadius = 6,
            Padding = 10,
            Content = new Label
            {
                Text = "Plain cell",
                FontFamily = "Inter",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
            }
        };

        // Top-right: inner 3×1 grid
        var innerGrid1 = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [Fr(1), Fr(1), Fr(1)],
            TemplateRows = [Fr(1)],
            ColumnGap = 4,
            Content =
            [
                SubCell("a", new Color(0xFA, 0xD7, 0xA0, 0xFF), 1, 1),
                SubCell("b", new Color(0xF0, 0xB2, 0x7A, 0xFF), 1, 2),
                SubCell("c", new Color(0xE5, 0x9B, 0x72, 0xFF), 1, 3),
            ]
        };
        var topRight = new Border
        {
            [RowStart] = 1,
            [ColumnStart] = 2,
            BackgroundColor = new Color(0xFD, 0xF2, 0xE9, 0xFF),
            CornerRadius = 6,
            BorderThickness = 1,
            BorderColor = new Color(0xCC, 0xCC, 0xCC, 0xFF),
            Padding = 8,
            Content = innerGrid1,
        };

        // Bottom-left: inner 2×2 grid
        var innerGrid2 = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [Fr(1), Fr(1)],
            TemplateRows = [Fr(1), Fr(1)],
            ColumnGap = 4,
            RowGap = 4,
            Content =
            [
                SubCell("1", new Color(0x82, 0xE0, 0xAA, 0xFF), 1, 1),
                SubCell("2", new Color(0x52, 0xBE, 0x80, 0xFF), 1, 2),
                SubCell("3", new Color(0x27, 0xAE, 0x60, 0xFF), 2, 1),
                SubCell("4", new Color(0x1E, 0x88, 0x45, 0xFF), 2, 2),
            ]
        };
        var bottomLeft = new Border
        {
            [RowStart] = 2,
            [ColumnStart] = 1,
            BackgroundColor = new Color(0xE9, 0xF7, 0xEF, 0xFF),
            CornerRadius = 6,
            BorderThickness = 1,
            BorderColor = new Color(0xCC, 0xCC, 0xCC, 0xFF),
            Padding = 8,
            Content = innerGrid2,
        };

        // Bottom-right: plain
        var bottomRight = new Border
        {
            [RowStart] = 2,
            [ColumnStart] = 2,
            BackgroundColor = new Color(0xAF, 0x7A, 0xC5, 0xFF),
            CornerRadius = 6,
            Padding = 10,
            Content = new Label
            {
                Text = "Plain cell",
                FontFamily = "Inter",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
            }
        };

        outerGrid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [Fr(1), Fr(1)],
            TemplateRows = [Fr(1), Fr(1)],
            ColumnGap = 12,
            RowGap = 12,
            Content = [topLeft, topRight, bottomLeft, bottomRight],
        };
        AddProtectedChild(outerGrid);
    }

    static Border SubCell(string text, Color bg, int row, int col) => new Border
    {
        [RowStart] = row,
        [ColumnStart] = col,
        BackgroundColor = bg,
        CornerRadius = 3,
        Padding = 4,
        Content = new Label
        {
            Text = text,
            FontFamily = "Inter",
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Middle,
        }
    };

    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
    {
        outerGrid.Measure(availableSize, context);
        return availableSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat padding = 24;
        outerGrid.Arrange(new Rect(
            rect.X + padding,
            rect.Y + padding,
            rect.Width - padding * 2,
            rect.Height - padding * 2), context);
    }
}
