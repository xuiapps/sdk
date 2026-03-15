using System;
using Xui.Core.Abstract;
using Xui.Core.Actual;
namespace Xui.Runtime.Android.Actual;

public class AndroidPlatform : IRuntime
{

    public AndroidDrawingContext AndroidDrawingContext { get; set; } = new AndroidDrawingContext();

    public IDispatcher MainDispatcher => throw new System.NotImplementedException();

    public IRunLoop CreateRunloop(Application applicationAbstract)
    {
        if (XuiApplication.Instance == null)
        {
            throw new Exception($"No instance of a {nameof(XuiApplication)} had been created.");            
        }

        if (XuiApplication.Instance.Abstract != null)
        {
            throw new Exception("Android platform can create only one run loop.");
        }

        XuiApplication.Instance.Abstract = applicationAbstract;
        return XuiApplication.Instance;
    }

    public Core.Actual.IWindow CreateWindow(Core.Abstract.IWindow windowAbstract)
    {
        if (XuiActivity.Instance == null)
        {
            throw new Exception($"No instance of a {nameof(XuiActivity)} had been created.");            
        }

        if (XuiActivity.Instance?.Abstract != null)
        {
            throw new Exception("Android platform can create only one window.");
        }

        XuiActivity.Instance!.Abstract = windowAbstract;
        return XuiActivity.Instance;
    }
}