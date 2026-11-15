using Xui.Apps.TestApp.Pages;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;

namespace Xui.Apps.TestApp;

/// <summary>Mobile settings for optional TestApp diagnostics.</summary>
internal sealed class MobileSettingsPage : ViewCollection, IPage
{
    private readonly MobileExampleHeader header = new("Settings");
    private readonly Label title = new()
    {
        Text = "Performance overlay",
        FontFamily = "Inter",
        FontSize = 17,
        FontWeight = FontWeight.SemiBold,
    };
    private readonly Label description = new()
    {
        Text = "Show live FPS, frame allocations, and managed heap size.",
        FontFamily = "Inter",
        FontSize = 13,
        TextColor = new Color(0x5E6068FF),
    };
    private readonly Toggle performanceToggle = new();

    public MobileSettingsPage()
    {
        performanceToggle.Changed = SetPerformanceOverlayEnabled;
        Add(header);
        Add(title);
        Add(description);
        Add(performanceToggle);
    }

    public PagePresentation Presentation => new();

    protected override void OnActivate()
    {
        base.OnActivate();
        if (this.TryFindParent<MobileNavigation>(out var navigation))
            performanceToggle.IsOn = navigation.IsPerformanceOverlayEnabled;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        header.Measure((available.Width, 48), context);
        title.Measure((available.Width - 116, 24), context);
        description.Measure((available.Width - 116, 40), context);
        performanceToggle.Measure(available, context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        header.Arrange((rect.X, rect.Y, rect.Width, 48), context);
        title.Arrange((rect.X + 20, rect.Y + 76, rect.Width - 116, 24), context);
        description.Arrange((rect.X + 20, rect.Y + 102, rect.Width - 116, 40), context);
        var toggleSize = performanceToggle.Measure(rect.Size, context);
        performanceToggle.Arrange((rect.X + 20, rect.Y + 89, toggleSize.Width, toggleSize.Height), context);
    }

    protected override void RenderCore(IContext context)
    {
        this.RenderChild(context, header);
        this.RenderChild(context, title);
        this.RenderChild(context, description);
        this.RenderChild(context, performanceToggle);
    }

    private void SetPerformanceOverlayEnabled(bool enabled)
    {
        if (this.TryFindParent<MobileNavigation>(out var navigation))
            navigation.SetPerformanceOverlayEnabled(enabled);
    }
}
