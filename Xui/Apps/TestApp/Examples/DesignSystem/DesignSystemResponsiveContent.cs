using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Chooses the desktop dashboard or dedicated phone composition for the design-system demo.</summary>
internal sealed class DesignSystemResponsiveContent : View
{
    private readonly View desktopContent;
    private View activeContent;
    private MobileDesignSystemView? mobileContent;

    public DesignSystemResponsiveContent(View desktopContent)
    {
        this.desktopContent = desktopContent;
        activeContent = desktopContent;
        AddProtectedChild(desktopContent);
    }

    public void UseMobilePresentation()
    {
        if (mobileContent is not null)
            return;

        mobileContent = new MobileDesignSystemView();
        activeContent = mobileContent;
        AddProtectedChild(mobileContent);
        Invalidate();
    }

    public override int Count => 1;

    public override View this[int index] => index == 0 ? activeContent : throw new IndexOutOfRangeException();

    protected override Size MeasureCore(Size available, IMeasureContext context) => activeContent.Measure(available, context);

    protected override void ArrangeCore(Rect rect, IMeasureContext context) => activeContent.Arrange(rect, context);

    protected override void RenderCore(IContext context) => this.RenderChild(context, activeContent);
}
