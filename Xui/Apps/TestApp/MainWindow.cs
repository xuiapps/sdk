using Xui.Apps.TestApp.Pages;
using Xui.Core.Abstract;

namespace Xui.Apps.BlankApp;

public class MainWindow : Window
{
    public MainWindow(IServiceProvider context) : base(context)
    {
        this.Title = "Xui TestApp";
        this.Content = new SdkNavigation();
    }
}