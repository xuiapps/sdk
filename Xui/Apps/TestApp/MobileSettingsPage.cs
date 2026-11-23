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
    private readonly VerticalStack description = Description("Show live FPS, allocations,", "and managed heap size.");
    private readonly Toggle performanceToggle = new() { Id = "PerformanceOverlayToggle" };
    private readonly Label developerTitle = new()
    {
        Text = "Developer & preview",
        FontFamily = "Inter",
        FontSize = 17,
        FontWeight = FontWeight.SemiBold,
    };
    private readonly VerticalStack developerDescription = Description("Show developer and preview", "features in examples.");
    private readonly Toggle developerToggle = new() { Id = "DeveloperFeaturesToggle" };
    private readonly Label symbolsTitle = new()
    {
        Text = "Floorplan symbols", FontFamily = "Inter", FontSize = 17, FontWeight = FontWeight.SemiBold,
    };
    private readonly VerticalStack symbolsDescription = Description("Reuse furniture drawings.", "Turn off to compare FPS.");
    private readonly Toggle symbolsToggle = new() { Id = "FloorplanSymbolsToggle" };

    public MobileSettingsPage()
    {
        performanceToggle.Changed = SetPerformanceOverlayEnabled;
        Add(header);
        Add(title);
        Add(description);
        Add(performanceToggle);
        Add(developerTitle);
        Add(developerDescription);
        Add(developerToggle);
        Add(symbolsTitle);
        Add(symbolsDescription);
        Add(symbolsToggle);
        symbolsToggle.Changed = enabled =>
        {
            if (GetService(typeof(IExampleSettings)) is IExampleSettings settings)
                settings.UseFloorplanSymbols = enabled;
        };
        developerToggle.Changed = enabled =>
        {
            if (GetService(typeof(ExampleSettings)) is ExampleSettings settings)
                settings.ShowDeveloperFeatures = enabled;
        };
    }

    public PagePresentation Presentation => new();

    protected override void OnActivate()
    {
        base.OnActivate();
        if (this.TryFindParent<MobileNavigation>(out var navigation))
            performanceToggle.IsOn = navigation.IsPerformanceOverlayEnabled;
        developerToggle.IsOn = (GetService(typeof(ExampleSettings)) as ExampleSettings)?.ShowDeveloperFeatures ?? false;
        symbolsToggle.IsOn = (GetService(typeof(IExampleSettings)) as IExampleSettings)?.UseFloorplanSymbols ?? true;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        header.Measure((available.Width, 48), context);
        title.Measure((available.Width - 116, 24), context);
        description.Measure((available.Width - 116, 40), context);
        performanceToggle.Measure(available, context);
        developerTitle.Measure((available.Width - 116, 48), context);
        developerDescription.Measure((available.Width - 116, 48), context);
        developerToggle.Measure(available, context);
        symbolsTitle.Measure((available.Width - 116, 24), context);
        symbolsDescription.Measure((available.Width - 116, 48), context);
        symbolsToggle.Measure(available, context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        header.Arrange((rect.X, rect.Y, rect.Width, 48), context);
        title.Arrange((rect.X + 20, rect.Y + 76, rect.Width - 116, 24), context);
        description.Arrange((rect.X + 20, rect.Y + 102, rect.Width - 116, 40), context);
        var toggleSize = performanceToggle.Measure(rect.Size, context);
        performanceToggle.Arrange((rect.Right - 20 - toggleSize.Width, rect.Y + 89, toggleSize.Width, toggleSize.Height), context);
        developerTitle.Arrange((rect.X + 20, rect.Y + 174, rect.Width - 116, 48), context);
        developerDescription.Arrange((rect.X + 20, rect.Y + 200, rect.Width - 116, 48), context);
        var developerSize = developerToggle.Measure(rect.Size, context);
        developerToggle.Arrange((rect.Right - 20 - developerSize.Width, rect.Y + 187, developerSize.Width, developerSize.Height), context);
        symbolsTitle.Arrange((rect.X + 20, rect.Y + 272, rect.Width - 116, 24), context);
        symbolsDescription.Arrange((rect.X + 20, rect.Y + 298, rect.Width - 116, 48), context);
        var symbolsSize = symbolsToggle.Measure(rect.Size, context);
        symbolsToggle.Arrange((rect.Right - 20 - symbolsSize.Width, rect.Y + 285, symbolsSize.Width, symbolsSize.Height), context);
    }

    protected override void RenderCore(IContext context)
    {
        this.RenderChild(context, title);
        this.RenderChild(context, description);
        this.RenderChild(context, performanceToggle);
        this.RenderChild(context, developerTitle);
        this.RenderChild(context, developerDescription);
        this.RenderChild(context, developerToggle);
        this.RenderChild(context, symbolsTitle);
        this.RenderChild(context, symbolsDescription);
        this.RenderChild(context, symbolsToggle);
        this.RenderChild(context, header);
    }

    private static VerticalStack Description(string first, string second) => new()
    {
        Content = [DescriptionLine(first), DescriptionLine(second)],
    };

    private static Label DescriptionLine(string text) => new()
    {
        Text = text, FontFamily = "Inter", FontSize = 13,
        TextColor = new Color(0x5E6068FF),
    };

    private void SetPerformanceOverlayEnabled(bool enabled)
    {
        if (this.TryFindParent<MobileNavigation>(out var navigation))
            navigation.SetPerformanceOverlayEnabled(enabled);
    }
}
