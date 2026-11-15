using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Examples.DesignSystem;
using Xui.Apps.TestApp.Pages.Canvas;
using Xui.Apps.TestApp.Pages.Gestures;
using Xui.Apps.TestApp.Pages.Grid;
using Xui.Apps.TestApp.Pages.Layers;
using Xui.Apps.TestApp.Pages.ThreeD;
using Xui.Apps.TestApp.Pages.WindowModes.MacOS;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.UI;
using static Xui.Core.Canvas.FontWeight;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages;

public class SdkHomePage : VerticalStack
{
    public SdkHomePage(Color? textColor = null, bool includeTitle = true)
    {
        var foreground = textColor ?? Colors.Black;
        if (includeTitle)
        {
            this.Add(new Label {
                Text = "Xui SDK Examples ",
                FontFamily = "Inter",
                FontSize = 24,
                FontWeight = Bold,
                TextColor = foreground,
            });
        }
        this.Add(new SdkExampleButton<TextMetricsExample>() {
            Id = "TextMetrics",
            Margin = 3,
            Text = "TextMetrics",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<TextLayoutExample>() {
            Id = "TextLayout",
            Margin = 3,
            Text = "Text Layout",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<NestedStacksExample>() {
            Id = "NestedStacks",
            Margin = 3,
            Text = "Nested Stacks",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<GridExample>() {
            Id = "GridLayout",
            Margin = 3,
            Text = "Grid Layout",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<ViewCollectionAlignmentExample>() {
            Id = "ViewCollectionAlignment",
            Margin = 3,
            Text = "ViewCollection Alignment",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<CanvasAnimationsExample>() {
            Id = "AnimatedHeart",
            Margin = 3,
            Text = "Canvas Animations",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<TextBoxExample>() {
            Id = "TextBox",
            Margin = 3,
            Text = "Text Input",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<CanvasTestsExample>() {
            Id = "CanvasTests",
            Margin = 3,
            Text = "Canvas Drawing",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<LayersExample>() {
            Id = "Layers",
            Margin = 3,
            Text = "Layers",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<ThreeDExample>() {
            Id = "3D",
            Margin = 3,
            Text = "3D",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<DesignSystemExample>() {
            Id = "DesignSystem",
            Margin = 3,
            Text = "Design System",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<GesturesExample>() {
            Id = "Gestures",
            Margin = 3,
            Text = "Gestures",
            TextColor = foreground,
        });
        this.Add(new SdkExampleButton<WindowModesMacOS>() {
            Id = "WindowModes",
            Margin = 3,
            Text = "Window Modes (macOS)",
            TextColor = foreground,
            RequiredPlatform = DevicePlatform.MacOS,
        });
    }
}
