using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Apps.TestApp.Pages.Grid.Tests;

/// <summary>
/// 14 items placed into a 4-column grid without explicit row/column positions —
/// the grid auto-places them left-to-right, top-to-bottom.
/// </summary>
public class AutoFlowTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid grid;

    public override int Count => 1;
    public override View this[int index] => grid;

    static readonly Color[] palette =
    [
        new Color(0x4A, 0x90, 0xD9, 0xFF),
        new Color(0x52, 0xBE, 0x80, 0xFF),
        new Color(0xAF, 0x7A, 0xC5, 0xFF),
        new Color(0xF0, 0xB2, 0x7A, 0xFF),
        new Color(0x85, 0xC1, 0xE9, 0xFF),
        new Color(0x82, 0xE0, 0xAA, 0xFF),
        new Color(0xF1, 0x94, 0x8A, 0xFF),
        new Color(0xFA, 0xD7, 0xA0, 0xFF),
        new Color(0xA9, 0xCC, 0xE3, 0xFF),
        new Color(0x1A, 0xBC, 0x9C, 0xFF),
        new Color(0xE8, 0x5D, 0x5D, 0xFF),
        new Color(0x9B, 0x59, 0xB6, 0xFF),
        new Color(0xF3, 0x9C, 0x12, 0xFF),
        new Color(0x34, 0x98, 0xDB, 0xFF),
    ];

    public AutoFlowTest()
    {
        var items = new View[14];
        for (int i = 0; i < 14; i++)
        {
            int n = i + 1;
            items[i] = new Border
            {
                BackgroundColor = palette[i],
                CornerRadius = 6,
                BorderThickness = 1,
                BorderColor = new Color(0x00, 0x00, 0x00, 0x20),
                Padding = 6,
                Content = new Label
                {
                    Text = n.ToString(),
                    FontFamily = "Inter",
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Middle,
                }
            };
        }

        grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [Fr(1), Fr(1), Fr(1), Fr(1)],
            AutoRows = 52,
            RowGap = 8,
            ColumnGap = 8,
            Content = items,
        };
        AddProtectedChild(grid);
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
