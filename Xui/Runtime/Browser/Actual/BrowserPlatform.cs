using System;
using System.Runtime.InteropServices.JavaScript;
using Xui.Core.Abstract;
using Xui.Core.Actual;
namespace Xui.Runtime.Browser.Actual;

public partial class BrowserPlatform : Xui.Core.Actual.IRuntime
{

    public IDispatcher MainDispatcher => throw new System.NotImplementedException();

    public IRunLoop CreateRunloop(Application applicationAbstract) => new BrowserRunLoop(applicationAbstract);

    public Core.Actual.IWindow CreateWindow(Core.Abstract.IWindow windowAbstract)
    {
        if (BrowserWindow.Instance != null)
        {
            throw new Exception("Only one instance of Window is supported in Browser Xui App.");
        }

        BrowserWindow.Instance = new BrowserWindow(windowAbstract);
        return BrowserWindow.Instance;
    }

    // [JSImport("dom.setInnerText", "main.js")]
    // internal static partial void SetInnerText(string selector, string content);
}
