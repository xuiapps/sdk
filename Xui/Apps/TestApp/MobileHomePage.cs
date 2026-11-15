using Xui.Core.Canvas;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>The branded, scrollable starting page for the mobile TestApp.</summary>
internal sealed class MobileHomePage : VerticalStack
{
    public MobileHomePage()
    {
        var hero = new VerticalStack
        {
            Margin = (30, 0, 30, 0),
        };
        hero.Add(new XuiLogoView
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = (24, 0, 10, 0),
        });
        hero.Add(new Label
        {
            Text = "Xui SDK Examples",
            FontFamily = "Inter",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            TextColor = new Color(0x24, 0x25, 0x29, 0xFF),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = (0, 0, 24, 0),
        });
        Add(hero);
        Add(new MobileSettingsButton
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = (0, 0, 12, 16),
        });
        this.Add(new MobileDemoMenu
        {
            Margin = (0, 16),
        });
    }
}
