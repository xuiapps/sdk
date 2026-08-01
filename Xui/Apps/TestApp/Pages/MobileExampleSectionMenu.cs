using Xui.Apps.TestApp.Examples;
using Xui.Core.Animation;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages;

/// <summary>A bottom sheet that exposes every section in a mobile example.</summary>
internal sealed class MobileExampleSectionMenu : View
{
    private readonly IExampleSectionNavigator sections;
    private readonly ScrollView scrollView;
    private readonly VerticalStack items = new();
    private bool isOpen;
    private NFloat openProgress;
    private NFloat animationStartProgress;
    private TimeSpan? animationStart;
    private Rect sheetFrame;

    /// <summary>Duration of the bottom-sheet entrance and exit.</summary>
    public TimeSpan TransitionDuration { get; set; } = TimeSpan.FromMilliseconds(240);

    /// <summary>Fast decelerating curve used for the sheet transition.</summary>
    public Easing.CubicBezier TransitionEasing { get; set; } = new((0.32f, 0.72f), (0f, 1f));

    public MobileExampleSectionMenu(IExampleSectionNavigator sections)
    {
        this.sections = sections;
        for (var index = 0; index < sections.SectionCount; index++)
        {
            var sectionIndex = index;
            items.Add(new MobileExampleSectionMenuItem(
                () => sections.GetSectionName(sectionIndex),
                () => SelectAndClose(sectionIndex)));
        }
        scrollView = new ScrollView
        {
            Direction = ScrollDirection.Vertical,
            Content = items,
        };
        AddProtectedChild(scrollView);
        sections.SectionChanged += OnSectionChanged;
    }

    public bool IsOpen
    {
        get => isOpen;
        set
        {
            if (isOpen == value) return;
            isOpen = value;
            animationStartProgress = openProgress;
            animationStart = null;
            RequestAnimationFrame();
            InvalidateRender();
        }
    }

    public override int Count => 1;

    public override View this[int index] => index == 0 ? scrollView : throw new IndexOutOfRangeException();

    public override bool HitTest(Point point) => isOpen && openProgress >= 1 && sheetFrame.Contains(point);

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        scrollView.Measure(available, context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat headerHeight = 48;
        NFloat contentBottomInset = 12;
        NFloat toolbarClearance = 72;
        var sheetWidth = rect.Width - 24;
        var contentSize = items.Measure((sheetWidth - 24, NFloat.PositiveInfinity), context);
        var maximumSheetHeight = rect.Height * 2 / 3 - toolbarClearance;
        var sheetHeight = NFloat.Min(maximumSheetHeight, headerHeight + contentSize.Height + contentBottomInset);
        var sheetBottom = rect.Bottom - toolbarClearance;
        sheetFrame = new Rect(
            rect.X + 12,
            sheetBottom - sheetHeight,
            sheetWidth,
            sheetHeight);
        scrollView.Arrange(new Rect(
            sheetFrame.X + 12,
            sheetFrame.Y + headerHeight,
            sheetFrame.Width - 24,
            sheetFrame.Height - headerHeight - contentBottomInset), context);
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        // The footer remains interactive while the sheet is open; its current
        // section button is the explicit close affordance.
    }

    protected override void RenderCore(IContext context)
    {
        if (openProgress <= 0)
            return;

        var design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        var foreground = design?.Colors.Surface.Foreground ?? Colors.Black;
        var offscreenOffset = this.Frame.Bottom - sheetFrame.Y + 12;
        var displayedSheet = new Rect(
            sheetFrame.X,
            sheetFrame.Y + (1 - openProgress) * offscreenOffset,
            sheetFrame.Width,
            sheetFrame.Height);
        context.BeginPath();
        context.RoundRect(displayedSheet, 24);
        context.SetFill(design?.Colors.Surface.Background ?? Colors.White);
        context.Fill();
        context.BeginPath();
        context.RoundRect(displayedSheet, 24);
        context.SetStroke(design?.Colors.OutlineVariant ?? Colors.LightGray);
        context.LineWidth = 1;
        context.Stroke();

        context.SetFont(new Font(17, "Inter", FontWeight.SemiBold));
        context.TextAlign = TextAlign.Left;
        context.TextBaseline = TextBaseline.Middle;
        context.SetFill(foreground);
        context.FillText("Examples", new Point(displayedSheet.X + 20, displayedSheet.Y + 24));
        context.Save();
        try
        {
            context.Translate((0, displayedSheet.Y - sheetFrame.Y));
            scrollView.Render(context);
        }
        finally
        {
            context.Restore();
        }
    }

    private void SelectAndClose(int index)
    {
        sections.SelectSection(index);
        IsOpen = false;
    }

    private void OnSectionChanged() => InvalidateRender();

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        animationStart ??= current;
        var elapsed = current - animationStart.Value;
        var linearProgress = NFloat.Clamp((NFloat)(elapsed / TransitionDuration), 0, 1);
        var easedProgress = TransitionEasing.Evaluate(linearProgress, 0.0005f);
        openProgress = isOpen
            ? animationStartProgress + (1 - animationStartProgress) * easedProgress
            : animationStartProgress * (1 - easedProgress);
        InvalidateRender();

        if (linearProgress < 1)
        {
            RequestAnimationFrame();
            return;
        }

        openProgress = isOpen ? 1 : 0;
        animationStart = null;
    }
}
