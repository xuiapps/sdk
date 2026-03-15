using System.Collections.Generic;
using Xui.Core.Actual;
using Xui.Core.Canvas;

namespace Xui.Runtime.Windows.Actual;

public class Win32Platform : IRuntime
{
    private Win32RunLoop? win32RunLoop;
    private readonly List<Win32Window> windows = new();

    public Win32Platform()
    {
    }

    internal IReadOnlyList<Win32Window> Windows => this.windows;
    
    public IDispatcher MainDispatcher => win32RunLoop!;

    // NOTE: This will have to be thread static, if we want to render in multiple threads.
    public static Stack<IContext> DisplayContextStack { get; } = new Stack<IContext>();

    public IRunLoop CreateRunloop(Xui.Core.Abstract.Application applicationAbstract) => this.win32RunLoop = new Win32RunLoop(this, applicationAbstract);

    public Xui.Core.Actual.IWindow CreateWindow(Xui.Core.Abstract.IWindow windowAbstract)
    {
        var window = new Win32Window(this, windowAbstract);
        this.windows.Add(window);
        return window;
    }

    internal void RemoveWindow(Win32Window window)
    {
        this.windows.Remove(window);
    }
}