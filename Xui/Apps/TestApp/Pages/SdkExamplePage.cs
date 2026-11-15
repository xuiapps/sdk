using Xui.Apps.TestApp.Examples;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages;

public class SdkExamplePage<T> : ViewCollection
    where T : Example, new()
{
    private T content;
    private View header;

    public SdkExamplePage() : this(new T()) { }

    public SdkExamplePage(T content)
    {
        this.content = content;

        this.header = new Border()
        {
            BorderThickness = (0, 0, 0, 0),
            BorderColor = Red,
            BackgroundColor = White,
            Content = new HorizontalStack()
            {
                Content =
                [
                    new SdkExampleBackButton(),
                    new Label()
                    {
                        Margin = (0, 0, 0, 24),
                        FontFamily = "Inter",
                        Text = this.content.Title
                    }
                ]
            }
        };

        this.Add(this.content);
        this.Add(this.header);
    }

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        var headerHeight = 24;

        this.header.Measure((availableBorderEdgeSize.Width, headerHeight), context);
        return this.content.Measure(availableBorderEdgeSize - (0, headerHeight), context) + new Size(0, headerHeight);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var headerHeight = 24;

        this.header.Arrange((rect.X, rect.Y, rect.Width, headerHeight), context);
        this.content.Arrange((rect.X, rect.Y + headerHeight, rect.Width, rect.Height - headerHeight), context);
    }
}
