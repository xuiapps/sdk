using System.Runtime.InteropServices;
using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.ThreeD.Tests;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.ThreeD;

/// <summary>
/// Example page demonstrating 3D rendering with GPU shaders.
/// </summary>
public class ThreeDExample : Example, IExampleSectionNavigator, IMobileExamplePresentation
{
    private readonly ThreeDTestPanel panel;

    public event Action? SectionChanged
    {
        add => panel.SectionChanged += value;
        remove => panel.SectionChanged -= value;
    }

    public int SectionCount => panel.SectionCount;

    public int SelectedSectionIndex => panel.SelectedSectionIndex;

    public ThreeDExample()
    {
        this.Title = "3D Texture";
        this.Content = panel = new ThreeDTestPanel();
    }

    public string GetSectionName(int index) => panel.GetSectionName(index);

    public void SelectSection(int index) => panel.SelectSection(index);

    public void UseMobilePresentation() => panel.UseMobilePresentation();

    public class ThreeDTestPanel : View
    {
        private VerticalStack list;
        private View? demo;
        private readonly List<(string Name, Func<View> Create)> sections = [];
        private int selectedSectionIndex;
        private bool mobilePresentation;

        public event Action? SectionChanged;

        public int SectionCount => sections.Count;

        public int SelectedSectionIndex => selectedSectionIndex;

        public override int Count => mobilePresentation
            ? this.demo is null ? 0 : 1
            : (this.list is not null ? 1 : 0) + (this.demo is not null ? 1 : 0);

        public override View this[int index] => mobilePresentation
            ? index == 0 && this.demo is not null ? this.demo : throw new IndexOutOfRangeException()
            : index switch
        {
            0 => this.list,
            1 when this.demo is not null => this.demo,
            _ => throw new IndexOutOfRangeException()
        };

        public View? Demo
        {
            get => this.demo;
            set => this.SetProtectedChild(ref this.demo, value);
        }

        public ThreeDTestPanel()
        {
            this.list = new VerticalStack();
            this.AddProtectedChild(this.list);

            this.AddTest<RotatingCubeTest>("3D Software Texture");
            this.AddTest<Tests.GpuHardwareCubeTest.GpuHardwareCubeTestView>("3D Hardware Texture");

            SelectSection(0);
        }

        public void AddTest<T>(string name) where T : View, new()
        {
            var sectionIndex = sections.Count;
            sections.Add((name, static () => new T()));
            this.list.Add(new ThreeDTestButton(() =>
            {
                SelectSection(sectionIndex);
            })
            {
                Id = name.Replace(" ", ""),
                Margin = 3,
                Text = name,
                FontFamily = "Inter",
            });
        }

        public string GetSectionName(int index) => sections[index].Name;

        public void SelectSection(int index)
        {
            if ((uint)index >= (uint)sections.Count || selectedSectionIndex == index && Demo is not null)
                return;

            selectedSectionIndex = index;
            Demo = sections[index].Create();
            SectionChanged?.Invoke();
            Invalidate();
        }

        public void UseMobilePresentation()
        {
            mobilePresentation = true;
            Invalidate();
        }

        protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
        {
            if (mobilePresentation)
            {
                demo?.Measure(new Size(300, 300), context);
                return availableBorderEdgeSize;
            }

            NFloat listWidth = 250;
            var demoSize = new Size(300, 300);

            this.list.Measure(new Size(listWidth, availableBorderEdgeSize.Height), context);
            this.demo?.Measure(demoSize, context);

            return availableBorderEdgeSize;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            if (mobilePresentation)
            {
                demo?.Arrange(new Rect(
                    rect.X + (rect.Width - 300) / 2,
                    rect.Y + (rect.Height - 300) / 2,
                    300,
                    300), context);
                return;
            }

            NFloat listWidth = 250;
            NFloat demoPadding = 20;

            this.list.Arrange(new Rect(rect.X, rect.Y, listWidth, rect.Height), context);
            this.demo?.Arrange(new Rect(
                rect.X + listWidth + demoPadding,
                rect.Y + demoPadding,
                300, 300), context);
        }

        protected override void RenderCore(IContext context)
        {
            if (mobilePresentation)
            {
                base.RenderCore(context);
                return;
            }

            NFloat listWidth = 250;

            // Gray background for the 3D demo area
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

    public class ThreeDTestButton : Label
    {
        private readonly Action onClick;
        private bool hover;
        private bool pressed;

        public ThreeDTestButton(Action onClick)
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
