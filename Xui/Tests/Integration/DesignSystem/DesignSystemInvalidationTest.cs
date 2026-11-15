using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Integration.DesignSystem;

public class DesignSystemInvalidationTest
{
    [Theory]
    [InlineData("Button")]
    [InlineData("Card")]
    [InlineData("Toggle")]
    [InlineData("Checkbox")]
    [InlineData("RadioButton")]
    [InlineData("Expander")]
    [InlineData("TextInput")]
    [InlineData("ButtonGroup")]
    [InlineData("ListView")]
    public void PaintChangesPreserveLayoutAndSizingChangesInvalidateIt(string kind)
    {
        var device = new Device();
        var theme = new XuiDesignSystem(new(), device);
        View widget = kind switch
        {
            "Button" => new Button { Text = "Continue" },
            "Card" => new Card { Content = new Label { Text = "Card" } },
            "Toggle" => new Toggle(),
            "Checkbox" => new Checkbox(),
            "RadioButton" => new RadioButton(),
            "Expander" => new Expander { Title = "Details" },
            "TextInput" => new TextInput(),
            "ButtonGroup" => new ButtonGroup { Content = [new ButtonGroupItem { Text = "First" }, new ButtonGroupItem { Text = "Second" }] },
            "ListView" => new ListView { Content = [new ListViewItem { ItemContent = new Label { Text = "First" } }] },
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        var host = new ThemeHost(theme) { Content = [widget] };
        var size = new Size(320, 240);
        using var stream = new MemoryStream();
        using var context = new SvgDrawingContext(size, stream, Xui.Core.Fonts.Inter.URIs);

        // Keep the drawing/measurement context stable, as on the device.
        void Frame()
        {
            var frame = new LayoutFrameContext(TimeSpan.Zero, TimeSpan.Zero, context, context, default);
            var update = new LayoutUpdate(LayoutPass.Measure | LayoutPass.Arrange | LayoutPass.Render,
                new MeasureConstraints(size, LayoutSizeMode.Exact, LayoutSizeMode.Exact),
                new ArrangeConstraints(size, default, (0, 0)));
            LayoutMeasurements measurements = default;
            host.Update(in frame, in update, ref measurements);
        }

        // A stretched leaf can first resolve its theme during rendering rather than measurement.
        Frame();
        Frame();
        Assert.False(NeedsLayout(host));
        theme.Update(new() { PrimaryHue = 90 }, device);
        Assert.False(NeedsLayout(host));
        Frame();
        theme.Update(new() { PrimaryHue = 90, Shape = ShapePreset.Square }, device);
        Assert.False(NeedsLayout(host));
        Frame();

        var compact = new XuiDesignSystemOptions { PrimaryHue = 90, Shape = ShapePreset.Square, Sizing = SizingPreset.Desktop };
        theme.Update(compact, device);
        Assert.True(NeedsLayout(widget));
        Frame();
        Assert.False(NeedsLayout(host));
        device.AccessibilityFontScale = (nfloat)1.5;
        theme.Update(compact, device);
        Assert.True(NeedsLayout(widget));
        Frame();
        Assert.False(NeedsLayout(host));
    }

    private static bool NeedsLayout(View view)
    {
        if ((view.Flags & (View.ViewFlags.MeasureChanged | View.ViewFlags.ArrangeChanged | View.ViewFlags.DescendantArrangeChanged)) != 0)
            return true;
        for (int i = 0; i < view.Count; i++)
            if (NeedsLayout(view[i])) return true;
        return false;
    }

    private sealed class ThemeHost(IDesignSystem theme) : ViewCollection
    {
        public override object? GetService(Type type) => type == typeof(IDesignSystem) ? theme : base.GetService(type);
    }

    private sealed class Device : IDeviceInfo
    {
        public event Action? Changed { add { } remove { } }
        public DevicePlatform Platform => DevicePlatform.iOS;
        public DeviceFormFactor FormFactor => DeviceFormFactor.Mobile;
        public PointerModel PointerModel => PointerModel.Touch;
        public nfloat AccessibilityFontScale { get; set; } = 1;
        public bool PrefersReducedMotion => false;
        public bool PrefersHighContrast => false;
        public ColorScheme ColorScheme => ColorScheme.Light;
    }
}
