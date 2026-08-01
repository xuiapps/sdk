using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Examples.DesignSystem;
using Xui.Apps.TestApp.Pages.Canvas;
using Xui.Apps.TestApp.Pages.Gestures;
using Xui.Apps.TestApp.Pages.Grid;
using Xui.Apps.TestApp.Pages.Layout;
using Xui.Apps.TestApp.Pages.ThreeD;
using Xui.Core.Canvas;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>Organizes the mobile TestApp demos into touch-friendly categories.</summary>
internal sealed class MobileDemoMenu : VerticalStack
{
    public MobileDemoMenu()
    {
        AddCanvas();
        AddLayout();
        AddInput();
        AddGpu();
        AddDesign();
        AddDeviceInfo();
    }

    private void AddCanvas()
    {
        var panel = AddPanel("Canvas");
        panel.AddExample<CanvasTestsExample>("CanvasTests", "Canvas Drawing");
        panel.AddExample<CanvasAnimationsExample>("AnimatedHeart", "Canvas Animations");
        panel.AddExample<TextMetricsExample>("TextMetrics", "Text Metrics");
    }

    private void AddLayout()
    {
        var panel = AddPanel("Layout");
        panel.AddExample<BasicsExample>("Basics", "Basics");
        panel.AddExample<GridExample>("GridLayout", "Grid Layout");
        panel.AddExample<FlexboxExample>("FlexboxLayout", "Flexbox Layout");
    }

    private void AddInput()
    {
        var panel = AddPanel("Input");
        panel.AddExample<GesturesExample>("Gestures", "Gestures");
        panel.AddExample<TextBoxExample>("TextBox", "Text Input");
    }

    private void AddGpu()
    {
        var panel = AddPanel("GPU");
        panel.AddExample<ThreeDExample>("3D", "3D");
    }

    private void AddDesign()
    {
        var panel = AddPanel("Design");
        panel.AddExample<DesignSystemExample>("DesignSystem", "Design System");
    }

    private void AddDeviceInfo()
    {
        var panel = AddPanel("Device Info");
        panel.AddExample<DeviceInfoExample>("DeviceInfo", "Device Info");
    }

    private MobileDemoCategoryPanel AddPanel(string heading)
    {
        var panel = new MobileDemoCategoryPanel(heading)
        {
            Margin = (0, 0, 28, 0),
        };
        this.Add(panel);
        return panel;
    }
}
