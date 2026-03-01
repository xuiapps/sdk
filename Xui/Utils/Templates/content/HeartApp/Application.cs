using Xui.Core.DI;

namespace NewBlankApp;

public class Application : Xui.Core.Abstract.Application
{
    public Application(IServiceProvider context) : base(context)
    {
    }

    public override void Start() =>
        this.CreateAndShowMainWindowOnce<MainWindow>();
}
