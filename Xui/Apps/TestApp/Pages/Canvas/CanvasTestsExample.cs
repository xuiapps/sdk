using System.Runtime.InteropServices;
using Xui.Apps.TestApp.Pages.Canvas.Tests;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.Canvas.FontWeight;

namespace Xui.Apps.TestApp.Pages.Canvas;

public class CanvasTestsExample : Example
{
    public CanvasTestsExample()
    {
        this.Title = "Canvas Tests";
        this.Content = new CanvasTestPanel();
    }

    public class CanvasTestPanel : View
    {
        private VerticalStack list;
        private View? canvas;

        public override int Count =>
            (this.list is not null ? 1 : 0) + (this.canvas is not null ? 1 : 0);

        public override View this[int index] => index switch
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
            this.AddTest<BitmapFillTest>("BitmapFill");
            this.AddTest<DrawImageTest>("DrawImage");

            // Select first test by default
            this.Canvas = new FillRectTest();
        }

        public void AddTest<T>(string name) where T : View, new()
        {
            this.list.Add(new CanvasTestButton(() =>
            {
                this.Canvas = new T();
            })
            {
                Id = name,
                Margin = 3,
                Text = name,
                FontFamily = ["Inter"],
            });
        }

        protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
        {
            NFloat listWidth = 250;
            var canvasSize = new Size(300, 300);

            this.list.Measure(new Size(listWidth, availableBorderEdgeSize.Height), context);
            this.canvas?.Measure(canvasSize, context);

            return availableBorderEdgeSize;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            NFloat listWidth = 250;
            NFloat canvasPadding = 20;

            this.list.Arrange(new Rect(rect.X, rect.Y, listWidth, rect.Height), context);
            this.canvas?.Arrange(new Rect(
                rect.X + listWidth + canvasPadding,
                rect.Y + canvasPadding,
                300, 300), context);
        }

        protected override void RenderCore(IContext context)
        {
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
