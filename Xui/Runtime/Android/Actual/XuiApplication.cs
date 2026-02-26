using System;
using Android.Content;
using Android.Runtime;

namespace Xui.Runtime.Android.Actual;

// [Application]
public class XuiApplication : global::Android.App.Application, Xui.Core.Actual.IRunLoop
{
    public static XuiApplication? Instance { get; private set; }
    public Xui.Core.Abstract.Application? Abstract { get; internal set; }

    public XuiApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
        if (Instance != null)
        {
            throw new Exception($"There may by only one instance of {nameof(XuiApplication)}.");
        }

        Instance = this;
	}

    public override void OnCreate()
    {
        base.OnCreate();
    }

    public override void OnTerminate()
    {
        base.OnTerminate();
    }

    public override void OnLowMemory()
    {
        base.OnLowMemory();
    }

    public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
    {
        base.OnTrimMemory(level);
    }

    public int Run()
    {
        this.Abstract?.Start();
        return 0;
    }

    public void Quit() { }
}
