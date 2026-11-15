using Xui.Apps.TestApp.Pages;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.DI;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

public class MainWindow : Window
{
    private readonly MobilePerformanceMetrics mobilePerformanceMetrics;

    public MainWindow(IServiceProvider context) : base(context)
    {
        this.Title = "Xui TestApp";
        var instruments = this.RootView.RenderSurfaceInstruments as LayoutFrameInstruments
            ?? new LayoutFrameInstruments();
        mobilePerformanceMetrics = new MobilePerformanceMetrics(instruments);
        var testSettings = context.GetService(typeof(ITestSettings)) as ITestSettings;
        // Window services include the actual-window/middleware chain. In the
        // emulator that is where the emulated mobile device identity lives;
        // the construction DI scope still describes the desktop host.
        var deviceInfo = this.GetService(typeof(IDeviceInfo)) as IDeviceInfo;
        View shell = deviceInfo?.FormFactor == DeviceFormFactor.Desktop
            ? new TestAppShell(
                new SdkNavigation(),
                instruments,
                testSettings?.IsDebugOverlayEnabled ?? true)
            : new MobileTestAppShell(mobilePerformanceMetrics);
        this.Content = new TestAppDesignRoot(shell);
    }

    public override void Render(ref RenderEventRef renderEventRef)
    {
        if (mobilePerformanceMetrics.IsEnabled)
            mobilePerformanceMetrics.RecordDisplayLinkCadence(in renderEventRef.Frame);
        if (mobilePerformanceMetrics.IsEnabled)
            mobilePerformanceMetrics.BeginFrame();
        base.Render(ref renderEventRef);
        if (mobilePerformanceMetrics.IsEnabled)
            mobilePerformanceMetrics.EndFrame();
    }
}
