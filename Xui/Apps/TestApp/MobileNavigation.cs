using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;

namespace Xui.Apps.TestApp;

/// <summary>TestApp's mobile navigation adapter over the reusable DevKit navigation widget.</summary>
internal sealed class MobileNavigation : NavigationView, IExampleNavigation
{
    private readonly MobilePerformanceOverlay performanceOverlay;
    private readonly MobilePerformanceMetrics performanceMetrics;

    public MobileNavigation(View homePage, MobilePerformanceMetrics performanceMetrics) : base(homePage)
    {
        this.performanceMetrics = performanceMetrics;
        performanceOverlay = new MobilePerformanceOverlay(performanceMetrics);
        OverlayContent = performanceOverlay;
    }

    public bool IsPerformanceOverlayEnabled => performanceOverlay.IsEnabled;

    public void SetPerformanceOverlayEnabled(bool enabled)
    {
        performanceMetrics.IsEnabled = enabled;
        performanceOverlay.IsEnabled = enabled;
    }

    public void PushExamplePage<TPage>() where TPage : Example, new() =>
        Push(new MobileExamplePage<TPage>());

    public void PopExample() => Pop();
}
