using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages;
using Xui.Core.Canvas;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Widgets;

namespace Xui.Apps.TestApp;

/// <summary>A full-width, token-driven mobile navigation row for a TestApp demo.</summary>
internal sealed class MobileDemoButton<TExample> : Button
    where TExample : Example, new()
{
    public MobileDemoButton()
    {
        Variant = ButtonVariant.Text;
        Role = ColorRole.Neutral;
        LabelTextAlign = TextAlign.Left;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        MinimumHeight = 44;
        Clicked = NavigateToExample;
    }

    private void NavigateToExample()
    {
        this.TryFindExampleNavigation(out var navigation);
        navigation?.PushExamplePage<TExample>();
    }
}
