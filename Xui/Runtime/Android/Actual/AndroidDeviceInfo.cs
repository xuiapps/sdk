using System.Runtime.InteropServices;
using Xui.Core.DI;

namespace Xui.Runtime.Android.Actual;

internal sealed class AndroidDeviceInfo : IDeviceInfo
{
    public event Action? Changed;
    public DevicePlatform Platform => DevicePlatform.Android;
    public DeviceFormFactor FormFactor { get; private set; } = DeviceFormFactor.Mobile;
    public PointerModel PointerModel => PointerModel.Touch;
    public NFloat AccessibilityFontScale { get; private set; } = 1;
    public bool PrefersReducedMotion { get; private set; }
    // The Java bridge does not yet expose Android's contrast preference.
    public bool PrefersHighContrast => false;
    public ColorScheme ColorScheme { get; private set; }
    internal void Update(float fontScale, bool tablet, bool dark, bool reduceMotion)
    {
        var form = tablet ? DeviceFormFactor.Tablet : DeviceFormFactor.Mobile;
        var scheme = dark ? ColorScheme.Dark : ColorScheme.Light;
        if (FormFactor == form && ColorScheme == scheme && AccessibilityFontScale == fontScale && PrefersReducedMotion == reduceMotion) return;
        FormFactor = form; ColorScheme = scheme; AccessibilityFontScale = fontScale; PrefersReducedMotion = reduceMotion;
        Changed?.Invoke();
    }
}
