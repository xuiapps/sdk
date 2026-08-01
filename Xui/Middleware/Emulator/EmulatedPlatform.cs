using System;
using Xui.Core.Canvas;
using Xui.GPU.Backends;
using Xui.GPU.Hardware;
using Xui.Core.DI;
using Xui.Middleware.Emulator.Devices;

namespace Xui.Middleware.Emulator;

/// <summary>
/// The mobile environment presented to an app running inside an emulator host.
/// It owns mobile-facing services and exposes only narrow host lifecycle requests.
/// </summary>
internal sealed class EmulatedPlatform
{
    private readonly EmulatorAppWindow hostWindow;

    public EmulatedPlatform(EmulatorAppWindow hostWindow, IServiceProvider applicationServices)
    {
        this.hostWindow = hostWindow;
        Services = new EmulatedServiceProvider(this, applicationServices);
    }

    public DeviceProfile Device => hostWindow.CurrentDevice;

    public IServiceProvider Services { get; }

    public void Show() => hostWindow.Show();

    public void Close() => hostWindow.Actual.Close();

    public void RequestRender() => hostWindow.Invalidate();

    private sealed class EmulatedServiceProvider(EmulatedPlatform platform, IServiceProvider applicationServices) : IServiceProvider
    {
        private readonly EmulatedDeviceInfo deviceInfo = new(platform);

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IDeviceInfo))
                return deviceInfo;

            // Images are platform resources. The emulated app renders through the
            // host's native context, so its image handles must come from that same
            // native image pipeline.
            if (serviceType == typeof(IImage))
                return platform.hostWindow.Actual.GetService(serviceType);

            // The emulated display is rendered by the host hardware. Keep the
            // shader generator paired with that GPU so the app generates MSL for
            // Metal hosts and HLSL for Direct3D hosts, rather than selecting from
            // the emulated phone's nominal platform.
            if (serviceType == typeof(IGpuDevice) || serviceType == typeof(IShaderBackend))
                return platform.hostWindow.Actual.GetService(serviceType);

            // The mobile surface must not expose the desktop composition runtime.
            if (serviceType == typeof(Xui.Core.Actual.IRuntime))
                return null;

            return applicationServices.GetService(serviceType);
        }
    }
}
