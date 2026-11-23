using Xui.Core.DI;
using Xui.DevKit.UI.Design;
using Xunit;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Tests.Integration.DesignSystem;

public partial class DesignSystemUpdateTest
{
    private sealed class Device : IDeviceInfo
    {
        public event Action? Changed { add { } remove { } }
        public DevicePlatform Platform => DevicePlatform.iOS;
        public DeviceFormFactor FormFactor => DeviceFormFactor.Mobile;
        public PointerModel PointerModel => PointerModel.Touch;
        public NFloat AccessibilityFontScale { get; set; } = 1;
        public bool PrefersReducedMotion { get; set; }
        public bool PrefersHighContrast { get; set; }
        public ColorScheme ColorScheme { get; set; }
    }
}
