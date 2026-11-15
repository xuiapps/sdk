using Android.Runtime;

namespace Xui.Runtime.Android.Actual;

/// <summary>Compatibility application for existing .NET Android app hosts.</summary>
public class XuiApplication : global::Android.App.Application
{
    public XuiApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) { }
}
