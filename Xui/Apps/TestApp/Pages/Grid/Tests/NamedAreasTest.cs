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
/// Classic page layout using named grid areas:
/// header spans full width, sidebar on the left, main content on the right,
/// footer spans full width at the bottom.
/// </summary>
public class NamedAreasTest : View
{
    private readonly global::Xui.Core.UI.Layout.Grid grid;

    public override int Count => 1;
    public override View this[int index] => grid;

    public NamedAreasTest()
    {
        grid = new global::Xui.Core.UI.Layout.Grid
        {
            TemplateColumns = [160, Fr(1)],
            TemplateRows = [50, Fr(1), 40],
            TemplateAreas =
            [
                "header  header",
                "sidebar content",
                "footer  footer",
            ],
            RowGap = 8,
            ColumnGap = 8,
            Content =
            [
                AreaCell("Header", new Color(0x4A, 0x90, 0xD9, 0xFF), "header"),
                AreaCell("Sidebar", new Color(0x52, 0xBE, 0x80, 0xFF), "sidebar"),
                AreaCell("Content", new Color(0xF5, 0xF5, 0xF5, 0xFF), "content", border: true),
                AreaCell("Footer", new Color(0xAF, 0x7A, 0xC5, 0xFF), "footer"),
            ]
        };
        AddProtectedChild(grid);
    }

    static Border AreaCell(string text, Color bg, string area, bool border = false) => new Border
    {
        [Area] = area,
        BackgroundColor = bg,
        CornerRadius = 6,
        BorderThickness = border ? 1 : 0,
        BorderColor = new Color(0xCC, 0xCC, 0xCC, 0xFF),
        Padding = 10,
        Content = new Label
        {
            Text = text,
            FontFamily = "Inter",
            FontSize = 13,
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
        NFloat padding = 24;
        grid.Arrange(new Rect(
            rect.X + padding,
            rect.Y + padding,
            rect.Width - padding * 2,
            rect.Height - padding * 2), context);
    }
}
