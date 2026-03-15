using System.Collections.Generic;
using Xui.Core.Actual;
using Xui.Core.Canvas;

namespace Xui.Runtime.MacOS.Actual;

public class MacOSPlatform : Xui.Core.Actual.IRuntime
{
    private MacOSRunLoop? macOSRunLoop;

    // NOTE: This will have to be thread static, if we want to render in multiple threads.
    internal static readonly Stack<IContext> DisplayContextStack = new Stack<IContext>();

    public MacOSPlatform()
    {
    }

    public IDispatcher MainDispatcher => macOSRunLoop!;

    public IRunLoop CreateRunloop(Xui.Core.Abstract.Application application) => this.macOSRunLoop = new MacOSRunLoop(application);

    public IWindow CreateWindow(Xui.Core.Abstract.IWindow window) => new MacOSWindow(window);
}
