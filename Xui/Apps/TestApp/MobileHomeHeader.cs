using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

/// <summary>A viewport overlay whose retained children move without being measured again.</summary>
internal sealed class MobileHomeHeader : ViewCollection
{
    public const int ExpandedHeight = 248;
    private readonly MobileFrameRate frameRate;
    private readonly MobileHomeHeaderMotion motion = new();
    private readonly object backdropKey = new();
    private readonly TransformView logo;
    private readonly TransformView title;
    private readonly TransformView heart;
    private readonly TransformView fps;
    private readonly TransformView settings;
    private IOverlay? ratingOverlay;
    private int rating;
    private NFloat lastProgress = -1;
    private NFloat lastTranslation;
    private Rect lastFrame;

    public MobileHomeHeader(MobileFrameRate frameRate)
    {
        this.frameRate = frameRate;
        Id = "HomeHeader";
        logo = Wrap(new XuiLogoView { Id = "HomeLogo" });
        title = Wrap(new Label
        {
            Id = "HomeTitle", Text = "Xui SDK Examples", FontFamily = "Inter", FontSize = 24,
            FontWeight = FontWeight.Bold, TextColor = new Color(0x242529FF),
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Middle,
        });
        heart = Wrap(new MobileHomeActionButton("RateApp", MobileHomeAction.Heart) { Clicked = ShowRating });
        fps = Wrap(new MobileHomeActionButton("HomeFPS", MobileHomeAction.Fps)
        {
            Clicked = () =>
            {
                if (this.TryFindParent<MobileNavigation>(out var navigation))
                    navigation.SetPerformanceOverlayEnabled(!navigation.IsPerformanceOverlayEnabled);
            },
        });
        settings = Wrap(new MobileSettingsButton());
    }

    private TransformView Wrap(View view)
    {
        var host = new TransformView { Content = view, UseCompositionGroup = true };
        Add(host);
        return host;
    }

    public void SetScrollOffset(NFloat offset)
    {
        motion.Scroll(offset);
        UpdateTransforms();
        RequestAnimationFrame();
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        var reduced = GetService(typeof(IDeviceInfo)) is IDeviceInfo { PrefersReducedMotion: true };
        if (motion.Advance(current, reduced)) RequestAnimationFrame();
        UpdateTransforms();
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        logo.Measure((64, 64), context);
        title.Measure((available.Width, 40), context);
        heart.Measure((48, 48), context);
        fps.Measure((56, 48), context);
        settings.Measure((48, 48), context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var x = rect.Center.X;
        logo.Arrange((x - 32, rect.Y + 24, 64, 64), context);
        title.Arrange((rect.X, rect.Y + 104, rect.Width, 40), context);
        heart.Arrange((x - 88, rect.Y + 174, 48, 48), context);
        fps.Arrange((x - 28, rect.Y + 174, 56, 48), context);
        settings.Arrange((x + 40, rect.Y + 174, 48, 48), context);
        lastProgress = -1;
        UpdateTransforms();
    }

    private void UpdateTransforms()
    {
        if (Frame.Width <= 0) return;
        if (lastProgress == motion.Progress && lastTranslation == motion.Translation && lastFrame == Frame) return;
        lastProgress = motion.Progress;
        lastTranslation = motion.Translation;
        lastFrame = Frame;
        var p = Easing.EaseInOutCubic(motion.Progress);
        Move(logo, (Frame.X + 26, Frame.Y + 28), 34f / 64, p, 1.125f);
        ((MobileHomeActionButton)heart.Content!).BackgroundOpacity = 1 - p;
        ((MobileHomeActionButton)fps.Content!).BackgroundOpacity = 1 - p;
        ((MobileSettingsButton)settings.Content!).BackgroundOpacity = 1 - p;
        Move(heart, (Frame.X + 68, Frame.Y + 28), 1, p);
        Move(fps, (Frame.Right - 80, Frame.Y + 28), 1, p);
        Move(settings, (Frame.Right - 28, Frame.Y + 28), 1, p);
        var textWidth = title.Content!.Frame.Width;
        // Reserve full-size touch targets on both sides of the centered title.
        var scale = NFloat.Min(16f / 24, NFloat.Max(0.25f, (Frame.Width - 224) / NFloat.Max(1, textWidth)));
        Move(title, (Frame.Center.X, Frame.Y + 28), scale, p);
        InvalidateRender();
        InvalidateHitTest();
    }

    private void Move(TransformView host, Point target, NFloat targetScale, NFloat p, NFloat initialScale = default)
    {
        var source = host.Content!.Frame.Center;
        if (initialScale == 0) initialScale = 1;
        var scale = initialScale + (targetScale - initialScale) * p;
        var x = source.X + (target.X - source.X) * p;
        var y = source.Y + motion.Translation + (target.Y - source.Y - motion.Translation) * p;
        host.Transform = new AffineTransform(scale, 0, 0, scale, x - source.X * scale, y - source.Y * scale);
    }

    public override bool HitTest(Point point) =>
        point.X >= Frame.X && point.X <= Frame.Right && point.Y >= Frame.Y &&
        point.Y <= Frame.Y + (ExpandedHeight + motion.Translation) * (1 - Easing.EaseInOutCubic(motion.Progress)) + 56 * Easing.EaseInOutCubic(motion.Progress);

    protected override void RenderCore(IContext context)
    {
        var p = Easing.EaseInOutCubic(motion.Progress);
        if (p > 0)
        {
            var top = Frame.Y;
            if (this.TryFindParent<RootView>(out var root))
                top = GlobalToLocal(root.Window.DisplayArea.TopLeft).Y;
            var background = new Rect(Frame.X, top, Frame.Width, Frame.Y + 56 - top);
            using var surface = context.Group(new GroupOptions
            {
                Key = backdropKey, ViewBacking = true, Bounds = background,
                Backdrop = p >= 0.95f ? BackdropMaterial.Blur : BackdropMaterial.None,
                Background = new Color(1, 1, 1, p * 0.85f), HasUpdates = true,
            });
            if (surface.NeedsRedraw)
            {
                context.SetStroke(new Color(0.78f, 0.8f, 0.83f, p * 0.6f));
                context.LineWidth = 1;
                context.BeginPath();
                context.MoveTo(background.BottomLeft); context.LineTo(background.BottomRight); context.Stroke();
            }
        }
        base.RenderCore(context);
    }

    private void ShowRating()
    {
        ratingOverlay ??= GetService(typeof(IOverlay)) as IOverlay;
        if (ratingOverlay is null) return;
        var button = heart.Content!;
        var bounds = this.TryFindParent<RootView>(out var root) ? root.Window.DisplayArea :
            new Rect(LocalToGlobal(Frame.TopLeft), Frame.Size);
        var width = NFloat.Min(320, NFloat.Max(0, bounds.Width - 24));
        var anchor = new Rect(bounds.Center.X - width / 2, button.LocalToGlobal(button.Frame.TopLeft).Y, width, button.Frame.Height);
        var panel = new MobileRatingPanel(rating, value => rating = value, CloseRating, GetService(typeof(IDesignSystem)) as IDesignSystem);
        ratingOverlay.Show(panel, anchor, size: (width, 208));
    }

    public void CloseRating() => ratingOverlay?.Close();

    protected override void OnActivate()
    {
        base.OnActivate();
        frameRate.Changed += UpdateFrameRate;
        if (frameRate.IsWarmingUp) ((MobileHomeActionButton)fps.Content!).StartWarmup(frameRate);
        if (frameRate.RoundedFramesPerSecond is { } rate) UpdateFrameRate(rate);
    }

    private void UpdateFrameRate(int rate) => ((MobileHomeActionButton)fps.Content!).SetFrameRate(rate);

    protected override void OnDeactivate()
    {
        frameRate.Changed -= UpdateFrameRate;
        CloseRating();
        base.OnDeactivate();
    }
}
