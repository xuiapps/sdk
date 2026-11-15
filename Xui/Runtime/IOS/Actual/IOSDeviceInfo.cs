using System;
using System.Runtime.InteropServices;
using Xui.Core.DI;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.UIKit;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Live device and accessibility traits for one iOS window.</summary>
internal sealed class IOSDeviceInfo(IOSWindowRootView rootView) : IDeviceInfo
{
    private static readonly Sel UserInterfaceIdiomSel = new("userInterfaceIdiom");
    private static readonly Sel UserInterfaceStyleSel = new("userInterfaceStyle");
    private static readonly Sel AccessibilityContrastSel = new("accessibilityContrast");
    private static readonly Sel PreferredContentSizeCategorySel = new("preferredContentSizeCategory");

    private Snapshot snapshot = default;
    private bool initialized;

    public event Action? Changed;

    public DevicePlatform Platform => DevicePlatform.iOS;
    public DeviceFormFactor FormFactor => Read().FormFactor;
    public PointerModel PointerModel => PointerModel.Touch;
    public NFloat AccessibilityFontScale => Read().AccessibilityFontScale;
    public bool PrefersReducedMotion => Read().PrefersReducedMotion;
    public bool PrefersHighContrast => Read().PrefersHighContrast;
    public ColorScheme ColorScheme => Read().ColorScheme;

    /// <summary>Refreshes the trait snapshot and raises one notification for an effective change.</summary>
    internal void Refresh()
    {
        var current = Read();
        if (!initialized)
        {
            snapshot = current;
            initialized = true;
            return;
        }

        if (snapshot == current)
            return;

        snapshot = current;
        Changed?.Invoke();
    }

    private Snapshot Read()
    {
        var traits = rootView.TraitCollection;
        var idiom = traits == 0 ? 0 : objc_msgSend_retInt(traits, UserInterfaceIdiomSel);
        var style = traits == 0 ? 0 : objc_msgSend_retInt(traits, UserInterfaceStyleSel);
        var contrast = traits == 0 ? 0 : objc_msgSend_retInt(traits, AccessibilityContrastSel);
        var category = traits == 0 ? null : CoreFoundation.ToString(objc_msgSend_retIntPtr(traits, PreferredContentSizeCategorySel));

        var formFactor = idiom == 1 ? DeviceFormFactor.Tablet : DeviceFormFactor.Mobile;
        var accessibilityFontScale = ContentSizeScale(category);
        var accessibilityIsReduceMotionEnabled = UIAccessibilityIsReduceMotionEnabled();
        var prefersHighContrast = contrast == 1 || UIAccessibilityDarkerSystemColorsEnabled();
        var colorScheme = style == 2 ? ColorScheme.Dark : ColorScheme.Light;

        return new Snapshot(
            formFactor,
            accessibilityFontScale,
            accessibilityIsReduceMotionEnabled,
            prefersHighContrast,
            colorScheme);
    }

    private static NFloat ContentSizeScale(string? category) => category switch
    {
        "UICTContentSizeCategoryXS" => 0.82f,
        "UICTContentSizeCategoryS" => 0.88f,
        "UICTContentSizeCategoryM" => 0.95f,
        "UICTContentSizeCategoryL" => 1f,
        "UICTContentSizeCategoryXL" => 1.12f,
        "UICTContentSizeCategoryXXL" => 1.23f,
        "UICTContentSizeCategoryXXXL" => 1.35f,
        "UICTContentSizeCategoryAccessibilityM" => 1.64f,
        "UICTContentSizeCategoryAccessibilityL" => 1.95f,
        "UICTContentSizeCategoryAccessibilityXL" => 2.35f,
        "UICTContentSizeCategoryAccessibilityXXL" => 2.76f,
        "UICTContentSizeCategoryAccessibilityXXXL" => 3.12f,
        _ => 1f,
    };

    private readonly record struct Snapshot(
        DeviceFormFactor FormFactor,
        NFloat AccessibilityFontScale,
        bool PrefersReducedMotion,
        bool PrefersHighContrast,
        ColorScheme ColorScheme);
}
