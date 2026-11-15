using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Middleware.Emulator.UI;

/// <summary>Draws the glyph centered within a toolbar button.</summary>
internal interface IToolButtonIcon
{
    void Render(IContext context, Point center);
}
