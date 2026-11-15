using System;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using Xui.Middleware.Emulator.Devices;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Middleware.Emulator.UI;

/// <summary>The emulator header with device selection and rotation controls.</summary>
internal sealed class HeaderView : Grid
{
    public HeaderView(DeviceProfile device)
    {
        TemplateColumns = [
            Px(OperatingSystem.IsMacOS() ? 64f : 0f),
            Fr(1),
            Auto,
            Px(OperatingSystem.IsWindows() ? 64f : 0f)];
        TemplateRows = [Fr(1)];

        var deviceSelector = new DeviceSelectorView(device);
        deviceSelector[ColumnStart] = 2;
        Add(deviceSelector);

        var rotationButtons = new Grid
        {
            TemplateColumns = [Px(34), Px(34)],
            TemplateRows = [Px(34)],
            Content = [
                new ToolButton(new RotateCounterClockwiseIcon()),
                new ToolButton(new RotateClockwiseIcon()) { [ColumnStart] = 2 },
            ]
        };

        var rotationToolbar = new ToolBar(rotationButtons)
        {
            Margin = OperatingSystem.IsMacOS() ? (0, 7, 0, 0) : default,
            HorizontalAlignment = OperatingSystem.IsMacOS()
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Middle,
        };
        rotationToolbar[ColumnStart] = 3;
        Add(rotationToolbar);
    }
}
