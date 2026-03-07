using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.Canvas;
using Xui.Apps.TestApp.Pages.Layers;
using Xui.Core.UI;
using static Xui.Core.Canvas.FontWeight;

namespace Xui.Apps.TestApp.Pages;

public class SdkHomePage : VerticalStack
{
    public SdkHomePage()
    {
        this.Add(new Label {
            Text = "Xui SDK Examples ",
            FontFamily = ["Inter"],
            FontSize = 24,
            FontWeight = Bold
        });
        this.Add(new SdkExampleButton<TextMetricsExample>() {
            Id = "TextMetrics",
            Margin = 3,
            Text = "TextMetrics",
        });
        this.Add(new SdkExampleButton<TextLayoutExample>() {
            Id = "TextLayout",
            Margin = 3,
            Text = "Text Layout",
        });
        this.Add(new SdkExampleButton<NestedStacksExample>() {
            Id = "NestedStacks",
            Margin = 3,
            Text = "Nested Stacks"
        });
        this.Add(new SdkExampleButton<ViewCollectionAlignmentExample>() {
            Id = "ViewCollectionAlignment",
            Margin = 3,
            Text = "ViewCollection Alignment"
        });
        this.Add(new SdkExampleButton<AnimatedHeartExample>() {
            Id = "AnimatedHeart",
            Margin = 3,
            Text = "Animated Heart"
        });
        this.Add(new SdkExampleButton<TextBoxExample>() {
            Id = "TextBox",
            Margin = 3,
            Text = "TextBox MVP"
        });
        this.Add(new SdkExampleButton<CanvasTestsExample>() {
            Id = "CanvasTests",
            Margin = 3,
            Text = "Canvas Tests"
        });
        this.Add(new SdkExampleButton<LayersExample>() {
            Id = "Layers",
            Margin = 3,
            Text = "Layers"
        });
    }
}