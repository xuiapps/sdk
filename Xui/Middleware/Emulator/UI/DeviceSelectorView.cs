using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using Xui.Middleware.Emulator.Devices;
using NFloat = System.Runtime.InteropServices.NFloat;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Middleware.Emulator.UI;

/// <summary>A center-toolbar device selector that yields width to the surrounding host controls.</summary>
internal sealed class DeviceSelectorView : ToolBar
{
    private const float PreferredWidth = 220;

    public DeviceSelectorView(DeviceProfile device)
        : base(CreateContent(device))
    {
        Margin = (0, 16, 0, 16);
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Middle;
    }

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        var desired = base.MeasureCore(availableBorderEdgeSize, context);
        return new Size(NFloat.Min(PreferredWidth, availableBorderEdgeSize.Width), desired.Height);
    }

    private static View CreateContent(DeviceProfile device) => new Grid
    {
        TemplateColumns = [Fr(1), Px(34)],
        TemplateRows = [Px(34)],
        Content = [
            new DeviceNameView(device.Model)
            {
                Margin = (0, 0, 0, 10),
                // Stretch establishes the label's frame as the first grid column.
                // Its renderer clips to that frame, before the fixed disclosure column.
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Middle,
            },
            new ToolButton(new DropdownIcon()) { [ColumnStart] = 2 },
        ]
    };
}
