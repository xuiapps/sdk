using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.UI.HorizontalAlignment;
using static Xui.Core.UI.VerticalAlignment;

namespace Xui.Apps.TestApp.Examples;

public class ViewCollectionAlignmentExample : Example
{
    public ViewCollectionAlignmentExample()
    {
        this.Title = "ViewCollection Alignment";

        this.Content = new ViewCollection
        {
            Margin = 8,
            Content =
            [
                Aligned("Left", Left, VerticalAlignment.Stretch, 0),
                Aligned("Center", Center, VerticalAlignment.Stretch, 0),
                Aligned("Right", Right, VerticalAlignment.Stretch, 0),

                Aligned("Top", HorizontalAlignment.Stretch, Top, 0),
                Aligned("Middle", HorizontalAlignment.Stretch, Middle, 0),
                Aligned("Bottom", HorizontalAlignment.Stretch, Bottom, 0),

                Aligned("Top Left", Left, Top, 24),
                Aligned("Top Center", Center, Top, 24),
                Aligned("Top Right", Right, Top, 24),

                Aligned("Middle Left", Left, Middle, 24),
                Aligned("Middle Center", Center, Middle, 24),
                Aligned("Middle Right", Right, Middle, 24),

                Aligned("Bottom Left", Left, Bottom, 24),
                Aligned("Bottom Center", Center, Bottom, 24),
                Aligned("Bottom Right", Right, Bottom, 24),
            ]
        };
    }

    private static View Aligned(string label, HorizontalAlignment h, VerticalAlignment v, Frame margin) =>
        new Border()
        {
            BorderColor = Black,
            BorderThickness = 1,
            CornerRadius = 5,
            Margin = margin,
            BackgroundColor = White,
            HorizontalAlignment = h,
            VerticalAlignment = v,
            Content =
            new Label
            {
                FontFamily = "Inter",
                Text = label,
                FontSize = 12,
                HorizontalAlignment = Center,
                VerticalAlignment = Middle
            }
        };
}
