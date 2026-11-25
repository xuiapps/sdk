using System;
using TestAppCanvas;
using Xui.Apps.TestApp.Pages.Canvas.Tests;
using Xui.Core.Math2D;

public static class Program
{
    public static int Main()
    {
        var context = new CanvasContext();
        var view = new FillRectTest();
        var desired = view.Measure((400, 240), context);
        var frame = view.Arrange((20, 30, 400, 240), context, desired);
        view.Render(context);
        return frame == new Rect(20, 30, 400, 240) ? 0 : 1;
    }
}
