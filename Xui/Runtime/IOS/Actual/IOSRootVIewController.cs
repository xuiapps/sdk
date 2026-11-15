using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.UIKit;

namespace Xui.Runtime.IOS.Actual;

public class IOSRootViewController : UIViewController
{
    public static new readonly Class Class = UIViewController.Class
        .Extend("XUIIOSRootViewController")
        .AddMethod("preferredStatusBarStyle", (StatusBarStyleCallback)PreferredStatusBarStyle, "q@:")
        .Register();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint StatusBarStyleCallback(nint self, nint sel);

    private static nint PreferredStatusBarStyle(nint self, nint sel) =>
        Marshalling.Get<IOSRootViewController>(self).Window.Appearance switch
        {
            Xui.Core.Actual.StatusBarAppearance.Light => 1, // UIStatusBarStyleLightContent
            Xui.Core.Actual.StatusBarAppearance.Dark => 3, // UIStatusBarStyleDarkContent
            _ => 0, // UIStatusBarStyleDefault: adapts to the current interface style.
        };

    public IOSRootViewController(IOSWindow window) : base(Class.New())
    {
        this.Window = window;
    }
    
    protected internal IOSWindow Window { get; }
}