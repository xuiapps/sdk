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
/// A grid with many rows inside a vertical ScrollView.
/// The grid is wider than the fixed column count, so the view scrolls to reveal all rows.
/// </summary>
public class ScrollableGridTest : View
{
    private readonly ScrollView scroll;

    public override int Count => 1;
    public override View this[int index] => scroll;

    static readonly Color[] palette =
    [
        new Color(0x4A, 0x90, 0xD9, 0xFF),
        new Color(0x52, 0xBE, 0x80, 0xFF),
        new Color(0xAF, 0x7A, 0xC5, 0xFF),
        new Color(0xF0, 0xB2, 0x7A, 0xFF),
        new Color(0x85, 0xC1, 0xE9, 0xFF),
    ];

    public ScrollableGridTest()
    {
        const int rows = 20;
        const int cols = 3;

        var items = new View[rows * cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                items[idx] = new Border
                {
                    [RowStart] = r + 1,
                    [ColumnStart] = c + 1,
                    BackgroundColor = palette[(r + c) % palette.Length],
                    BorderThickness = 1,
                    BorderColor = new Color(0x00, 0x00, 0x00, 0x20),
                    CornerRadius = 4,
                    Padding = 8,
                    Content = new Label
                    {
                        Text = $"Row {r + 1}, Col {c + 1}",
                        FontFamily = "Inter",
                        FontSize = 11,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Middle,
                    }
                };
            }
        }

        var grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [Fr(1), Fr(1), Fr(1)],
            AutoRows = 48,
            RowGap = 6,
            ColumnGap = 8,
            Content = items,
        };

        scroll = new ScrollView
        {
            Direction = ScrollDirection.Vertical,
            ScrollbarEndInset = 0,
            Content = grid,
        };
        AddProtectedChild(scroll);
    }

    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
    {
        scroll.Measure((NFloat.Max(0, availableSize.Width - 40), availableSize.Height), context);
        return availableSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat padding = 20;
        scroll.Arrange(new Rect(
            rect.X + padding,
            rect.Y,
            NFloat.Max(0, rect.Width - padding * 2),
            rect.Height), context);
    }
}
