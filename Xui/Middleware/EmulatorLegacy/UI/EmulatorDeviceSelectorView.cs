using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Middleware.EmulatorLegacy.UI;

/// <summary>The centered device-preset selector. Its menu behavior is added separately.</summary>
internal sealed class EmulatorDeviceSelectorView : Border
{
    private readonly EmulatorHeaderChevronButtonView chevronButton = new();

    public EmulatorDeviceSelectorView(string deviceName)
    {
        var label = new Label
        {
            Text = deviceName,
            FontSize = 13,
            FontWeight = 600,
            Margin = (0, 8, 0, 10),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Middle
        };
        chevronButton[ColumnStart] = 2;

        CornerRadius = 999;
        BackgroundColor = Colors.White;
        Padding = 2;
        MinimumWidth = 220;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Middle;
        Content = new Grid
        {
            TemplateColumns = [Fr(1), Px(34)],
            TemplateRows = [Px(34)],
            Content = [label, chevronButton]
        };
    }

    public bool IsInteractive(Point point) => chevronButton.Frame.Contains(point);
}
