using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Apps.XuiSDK.Icons;
using Xui.Apps.XuiSDK.Pages;

namespace Xui.Apps.XuiSDK;

public class NavigationShell : ViewCollection
{
    private static readonly nfloat NavWidth = 220;
    private static readonly nfloat HeaderHeight = 48;

    private readonly NavButton homeButton;
    private readonly NavButton techButton;
    private readonly NavButton archButton;
    private readonly NavButton contactButton;
    private readonly NavButton scrollButton;

    private readonly HomePage homePage = new();
    private readonly TechPage techPage = new();
    private readonly ArchitecturePage archPage = new();
    private readonly ContactPage contactPage = new();
    private readonly ScrollDemoPage scrollPage = new();

    private View activePage;
    private string activePageName = "Home";

    // Animated indicator bar state
    private nfloat indicatorY;
    private nfloat indicatorTargetY;
    private nfloat indicatorVelocity;
    private bool indicatorAnimating;
    private int selectedIndex;

    public NavigationShell()
    {
        homeButton = new NavButton { Text = "Home", NavIcon = new HomeIcon() };
        techButton = new NavButton { Text = "Tech", NavIcon = new SpyGlassIcon() };
        archButton = new NavButton { Text = "Architecture", NavIcon = new WalletIcon() };
        contactButton = new NavButton { Text = "Contact", NavIcon = new LocationPinIcon() };
        scrollButton = new NavButton { Text = "Scroll Demo", NavIcon = new HomeIcon() };

        homeButton.OnClick = () => NavigateTo("Home");
        techButton.OnClick = () => NavigateTo("Tech");
        archButton.OnClick = () => NavigateTo("Architecture");
        contactButton.OnClick = () => NavigateTo("Contact");
        scrollButton.OnClick = () => NavigateTo("Scroll");

        homeButton.IsSelected = true;
        activePage = homePage;
        selectedIndex = 0;

        Add(homeButton);
        Add(techButton);
        Add(archButton);
        Add(contactButton);
        Add(scrollButton);
        Add(homePage);
    }

    private NavButton ButtonAtIndex(int index) => index switch
    {
        0 => homeButton,
        1 => techButton,
        2 => archButton,
        3 => contactButton,
        4 => scrollButton,
        _ => homeButton
    };

    private nfloat GetIndicatorY(int index)
    {
        var btn = ButtonAtIndex(index);
        return btn.Frame.Y + 8;
    }

    private void NavigateTo(string page)
    {
        if (page == activePageName)
            return;

        homeButton.IsSelected = page == "Home";
        techButton.IsSelected = page == "Tech";
        archButton.IsSelected = page == "Architecture";
        contactButton.IsSelected = page == "Contact";
        scrollButton.IsSelected = page == "Scroll";

        int newIndex = page switch
        {
            "Tech" => 1,
            "Architecture" => 2,
            "Contact" => 3,
            "Scroll" => 4,
            _ => 0
        };

        selectedIndex = newIndex;
        indicatorTargetY = GetIndicatorY(newIndex);
        indicatorAnimating = true;
        RequestAnimationFrame();

        Remove(activePage);

        activePage = page switch
        {
            "Tech" => techPage,
            "Architecture" => archPage,
            "Contact" => contactPage,
            "Scroll" => scrollPage,
            _ => homePage
        };
        activePageName = page;

        Add(activePage);

        InvalidateMeasure();
        InvalidateRender();
    }

    protected override void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
    {
        if (!indicatorAnimating)
            return;

        nfloat dt = (nfloat)(currentTime - previousTime).TotalSeconds;
        nfloat smoothTime = 0.12f;
        nfloat maxSpeed = 2000f;

        indicatorY = Easing.SmoothDamp(indicatorY, indicatorTargetY, ref indicatorVelocity, smoothTime, maxSpeed, dt);

        if (nfloat.Abs(indicatorY - indicatorTargetY) < 0.5f && nfloat.Abs(indicatorVelocity) < 1f)
        {
            indicatorY = indicatorTargetY;
            indicatorVelocity = 0;
            indicatorAnimating = false;
        }
        else
        {
            RequestAnimationFrame();
        }

        InvalidateRender();
    }

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        var navButtonSize = new Size(NavWidth - 16, 36);
        homeButton.Measure(navButtonSize, context);
        techButton.Measure(navButtonSize, context);
        archButton.Measure(navButtonSize, context);
        contactButton.Measure(navButtonSize, context);
        scrollButton.Measure(navButtonSize, context);

        var contentSize = new Size(
            availableBorderEdgeSize.Width - NavWidth,
            availableBorderEdgeSize.Height - HeaderHeight);
        activePage.Measure(contentSize, context);

        return availableBorderEdgeSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        nfloat navX = rect.X + 8;
        nfloat navY = rect.Y + HeaderHeight + 12;
        nfloat buttonHeight = 36;
        nfloat buttonSpacing = 2;

        homeButton.Arrange(new Rect(navX, navY, NavWidth - 16, buttonHeight), context);
        navY += buttonHeight + buttonSpacing;
        techButton.Arrange(new Rect(navX, navY, NavWidth - 16, buttonHeight), context);
        navY += buttonHeight + buttonSpacing;
        archButton.Arrange(new Rect(navX, navY, NavWidth - 16, buttonHeight), context);
        navY += buttonHeight + buttonSpacing;
        contactButton.Arrange(new Rect(navX, navY, NavWidth - 16, buttonHeight), context);
        navY += buttonHeight + buttonSpacing;
        scrollButton.Arrange(new Rect(navX, navY, NavWidth - 16, buttonHeight), context);

        // Initialize indicator position on first arrange
        if (!indicatorAnimating && indicatorY == 0)
        {
            indicatorY = GetIndicatorY(selectedIndex);
            indicatorTargetY = indicatorY;
        }

        var contentRect = new Rect(
            rect.X + NavWidth,
            rect.Y + HeaderHeight,
            rect.Width - NavWidth,
            rect.Height - HeaderHeight);
        activePage.Arrange(contentRect, context);
    }

    protected override void RenderCore(IContext context)
    {
        var rect = this.Frame;

        // Xui logo in header (64x64 scaled to 24x24)
        nfloat logoSize = 24;
        nfloat logoScale = logoSize / 64;
        nfloat logoX = rect.X + 12;
        nfloat logoY = rect.Y + (HeaderHeight - logoSize) / 2;
        context.Save();
        context.Translate((logoX, logoY));
        context.Scale((logoScale, logoScale));
        XuiLogo.Instance.Render(context);
        context.Restore();

        // Header title
        context.SetFont(new Font(13, ["Segoe UI"], fontWeight: FontWeight.SemiBold));
        context.TextBaseline = TextBaseline.Middle;
        context.TextAlign = TextAlign.Left;
        context.SetFill(new Color(0x1A1A1AFF));
        context.FillText("Xui SDK", new Point(logoX + logoSize + 8, rect.Y + HeaderHeight / 2));

        // Header separator line
        context.SetStroke(new Color(0x00000015));
        context.LineWidth = 1;
        context.BeginPath();
        context.MoveTo(new Point(rect.X, rect.Y + HeaderHeight));
        context.LineTo(new Point(rect.X + rect.Width, rect.Y + HeaderHeight));
        context.Stroke();

        // Nav/content separator
        context.SetStroke(new Color(0x00000010));
        context.BeginPath();
        context.MoveTo(new Point(rect.X + NavWidth, rect.Y + HeaderHeight));
        context.LineTo(new Point(rect.X + NavWidth, rect.Y + rect.Height));
        context.Stroke();

        // Render children (nav buttons + active page)
        base.RenderCore(context);

        // Animated indicator bar (drawn on top of everything in nav area)
        nfloat barX = rect.X + 10;
        nfloat barHeight = 20;
        context.SetFill(Colors.White);
        context.BeginPath();
        context.RoundRect(new Rect(barX, indicatorY, 3, barHeight), 1.5f);
        context.Fill();
    }
}
