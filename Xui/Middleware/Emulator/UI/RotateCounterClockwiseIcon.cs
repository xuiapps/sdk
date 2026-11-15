using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Middleware.Emulator.UI;

/// <summary>Renders a counter-clockwise rotation glyph.</summary>
internal sealed class RotateCounterClockwiseIcon : IToolButtonIcon
{
    public void Render(IContext context, Point center)
    {
        context.Save();
        try
        {
            context.Translate(center);
            context.Scale((-1, 1));
            RotateClockwiseIcon.RenderAtOrigin(context);
        }
        finally
        {
            context.Restore();
        }
    }
}
