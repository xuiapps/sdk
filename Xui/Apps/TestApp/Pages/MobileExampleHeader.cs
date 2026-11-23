using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
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
    private readonly MobileHomeActionButton fps = new("PageFPS", MobileHomeAction.Fps) { BackgroundOpacity = 0 };
    private MobileFrameRate? frameRate;

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
        Add(fps);
        fps.Clicked = () =>
        {
            if (this.TryFindParent<MobileNavigation>(out var navigation))
                navigation.SetPerformanceOverlayEnabled(!navigation.IsPerformanceOverlayEnabled);
        };
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        if (!this.TryFindParent<MobileNavigation>(out var navigation)) return;
        frameRate = navigation.FrameRate;
        frameRate.Changed += UpdateFrameRate;
        if (frameRate.IsWarmingUp) fps.StartWarmup(frameRate);
        if (frameRate.RoundedFramesPerSecond is { } rate) UpdateFrameRate(rate);
    }

    protected override void OnDeactivate()
    {
        if (frameRate is not null) frameRate.Changed -= UpdateFrameRate;
        frameRate = null;
        base.OnDeactivate();
    }

    private void UpdateFrameRate(int rate) => fps.SetFrameRate(rate);

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        backButton.Measure((BackButtonWidth, available.Height), context);
        title.Measure((NFloat.Max(0, available.Width - BackButtonWidth * 2), available.Height), context);
        fps.Measure((56, available.Height), context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        backButton.Arrange((rect.X, rect.Y, BackButtonWidth, rect.Height), context);
        title.Arrange((rect.X + BackButtonWidth, rect.Y, NFloat.Max(0, rect.Width - BackButtonWidth * 2), rect.Height), context);
        fps.Arrange((rect.Right - 56, rect.Y, 56, rect.Height), context);
    }

    protected override void RenderCore(IContext context)
    {
        var design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        title.TextColor = design?.Colors.Surface.Foreground ?? Colors.Black;

        // The header is laid out inside the safe area, but its blurred surface
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
        using var surface = context.Group(new GroupOptions
        {
            Key = this, ViewBacking = true, Bounds = background, Backdrop = BackdropMaterial.Blur,
            HasUpdates = RequiresRender(context),
        });
        if (!surface.NeedsRedraw) return;

        this.RenderChild(context, backButton);
        this.RenderChild(context, title);
        this.RenderChild(context, fps);

        context.BeginPath();
        context.MoveTo(this.Frame.BottomLeft);
        context.LineTo(this.Frame.BottomRight);
        context.SetStroke(design?.Colors.OutlineVariant ?? Colors.LightGray);
        context.LineWidth = 1;
        context.Stroke();
    }
}
