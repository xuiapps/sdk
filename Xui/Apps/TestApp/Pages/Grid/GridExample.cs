using System.Runtime.InteropServices;
using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.Grid.Tests;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Grid;

public class GridExample : Example, IExampleSectionNavigator, IMobileExamplePresentation
{
    private readonly GridExamplePanel panel;

    public bool ShowsMobileSectionControls => false;

    public event Action? SectionChanged
    {
        add => panel.SectionChanged += value;
        remove => panel.SectionChanged -= value;
    }

    public int SectionCount => panel.SectionCount;

    public int SelectedSectionIndex => panel.SelectedSectionIndex;

    public GridExample()
    {
        Title = "Grid Layout";
        Content = panel = new GridExamplePanel();
    }

    public string GetSectionName(int index) => panel.GetSectionName(index);

    public void SelectSection(int index) => panel.SelectSection(index);

    public void UseMobilePresentation() => panel.UseMobilePresentation();

    public class GridExamplePanel : View
    {
        private readonly VerticalStack list;
        private View? preview;
        private readonly List<(string Name, Func<View> Create)> sections = [];
        private int selectedSectionIndex;
        private ExampleCardCarousel? carousel;

        public event Action? SectionChanged;

        public int SectionCount => sections.Count;

        public int SelectedSectionIndex => carousel?.SelectedIndex ?? selectedSectionIndex;

        public override int Count => carousel is not null
            ? 1
            : 1 + (preview is not null ? 1 : 0);

        public override View this[int index] => carousel is not null
            ? index == 0 ? carousel : throw new IndexOutOfRangeException()
            : index switch
        {
            0 => list,
            1 when preview is not null => preview,
            _ => throw new IndexOutOfRangeException(),
        };

        public View? Preview
        {
            get => preview;
            set => SetProtectedChild(ref preview, value);
        }

        public GridExamplePanel()
        {
            list = new VerticalStack();
            AddProtectedChild(list);

            AddTest<BasicFixedGridTest>("Basic fixed grid");
            AddTest<GapsGridTest>("Grid with gaps");
            AddTest<FractionalGridTest>("Fractional units");
            AddTest<AutoTracksGridTest>("Auto tracks");
            AddTest<SpanningCellsTest>("Spanning cells");
            AddTest<NamedAreasTest>("Named areas");
            AddTest<AutoFlowTest>("Auto flow");
            AddTest<AlignmentTest>("Alignment");
            AddTest<FixedContainerTest>("Fixed container");
            AddTest<ScrollableGridTest>("Scrollable grid");
            AddTest<NestedGridsTest>("Nested grids");
            AddTest<MinMaxTracksTest>("MinMax tracks");

            SelectSection(0);
        }

        public void AddTest<T>(string name) where T : View, new()
        {
            var sectionIndex = sections.Count;
            sections.Add((name, static () => new T()));
            list.Add(new GridNavButton(() => SelectSection(sectionIndex))
            {
                Id = name,
                Margin = 3,
                Text = name,
                FontFamily = "Inter",
            });
        }

        public string GetSectionName(int index) => sections[index].Name;

        public void SelectSection(int index)
        {
            if (carousel is not null)
            {
                carousel.Select(index);
                return;
            }
            if ((uint)index >= (uint)sections.Count || selectedSectionIndex == index && Preview is not null)
                return;

            selectedSectionIndex = index;
            Preview = sections[index].Create();
            SectionChanged?.Invoke();
            Invalidate();
        }

        public void UseMobilePresentation()
        {
            if (carousel is not null) return;
            Preview = null;
            RemoveProtectedChild(list);
            carousel = new ExampleCardCarousel(sections, selectedSectionIndex)
            {
                Id = "GridLayoutCarousel",
                MaximumCardHeight = 632,
            };
            carousel.SelectionChanged += () => SectionChanged?.Invoke();
            AddProtectedChild(carousel);
            Invalidate();
        }

        protected override Size MeasureCore(Size availableSize, IMeasureContext context)
        {
            if (carousel is not null)
            {
                carousel.Measure(availableSize, context);
                return availableSize;
            }

            NFloat listWidth = 200;

            list.Measure(new Size(listWidth, availableSize.Height), context);
            preview?.Measure(new Size(availableSize.Width - listWidth, availableSize.Height), context);

            return availableSize;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            if (carousel is not null)
            {
                carousel.Arrange(rect, context);
                return;
            }

            NFloat listWidth = 200;

            list.Arrange(new Rect(rect.X, rect.Y, listWidth, rect.Height), context);
            preview?.Arrange(new Rect(rect.X + listWidth, rect.Y, rect.Width - listWidth, rect.Height), context);
        }

        protected override void RenderCore(IContext context)
        {
            if (carousel is not null)
            {
                base.RenderCore(context);
                return;
            }

            NFloat listWidth = 200;

            context.SetFill(new Color(0xE0, 0xE0, 0xE0, 0xFF));
            context.FillRect(new Rect(Frame.X + listWidth, Frame.Y, 1, Frame.Height));

            base.RenderCore(context);
        }
    }

    public class GridNavButton : Label
    {
        private readonly Action onClick;
        private bool hover;
        private bool pressed;

        public GridNavButton(Action onClick)
        {
            this.onClick = onClick;
        }

        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Enter)
            {
                hover = true;
                InvalidateRender();
            }
            else if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Leave)
            {
                hover = false;
                InvalidateRender();
            }
            else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
            {
                CapturePointer(e.PointerId);
                pressed = true;
                InvalidateRender();
            }
            else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Up)
            {
                ReleasePointer(e.PointerId);
                onClick();
                pressed = false;
                InvalidateRender();
            }

            base.OnPointerEvent(ref e, phase);
        }

        protected override void RenderCore(IContext context)
        {
            if (pressed)
            {
                context.SetFill(Yellow);
                context.FillRect(Frame);
            }
            else if (hover)
            {
                context.SetFill(LightGray);
                context.FillRect(Frame);
            }

            base.RenderCore(context);
        }
    }
}
