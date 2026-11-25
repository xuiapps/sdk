using Xui.Core.UI;
using Xui.Apps.TestApp.Pages.Canvas.Tests;

namespace Xui.Apps.TestApp;

/// <summary>First browser catalogue: real TestApp cards and navigation, without optional async/GPU showcases.</summary>
public sealed class BrowserMenu : View
{
    public static View CreateRoot(MobilePerformanceMetrics metrics) =>
        new TestAppDesignRoot(new MobileTestAppShell(metrics, new BrowserMenu(), diagnosticsOverlay: null));

    private readonly MobileDemoCategoryPanel canvas;
    public BrowserMenu()
    {
        Margin = (0, 16);
        canvas = new MobileDemoCategoryPanel("Canvas");
        canvas.AddExample<BrowserCanvasExample>("CanvasDrawing", "Canvas Drawing", MobileDemoIcon.Drawing);
        AddProtectedChild(canvas);
    }
    public override int Count => 1;
    public override View this[int index] => index == 0 ? canvas : throw new System.IndexOutOfRangeException();
}

public sealed class BrowserCanvasExample : Example
{
    public BrowserCanvasExample() { Title = "Canvas Drawing"; Content = new FillRectTest(); }
}
