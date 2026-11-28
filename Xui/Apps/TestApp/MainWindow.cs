using Xui.Apps.TestApp.Pages;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.DI;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

public class MainWindow : Window
{
    private readonly MobilePerformanceMetrics mobilePerformanceMetrics;
    private readonly bool sampleFrameRate;
    private readonly TimeProvider frameClock;
    private readonly long frameClockOrigin;
    private bool handlingFrame;
    private bool tickSinceRender;
    private TimeSpan latestFrameTime;
    private const View.ViewFlags AnimationFlags = View.ViewFlags.Animated | View.ViewFlags.DescendantAnimated;
    private TimeSpan FrameTime => frameClock.GetElapsedTime(frameClockOrigin);

    public MainWindow(IServiceProvider context) : base(context)
    {
        this.Title = "Xui TestApp";
        if (GetService(typeof(Xui.Core.Actual.IStatusBar)) is Xui.Core.Actual.IStatusBar statusBar)
            statusBar.Appearance = Xui.Core.Actual.StatusBarAppearance.Dark;
        var instruments = this.RootView.RenderSurfaceInstruments as LayoutFrameInstruments
            ?? new LayoutFrameInstruments();
        mobilePerformanceMetrics = new MobilePerformanceMetrics(instruments);
        frameClock = context.GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
        frameClockOrigin = frameClock.GetTimestamp();
        var testSettings = context.GetService(typeof(ITestSettings)) as ITestSettings;
        // Window services include the actual-window/middleware chain. In the
        // emulator that is where the emulated mobile device identity lives;
        // the construction DI scope still describes the desktop host.
        var deviceInfo = this.GetService(typeof(IDeviceInfo)) as IDeviceInfo;
        sampleFrameRate = deviceInfo?.FormFactor != DeviceFormFactor.Desktop && (testSettings?.IsDebugOverlayEnabled ?? true);
        if (sampleFrameRate) mobilePerformanceMetrics.FrameRate.BeginWarmup();
        View shell = deviceInfo?.FormFactor == DeviceFormFactor.Desktop
            ? new TestAppShell(
                new DesktopDemoShell(),
                instruments,
                testSettings?.IsDebugOverlayEnabled ?? true)
            : new MobileTestAppShell(mobilePerformanceMetrics);
        this.Content = new TestAppDesignRoot(shell);
    }

    public override void Invalidate()
    {
        if (sampleFrameRate && !handlingFrame) mobilePerformanceMetrics.FrameRate.Request(FrameTime);
        base.Invalidate();
    }

    public override void OnAnimationFrame(ref FrameEventRef frame)
    {
        latestFrameTime = FrameTime;
        tickSinceRender = true;
        handlingFrame = true;
        try
        {
            if (sampleFrameRate) mobilePerformanceMetrics.FrameRate.Observe(latestFrameTime, frame.Delta);
            base.OnAnimationFrame(ref frame);
            if (sampleFrameRate && (RootView.Flags & AnimationFlags) != 0)
                mobilePerformanceMetrics.FrameRate.RequestAnimation(latestFrameTime);
        }
        finally { handlingFrame = false; }
    }

    public override void Render(ref RenderEventRef renderEventRef)
    {
        handlingFrame = true;
        try
        {
            // Some hosts render directly without a preceding animation callback.
            // Never acknowledge the *next* animation request again in this tick's render.
            if (sampleFrameRate && !tickSinceRender)
                mobilePerformanceMetrics.FrameRate.Observe(FrameTime, renderEventRef.Frame.Delta, animationTick: false);
            if (mobilePerformanceMetrics.IsEnabled)
            {
                mobilePerformanceMetrics.RecordDisplayLinkCadence(in renderEventRef.Frame);
                mobilePerformanceMetrics.BeginFrame();
            }
            base.Render(ref renderEventRef);
            if (mobilePerformanceMetrics.IsEnabled) mobilePerformanceMetrics.EndFrame();
            if (sampleFrameRate && (RootView.Flags & AnimationFlags) != 0)
                mobilePerformanceMetrics.FrameRate.RequestAnimation(tickSinceRender ? latestFrameTime : FrameTime);
        }
        finally { handlingFrame = false; tickSinceRender = false; }
    }
}
