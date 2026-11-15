using Xui.Apps.TestApp.Examples;
using Xui.Core.UI;

namespace Xui.Apps.TestApp.Pages;

/// <summary>Common navigation operations used by TestApp's desktop and mobile shells.</summary>
internal interface IExampleNavigation
{
    void PopExample();

    void PushExamplePage<TPage>() where TPage : Example, new();
}

internal static class ExampleNavigationExtensions
{
    public static bool TryFindExampleNavigation(this View view, out IExampleNavigation? navigation)
    {
        foreach (var parent in view.Parents())
        {
            if (parent is IExampleNavigation candidate)
            {
                navigation = candidate;
                return true;
            }
        }

        navigation = null;
        return false;
    }
}
