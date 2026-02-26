using Xui.Core.Abstract;
using Xui.Core.Math2D;
using static Xui.Core.Abstract.IWindow.IDesktopStyle;

namespace Xui.Apps.XuiSDK;

public class MainWindow : Xui.Core.Abstract.Window, IWindow.IDesktopStyle
{
    WindowBackdrop IWindow.IDesktopStyle.Backdrop => WindowBackdrop.Mica;
    Size? IWindow.IDesktopStyle.StartupSize => new Size(900, 600);
    WindowClientArea IWindow.IDesktopStyle.ClientArea => WindowClientArea.Extended;

    public MainWindow(IServiceProvider context) : base(context)
    {
        Content = new NavigationShell();
    }
}
