using Xui.Core.DI;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp;

/// <summary>
/// Provides TestApp's shared brand tokens to the complete application view tree.
/// Individual examples may provide a local design system when demonstrating token editing.
/// </summary>
internal sealed class TestAppDesignRoot : View, IFullScreenView
{
    private readonly View content;
    private XuiDesignSystem? designSystem;
    private IDeviceInfo? device;

    public TestAppDesignRoot(View content)
    {
        this.content = content;
        this.AddProtectedChild(content);
    }

    public override int Count => 1;

    public bool DrawsOpaqueFullScreen => content is IFullScreenView { DrawsOpaqueFullScreen: true };

    public override View this[int index] => index == 0
        ? content
        : throw new IndexOutOfRangeException();

    public override object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IDesignSystem) && designSystem is not null)
            return designSystem;

        return base.GetService(serviceType);
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        device = this.GetService(typeof(IDeviceInfo)) as IDeviceInfo;
        if (device is null)
            return;

        designSystem = new XuiDesignSystem(CreateOptions(device), device);
        device.Changed += OnDeviceChanged;
    }

    protected override void OnDeactivate()
    {
        if (device is not null)
            device.Changed -= OnDeviceChanged;
        device = null;
        base.OnDeactivate();
    }

    private void OnDeviceChanged()
    {
        if (device is null || designSystem is null)
            return;

        designSystem.Update(CreateOptions(device), device);
        Invalidate();
    }

    private static XuiDesignSystemOptions CreateOptions(IDeviceInfo device) => new()
    {
        // The two Xui logo stops are approximately magenta 304° and violet 272°.
        PrimaryHue = 304,
        SecondaryHue = 272,
        TertiaryHue = 332,
        Harmony = ColorHarmony.Analogous,
        Chroma = 0.22f,
        NeutralStyle = NeutralStyle.SecondaryApp,
        Shape = ShapePreset.Soft,
        RoundnessFactor = 1.5f,
        Sizing = device.FormFactor == DeviceFormFactor.Desktop
            ? SizingPreset.Desktop
            : SizingPreset.Mobile,
        Motion = MotionPreset.Normal,
        DefaultFontFamily = "Inter",
    };

    protected override Size MeasureCore(Size available, IMeasureContext context) =>
        content.Measure(available, context);

    protected override void ArrangeCore(Rect rect, IMeasureContext context) =>
        content.Arrange(rect, context);

    protected override void RenderCore(IContext context) =>
        this.RenderChild(context, content);
}
