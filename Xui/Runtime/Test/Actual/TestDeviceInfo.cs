using Xui.Core.DI;
using nfloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Runtime.Test.Actual;

/// <summary>A deterministic desktop identity, independent of the test host OS.</summary>
public sealed class TestDeviceInfo : IDeviceInfo
{
    /// <inheritdoc/>
    public event Action? Changed { add { } remove { } }
    /// <inheritdoc/>
    public DevicePlatform Platform { get; init; } = DevicePlatform.Windows;
    /// <inheritdoc/>
    public DeviceFormFactor FormFactor => DeviceFormFactor.Desktop;
    /// <inheritdoc/>
    public PointerModel PointerModel => PointerModel.Mouse;
    /// <inheritdoc/>
    public nfloat AccessibilityFontScale => 1;
    /// <inheritdoc/>
    public bool PrefersReducedMotion => false;
    /// <inheritdoc/>
    public bool PrefersHighContrast => false;
    /// <inheritdoc/>
    public ColorScheme ColorScheme => ColorScheme.Light;
}
