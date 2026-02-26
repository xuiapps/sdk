using Xui.Core.Actual;

namespace Xui.Runtime.Test.Actual;

public class TestRunLoop : IRunLoop
{
    private readonly TestPlatform platform;
    private readonly Xui.Core.Abstract.Application application;

    public TestRunLoop(TestPlatform platform, Xui.Core.Abstract.Application application)
    {
        this.platform = platform;
        this.application = application;
    }

    public int Run()
    {
        this.application.Start();
        return 0;
    }

    public void Quit() { }
}
