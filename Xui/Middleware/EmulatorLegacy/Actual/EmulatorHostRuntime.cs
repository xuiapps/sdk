using System;
using Xui.Core.Abstract;
using Xui.Core.Actual;

namespace Xui.Middleware.EmulatorLegacy.Actual;

/// <summary>Creates the one physical desktop window used by an emulator host.</summary>
internal sealed class EmulatorHostRuntime(IRuntime basePlatform) : IRuntime
{
    public IDispatcher MainDispatcher => basePlatform.MainDispatcher;

    public IRunLoop CreateRunloop(Application applicationAbstract) =>
        basePlatform.CreateRunloop(applicationAbstract);

    public Xui.Core.Actual.IWindow CreateWindow(Xui.Core.Abstract.IWindow windowAbstract, IServiceProvider applicationServices) =>
        basePlatform.CreateWindow(windowAbstract, applicationServices);
}
