using Xui.Core.Canvas;

namespace Xui.Runtime.Android.Composition;

internal interface IAndroidResources
{
    int Font(Font font);
    int Image(IImage image);
}
