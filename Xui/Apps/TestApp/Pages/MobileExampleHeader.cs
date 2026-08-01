using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages;

/// <summary>The persistent mobile example header with a centered title and back affordance.</summary>
public sealed class MobileExampleHeader : ViewCollection
{
    private static readonly NFloat BackButtonWidth = 84;
    private readonly MobileExampleBackButton backButton = new();
    private readonly Label title;

    public MobileExampleHeader(string text)
    {
        title = new Label
        {
            Text = text,
            FontFamily = "Inter",
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Middle,
        };
        Add(backButton);
        Add(title);
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        backButton.Measure((BackButtonWidth, available.Height), context);
        title.Measure(available, context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        backButton.Arrange((rect.X, rect.Y, BackButtonWidth, rect.Height), context);
        title.Arrange(rect, context);
    }

    protected override void RenderCore(IContext context)
    {
        var design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        title.TextColor = design?.Colors.Surface.Foreground ?? Colors.Black;

        // The header is laid out inside the safe area, but its opaque surface
        // belongs to the window chrome and must cover the area above it too.
        var background = this.Frame;
        if (this.TryFindParent<RootView>(out var rootView))
        {
            var displayArea = rootView.Window.DisplayArea;
            background = new Rect(
                displayArea.X,
                displayArea.Y,
                displayArea.Width,
                this.Frame.Bottom - displayArea.Y);
        }
        context.SetFill(Colors.White);
        context.FillRect(background);

        this.RenderChild(context, backButton);
        this.RenderChild(context, title);

        context.BeginPath();
        context.MoveTo(this.Frame.BottomLeft);
        context.LineTo(this.Frame.BottomRight);
        context.SetStroke(design?.Colors.OutlineVariant ?? Colors.LightGray);
        context.LineWidth = 1;
        context.Stroke();
    }
}
