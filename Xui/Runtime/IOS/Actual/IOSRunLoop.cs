using System;
using System.Threading;
using static Xui.Runtime.IOS.CoreFoundation;

namespace Xui.Runtime.IOS.Actual;

public class IOSRunLoop : Xui.Core.Actual.IRunLoop, Xui.Core.Actual.IDispatcher
{
    private SynchronizationContext synchronizationContext;

    protected Xui.Core.Abstract.Application Application { get; }

    public IOSRunLoop(Xui.Core.Abstract.Application application)
    {
        this.synchronizationContext = new IOSSynchronizationContext(this);
        SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);
        this.Application = application;
    }

    public void Post(Action callback)
    {
        throw new NotImplementedException();
    }

    public static bool ApplicationDidFinishLaunchingWithOptions(nint self, nint sel, nint app, nint opt)
    {
        return true;
    }

    public int Run()
    {
        IOSApplicationDelegate.ApplicationOnStack = this.Application;
        using var nsAppDelegateName = new CFStringRef(IOSApplicationDelegate.Class.Name);
        return Xui.Runtime.IOS.UIKit.UIApplicationMain(0, 0, 0, nsAppDelegateName);
    }

    public void Quit() { }
}