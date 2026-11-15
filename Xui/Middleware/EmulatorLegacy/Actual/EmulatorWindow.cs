using System;
using System.Runtime.InteropServices;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Middleware.EmulatorLegacy.UI;

namespace Xui.Middleware.EmulatorLegacy.Actual;

/// <summary>
/// The actual-window adapter exposed to the emulated application. The physical desktop
/// window is deliberately hosted by <see cref="EmulatorHostWindow"/> and knows nothing
/// about the virtual mobile window behind this adapter.
/// </summary>
public partial class EmulatorWindow : Xui.Core.Middleware.IWindow
{
    private readonly LinkedEmulatorWindow emulator;
    private readonly EmulatorDeviceInfo deviceInfo;
    private EmulatorHostWindow? host;
    private PhoneFrameView? phoneFrame;
    private EmulatorStatusBarStyle? statusBarStyleOverride;

    public EmulatorWindow(Xui.Core.Abstract.IWindow appWindow, IServiceProvider nextServiceProvider)
    {
        emulator = new LinkedEmulatorWindow(appWindow);
        deviceInfo = new EmulatorDeviceInfo(this);
        NextServiceProvider = nextServiceProvider;
    }

    internal LinkedEmulatorWindow Emulator => emulator;

    public Xui.Core.Abstract.IWindow AppWindow => emulator.AppWindow;

    Xui.Core.Abstract.IWindow Xui.Core.Actual.IWindow.Abstract => AppWindow;

    /// <summary>The physical desktop window owned by the emulator host.</summary>
    public Xui.Core.Actual.IWindow? Actual => host?.Actual;

    public IServiceProvider NextServiceProvider { get; }

    private DeviceProfile currentDevice = DeviceCatalog.All[0];

    public DeviceProfile CurrentDevice
    {
        get => currentDevice;
        set
        {
            if (currentDevice.Equals(value))
                return;

            currentDevice = value;
            phoneFrame?.InvalidateMeasure();
        }
    }

    public bool RenderTouchIndicators { get; set; } = true;

    public EmulatorStatusBarStyle? StatusBarStyleOverride
    {
        get => statusBarStyleOverride;
        set => statusBarStyleOverride = value;
    }

    public Rect DisplayArea { get => emulator.DisplayArea; set => emulator.DisplayArea = value; }

    public Rect SafeArea { get => emulator.SafeArea; set => emulator.SafeArea = value; }

    public NFloat ScreenCornerRadius { get => emulator.ScreenCornerRadius; set => emulator.ScreenCornerRadius = value; }

    public string Title { get => host?.Title ?? string.Empty; set { if (host is not null) host.Title = value; } }

    public bool RequireKeyboard { get => host?.RequireKeyboard ?? false; set { if (host is not null) host.RequireKeyboard = value; } }

    public ITextMeasureContext? TextMeasureContext => host?.TextMeasureContext;

    internal EmulatorGeometry Geometry => phoneFrame?.Geometry
        ?? EmulatorGeometry.CreateSnapshot(CurrentDevice);

    internal void AttachHost(EmulatorHostWindow host) => this.host = host;

    internal void AttachView(EmulatorWindowView view, PhoneFrameView phoneFrame) =>
        this.phoneFrame = phoneFrame;

    public void Invalidate() => host?.Invalidate();

    public void Show() => host?.Show();

    public void Close() => host?.Actual.Close();

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IDeviceInfo))
            return deviceInfo;

        return host?.GetService(serviceType) ?? NextServiceProvider.GetService(serviceType);
    }

    public Point MapEmulatorToHost(Point emulatorPoint) =>
        phoneFrame?.MapEmulatorToHost(emulatorPoint) ?? Geometry.MapEmulatorToHost(emulatorPoint);

    public Point MapEmulatorToHost(Point emulatorPoint, Size hostSize) =>
        Geometry.MapEmulatorToHost(emulatorPoint);

    public Point MapEmulatorToSnapshot(Point emulatorPoint) =>
        EmulatorGeometry.CreateSnapshot(CurrentDevice).MapEmulatorToHost(emulatorPoint);

    public Size SnapshotSize => new(
        CurrentDevice.LogicalResolution.Width + 48f,
        CurrentDevice.LogicalResolution.Height + 48f);

    internal void SetMobileDisplay(Rect displayArea, DeviceProfile device)
    {
        emulator.DisplayArea = displayArea;
        emulator.SafeArea = displayArea - device.SafeAreaInsetsPortrait;
        emulator.ScreenCornerRadius = device.ScreenCornerRadius;
    }

    internal void RenderMobile(ref RenderEventRef render) => emulator.Render(ref render);

    internal void RenderTouchIndicator(IContext context) => emulator.RenderTouchIndicator(context);

    internal EmulatorStatusBarStyle ResolveStatusBarStyle() => statusBarStyleOverride ?? default;

    internal IClock ResolveClock() =>
        NextServiceProvider.GetService<IClock>() ?? SystemClock.Default;

    public void RenderSnapshot(ref RenderEventRef render)
    {
        var geometry = EmulatorGeometry.CreateSnapshot(CurrentDevice);
        var root = new EmulatorRootView(this);
        root.RenderSnapshot(render.Context, in geometry);
        new EmulatorChromeRenderer().Render(
            render.Context,
            in geometry,
            in render.Rect,
            CurrentDevice,
            ResolveStatusBarStyle(),
            ResolveClock());
    }

    // The following abstract-side members preserve the middleware surface for callers that
    // already hold an EmulatorWindow. Native desktop callbacks target EmulatorHostWindow.
    public void Closed() => emulator.Closed();
    public bool Closing() => emulator.Closing();
    public void OnAnimationFrame(ref FrameEventRef animationFrame) => emulator.OnAnimationFrame(ref animationFrame);
    public void OnMouseDown(ref MouseDownEventRef evRef) => emulator.OnMouseDown(ref evRef);
    public void OnMouseMove(ref MouseMoveEventRef evRef) => emulator.OnMouseMove(ref evRef);
    public void OnMouseUp(ref MouseUpEventRef evRef) => emulator.OnMouseUp(ref evRef);
    public void OnScrollWheel(ref ScrollWheelEventRef evRef) => emulator.OnScrollWheel(ref evRef);
    public void OnTouch(ref TouchEventRef touchEventRef) => emulator.OnTouch(ref touchEventRef);
    public void Render(ref RenderEventRef render) => emulator.Render(ref render);
    public void WindowHitTest(ref WindowHitTestEventRef evRef) { }
    public void OnKeyDown(ref KeyEventRef e) => emulator.OnKeyDown(ref e);
    public void OnChar(ref KeyEventRef e) => emulator.OnChar(ref e);

    public object? GetLinkedService(Type serviceType) =>
        (AppWindow as IServiceProvider)?.GetService(serviceType);
}
