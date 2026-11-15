using Xui.Core.Canvas;
using Xui.Core.UI;

namespace Xui.Middleware.Emulator.UI;

/// <summary>A white, fully rounded toolbar surface for a compact group of controls.</summary>
internal class ToolBar : Border
{
    public ToolBar(View content)
    {
        CornerRadius = 999;
        BackgroundColor = Colors.White;
        Padding = 2;
        Content = content;
    }
}
