using System.Runtime.InteropServices;
using Xui.Apps.TestApp.Examples.Layers;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.Canvas.FontWeight;

namespace Xui.Apps.TestApp.Pages.Layers;

public class LayersExample : Example
{
    public LayersExample()
    {
        this.Title = "Layers";
        this.Content = new LayersPanel();
    }

    public class LayersPanel : View
    {
        private VerticalStack list;
        private View? demo;

        public override int Count =>
            (this.list is not null ? 1 : 0) + (this.demo is not null ? 1 : 0);

        public override View this[int index] => index switch
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

        public LayersPanel()
        {
            this.list = new VerticalStack();
            this.AddProtectedChild(this.list);

            this.AddDemo<BorderDemo>("Border");
            this.AddDemo<BorderWithPaddingDemo>("Border + Padding");
            this.AddDemo<CheckboxDemo>("Checkbox");
            this.AddDemo<VerticalStackDemo>("Vertical Mono Stack");
            this.AddDemo<VerticalPolyStackDemo>("Vertical Poly Stack");
            this.AddDemo<NumPadDemo>("Numpad");

            // Select first demo by default
            this.Demo = new BorderDemo();
        }

        public void AddDemo<T>(string name) where T : View, new()
        {
            this.list.Add(new LayersDemoButton(() =>
            {
                this.Demo = new T();
            })
            {
                Id = name,
                Margin = 3,
                Text = name,
                FontFamily = ["Inter"]
            });
        }

        protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
        {
            NFloat listWidth = 200;

            this.list.Measure(new Size(listWidth, availableBorderEdgeSize.Height), context);
            this.demo?.Measure(new Size(availableBorderEdgeSize.Width - listWidth, availableBorderEdgeSize.Height), context);

            return availableBorderEdgeSize;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            NFloat listWidth = 200;

            this.list.Arrange(new Rect(rect.X, rect.Y, listWidth, rect.Height), context);
            this.demo?.Arrange(new Rect(rect.X + listWidth, rect.Y, rect.Width - listWidth, rect.Height), context);
        }

        protected override void RenderCore(IContext context)
        {
            NFloat listWidth = 200;

            // Light gray background for the demo panel area
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

    public class LayersDemoButton : Label
    {
        private readonly Action onClick;
        private bool hover;
        private bool pressed;

        public LayersDemoButton(Action onClick)
        {
            this.onClick = onClick;
        }

        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (e.State.PointerType == PointerType.Mouse)
            {
                if (e.Type == PointerEventType.Enter)
                {
                    hover = true;
                    this.InvalidateRender();
                }
                else if (e.Type == PointerEventType.Leave)
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
