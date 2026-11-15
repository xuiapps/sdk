using System.Runtime.InteropServices;
using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.Layers.Tests;

using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Layers;

public class LayersExample : Example, IExampleSectionNavigator, IMobileExamplePresentation
{
    private readonly LayersPanel panel;

    public event Action? SectionChanged
    {
        add => panel.SectionChanged += value;
        remove => panel.SectionChanged -= value;
    }

    public int SectionCount => panel.SectionCount;

    public int SelectedSectionIndex => panel.SelectedSectionIndex;

    public LayersExample()
    {
        Title = "Layers";
        Content = panel = new LayersPanel();
    }

    public string GetSectionName(int index) => panel.GetSectionName(index);

    public void SelectSection(int index) => panel.SelectSection(index);

    public void UseMobilePresentation() => panel.UseMobilePresentation();

    public class LayersPanel : View
    {
        private readonly VerticalStack list;
        private View? preview;
        private readonly List<(string Name, Func<View> Create)> sections = [];
        private int selectedSectionIndex;
        private bool mobilePresentation;

        public event Action? SectionChanged;

        public int SectionCount => sections.Count;

        public int SelectedSectionIndex => selectedSectionIndex;

        public override int Count => mobilePresentation
            ? preview is null ? 0 : 1
            : 1 + (preview is not null ? 1 : 0);

        public override View this[int index] => mobilePresentation
            ? index == 0 && preview is not null ? preview : throw new IndexOutOfRangeException()
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

        public LayersPanel()
        {
            list = new VerticalStack();
            AddProtectedChild(list);

            AddTest<BorderLayerTest>("BorderLayer");
            AddTest<TextBoxLayerTest>("TextBox (layered)");
            AddTest<DockLayerTest>("DockLayer widgets");
            AddTest<PopupDemo>("Overlay (IOverlay)");

            SelectSection(0);
        }

        public void AddTest<T>(string name) where T : View, new()
        {
            var sectionIndex = sections.Count;
            sections.Add((name, static () => new T()));
            list.Add(new LayerTestButton(() => SelectSection(sectionIndex))
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
            if ((uint)index >= (uint)sections.Count || selectedSectionIndex == index && Preview is not null)
                return;

            selectedSectionIndex = index;
            Preview = sections[index].Create();
            SectionChanged?.Invoke();
            Invalidate();
        }

        public void UseMobilePresentation()
        {
            mobilePresentation = true;
            Invalidate();
        }

        protected override Size MeasureCore(Size availableSize, IMeasureContext context)
        {
            if (mobilePresentation)
            {
                preview?.Measure(availableSize, context);
                return availableSize;
            }

            NFloat listWidth = 200;

            list.Measure(new Size(listWidth, availableSize.Height), context);
            preview?.Measure(new Size(availableSize.Width - listWidth, availableSize.Height), context);

            return availableSize;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            if (mobilePresentation)
            {
                preview?.Arrange(rect, context);
                return;
            }

            NFloat listWidth = 200;

            list.Arrange(new Rect(rect.X, rect.Y, listWidth, rect.Height), context);
            preview?.Arrange(new Rect(rect.X + listWidth, rect.Y, rect.Width - listWidth, rect.Height), context);
        }

        protected override void RenderCore(IContext context)
        {
            if (mobilePresentation)
            {
                base.RenderCore(context);
                return;
            }

            NFloat listWidth = 200;

            // Divider between list and preview
            context.SetFill(new Color(0xE0, 0xE0, 0xE0, 0xFF));
            context.FillRect(new Rect(Frame.X + listWidth, Frame.Y, 1, Frame.Height));

            base.RenderCore(context);
        }
    }

    public class LayerTestButton : Label
    {
        private readonly Action onClick;
        private bool hover;
        private bool pressed;

        public LayerTestButton(Action onClick)
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
