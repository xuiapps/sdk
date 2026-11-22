using Xui.Apps.TestApp.Examples;
using Xui.Core.Canvas;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;

namespace Xui.Apps.TestApp;

/// <summary>A section heading above a card containing related TestApp demos.</summary>
internal sealed class MobileDemoCategoryPanel : VerticalStack
{
    private readonly VerticalStack items;

    public MobileDemoCategoryPanel(string heading)
    {
        items = new VerticalStack();
        Add(new Label
        {
            Text = heading,
            FontFamily = "Inter",
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            TextColor = new Color(0x24, 0x25, 0x29, 0xFF),
            Margin = (8, 0, 10, 32),
        });
        Add(new MobileDemoCard
        {
            Content = items,
        });
    }

    public MobileDemoButton<TExample> AddExample<TExample>(string id, string text, MobileDemoIcon icon)
        where TExample : Example, new()
    {
        var button = new MobileDemoButton<TExample>
        {
            Id = id,
            Text = text,
            Icon = icon,
            Margin = (0, 0, 2, 0),
        };
        items.Add(button);
        return button;
    }
}
