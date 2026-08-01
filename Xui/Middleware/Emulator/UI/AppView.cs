using Xui.Core.Canvas;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using Xui.Middleware.Emulator.Devices;
using Xui.Middleware.Emulator.UI.Phone;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Middleware.Emulator.UI;

/// <summary>The desktop-facing emulator composition.</summary>
internal sealed class AppView : Grid
{
    private readonly PhoneFrameView phoneFrame;

    public AppView(DeviceProfile device)
    {
        var header = new HeaderView(device);
        phoneFrame = new PhoneFrameView(device)
        {
            Margin = 8,
            [RowStart] = 2
        };
        var footer = new FooterView
        {
            [RowStart] = 3
        };

        TemplateColumns = [Fr(1)];
        TemplateRows = [Px(52), Fr(1), Px(24)];

        Add(header);
        Add(phoneFrame);
        Add(footer);
    }

    internal void Attach(EmulatedWindow emulatedWindow) => phoneFrame.Attach(emulatedWindow);
}
