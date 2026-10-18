using System;
using Xui.Core.Actual;

namespace Xui.Middleware.EmulatorLegacy.Actual;

/// <summary>Overrides only the runtime while retaining the app's window service scope.</summary>
internal sealed class EmulatorHostServiceProvider(
    IServiceProvider inner,
    IRuntime runtime,
    EmulatorWindow emulator) : IServiceProvider
{
    public EmulatorWindow Emulator { get; } = emulator;

    public object? GetService(Type serviceType) =>
        serviceType == typeof(IRuntime) ? runtime : inner.GetService(serviceType);
}
