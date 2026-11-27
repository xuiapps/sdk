using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using Xui.Core.Abstract;
using Xui.Core.Actual;

namespace Xui.Runtime.Browser.Actual;

/// <summary>The .NET WebAssembly runtime adapter for a browser Canvas surface.</summary>
public partial class BrowserPlatform : IRuntime
{
    public IDispatcher MainDispatcher { get; } = new BrowserDispatcher();

    public static async Task InitializeAsync(string mode = ".NET WebAssembly")
    {
        using var document = JSHost.GlobalThis.GetPropertyAsJSObject("document")
            ?? throw new PlatformNotSupportedException("A browser document is required.");
        var moduleUrl = new Uri(new Uri(document.GetPropertyAsString("baseURI")!), "xui-browser.js");
        await JSHost.ImportAsync("xui-browser", moduleUrl.AbsoluteUri);
        await Initialize(mode);
    }

    [JSImport("initialize", "xui-browser")]
    private static partial Task Initialize(string mode);

    public IRunLoop CreateRunloop(Application applicationAbstract) => new BrowserRunLoop(applicationAbstract);

    public Core.Actual.IWindow CreateWindow(Core.Abstract.IWindow windowAbstract, IServiceProvider applicationServices)
    {
        if (BrowserWindow.Instance != null)
            throw new NotSupportedException("The browser host supports one top-level window per canvas.");
        return BrowserWindow.Instance = new BrowserWindow(windowAbstract, applicationServices);
    }

    private sealed class BrowserDispatcher : IDispatcher
    {
        public void Post(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            callbacks.Enqueue(callback);
            ScheduleDispatch();
        }
    }

    // This host uses the single-threaded browser WASM runtime. A browser task is
    // the UI dispatcher even when SynchronizationContext.Current is null.
    private static readonly Queue<Action> callbacks = new();
    [JSImport("scheduleDispatch", "xui-browser")]
    private static partial void ScheduleDispatch();
    [JSExport]
    internal static void Dispatch()
    {
        var count = callbacks.Count;
        for (var i = 0; i < count; i++) callbacks.Dequeue()();
    }
}
