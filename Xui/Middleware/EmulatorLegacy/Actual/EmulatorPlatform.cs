using System;
using Xui.Core.Abstract;
using Xui.Core.Actual;
namespace Xui.Middleware.EmulatorLegacy.Actual;

/// <summary>
/// A runtime middleware layer that intercepts platform calls to inject emulator behavior.
///
/// This class wraps a base platform implementation (e.g., Windows or macOS)
/// and adapts it to simulate a mobile device environment for desktop testing.
/// </summary>
public class EmulatorPlatform : IRuntime
{
    internal IRuntime BasePlatform;

    /// <summary>
    /// Gets the main thread dispatcher from the base platform.
    /// </summary>
    public IDispatcher MainDispatcher => this.BasePlatform.MainDispatcher;

    /// <summary>
    /// Initializes a new <see cref="EmulatorPlatform"/> that wraps the specified base platform runtime.
    /// </summary>
    /// <param name="basePlatform">The platform-specific runtime to wrap (e.g., Windows, macOS).</param>
    public EmulatorPlatform(IRuntime basePlatform)
    {
        this.BasePlatform = basePlatform;
    }

    /// <summary>
    /// Forwards run loop creation directly to the base platform.
    /// </summary>
    /// <param name="applicationAbstract">The abstract application instance.</param>
    /// <returns>The native run loop for the base platform.</returns>
    public IRunLoop CreateRunloop(Application applicationAbstract) =>
        this.BasePlatform.CreateRunloop(applicationAbstract);

    /// <summary>
    /// Intercepts window creation to insert a simulated mobile emulator window between the abstract and platform layers.
    /// The created window wraps the base platform window with an <see cref="EmulatorWindow"/>,
    /// allowing for input redirection, visual chrome, and runtime controls (e.g., orientation switching).
    /// </summary>
    /// <param name="windowAbstract">The abstract window defined by the application.</param>
    /// <param name="applicationServices">The window/application service chain reached after platform services.</param>
    /// <returns>An actual window with emulator middleware applied.</returns>
    public Xui.Core.Actual.IWindow CreateWindow(Xui.Core.Abstract.IWindow windowAbstract, IServiceProvider applicationServices)
    {
        var emulator = new EmulatorWindow(windowAbstract, applicationServices);
        var hostRuntime = new EmulatorHostRuntime(this.BasePlatform);
        var hostServices = new EmulatorHostServiceProvider(applicationServices, hostRuntime, emulator);
        var host = new EmulatorHostWindow(hostServices, emulator);
        emulator.AttachHost(host);
        return emulator;
    }
}
