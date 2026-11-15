using System.Runtime.InteropServices;
using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.Canvas.Tests;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.Canvas.FontWeight;

namespace Xui.Apps.TestApp.Pages.Canvas;

public class CanvasTestsExample : Example, IExampleSectionNavigator, IMobileExamplePresentation
{
    private readonly CanvasTestPanel panel;

    public bool ShowsMobileSectionControls => false;

    public event Action? SectionChanged
    {
        add => panel.SectionChanged += value;
        remove => panel.SectionChanged -= value;
    }

    public int SectionCount => panel.SectionCount;

    public int SelectedSectionIndex => panel.SelectedSectionIndex;

    public CanvasTestsExample()
    {
        this.Title = "Canvas Drawing";
        this.Content = panel = new CanvasTestPanel();
    }

    public string GetSectionName(int index) => panel.GetSectionName(index);

    public void SelectSection(int index) => panel.SelectSection(index);

    public void UseMobilePresentation() => panel.UseMobilePresentation();

    public class CanvasTestPanel : View, IExampleSectionNavigator
    {
        private VerticalStack list;
        private View? canvas;
        private readonly List<(string Name, Func<View> Create)> sections = [];
        private int selectedSectionIndex;
        private ExampleCardCarousel? carousel;

        public event Action? SectionChanged;

        public int SectionCount => sections.Count;

        public int SelectedSectionIndex => carousel?.SelectedIndex ?? selectedSectionIndex;

        public override int Count => carousel is not null
            ? 1
            : (this.list is not null ? 1 : 0) + (this.canvas is not null ? 1 : 0);

        public override View this[int index] => carousel is not null
            ? index == 0 ? carousel : throw new IndexOutOfRangeException()
            : index switch
        {
            0 => this.list,
            1 when this.canvas is not null => this.canvas,
            _ => throw new IndexOutOfRangeException()
        };

        public View? Canvas
        {
            get => this.canvas;
            set => this.SetProtectedChild(ref this.canvas, value);
        }

        public CanvasTestPanel()
        {
            this.list = new VerticalStack();
            this.AddProtectedChild(this.list);

            this.AddTest<FillRectTest>("FillRect");
            this.AddTest<QuadraticCurveTest>("QuadraticCurve");
            this.AddTest<CubicCurveTest>("CubicCurve");
            this.AddTest<HeartCurveTest>("HeartCurve");
            this.AddTest<ArcTest>("Arc");
            this.AddTest<ArcFlowerTest>("ArcFlower");
            this.AddTest<EllipseTest>("Ellipse");
            this.AddTest<ArcToTest>("ArcTo");
            this.AddTest<ArcToFlowerTest>("ArcToFlower");
            this.AddTest<RoundRectTest>("RoundRect");
            this.AddTest<LineCapJoinTest>("LineCapJoin");
            this.AddTest<DashPatternTest>("DashPattern");
            this.AddTest<FillRuleTest>("FillRule");
            this.AddTest<PathContinuationTest>("PathContinuation");
            this.AddTest<ClipTest>("Clip");
            this.AddTest<TransformTest>("Transform");
            this.AddTest<StarTest>("Star");
            this.AddTest<GlobalAlphaTest>("GlobalAlpha");
            this.AddTest<TextTest>("Text");
            this.AddTest<RainbowTextFillTest>("RainbowTextFill");
            this.AddTest<DrawImageTest>("DrawImage");
            this.AddTest<BitmapFillTest>("BitmapFill");

            SelectSection(0);
        }

        public void AddTest<T>(string name) where T : View, new()
        {
            var sectionIndex = sections.Count;
            sections.Add((name, static () => new T()));
            this.list.Add(new CanvasTestButton(() =>
            {
                SelectSection(sectionIndex);
            })
            {
                Id = name,
                Margin = 3,
                Text = name,
                FontFamily = "Inter",
            });
        }

        public string GetSectionName(int index) => sections[index].Name;

        public void UseMobilePresentation()
        {
            if (carousel is not null) return;
            Canvas = null;
            RemoveProtectedChild(list);
            carousel = new ExampleCardCarousel(sections, selectedSectionIndex, CanvasCardDescriptions.Drawing);
            carousel.SelectionChanged += () => SectionChanged?.Invoke();
            AddProtectedChild(carousel);
            Invalidate();
        }

        public void SelectSection(int index)
        {
            if (carousel is not null)
            {
                carousel.Select(index);
                return;
            }
            if ((uint)index >= (uint)sections.Count || selectedSectionIndex == index && Canvas is not null)
                return;

            selectedSectionIndex = index;
            Canvas = sections[index].Create();
            SectionChanged?.Invoke();
            Invalidate();
        }

        protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
        {
            if (carousel is not null)
                return carousel.Measure(availableBorderEdgeSize, context);

            NFloat listWidth = 250;
            var canvasSize = new Size(600, 300);

            this.list.Measure(new Size(listWidth, availableBorderEdgeSize.Height), context);
            this.canvas?.Measure(canvasSize, context);

            return availableBorderEdgeSize;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            if (carousel is not null)
            {
                carousel.Arrange(rect, context);
                return;
            }

            NFloat listWidth = 250;
            NFloat canvasPadding = 20;

            this.list.Arrange(new Rect(rect.X, rect.Y, listWidth, rect.Height), context);
            this.canvas?.Arrange(new Rect(
                rect.X + listWidth + canvasPadding,
                rect.Y + canvasPadding,
                600, 300), context);
        }

        protected override void RenderCore(IContext context)
        {
            if (carousel is not null)
            {
                base.RenderCore(context);
                return;
            }

            NFloat listWidth = 250;

            // Gray background for the canvas area (matches HTML body #f0f0f0)
            context.SetFill(new Color(0xF0, 0xF0, 0xF0, 0xFF));
            context.FillRect(new Rect(
                this.Frame.X + listWidth,
                this.Frame.Y,
                this.Frame.Width - listWidth,
                this.Frame.Height));

            context.Save();
            base.RenderCore(context);
            context.Restore();
        }
    }

    public class CanvasTestButton : Label
    {
        private readonly Action onClick;
        private bool hover;
        private bool pressed;

        public CanvasTestButton(Action onClick)
        {
            this.onClick = onClick;
        }

        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Enter)
            {
                hover = true;
                this.InvalidateRender();
            }
            else if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Leave)
            {
                hover = false;
                this.InvalidateRender();
            }
            else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
            {
                this.CapturePointer(e.PointerId);
                pressed = true;
                this.InvalidateRender();
            }
            else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Up)
            {
                this.ReleasePointer(e.PointerId);
                this.onClick();
                pressed = false;
                this.InvalidateRender();
            }

            base.OnPointerEvent(ref e, phase);
        }

        protected override void RenderCore(IContext context)
        {
            if (this.pressed)
            {
                context.SetFill(Yellow);
                context.FillRect(this.Frame);
            }
            else if (this.hover)
            {
                context.SetFill(LightGray);
                context.FillRect(this.Frame);
            }

            base.RenderCore(context);
        }
    }
}
