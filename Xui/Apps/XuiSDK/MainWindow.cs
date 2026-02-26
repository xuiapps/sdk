using System.Runtime.InteropServices;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using static Xui.Core.Abstract.IWindow.IDesktopStyle;

namespace Xui.Apps.XuiSDK;

public class MainWindow : Xui.Core.Abstract.Window, IWindow.IDesktopStyle
{
    private NFloat HeaderHeight = 48;
    
    WindowBackdrop IWindow.IDesktopStyle.Backdrop => WindowBackdrop.Mica;
    Size? IWindow.IDesktopStyle.StartupSize => new Size(900, 600);
    WindowClientArea IWindow.IDesktopStyle.ClientArea => WindowClientArea.Extended;

    public MainWindow(IServiceProvider context) : base(context)
    {
        Content = new NavigationShell();
    }

    public override void WindowHitTest(ref WindowHitTestEventRef evRef)
    {
        if (evRef.Point.Y < HeaderHeight)
            evRef.Area = WindowHitTestEventRef.WindowArea.Title;
    }
}
