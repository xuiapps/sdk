using System;
using System.Runtime.InteropServices;
using Xui.Core.DI;
using Xui.Middleware.EmulatorLegacy.Devices;

namespace Xui.Middleware.EmulatorLegacy.Actual;

/// <summary>Device characteristics of the logical device currently shown by an emulator window.</summary>
internal sealed class EmulatorDeviceInfo(EmulatorWindow window) : IDeviceInfo
{
    public event Action? Changed { add { } remove { } }
    private DeviceProfile Device => window.CurrentDevice;

    public DevicePlatform Platform => Device.Brand switch
    {
        Brand.Apple => DevicePlatform.iOS,
        Brand.Android => DevicePlatform.Android,
        _ => DevicePlatform.Android,
    };

    public DeviceFormFactor FormFactor => Device.DeviceType switch
    {
        DeviceType.Tablet => DeviceFormFactor.Tablet,
        _ => DeviceFormFactor.Mobile,
    };

    public PointerModel PointerModel => PointerModel.Touch;
    public NFloat AccessibilityFontScale => 1;
    public bool PrefersReducedMotion => false;
    public bool PrefersHighContrast => false;
    public ColorScheme ColorScheme => ColorScheme.Light;
}
