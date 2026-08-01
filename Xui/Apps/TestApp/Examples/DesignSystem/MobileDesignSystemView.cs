using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Widgets;
using static Xui.Core.Canvas.FontWeight;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Phone-specific palette and preview pages for the design-system demo.</summary>
internal sealed class MobileDesignSystemView : View
{
    private readonly View palettePage;
    private readonly View previewPage;
    private readonly MobileDesignSystemToolbar toolbar;
    private int pageIndex;
    private int outgoingPageIndex;
    private TimeSpan? transitionStart;
    private NFloat transitionProgress = 1;

    public MobileDesignSystemView()
    {
        palettePage = CreatePalettePage();
        previewPage = CreatePreviewPage();
        toolbar = new MobileDesignSystemToolbar(SelectPage);
        AddProtectedChild(palettePage);
        AddProtectedChild(previewPage);
        AddProtectedChild(toolbar);
    }

    public override int Count => 2;

    public override View this[int index] => index switch
    {
        0 => pageIndex == 0 ? palettePage : previewPage,
        1 => toolbar,
        _ => throw new IndexOutOfRangeException(),
    };

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        palettePage.Measure(available, context);
        previewPage.Measure(available, context);
        toolbar.Measure((available.Width - 32, 48), context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var pageRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height - 64);
        palettePage.Arrange(pageRect, context);
        previewPage.Arrange(pageRect, context);
        toolbar.Arrange(new Rect(rect.X + 16, rect.Bottom - 56, rect.Width - 32, 48), context);
    }

    protected override void RenderCore(IContext context)
    {
        var pageRect = new Rect(this.Frame.X, this.Frame.Y, this.Frame.Width, this.Frame.Height - 64);
        context.Save();
        try
        {
            context.BeginPath();
            context.Rect(pageRect);
            context.Clip();

            if (transitionStart is null)
            {
                CurrentPage.Render(context);
            }
            else
            {
                var distance = pageRect.Width;
                var incomingOffset = pageIndex == 1 ? (1 - transitionProgress) * distance : -(1 - transitionProgress) * distance;
                var outgoingOffset = pageIndex == 1 ? -transitionProgress * distance : transitionProgress * distance;
                context.Save();
                context.Translate((outgoingOffset, 0));
                OutgoingPage.Render(context);
                context.Restore();
                context.Save();
                context.Translate((incomingOffset, 0));
                CurrentPage.Render(context);
                context.Restore();
            }
        }
        finally
        {
            context.Restore();
        }

        toolbar.SelectedPage = pageIndex;
        toolbar.Render(context);
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        if (transitionStart is null)
            return;

        var progress = NFloat.Clamp((NFloat)((current - transitionStart.Value) / TimeSpan.FromMilliseconds(260)), 0, 1);
        transitionProgress = new Easing.CubicBezier((0.32f, 0.72f), (0f, 1f)).Evaluate(progress, 0.0005f);
        InvalidateRender();
        if (progress < 1)
        {
            RequestAnimationFrame();
            return;
        }

        transitionStart = null;
        transitionProgress = 1;
    }

    private View CurrentPage => pageIndex == 0 ? palettePage : previewPage;
    private View OutgoingPage => outgoingPageIndex == 0 ? palettePage : previewPage;

    private void SelectPage(int index)
    {
        if (index == pageIndex)
            return;

        outgoingPageIndex = pageIndex;
        pageIndex = index;
        transitionProgress = 0;
        transitionStart = null;
        RequestAnimationFrame();
        InvalidateRender();
    }

    private static View CreatePalettePage() => new ScrollView
    {
        Direction = ScrollDirection.Vertical,
        Content = new VerticalStack
        {
            Margin = 12,
            Content =
            [
                new Card { Margin = (0, 0, 12, 0), Content = new VerticalStack { Content = [
                    new Label { Text = "Palette", FontSize = 18, FontWeight = SemiBold, Margin = (0, 0, 12, 0) },
                    new ColorWheelView { HorizontalAlignment = HorizontalAlignment.Center },
                    new Label { Text = "Harmony", FontSize = 14, FontWeight = SemiBold, Margin = (8, 0, 4, 0) },
                    new HarmonyListView(),
                ] } },
                new Card { Margin = (0, 0, 12, 0), Content = new VerticalStack { Content = [
                    new Label { Text = "Surface Style", FontSize = 14, FontWeight = SemiBold, Margin = (0, 0, 8, 0) },
                    new MobilePresetGrid(MobilePresetVisual.SurfaceStyle, [
                        ("Mono", e => e.NeutralStyle == NeutralStyle.Monochrome, e => e.SetNeutralStyle(NeutralStyle.Monochrome)),
                        ("2nd App", e => e.NeutralStyle == NeutralStyle.SecondaryApp, e => e.SetNeutralStyle(NeutralStyle.SecondaryApp)),
                        ("2nd Surface", e => e.NeutralStyle == NeutralStyle.SecondarySurface, e => e.SetNeutralStyle(NeutralStyle.SecondarySurface)),
                        ("3rd App", e => e.NeutralStyle == NeutralStyle.TertiaryApp, e => e.SetNeutralStyle(NeutralStyle.TertiaryApp)),
                        ("3rd Surface", e => e.NeutralStyle == NeutralStyle.TertiarySurface, e => e.SetNeutralStyle(NeutralStyle.TertiarySurface)),
                    ]),
                ] } },
                new Card { Margin = (0, 0, 12, 0), Content = new VerticalStack { Content = [
                    new Label { Text = "Shape", FontSize = 14, FontWeight = SemiBold, Margin = (0, 0, 8, 0) },
                    new MobilePresetGrid(MobilePresetVisual.Shape, [
                        ("Square", e => e.ShapePreset == ShapePreset.Square, e => e.SetShapePreset(ShapePreset.Square)),
                        ("Desktop", e => e.ShapePreset == ShapePreset.Desktop, e => e.SetShapePreset(ShapePreset.Desktop)),
                        ("Rounded", e => e.ShapePreset == ShapePreset.Rounded, e => e.SetShapePreset(ShapePreset.Rounded)),
                        ("Pill", e => e.ShapePreset == ShapePreset.RoundedPill, e => e.SetShapePreset(ShapePreset.RoundedPill)),
                        ("Soft", e => e.ShapePreset == ShapePreset.Soft, e => e.SetShapePreset(ShapePreset.Soft)),
                    ]),
                ] } },
                new Card { Margin = (0, 0, 12, 0), Content = new VerticalStack { Content = [
                    new Label { Text = "Sizing", FontSize = 14, FontWeight = SemiBold, Margin = (0, 0, 8, 0) },
                    new MobilePresetGrid(MobilePresetVisual.Sizing, [
                        ("Desktop", e => e.SizingPreset == SizingPreset.Desktop, e => e.SetSizingPreset(SizingPreset.Desktop)),
                        ("Touch", e => e.SizingPreset == SizingPreset.TouchEnabled, e => e.SetSizingPreset(SizingPreset.TouchEnabled)),
                        ("Mobile", e => e.SizingPreset == SizingPreset.Mobile, e => e.SetSizingPreset(SizingPreset.Mobile)),
                    ]),
                ] } },
                new Card { Content = new VerticalStack { Content = [
                    new Label { Text = "Motion", FontSize = 14, FontWeight = SemiBold, Margin = (0, 0, 8, 0) },
                    new MobilePresetGrid(MobilePresetVisual.Motion, [
                        ("None", e => e.MotionPreset == MotionPreset.None, e => e.SetMotionPreset(MotionPreset.None)),
                        ("Short", e => e.MotionPreset == MotionPreset.Short, e => e.SetMotionPreset(MotionPreset.Short)),
                        ("Normal", e => e.MotionPreset == MotionPreset.Normal, e => e.SetMotionPreset(MotionPreset.Normal)),
                        ("Long", e => e.MotionPreset == MotionPreset.Long, e => e.SetMotionPreset(MotionPreset.Long)),
                    ]),
                ] } },
            ],
        },
    };

    private static View CreatePreviewPage() => new ScrollView
    {
        Direction = ScrollDirection.Vertical,
        Content = new VerticalStack
        {
            Margin = 12,
            Content =
            [
                new Label { Text = "Widget Preview", FontSize = 18, FontWeight = SemiBold, Margin = (0, 0, 12, 0) },
                new Card { Content = new VerticalStack { Content = [
                    new Label { Text = "Actions", FontSize = 16, FontWeight = SemiBold, Margin = (0, 0, 8, 0) },
                    new Button { Text = "Primary action", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = (0, 0, 8, 0) },
                    new Button { Text = "Secondary action", Role = ColorRole.Secondary, HorizontalAlignment = HorizontalAlignment.Stretch },
                ] } },
                new Card { Content = new VerticalStack { Content = [
                    new Label { Text = "Surface", FontSize = 16, FontWeight = SemiBold, Margin = (0, 0, 8, 0) },
                    new Label { Text = "Cards, spacing, typography, and controls update as the palette changes.", FontSize = 14 },
                ] } },
                new Expander
                {
                    Title = "Motion preview",
                    IsExpanded = true,
                    Margin = (0, 0, 12, 0),
                    Content = new VerticalStack { Content = [
                        new Label { Text = "Expand and collapse this card to preview the selected motion token.", FontSize = 14, Margin = (0, 0, 12, 0) },
                        new Button { Text = "Preview action", HorizontalAlignment = HorizontalAlignment.Stretch },
                    ] },
                },
                new Card { Role = ColorRole.Primary, Content = new VerticalStack { Content = [
                    new Label { Text = "Primary", FontSize = 16, FontWeight = SemiBold },
                    new Label { Text = "A branded container", FontSize = 14, Margin = (4, 0, 0, 0) },
                ] } },
            ],
        },
    };

}
