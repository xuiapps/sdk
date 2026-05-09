using System.Runtime.InteropServices;
using Xui.Core.DI;

namespace Xui.Runtime.MacOS.Actual;

internal sealed class MacOSDeviceInfo : IDeviceInfo
{
    public static readonly MacOSDeviceInfo Instance = new();

    public DevicePlatform Platform => DevicePlatform.MacOS;
    public DeviceFormFactor FormFactor => DeviceFormFactor.Desktop;
    public PointerModel PointerModel => PointerModel.Mouse;
    public NFloat AccessibilityFontScale => 1;
    public bool PrefersReducedMotion => false;
    public bool PrefersHighContrast => false;
    public ColorScheme ColorScheme => ColorScheme.Light;
}
