namespace Xui.Core.DI;

/// <summary>
/// Provides information about the current device, platform, and user preferences.
/// Resolved via <c>this.GetService&lt;IDeviceInfo&gt;()</c> from any <see cref="Xui.Core.UI.View"/>.
/// The emulator replaces this with a mock to simulate phone or tablet form factors.
/// </summary>
public interface IDeviceInfo
{
    /// <summary>Gets the operating system the app is running on.</summary>
    DevicePlatform Platform { get; }

    /// <summary>Gets the form factor of the current device.</summary>
    DeviceFormFactor FormFactor { get; }

    /// <summary>Gets the primary pointing device available to the user.</summary>
    PointerModel PointerModel { get; }

    /// <summary>Accessibility font scale multiplier (1.0 = default).</summary>
    nfloat AccessibilityFontScale { get; }

    /// <summary>True if the user has requested reduced motion.</summary>
    bool PrefersReducedMotion { get; }

    /// <summary>True if the user has requested high-contrast mode.</summary>
    bool PrefersHighContrast { get; }

    /// <summary>Light or dark appearance preference.</summary>
    ColorScheme ColorScheme { get; }
}

/// <summary>The operating system platform.</summary>
public enum DevicePlatform
{
    /// <summary>Microsoft Windows.</summary>
    Windows,
    /// <summary>Apple macOS.</summary>
    MacOS,
    /// <summary>Web browser (WASM).</summary>
    Browser,
    /// <summary>Apple iOS.</summary>
    iOS,
    /// <summary>Google Android.</summary>
    Android,
}

/// <summary>The physical form factor of the device.</summary>
public enum DeviceFormFactor
{
    /// <summary>Desktop or laptop computer.</summary>
    Desktop,
    /// <summary>Handheld phone.</summary>
    Mobile,
    /// <summary>Tablet device.</summary>
    Tablet,
}

/// <summary>The primary pointing device available to the user.</summary>
public enum PointerModel
{
    /// <summary>Finger on a touch screen.</summary>
    Touch,
    /// <summary>Stylus or pen.</summary>
    Stylus,
    /// <summary>Mouse or trackpad.</summary>
    Mouse,
    /// <summary>D-pad or game controller.</summary>
    Controller,
    /// <summary>Eye-tracking or gaze-based input.</summary>
    Eye,
}

/// <summary>Whether the current effective appearance is light or dark.</summary>
public enum ColorScheme
{
    /// <summary>Light appearance — dark text on light backgrounds.</summary>
    Light,
    /// <summary>Dark appearance — light text on dark backgrounds.</summary>
    Dark,
}
