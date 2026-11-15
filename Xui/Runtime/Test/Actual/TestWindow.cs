using Xui.Core.Canvas;

namespace Xui.Runtime.Test.Actual;

public class TestWindow : Xui.Core.Actual.IWindow
{
    public Xui.Core.Abstract.IWindow Abstract { get; }
    internal bool Invalid { get; set; }
    private readonly TestPlatform platform;
    private readonly Xui.Runtime.Software.Actual.SoftwareImagePipeline images = new();

    public TestWindow(TestPlatform platform, Xui.Core.Abstract.IWindow abstractWindow, IServiceProvider nextServiceProvider)
    {
        this.platform = platform;
        this.Abstract = abstractWindow;
        this.NextServiceProvider = nextServiceProvider;
    }

    public string Title { get; set; } = "";

    public bool RequireKeyboard { get; set; }

    public ITextMeasureContext? TextMeasureContext { get; set; }

    public IServiceProvider NextServiceProvider { get; }

    public void Show()
    {
    }

    public void Invalidate()
    {
        Invalid = true;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IImage)) return images.CreateImage();
        if (serviceType == typeof(Xui.Core.Actual.IImagePipeline)) return images;
        return this.NextServiceProvider.GetService(serviceType);
    }
}
