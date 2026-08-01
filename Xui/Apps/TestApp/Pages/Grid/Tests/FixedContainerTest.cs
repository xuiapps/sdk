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
/// Grid inside a fixed-size container (300×240px), centered on screen.
/// Shows how the grid behaves when its container has a known, constrained size.
/// Uses fr columns and rows that divide the fixed space.
/// </summary>
public class FixedContainerTest : View
{
    private readonly Border container;

    public override int Count => 1;
    public override View this[int index] => container;

    public FixedContainerTest()
    {
        var grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [Fr(1), Fr(2), Fr(1)],
            TemplateRows = [Fr(1), Fr(1), Fr(1)],
            ColumnGap = 6,
            RowGap = 6,
            Content =
            [
                Cell("A", new Color(0x4A, 0x90, 0xD9, 0xFF), 1, 1),
                Cell("B", new Color(0x52, 0xBE, 0x80, 0xFF), 1, 2),
                Cell("C", new Color(0xAF, 0x7A, 0xC5, 0xFF), 1, 3),
                Cell("D", new Color(0xF0, 0xB2, 0x7A, 0xFF), 2, 1),
                Cell("E (2fr wide)", new Color(0x85, 0xC1, 0xE9, 0xFF), 2, 2),
                Cell("F", new Color(0x82, 0xE0, 0xAA, 0xFF), 2, 3),
                Cell("G", new Color(0xF1, 0x94, 0x8A, 0xFF), 3, 1),
                Cell("H", new Color(0xFA, 0xD7, 0xA0, 0xFF), 3, 2),
                Cell("I", new Color(0xA9, 0xCC, 0xE3, 0xFF), 3, 3),
            ]
        };

        container = new Border
        {
            BackgroundColor = new Color(0xFF, 0xFF, 0xFF, 0xFF),
            BorderThickness = 2,
            BorderColor = new Color(0xCC, 0xCC, 0xCC, 0xFF),
            CornerRadius = 8,
            Padding = 10,
            Content = grid,
        };
        AddProtectedChild(container);
    }

    static Border Cell(string text, Color bg, int row, int col) => new Border
    {
        [RowStart] = row,
        [ColumnStart] = col,
        BackgroundColor = bg,
        CornerRadius = 4,
        Padding = 6,
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
        container.Measure(new Size(320, 260), context);
        return availableSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat w = 320;
        NFloat h = 260;
        NFloat x = rect.X + (rect.Width - w) / 2;
        NFloat y = rect.Y + (rect.Height - h) / 2;
        container.Arrange(new Rect(x, y, w, h), context);
    }
}
