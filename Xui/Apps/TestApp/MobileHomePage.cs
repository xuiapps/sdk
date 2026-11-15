using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;

namespace Xui.Apps.TestApp;

/// <summary>The mobile home menu, with a header outside the content's scroll transform.</summary>
internal sealed class MobileHomePage : ScrollView, INavigationAware
{
    private readonly MobileHomeHeader header;

    public MobileHomePage(MobileFrameRate frameRate)
    {
        header = new MobileHomeHeader(frameRate);
        Id = "HomeScroll";
        UseCompositionGroup = true;
        Direction = ScrollDirection.Vertical;
        ClipToViewport = false;
        var menu = new VerticalStack();
        menu.Add(new View { MinimumHeight = MobileHomeHeader.ExpandedHeight, MaximumHeight = MobileHomeHeader.ExpandedHeight });
        menu.Add(new MobileDemoMenu { Margin = (0, 16) });
        Content = menu;
        Overlay = header;
        ScrollChanged += offset => header.SetScrollOffset(offset.Y);
    }

    public void OnNavigationStateChanged(in NavigationStateChange change)
    {
        if (change.Phase == NavigationPhase.WillDisappear) header.CloseRating();
    }
}
