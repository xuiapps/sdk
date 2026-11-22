using Xui.Core.Canvas;

namespace Xui.Runtime.Android.Composition;

internal interface IAndroidResources
{
    int Symbol(Xui.Core.Canvas.Symbols.SymbolContent content);
    int Font(Font font);
    int Image(IImage image);
}
