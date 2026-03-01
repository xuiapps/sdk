using Xui.Core.Abstract;
using Xui.Core.UI;

namespace NewBlankApp;

public class MainWindow : Window
{
    public MainWindow(IServiceProvider context) : base(context)
    {
        this.Title = "New Blank App";
        this.Content = new HeartView
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Middle
        };
    }
}
