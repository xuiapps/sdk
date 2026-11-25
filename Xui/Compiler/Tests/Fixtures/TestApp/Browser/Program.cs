using System;
using Xui.Apps.TestApp;
using Xui.Core.Math2D;
using TestAppCanvas;

public static class Program
{
    public static int Main()
    {
        try
        {
            var metrics = new MobilePerformanceMetrics();
            var context = new CanvasContext();
            var window = new Xui.Core.Abstract.Window(new BrowserPlatform(context))
            {
                DisplayArea = (0, 0, 393, 759), SafeArea = (0, 0, 393, 759),
                Content = BrowserMenu.CreateRoot(metrics)
            };
            var shell = window.RootView;
            var desired = shell.Measure((393, 759), context);
            shell.Arrange(new Rect(0, 0, 393, 759), context, desired);
            shell.Render(context);
            return 0;
        }
        catch (Exception error)
        {
            NativeCanvas.Command("Error", text: error.Message);
            return 1;
        }
    }
}
