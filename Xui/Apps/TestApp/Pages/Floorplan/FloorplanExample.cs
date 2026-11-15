using Xui.Apps.TestApp.Examples;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp.Pages.Floorplan;

public sealed class FloorplanExample : Example, IMobileExamplePresentation
{
    private bool mobilePresentation;
    public FloorplanExample()
    {
        Title = "Office Floorplan";
        Content = new FloorplanView();
    }

    public bool DrawsUnderHeader => mobilePresentation;
    public void UseMobilePresentation() => mobilePresentation = true;

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        // Keep the shell's controls in the safe area, but let the map supply the
        // backdrop beneath the native blur, including the status-bar region.
        if (mobilePresentation && this.TryFindParent<RootView>(out var root))
            rect = root.Window.DisplayArea;
        base.ArrangeCore(rect, context);
    }
}
