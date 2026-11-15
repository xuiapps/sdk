using Xui.Apps.TestApp.Pages;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>
/// Mobile-specific application root. It deliberately owns its navigation host so
/// the phone experience can evolve independently from the desktop TestApp shell.
/// </summary>
public sealed class MobileTestAppShell : View, IFullScreenView
{
    private readonly MobileNavigation navigation;

    public MobileTestAppShell(MobilePerformanceMetrics performanceMetrics)
    {
        navigation = new MobileNavigation(new ScrollView
        {
            Direction = ScrollDirection.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ClipToViewport = false,
            Content = new MobileHomePage(),
        }, performanceMetrics);
        this.AddProtectedChild(navigation);
    }

    public override int Count => 1;

    public bool DrawsOpaqueFullScreen => navigation.DrawsOpaqueFullScreen;

    public override View this[int index] => index == 0
        ? navigation
        : throw new IndexOutOfRangeException();

    protected override Size MeasureCore(Size available, IMeasureContext context) =>
        navigation.Measure(available, context);

    protected override void ArrangeCore(Rect rect, IMeasureContext context) =>
        navigation.Arrange(rect, context);

    protected override void RenderCore(IContext context) => this.RenderChild(context, navigation);
}
