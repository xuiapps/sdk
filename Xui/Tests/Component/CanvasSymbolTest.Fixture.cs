using System.Text;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Symbols;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public partial class CanvasSymbolTest
{
    private static void Define(IContext context, object key, Color color, bool update = false)
    {
        using var symbol = context.Symbol(new SymbolOptions { Key = key, Bounds = (0, 0, 20, 20), HasUpdates = update });
        if (symbol.NeedsDrawing)
        {
            symbol.Context.SetFill(color);
            symbol.Context.FillRect((2, 2, 16, 16));
        }
    }
}
