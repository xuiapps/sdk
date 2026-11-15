using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp;

/// <summary>A local rating chooser. No store review is submitted by this demo.</summary>
internal sealed class MobileRatingPanel : ViewCollection
{
    private readonly Label title = new() { Text = "Rate this app", FontFamily = "Inter", FontSize = 22, FontWeight = FontWeight.Bold };
    private readonly Label status = new() { Text = "Choose 1 to 5 stars", FontFamily = "Inter", FontSize = 14, TextColor = new Color(0x626975FF) };
    private readonly MobileHomeActionButton[] stars = new MobileHomeActionButton[5];
    private readonly Button done;
    private readonly IDesignSystem? design;

    public MobileRatingPanel(int rating, Action<int> changed, Action close, IDesignSystem? design)
    {
        this.design = design;
        Id = "RatingPanel";
        Add(title); Add(status);
        for (var i = 0; i < stars.Length; i++)
        {
            var value = i + 1;
            stars[i] = new MobileHomeActionButton($"Rating{value}", MobileHomeAction.Star)
            {
                Clicked = () => { Select(value); changed(value); },
            };
            Add(stars[i]);
        }
        done = new Button { Id = "RatingDone", Text = "Done", Clicked = close };
        Add(done);
        if (rating > 0) Select(rating);
    }

    public override object? GetService(Type type) => type == typeof(IDesignSystem) ? design : base.GetService(type);

    private void Select(int value)
    {
        for (var i = 0; i < stars.Length; i++) stars[i].Selected = i < value;
        status.Text = value == 1 ? "1 of 5 stars" : $"{value} of 5 stars";
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        title.Arrange((rect.X + 20, rect.Y + 20, rect.Width - 40, 32), context);
        status.Arrange((rect.X + 20, rect.Y + 56, rect.Width - 40, 24), context);
        var step = (rect.Width - 32) / 5;
        for (var i = 0; i < stars.Length; i++) stars[i].Arrange((rect.X + 16 + step * i, rect.Y + 88, step, 48), context);
        done.Arrange((rect.X + 20, rect.Y + 152, rect.Width - 40, 40), context);
    }

    protected override void RenderCore(IContext context)
    {
        context.BeginPath(); context.RoundRect(Frame.Expand(-1), 20);
        context.SetFill(Colors.White); context.Fill();
        context.SetStroke(new Color(0xCDD1D8FF)); context.LineWidth = 1; context.Stroke();
        base.RenderCore(context);
    }
}
