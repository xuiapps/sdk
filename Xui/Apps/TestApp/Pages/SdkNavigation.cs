using Xui.Apps.TestApp.Examples;
using Xui.Core.Abstract;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp.Pages;

public class SdkNavigation : View, IExampleNavigation
{
    private View? content;

    private readonly View homePage;

    private View? currentExample;

    private View? Content
    {
        get => this.content;
        set => this.SetProtectedChild(ref this.content, value);
    }

    public override int Count => this.content is null ? 0 : 1;

    public override View this[int index] =>
        index == 0 && this.content is not null ? this.content : throw new IndexOutOfRangeException();

    public SdkNavigation(View? homePage = null)
    {
        this.homePage = homePage ?? new SdkHomePage();
        this.Content = this.homePage;
    }

    public void PopExample()
    {
        this.currentExample = null;
        this.Content = this.homePage;
    }

    public void PushExamplePage<TPage>() where TPage : Example, new()
    {
        this.currentExample = new SdkExamplePage<TPage>();
        this.Content = this.currentExample;
    }

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        if (this.TryFindParent<RootView>(out var rootView))
        {
            availableBorderEdgeSize = rootView.Window.SafeArea.Size;
        }

        return this.Content?.Measure(availableBorderEdgeSize, context) ?? (0, 0);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        if (this.TryFindParent<RootView>(out var rootView))
        {
            rect = rootView.Window.SafeArea;
        }

        this.Content?.Arrange(rect, context);
    }
}
