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
/// Shows how GridJustifyItems and GridAlignItems affect cell content placement.
/// Each row uses a different combination of alignment.
/// </summary>
public class AlignmentTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid grid;

    public override int Count => 1;
    public override View this[int index] => grid;

    public AlignmentTest()
    {
        grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [Fr(1), Fr(1), Fr(1)],
            TemplateRows = [70, 70, 70, 70],
            ColumnGap = 10,
            RowGap = 10,
            Content =
            [
                // Row 1: stretch (default)
                AlignCell("Stretch\n(default)", new Color(0x5D, 0xAD, 0xE8, 0xFF), 1, 1, JustifyItems.Stretch, AlignItems.Stretch),
                AlignCell("Stretch\n(default)", new Color(0x52, 0xBE, 0x80, 0xFF), 1, 2, JustifyItems.Stretch, AlignItems.Stretch),
                AlignCell("Stretch\n(default)", new Color(0xAF, 0x7A, 0xC5, 0xFF), 1, 3, JustifyItems.Stretch, AlignItems.Stretch),
                // Row 2: start
                AlignCell("Start", new Color(0xF0, 0xB2, 0x7A, 0xFF), 2, 1, JustifyItems.Start, AlignItems.Start),
                AlignCell("Start", new Color(0x85, 0xC1, 0xE9, 0xFF), 2, 2, JustifyItems.Start, AlignItems.Start),
                AlignCell("Start", new Color(0x82, 0xE0, 0xAA, 0xFF), 2, 3, JustifyItems.Start, AlignItems.Start),
                // Row 3: center
                AlignCell("Center", new Color(0xF1, 0x94, 0x8A, 0xFF), 3, 1, JustifyItems.Center, AlignItems.Center),
                AlignCell("Center", new Color(0xFA, 0xD7, 0xA0, 0xFF), 3, 2, JustifyItems.Center, AlignItems.Center),
                AlignCell("Center", new Color(0xA9, 0xCC, 0xE3, 0xFF), 3, 3, JustifyItems.Center, AlignItems.Center),
                // Row 4: end
                AlignCell("End", new Color(0x1A, 0xBC, 0x9C, 0xFF), 4, 1, JustifyItems.End, AlignItems.End),
                AlignCell("End", new Color(0xE8, 0x5D, 0x5D, 0xFF), 4, 2, JustifyItems.End, AlignItems.End),
                AlignCell("End", new Color(0x9B, 0x59, 0xB6, 0xFF), 4, 3, JustifyItems.End, AlignItems.End),
            ]
        };
        AddProtectedChild(grid);
    }

    static View AlignCell(string text, Color bg, int row, int col, JustifyItems ji, AlignItems ai)
    {
        // Wrap in a cell background to show the full cell area, then use a smaller inner box
        var inner = new Border
        {
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

        // Container that positions the inner box according to alignment
        return new Border
        {
            [RowStart] = row,
            [ColumnStart] = col,
            BackgroundColor = new Color(0xE8, 0xE8, 0xE8, 0xFF),
            CornerRadius = 2,
            Content = inner,
        };
    }

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
