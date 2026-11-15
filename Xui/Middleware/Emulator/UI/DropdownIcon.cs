using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Middleware.Emulator.UI;

/// <summary>Renders a compact downward disclosure chevron.</summary>
internal sealed class DropdownIcon : IToolButtonIcon
{
    public void Render(IContext context, Point center)
    {
        context.BeginPath();
        context.MoveTo(center + (-4f, -1.5f));
        context.LineTo(center + (0, 2.5f));
        context.LineTo(center + (4f, -1.5f));
        context.SetStroke(new Color(0x4A4A4AFF));
        context.LineWidth = 1.5f;
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Round;
        context.Stroke();
    }
}
