using Xui.Core.UI.Layout;
using Xui.Middleware.EmulatorLegacy.Actual;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Middleware.EmulatorLegacy.UI;

/// <summary>The desktop-facing composition for an emulated mobile window.</summary>
internal sealed class EmulatorWindowView : Grid
{
    public EmulatorWindowView(EmulatorWindow emulator)
    {
        var header = new EmulatorHeaderToolbarView(emulator);
        var phoneFrame = new PhoneFrameView(emulator) { [RowStart] = 3 };
        var footer = new EmulatorFooterView { [RowStart] = 4 };

        TemplateColumns = [Fr(1)];
        TemplateRows = [Px(52), Px(8), Fr(1), Px(24)];

        Add(header);
        Add(phoneFrame);
        Add(footer);

        emulator.AttachView(this, phoneFrame);
    }
}
