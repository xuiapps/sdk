using System;
using Xui.Core.Abstract;
using Xui.Core.Actual;

namespace Xui.Middleware.Emulator;

/// <summary>
/// Creates the desktop emulator host while the application continues to create
/// its ordinary mobile-facing window.
/// </summary>
public sealed class EmulatorAppPlatform(IRuntime basePlatform) : IRuntime
{
    /// <inheritdoc />
    public IDispatcher MainDispatcher => basePlatform.MainDispatcher;

    /// <inheritdoc />
    public IRunLoop CreateRunloop(Application applicationAbstract) =>
        basePlatform.CreateRunloop(applicationAbstract);

    /// <inheritdoc />
    public Xui.Core.Actual.IWindow CreateWindow(
        Xui.Core.Abstract.IWindow windowAbstract,
        IServiceProvider applicationServices)
    {
        var hostServices = new HostServiceProvider(applicationServices, basePlatform);
        var hostWindow = new EmulatorAppWindow(hostServices);
        var emulatedPlatform = new EmulatedPlatform(hostWindow, applicationServices);
        var emulatedWindow = new EmulatedWindow(windowAbstract, emulatedPlatform);
        hostWindow.Attach(emulatedWindow);
        return emulatedWindow;
    }

    private sealed class HostServiceProvider(IServiceProvider applicationServices, IRuntime runtime) : IServiceProvider
    {
        public object? GetService(Type serviceType) =>
            serviceType == typeof(IRuntime)
                ? runtime
                : applicationServices.GetService(serviceType);
    }
}
