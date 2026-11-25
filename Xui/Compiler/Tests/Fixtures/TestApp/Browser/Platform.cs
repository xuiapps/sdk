using System;
using System.Runtime.InteropServices;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Abstract = Xui.Core.Abstract;

namespace TestAppCanvas;

// The browser supplies only platform services; Window, RootView and the design root are real Xui.
public sealed class BrowserPlatform(CanvasContext context) : IServiceProvider, IRuntime, IDeviceInfo
{
    public object GetService(Type type) => type == typeof(IRuntime) || type == typeof(IDeviceInfo) ? this : null;
    public IDispatcher MainDispatcher => throw new NotSupportedException("Browser dispatcher is not connected yet.");
    public IRunLoop CreateRunloop(Abstract.Application application) => throw new NotSupportedException("The browser owns its event loop.");
    public IWindow CreateWindow(Abstract.IWindow window, IServiceProvider services) => new BrowserWindow(window, services, context);
    public event Action Changed { add { } remove { } }
    public DevicePlatform Platform => DevicePlatform.Browser;
    public DeviceFormFactor FormFactor => DeviceFormFactor.Mobile;
    public PointerModel PointerModel => PointerModel.Touch;
    public NFloat AccessibilityFontScale => 1;
    public bool PrefersReducedMotion => false;
    public bool PrefersHighContrast => false;
    public ColorScheme ColorScheme => ColorScheme.Light;
}

public sealed class BrowserWindow(Abstract.IWindow window, IServiceProvider services, CanvasContext context) : IWindow
{
    public Abstract.IWindow Abstract => window;
    public IServiceProvider NextServiceProvider => services;
    public string Title { get; set; } = "TestApp";
    public bool RequireKeyboard { get; set; }
    public ITextMeasureContext TextMeasureContext => context;
    public bool Dirty = true;
    public object GetService(Type type) => services.GetService(type);
    public void Show() => Invalidate();
    public void Invalidate() => Dirty = true;
}
