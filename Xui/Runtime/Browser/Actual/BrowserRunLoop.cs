using Xui.Core.Abstract;

namespace Xui.Runtime.Browser.Actual;

public partial class BrowserRunLoop : Xui.Core.Actual.IRunLoop
{
    public Application Abstract { get; }

    public BrowserRunLoop(Application applicationAbstract)
    {
        this.Abstract = applicationAbstract;
    }

    public int Run()
    {
        this.Abstract.Start();
        return 0;
    }

    public void Quit() { }
}
