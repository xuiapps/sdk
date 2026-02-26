using Xui.Core.DI;

namespace Xui.Apps.ClockApp;

public class Application : Xui.Core.Abstract.Application
{
    public Application(IServiceProvider context) : base(context) { }

    public override void Start() =>
        this.CreateAndShowMainWindowOnce<MainWindow>();
}
