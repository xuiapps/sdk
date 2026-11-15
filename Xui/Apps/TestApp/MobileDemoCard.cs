using Xui.Core.Canvas;
using Xui.DevKit.UI.Widgets;

namespace Xui.Apps.TestApp;

/// <summary>The shared soft-shadow card treatment for the home screen and mobile demos.</summary>
internal sealed class MobileDemoCard : Card
{
    internal const int SharedShadowExtent = 12;
    internal static readonly Color SharedShadowColor = new(0f, 0f, 0f, 0.06f);

    public MobileDemoCard()
    {
        ShadowExtent = SharedShadowExtent;
        ShadowColor = SharedShadowColor;
    }
}
