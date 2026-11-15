using Android.OS;
using Java.Interop;

namespace Xui.Runtime.Android.Actual;

/// <summary>Optional .NET Android startup adapter. Rendering and native views live in Java.</summary>
public class XuiActivity : global::Android.App.Activity, IServiceProvider
{
    private AndroidWindow? window;
    private AndroidHapticFeedback? haptics;
    private AndroidVibration? vibration;
    object? IServiceProvider.GetService(Type type) => type == typeof(Xui.Core.Actual.IHapticFeedback) ? haptics :
        type == typeof(Xui.Core.Actual.IVibration) ? vibration ??= new AndroidVibration(Window!.DecorView) : null;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Window!.SetFlags(global::Android.Views.WindowManagerFlags.HardwareAccelerated,
            global::Android.Views.WindowManagerFlags.HardwareAccelerated);
        if (OperatingSystem.IsAndroidVersionAtLeast(30) && !OperatingSystem.IsAndroidVersionAtLeast(35))
            Window.SetDecorFitsSystemWindows(false);
        JniEnvironment.References.GetJavaVM(out var vm);
        window = new AndroidWindow(vm, Handle) { HostServices = this };
        haptics = new AndroidHapticFeedback(Window.DecorView);
    }
    protected override void OnResume() { base.OnResume(); window?.SetActive(true); }
    protected override void OnPause() { window?.SetActive(false); vibration?.Stop(); base.OnPause(); }
    protected override void OnDestroy() { vibration?.Dispose(); haptics?.Dispose(); window?.Dispose(); window = null; base.OnDestroy(); }
}
