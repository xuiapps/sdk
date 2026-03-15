using Xui.Core.Canvas;

namespace Xui.Runtime.Test.Actual;

public class TestWindow : Xui.Core.Actual.IWindow
{
    internal Xui.Core.Abstract.IWindow Abstract { get; }
    internal bool Invalid { get; set; }
    private readonly TestPlatform platform;

    public TestWindow(TestPlatform platform, Xui.Core.Abstract.IWindow abstractWindow)
    {
        this.platform = platform;
        this.Abstract = abstractWindow;
    }

    public string Title { get; set; } = "";

    public bool RequireKeyboard { get; set; }

    public ITextMeasureContext? TextMeasureContext { get; set; }

    public void Show()
    {
    }

    public void Invalidate()
    {
        Invalid = true;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IContext)) return this.platform.CurrentDrawingContext;
        return null;
    }
}
