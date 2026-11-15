using System;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using Xui.Middleware.EmulatorLegacy.Actual;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Middleware.EmulatorLegacy.UI;

internal sealed class EmulatorHeaderToolbarView : Grid
{
    private readonly EmulatorHeaderButtonView rotateCcwButton;
    private readonly EmulatorHeaderButtonView rotateCwButton;

    public EmulatorHeaderToolbarView(EmulatorWindow emulator)
    {
        rotateCcwButton = new EmulatorHeaderButtonView(clockwise: false);
        rotateCwButton = new EmulatorHeaderButtonView(clockwise: true);
        rotateCwButton[ColumnStart] = 2;

        TemplateColumns = [
            Px(OperatingSystem.IsMacOS() ? 64f : 0f),
            Fr(1),
            Px(OperatingSystem.IsMacOS() ? 64f : 0f),
            Fr(1),
            Auto,
            Px(OperatingSystem.IsWindows() ? 64f : 0f)];
        TemplateRows = [Fr(1)];

        var deviceSelector = new EmulatorDeviceSelectorView(emulator.CurrentDevice.Model);
        deviceSelector[ColumnStart] = 3;
        Add(deviceSelector);

        var buttons = new Grid
        {
            Content = [
                rotateCcwButton,
                rotateCwButton
            ],
            TemplateColumns = [Px(34), Px(34)],
            TemplateRows = [Px(34)]
        };

        var toolbar = new Border
        {
            CornerRadius = 999,
            BackgroundColor = Colors.White,
            Padding = 2,
            Margin = OperatingSystem.IsMacOS() ? (0, 7, 0, 0) : default,
            HorizontalAlignment = OperatingSystem.IsMacOS()
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Middle,
            Content = buttons
        };
        toolbar[ColumnStart] = 5;
        Add(toolbar);
    }

    public bool IsInteractive(Point point) =>
        rotateCcwButton.Frame.Contains(point) || rotateCwButton.Frame.Contains(point);
}
