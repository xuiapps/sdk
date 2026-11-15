using System;
using Xui.Core.DI;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator;

/// <summary>Device characteristics reported to code running inside the emulated phone.</summary>
internal sealed class EmulatedDeviceInfo : IDeviceInfo
{
    public event Action? Changed { add { } remove { } }
    public EmulatedDeviceInfo(EmulatedPlatform platform)
    {
    }

    public DevicePlatform Platform => DevicePlatform.iOS;
    public DeviceFormFactor FormFactor => DeviceFormFactor.Mobile;
    public PointerModel PointerModel => PointerModel.Touch;
    public NFloat AccessibilityFontScale => 1;
    public bool PrefersReducedMotion => false;
    public bool PrefersHighContrast => false;
    public ColorScheme ColorScheme => ColorScheme.Light;
}
