using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp.Pages.FloorplanGpu;

public sealed class FloorplanGpuExample : Example, IMobileExamplePresentation
{
    private bool mobilePresentation;
    public FloorplanGpuExample() : this(FloorplanGpuModel.LoadSharedAsync()) { }
    public FloorplanGpuExample(Task<FloorplanGpuModel> model)
    {
        Title = "Floorplan GPU";
        Content = new FloorplanGpuLoadingView(model);
    }
    public bool DrawsUnderHeader => mobilePresentation;
    public void UseMobilePresentation() => mobilePresentation = true;
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        if (mobilePresentation && this.TryFindParent<RootView>(out var root)) rect = root.Window.DisplayArea;
        base.ArrangeCore(rect, context);
    }
}
