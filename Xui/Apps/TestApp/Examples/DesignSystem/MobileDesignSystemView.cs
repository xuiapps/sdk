using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Phone palette editor and live UI preview, with independent retained scroll positions.</summary>
internal sealed class MobileDesignSystemView : View
{
    private readonly DemoScroll palettePage;
    private readonly DemoScroll previewPage;
    private readonly MobileDesignSystemToolbar toolbar;
    private int pageIndex;
    private bool transitioning;
    private TimeSpan? transitionStart;
    private NFloat position, from;
    private IDesignSystem? design;
    private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(180);
    private int pointerId = -1, resumeTarget;
    private bool dragging;
    private Point pointerStart;
    private NFloat dragFrom, lastX, velocity;
    private long lastTick;
    private TimeProvider clock = TimeProvider.System;

    public MobileDesignSystemView()
    {
        Id = "DesignSystem.Mobile";
        palettePage = CreatePalettePage();
        previewPage = CreatePreviewPage();
        previewPage.InputEnabled = false;
        toolbar = new MobileDesignSystemToolbar(SelectPage) { Id = "DesignSystem.Tabs" };
        AddProtectedChild(palettePage);
        AddProtectedChild(previewPage);
        AddProtectedChild(toolbar);
    }

    // Both pages belong to the tree throughout, so token notifications and lifecycle stay intact.
    public override int Count => 3;
    public override View this[int index] => index switch
    {
        0 => palettePage, 1 => previewPage, 2 => toolbar,
        _ => throw new IndexOutOfRangeException(),
    };

    protected override void OnActivate()
    {
        base.OnActivate();
        design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        clock = GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
        if (design is not null) design.Changed += TokensChanged;
    }

    protected override void OnDeactivate()
    {
        if (design is not null) design.Changed -= TokensChanged;
        design = null;
        var pointer = pointerId;
        pointerId = -1;
        dragging = false;
        FinishTransition();
        if (pointer >= 0) ReleasePointer(pointer);
        base.OnDeactivate();
    }

    private void TokensChanged(DesignSystemChange changes)
    {
        InvalidateRender();
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var toolbarSize = toolbar.Measure((available.Width - 40, available.Height), context);
        var size = new Size(available.Width, NFloat.Max(0, available.Height - toolbarSize.Height - 20));
        palettePage.Measure(size, context);
        previewPage.Measure(size, context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var toolbarSize = toolbar.Measure((rect.Width - 40, rect.Height), context);
        var page = new Rect(rect.X, rect.Y, rect.Width, NFloat.Max(0, rect.Height - toolbarSize.Height - 20));
        palettePage.Arrange(page, context);
        previewPage.Arrange(page, context);
        toolbar.Arrange((rect.X + 20, rect.Bottom - toolbarSize.Height - 12, rect.Width - 40, toolbarSize.Height), context);
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFill(design?.Colors.Application.Background ?? Colors.White);
        context.FillRect(Frame);
        context.Save();
        context.BeginPath(); context.Rect(palettePage.Frame); context.Clip();
        if (transitioning || dragging || position != pageIndex)
        {
            context.Save(); context.Translate((-position * Frame.Width, 0));
            RenderChild(context, palettePage); context.Restore();
            context.Save(); context.Translate(((1 - position) * Frame.Width, 0));
            RenderChild(context, previewPage); context.Restore();
        }
        else RenderChild(context, pageIndex == 0 ? palettePage : previewPage);
        context.Restore();

        // Match the header's opaque chrome, including the home-indicator safe area.
        var display = this.TryFindParent<RootView>(out var root) ? root.Window.DisplayArea : Frame;
        var footer = new Rect(display.Left, palettePage.Frame.Bottom, display.Width,
            NFloat.Max(0, display.Bottom - palettePage.Frame.Bottom));
        context.SetFill(design?.Colors.Surface.Background ?? Colors.White);
        context.FillRect(footer);
        context.BeginPath();
        context.MoveTo((footer.Left, footer.Top + (NFloat)0.5));
        context.LineTo((footer.Right, footer.Top + (NFloat)0.5));
        context.SetStroke(design?.Colors.OutlineVariant ?? Colors.LightGray);
        context.LineWidth = 1;
        context.Stroke();
        RenderChild(context, toolbar);
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        // Scroll inertia, spring return, expanders, and selection indicators need these ticks too.
        if (transitioning || dragging || pageIndex == 0) palettePage.Animate(previous, current);
        if (transitioning || dragging || pageIndex == 1) previewPage.Animate(previous, current);
        toolbar.Animate(previous, current);
        if (!transitioning) return;
        transitionStart ??= previous;
        var t = NFloat.Clamp((NFloat)((current - transitionStart.Value) / TransitionDuration), 0, 1);
        SetPosition(from + (pageIndex - from) * Easing.EaseInOutCubic(t));
        if (t < 1) RequestAnimationFrame();
        else FinishTransition();
    }

    private void SelectPage(int index)
    {
        if (dragging || (index == pageIndex && transitioning)) return;
        pageIndex = index;
        toolbar.SelectedPage = index;
        toolbar.RefreshSelection();
        from = position;
        transitionStart = null;
        // Demo motion presets affect the examples, not navigation. Honor the OS
        // accessibility preference independently of the editable theme.
        transitioning = position != index && GetService(typeof(IDeviceInfo)) is not IDeviceInfo { PrefersReducedMotion: true };
        palettePage.InputEnabled = previewPage.InputEnabled = false;
        if (transitioning) RequestAnimationFrame();
        else FinishTransition();
        InvalidateHitTest();
        InvalidateRender();
    }

    private void FinishTransition()
    {
        transitioning = false;
        SetPosition(pageIndex);
        palettePage.InputEnabled = pageIndex == 0;
        previewPage.InputEnabled = pageIndex == 1;
        InvalidateHitTest();
        InvalidateRender();
    }

    private void SetPosition(NFloat value)
    {
        position = value;
        toolbar.PagePosition = value;
        toolbar.InvalidateRender();
        InvalidateRender();
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        var inPage = palettePage.Frame.Contains(e.State.Position);
        var edge = this.TryFindParent<NavigationView>(out var navigation) && navigation.CanGoBack
            ? navigation.BackSwipeEdgeWidth : 0;
        if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down && inPage && e.State.Position.X > Frame.Left + edge)
            e.PushPotentialGesture(this, PointerGestures.DragHorizontalTentative);
        if (phase != EventPhase.Bubble) return;

        if (e.Type == PointerEventType.Down && !dragging)
        {
            pointerId = -1;
            if (!inPage || e.State.Position.X <= Frame.Left + edge || !CanTakeDrag(e.PointerId)) return;
            pointerId = e.PointerId;
            pointerStart = e.State.Position;
            dragFrom = position;
            resumeTarget = pageIndex;
            lastX = pointerStart.X;
            lastTick = clock.GetTimestamp();
            velocity = 0;
            // Hold a moving page under the finger. A tap resumes its destination.
            transitioning = false;
            return;
        }
        if (e.PointerId != pointerId) return;
        if (e.Type == PointerEventType.Move)
        {
            var delta = e.State.Position - pointerStart;
            if (!dragging)
            {
                if (!CanTakeDrag(pointerId) || (NFloat.Abs(delta.Y) > 8 && NFloat.Abs(delta.Y) > NFloat.Abs(delta.X)))
                {
                    pointerId = -1;
                    SelectPage(resumeTarget);
                    return;
                }
                if (NFloat.Abs(delta.X) <= 6 || NFloat.Abs(delta.X) <= NFloat.Abs(delta.Y)) return;
                dragging = true;
                CapturePointer(pointerId, PointerGestures.Drag);
                palettePage.InputEnabled = previewPage.InputEnabled = false;
                InvalidateHitTest();
            }
            UpdateDrag(e.State.Position);
        }
        else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
        {
            var captured = dragging;
            var target = resumeTarget;
            if (dragging && e.Type == PointerEventType.Up)
            {
                UpdateDrag(e.State.Position);
                var recentVelocity = clock.GetElapsedTime(lastTick).TotalMilliseconds < 100 ? velocity : 0;
                var travelled = (position - dragFrom) * Frame.Width;
                var commitDistance = Frame.Width * 0.2f;
                if (NFloat.Abs(travelled) >= commitDistance)
                    target = travelled > 0 ? 1 : 0;
                else if (NFloat.Abs(travelled) >= 12 && NFloat.Abs(recentVelocity) >= 220)
                    target = recentVelocity < 0 ? 1 : 0;
            }
            var pointer = pointerId;
            pointerId = -1;
            dragging = false;
            SelectPage(target);
            if (captured && e.Type != PointerEventType.LostCapture) ReleasePointer(pointer);
        }
    }

    private bool CanTakeDrag(int pointer) => GetCapturedGesture(pointer) is null or ITap or IDragVerticalTentative;

    private void UpdateDrag(Point point)
    {
        var now = clock.GetTimestamp();
        var elapsed = (NFloat)clock.GetElapsedTime(lastTick, now).TotalSeconds;
        if (point.X != lastX)
        {
            if (elapsed >= 0.001f) velocity = (point.X - lastX) / elapsed;
            lastX = point.X;
            lastTick = now;
        }
        SetPosition(NFloat.Clamp(dragFrom - (point.X - pointerStart.X) / NFloat.Max(1, Frame.Width), 0, 1));
    }

    private static DemoScroll CreatePalettePage() => new()
    {
        Id = "DesignSystem.PaletteScroll",
        Content = new VerticalStack
        {
            Margin = new Frame { Top = 20, Right = 16, Bottom = 24, Left = 16 },
            Content =
            [
                Copy("MAKE IT YOURS", accent: true, alignWithCardContent: true),
                Copy("One system. Your style.", heading: true, alignWithCardContent: true),
                Copy("Tune the palette. See every detail respond.", alignWithCardContent: true),
                Section("Palette", "Color & surfaces", "Choose a hue on the ring. Mix light and shade inside.",
                    new ColorWheelView
                    {
                        Id = "DesignSystem.Hue", HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Frame { Top = 8, Bottom = 16 },
                    },
                    new MobilePresetGrid(MobilePresetVisual.Harmony, [
                        ("Pair", e => e.Harmony == ColorHarmony.Complementary, e => e.SetHarmony(ColorHarmony.Complementary)),
                        ("Nearby", e => e.Harmony == ColorHarmony.Analogous, e => e.SetHarmony(ColorHarmony.Analogous)),
                        ("Split", e => e.Harmony == ColorHarmony.SplitComplementary, e => e.SetHarmony(ColorHarmony.SplitComplementary)),
                        ("Triad", e => e.Harmony == ColorHarmony.Triadic, e => e.SetHarmony(ColorHarmony.Triadic)),
                        ("Tetrad", e => e.Harmony == ColorHarmony.Tetradic, e => e.SetHarmony(ColorHarmony.Tetradic)),
                    ]) { Id = "DesignSystem.Harmony" },
                    Subheading("Surfaces"),
                    Copy("Let accent colors flow into the background."),
                    new MobilePresetGrid(MobilePresetVisual.SurfaceStyle, [
                        ("Mono", e => e.NeutralStyle == NeutralStyle.Monochrome, e => e.SetNeutralStyle(NeutralStyle.Monochrome)),
                        ("2nd\napp", e => e.NeutralStyle == NeutralStyle.SecondaryApp, e => e.SetNeutralStyle(NeutralStyle.SecondaryApp)),
                        ("2nd\ncard", e => e.NeutralStyle == NeutralStyle.SecondarySurface, e => e.SetNeutralStyle(NeutralStyle.SecondarySurface)),
                        ("3rd\napp", e => e.NeutralStyle == NeutralStyle.TertiaryApp, e => e.SetNeutralStyle(NeutralStyle.TertiaryApp)),
                        ("3rd\ncard", e => e.NeutralStyle == NeutralStyle.TertiarySurface, e => e.SetNeutralStyle(NeutralStyle.TertiarySurface)),
                    ]) { Id = "DesignSystem.Surface" }),
                Section("Shape", "Shape & density", "Crisp corners or a softer silhouette.",
                    new MobilePresetGrid(MobilePresetVisual.Shape, [
                        ("Square", e => e.ShapePreset == ShapePreset.Square, e => e.SetShapePreset(ShapePreset.Square)),
                        ("Subtle", e => e.ShapePreset == ShapePreset.Desktop, e => e.SetShapePreset(ShapePreset.Desktop)),
                        ("Round", e => e.ShapePreset == ShapePreset.Rounded, e => e.SetShapePreset(ShapePreset.Rounded)),
                        ("Pill", e => e.ShapePreset == ShapePreset.RoundedPill, e => e.SetShapePreset(ShapePreset.RoundedPill)),
                        ("Soft", e => e.ShapePreset == ShapePreset.Soft, e => e.SetShapePreset(ShapePreset.Soft)),
                    ]) { Id = "DesignSystem.Shape" },
                    Subheading("Density"),
                    Copy("Choose how much room controls have."),
                    new MobilePresetGrid(MobilePresetVisual.Sizing, [
                        ("Desktop", e => e.SizingPreset == SizingPreset.Desktop, e => e.SetSizingPreset(SizingPreset.Desktop)),
                        ("Touch", e => e.SizingPreset == SizingPreset.TouchEnabled, e => e.SetSizingPreset(SizingPreset.TouchEnabled)),
                        ("Mobile", e => e.SizingPreset == SizingPreset.Mobile, e => e.SetSizingPreset(SizingPreset.Mobile)),
                    ]) { Id = "DesignSystem.Sizing" }),
                Section("Motion", "Motion", "Try your timing in the Preview tab.",
                    new MobilePresetGrid(MobilePresetVisual.Motion, [
                        ("None", e => e.MotionPreset == MotionPreset.None, e => e.SetMotionPreset(MotionPreset.None)),
                        ("Quick", e => e.MotionPreset == MotionPreset.Short, e => e.SetMotionPreset(MotionPreset.Short)),
                        ("Natural", e => e.MotionPreset == MotionPreset.Normal, e => e.SetMotionPreset(MotionPreset.Normal)),
                        ("Relaxed", e => e.MotionPreset == MotionPreset.Long, e => e.SetMotionPreset(MotionPreset.Long)),
                    ]) { Id = "DesignSystem.Motion" }),
            ],
        },
    };

    private static DemoScroll CreatePreviewPage()
    {
        var status = Copy("Ready when you are.");
        status.Id = "DesignSystem.ActionStatus";
        return new DemoScroll
        {
            Id = "DesignSystem.PreviewScroll",
            Content = new VerticalStack
            {
                Margin = new Frame { Top = 20, Right = 16, Bottom = 24, Left = 16 },
                Content =
                [
                    Copy("LIVE PREVIEW", accent: true, alignWithCardContent: true),
                    Copy("Your style, in action.", heading: true, alignWithCardContent: true),
                    Copy("Real controls. One shared set of tokens.", alignWithCardContent: true),
                    Panel("Hero", "Weekend edition", new MobileDemoCard
                    {
                        Id = "DesignSystem.HeroCard", Role = ColorRole.Primary,
                        Content = new VerticalStack { Content = [
                            new DesignSystemLabel { Text = "Go somewhere new.", Heading = true, OnPrimary = true, Margin = new Frame { Bottom = 8 } },
                            new DesignSystemLabel { Text = "A little color goes a long way.", OnPrimary = true, Margin = new Frame { Bottom = 20 } },
                            new Button { Id = "DesignSystem.PrimaryAction", Text = "Plan a trip", HorizontalAlignment = HorizontalAlignment.Stretch,
                                Clicked = () => status.Text = "Your next adventure is planned." },
                        ] },
                    }),
                    Section("Actions", "Everyday actions", "Color, shape, and spacing follow your choices.",
                        new Button { Text = "Save for later", Variant = ButtonVariant.Outline, Role = ColorRole.Secondary,
                            HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Frame { Bottom = 12 }, Clicked = () => status.Text = "Saved for your next adventure." }, status),
                    Section("Controls", "Make yourself at home", "Small details make the interface feel yours.",
                        new ControlRow("Notifications", new Toggle { IsOn = true }),
                        new ControlRow("Travel reminders", new Checkbox { IsChecked = true })),
                    new MobileDemoExpander
                    {
                        Id = "DesignSystem.MotionPreview", Title = "A little more detail", IsExpanded = false, Margin = new Frame { Bottom = 16 },
                        Content = new VerticalStack { Content = [Copy("Open, close, and feel the selected timing."),
                            new Button { Text = "Explore more", HorizontalAlignment = HorizontalAlignment.Stretch }] },
                    },
                ],
            },
        };
    }

    private static DesignSystemLabel Copy(string text, bool heading = false, bool accent = false, bool alignWithCardContent = false) => new()
    { Text = text, Heading = heading, Accent = accent, AlignWithCardContent = alignWithCardContent,
        Margin = new Frame { Bottom = heading ? 8 : 12 } };

    private static DesignSystemLabel Subheading(string text) => new()
    {
        Text = text, Emphasized = true,
        Margin = new Frame { Top = 20, Bottom = 8 },
    };

    private static VerticalStack Section(string id, string title, string description, params View[] controls)
    {
        var content = new VerticalStack();
        content.Add(Copy(description));
        foreach (var control in controls) content.Add(control);
        return Panel(id, title, new MobileDemoCard { Id = "DesignSystem.Section." + id, Content = content });
    }

    private static VerticalStack Panel(string id, string title, Card card) => new()
    {
        Margin = new Frame { Bottom = 28 },
        Content =
        [
            new DesignSystemLabel
            {
                Id = "DesignSystem.Heading." + id, Text = title, SectionHeading = true,
            },
            card,
        ],
    };

    private sealed class DemoScroll : ScrollView
    {
        // These viewports end at flat header/footer chrome, not the device corners.
        // Content owns its spacing; the scrollbar spans the whole viewport.
        public DemoScroll() => ScrollbarEndInset = 0;

        public bool InputEnabled { get; set; } = true;
        public override bool HitTest(Point point) => InputEnabled && base.HitTest(point);
    }

    private sealed class ControlRow : ViewCollection
    {
        private readonly Label label;
        private readonly View control;
        public ControlRow(string text, View control)
        {
            Add(label = Copy(text));
            label.Margin = 0;
            Add(this.control = control);
            control.HorizontalAlignment = HorizontalAlignment.Right;
            control.VerticalAlignment = VerticalAlignment.Middle;
        }
        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            var size = control.Measure(available, context);
            var text = label.Measure((NFloat.Max(0, available.Width - size.Width - 16), available.Height), context);
            return (available.Width, NFloat.Max(52, NFloat.Max(size.Height, text.Height) + 16));
        }
        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            var size = control.Measure(rect.Size, context);
            var labelSize = label.Measure((rect.Width - size.Width - 16, rect.Height), context);
            label.Arrange((rect.X, rect.Center.Y - labelSize.Height / 2, rect.Width - size.Width - 16, labelSize.Height), context);
            control.Arrange((rect.Right - size.Width, rect.Y + 8, size.Width, rect.Height - 16), context);
        }
    }
}
