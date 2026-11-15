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
internal sealed class MobileDemoMenu : MobileSurfaceStack
{
    public MobileDemoMenu()
    {
        AddCanvas();
        AddLayout();
        AddInput();
        AddHaptics();
        AddGpu();
        AddDesign();
        AddDeviceInfo();
        AddPerformance();
    }

    private void AddCanvas()
    {
        var panel = AddPanel("Canvas");
        panel.AddExample<CanvasTestsExample>("CanvasTests", "Canvas Drawing", MobileDemoIcon.Drawing);
        panel.AddExample<CanvasAnimationsExample>("AnimatedHeart", "Canvas Animations", MobileDemoIcon.Animation);
        panel.AddExample<TextMetricsExample>("TextMetrics", "Text Metrics", MobileDemoIcon.TextMetrics);
    }

    private void AddLayout()
    {
        var panel = AddPanel("Layout");
        panel.AddExample<BasicsExample>("Basics", "Basics", MobileDemoIcon.Basics);
        panel.AddExample<GridExample>("GridLayout", "Grid Layout", MobileDemoIcon.Grid);
        panel.AddExample<FlexboxExample>("FlexboxLayout", "Flexbox Layout", MobileDemoIcon.Flexbox);
    }

    private void AddInput()
    {
        var panel = AddPanel("Input");
        panel.AddExample<GesturesExample>("Gestures", "Gestures", MobileDemoIcon.Gestures);
        panel.AddExample<TextBoxExample>("TextBox", "Text Input", MobileDemoIcon.TextInput);
    }

    private void AddHaptics()
    {
        var panel = AddPanel("Haptics & Vibration");
        panel.AddExample<HapticsExample>("Haptics", "Haptics", MobileDemoIcon.Haptics);
        panel.AddExample<LiveVibrationExample>("VibrationLive", "Live vibration", MobileDemoIcon.Vibration);
        panel.AddExample<RecordedVibrationExample>("VibrationRecord", "Record & repeat", MobileDemoIcon.Recording);
    }

    private void AddGpu()
    {
        var panel = AddPanel("GPU");
        panel.AddExample<ThreeDExample>("3D", "3D", MobileDemoIcon.Cube);
    }

    private void AddDesign()
    {
        var panel = AddPanel("Design");
        panel.AddExample<DesignSystemExample>("DesignSystem", "Design System", MobileDemoIcon.Design);
    }

    private void AddDeviceInfo()
    {
        var panel = AddPanel("Device Info");
        panel.AddExample<DeviceInfoExample>("DeviceInfo", "Device Info", MobileDemoIcon.Device);
    }

    private void AddPerformance()
    {
        var panel = AddPanel("Performance");
        panel.AddExample<Pages.Market.MarketExample>("MarketPulse", "Market Pulse · 5,000 rows", MobileDemoIcon.Market);
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
