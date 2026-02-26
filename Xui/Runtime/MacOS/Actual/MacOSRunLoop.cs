using System;
using System.Threading;
using Xui.Runtime.MacOS;
using static Xui.Runtime.MacOS.AppKit;
using static Xui.Runtime.MacOS.Block;

namespace Xui.Runtime.MacOS.Actual;

public class MacOSRunLoop : Xui.Core.Actual.IRunLoop, Xui.Core.Actual.IDispatcher
{
    private SynchronizationContext synchronizationContext;

    protected Xui.Core.Abstract.Application Application { get; }

    public MacOSRunLoop(Xui.Core.Abstract.Application application)
    {
        this.synchronizationContext = new MacOSSynchronizationContext(this);
        SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);
        this.Application = application;
    }

    public int Run()
    {
        var nsAppRef = NSApplication.SharedApplication!;
        nsAppRef.SetActivationPolicy(NSApplicationActivationPolicy.Regular);

        using var applicationDelegate = new MacOSApplicationDelegate(this.Application);
        nsAppRef.Delegate = applicationDelegate;

        nsAppRef.Activate();

        nsAppRef.Run();
        this.Application.OnExit();
        return 0;
    }

    public void Quit() => NSApplication.SharedApplication?.Terminate();

    public void Post(Action callback)
    {
        var blockRef = new BlockRef(callback);
        Dispatch.DispatchAsync(Dispatch.MainQueue, blockRef);
    }
}