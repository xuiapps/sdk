using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Examples.DesignSystem;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.Canvas;
using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Apps.TestApp.Pages.FloorplanGpu;
using Xui.Apps.TestApp.Pages.Gestures;
using Xui.Apps.TestApp.Pages.Grid;
using Xui.Apps.TestApp.Pages.Layers;
using Xui.Apps.TestApp.Pages.Layout;
using Xui.Apps.TestApp.Pages.Market;
using Xui.Apps.TestApp.Pages.ThreeD;
using Xui.Apps.TestApp.Pages.WindowModes.MacOS;
using Xui.Core.Actual;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

internal sealed record ExampleCatalogEntry(
    string Id,
    string Name,
    MobileDemoIcon Icon,
    Func<Example> Create,
    bool Mobile,
    Func<View, bool>? IsDesktopAvailable = null);

internal sealed record ExampleCatalogCategory(string Name, ExampleCatalogEntry[] Entries);

internal static class ExampleCatalog
{
    public static readonly ExampleCatalogCategory[] Categories =
    [
        Category("Canvas",
            Entry("CanvasTests", "Canvas Drawing", MobileDemoIcon.Drawing, () => new CanvasTestsExample()),
            Entry("AnimatedHeart", "Canvas Animations", MobileDemoIcon.Animation, () => new CanvasAnimationsExample()),
            Entry("TextMetrics", "Text Metrics", MobileDemoIcon.TextMetrics, () => new TextMetricsExample())),
        Category("Layout",
            Entry("Basics", "Basics", MobileDemoIcon.Basics, () => new BasicsExample()),
            Entry("GridLayout", "Grid Layout", MobileDemoIcon.Grid, () => new GridExample()),
            Entry("FlexboxLayout", "Flexbox Layout", MobileDemoIcon.Flexbox, () => new FlexboxExample())),
        Category("Input",
            Entry("Gestures", "Gestures", MobileDemoIcon.Gestures, () => new GesturesExample()),
            Entry("TextBox", "Text Input", MobileDemoIcon.TextInput, () => new TextBoxExample())),
        Category("Haptics & Vibration",
            Entry("Haptics", "Haptics", MobileDemoIcon.Haptics, () => new HapticsExample(),
                view => view.GetService(typeof(IHapticFeedback)) is IHapticFeedback),
            Entry("VibrationLive", "Live vibration", MobileDemoIcon.Vibration, () => new LiveVibrationExample(),
                view => view.GetService(typeof(IVibration)) is IVibration { IsSupported: true }),
            Entry("VibrationRecord", "Record & repeat", MobileDemoIcon.Recording, () => new RecordedVibrationExample(),
                view => view.GetService(typeof(IVibration)) is IVibration { IsSupported: true })),
        Category("GPU",
            Entry("3D", "3D", MobileDemoIcon.Cube, () => new ThreeDExample())),
        Category("Design",
            Entry("DesignSystem", "Design System", MobileDemoIcon.Design, () => new DesignSystemExample())),
        Category("Device Info",
            Entry("DeviceInfo", "Device Info", MobileDemoIcon.Device, () => new DeviceInfoExample())),
        Category("Performance",
            Entry("MarketPulse", "Market Pulse · 5,000 rows", MobileDemoIcon.Market, () => new MarketExample()),
            Entry("OfficeFloorplan", "Office Floorplan · Vector map", MobileDemoIcon.Basics, () => new FloorplanExample()),
            Entry("FloorplanGpu", "Floorplan GPU", MobileDemoIcon.Basics, () => new FloorplanGpuExample())),
        Category("Desktop",
            Entry("TextLayout", "Text Layout", MobileDemoIcon.TextMetrics, () => new TextLayoutExample(), mobile: false),
            Entry("NestedStacks", "Nested Stacks", MobileDemoIcon.Basics, () => new NestedStacksExample(), mobile: false),
            Entry("ViewCollectionAlignment", "View Collection Alignment", MobileDemoIcon.Grid, () => new ViewCollectionAlignmentExample(), mobile: false),
            Entry("Layers", "Layers", MobileDemoIcon.Drawing, () => new LayersExample(), mobile: false),
            Entry("WindowModes", "Window Modes (macOS)", MobileDemoIcon.Device, () => new WindowModesMacOS(), mobile: false,
                view => view.GetService(typeof(Xui.Core.DI.IDeviceInfo)) is Xui.Core.DI.IDeviceInfo { Platform: Xui.Core.DI.DevicePlatform.MacOS }))
    ];

    private static ExampleCatalogCategory Category(string name, params ExampleCatalogEntry[] entries) => new(name, entries);

    private static ExampleCatalogEntry Entry(string id, string name, MobileDemoIcon icon, Func<Example> create,
        Func<View, bool>? isDesktopAvailable = null)
        => new(id, name, icon, create, true, isDesktopAvailable);

    private static ExampleCatalogEntry Entry(string id, string name, MobileDemoIcon icon, Func<Example> create, bool mobile,
        Func<View, bool>? isDesktopAvailable = null)
        => new(id, name, icon, create, mobile, isDesktopAvailable);
}
