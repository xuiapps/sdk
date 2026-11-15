using Xui.Core.Animation;
using Xui.Apps.TestApp.Pages.Canvas;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages;

/// <summary>Retained example cards that follow a drag and settle on one centered example.</summary>
internal sealed class ExampleCardCarousel : ViewCollection
{
    private static readonly Easing.CubicBezier Easing = new((0.22f, 1), (0.36f, 1));
    private readonly List<TransformView> cards = [];
    private NFloat position;
    private NFloat stride = 1;
    private NFloat indicatorY;
    private NFloat animationFrom;
    private int animationTarget;
    private bool settling;
    private TimeSpan? animationStart;
    private int pointerId = -1;
    private Point pointerStart;
    private NFloat dragStart;
    private int dragIndex;
    private bool dragging;
    private NFloat lastX;
    private NFloat velocity, presentationVelocity;
    private long lastTick;
    private TimeProvider clock = TimeProvider.System;
    private bool elastic;
    private bool springReturn;
    private NFloat springVelocity;
    private SpringMotionCurve? spring;

    public NFloat MaximumCardHeight { get; init; } = 472;
    public bool RespectChildGestures { get; init; }

    public int SelectedIndex { get; private set; }
    public event Action? SelectionChanged;

    public ExampleCardCarousel(IReadOnlyList<(string Name, Func<View> Create)> sections, int selectedIndex,
        Func<string, ExampleCardDescription>? describe = null, NFloat? cardPadding = null, bool clipCardContent = false)
    {
        Id = "CanvasDrawingCarousel";
        position = SelectedIndex = selectedIndex;
        for (var i = 0; i < sections.Count; i++)
        {
            var section = sections[i];
            var card = new TransformView
            {
                Id = $"CanvasCard.{section.Name}",
                Content = new CardContent(section.Create(), DisplayName(section.Name), $"{i + 1} / {sections.Count}",
                    describe?.Invoke(section.Name), cardPadding, clipCardContent),
            };
            cards.Add(card);
            Add(card);
        }
    }

    private static string DisplayName(string name) =>
        System.Text.RegularExpressions.Regex.Replace(name, "(?<=[a-z])(?=[A-Z])", " ");

    public void Select(int index)
    {
        if ((uint)index >= (uint)cards.Count) return;
        Settle(index);
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var size = CardSize(available);
        foreach (var card in cards) card.Measure(size, context);
        return available;
    }

    private Size CardSize(Size available) => new(
        NFloat.Max(1, NFloat.Min(420, available.Width - 72)),
        NFloat.Max(1, NFloat.Min(MaximumCardHeight, available.Height - 72)));

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var size = CardSize(rect.Size);
        // Keep a peek of the smaller neighboring cards visible at rest.
        stride = size.Width * 0.9f + 16;
        var cardRect = new Rect(rect.X + (rect.Width - size.Width) / 2,
            rect.Y + (rect.Height - 32 - size.Height) / 2, size.Width, size.Height);
        indicatorY = cardRect.Bottom + 24;
        foreach (var card in cards) card.Arrange(cardRect, context);
        UpdateTransforms();
    }

    private void UpdateTransforms()
    {
        for (var i = 0; i < cards.Count; i++)
        {
            var distance = i - position;
            var scale = 1 - 0.2f * (1 - Core.Animation.Easing.EaseInOutCubic(
                NFloat.Max(0, 1 - NFloat.Abs(distance))));
            var center = cards[i].Frame.Center;
            cards[i].Transform = new AffineTransform(scale, 0, 0, scale,
                distance * stride + center.X * (1 - scale), center.Y * (1 - scale));
        }
        InvalidateRender();
    }

    protected override void RenderCore(IContext context)
    {
        context.Save();
        try
        {
            context.BeginPath();
            context.Rect(Frame);
            context.Clip();
            foreach (var card in cards) RenderChild(context, card, Frame);
            RenderIndicator(context);
        }
        finally { context.Restore(); }
    }

    private void RenderIndicator(IContext context)
    {
        if (cards.Count == 0) return;

        // The two nearest dots share the emphasis throughout dragging and settling.
        // Growing dots also gently shift their neighbors, keeping the whole row centered.
        NFloat width = 6 * (cards.Count - 1);
        for (var i = 0; i < cards.Count; i++) width += 2 * DotRadius(i);
        var scale = NFloat.Min(1, NFloat.Max(1, Frame.Width - 32) / width);
        var x = Frame.Center.X - width * scale / 2;
        for (var i = 0; i < cards.Count; i++)
        {
            var emphasis = DotEmphasis(i);
            var radius = DotRadius(i) * scale;
            var gray = 0.7f - 0.22f * emphasis;
            context.SetFill(new Color(gray, gray, gray, 1));
            context.BeginPath();
            context.Arc((x + radius, indicatorY), radius, 0, NFloat.Pi * 2);
            context.Fill();
            x += 2 * radius + 6 * scale;
        }
    }

    private NFloat DotRadius(int index) => 3 + 2 * DotEmphasis(index);

    private NFloat DotEmphasis(int index) => Core.Animation.Easing.EaseInOutCubic(
        NFloat.Max(0, 1 - NFloat.Abs(index - position)));

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;
        if (e.Type == PointerEventType.Down && pointerId == -1)
        {
            if (RespectChildGestures && GetCapturedGesture(e.PointerId) is { } captured && captured is not ITap and not IDragVerticalTentative)
                return;
            // Leave the navigation view's edge gesture available even on the last card.
            var edge = this.TryFindParent<NavigationView>(out var navigation) ? navigation.BackSwipeEdgeWidth : 25;
            if (e.State.Position.X <= edge) return;
            elastic = GetService(typeof(IDeviceInfo)) is IDeviceInfo
                { Platform: DevicePlatform.iOS or DevicePlatform.MacOS, PrefersReducedMotion: false };
            clock = GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
            pointerId = e.PointerId;
            settling = false; spring = null;
            pointerStart = e.State.Position;
            lastX = pointerStart.X;
            lastTick = clock.GetTimestamp();
            velocity = presentationVelocity = 0;
            return;
        }
        if (e.PointerId != pointerId) return;

        if (e.Type == PointerEventType.Move)
        {
            var dx = e.State.Position.X - pointerStart.X;
            var dy = e.State.Position.Y - pointerStart.Y;
            if (!dragging)
            {
                if (RespectChildGestures && GetCapturedGesture(e.PointerId) is { } captured && captured is not ITap and not IDragVerticalTentative)
                {
                    pointerId = -1;
                    if (position != SelectedIndex) Settle(SelectedIndex);
                    return;
                }
                if (NFloat.Abs(dy) > 8 && NFloat.Abs(dy) > NFloat.Abs(dx))
                {
                    pointerId = -1;
                    if (position != SelectedIndex) Settle(SelectedIndex);
                    return;
                }
                if (NFloat.Abs(dx) <= 8) return;
                dragging = true;
                settling = false;
                spring = null;
                dragStart = elastic ? ElasticMotion.Unresist(position, 0, cards.Count - 1, Frame.Width / stride) : position;
                dragIndex = Math.Clamp((int)NFloat.Round(position), 0, cards.Count - 1);
                CapturePointer(e.PointerId, PointerGestures.Drag);
            }
            var now = clock.GetTimestamp();
            var dt = (NFloat)clock.GetElapsedTime(lastTick, now).TotalSeconds;
            if (dt >= 0.001f) velocity = (e.State.Position.X - lastX) / dt;
            lastX = e.State.Position.X;
            lastTick = now;
            var desired = dragStart - dx / stride;
            var min = NFloat.Max(0, dragIndex - 1);
            var max = NFloat.Min(cards.Count - 1, dragIndex + 1);
            var previousPosition = position;
            position = elastic && ((desired < 0 && min == 0) || (desired > cards.Count - 1 && max == cards.Count - 1))
                ? ElasticMotion.Resist(desired, 0, cards.Count - 1, Frame.Width / stride)
                : NFloat.Clamp(desired, min, max);
            if (dt >= 0.001f) presentationVelocity = (position - previousPosition) * stride / dt;
            UpdateTransforms();
        }
        else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
        {
            if (!dragging && e.Type == PointerEventType.Up &&
                NFloat.Abs(e.State.Position.Y - indicatorY) <= 20 &&
                NFloat.Abs(pointerStart.Y - indicatorY) <= 20 &&
                NFloat.Abs(e.State.Position.X - pointerStart.X) < 8)
                SelectIndicator(e.State.Position.X);
            var id = pointerId;
            pointerId = -1;
            if (dragging)
            {
                dragging = false;
                var target = SelectedIndex;
                if (e.Type == PointerEventType.Up)
                {
                    var delta = position - dragIndex;
                    var fast = clock.GetElapsedTime(lastTick).TotalMilliseconds < 100 && NFloat.Abs(velocity) > 500;
                    target = dragIndex + (fast ? (velocity < 0 ? 1 : -1) : NFloat.Abs(delta) > 0.2f ? (delta > 0 ? 1 : -1) : 0);
                }
                if (position < 0) target = 0;
                else if (position > cards.Count - 1) target = cards.Count - 1;
                springVelocity = e.Type == PointerEventType.Up && clock.GetElapsedTime(lastTick).TotalMilliseconds < 100
                    ? NFloat.Clamp(presentationVelocity / stride, -4, 4) : 0;
                Settle(Math.Clamp(target, 0, cards.Count - 1));
            }
            if (!dragging && !settling && position != SelectedIndex)
            {
                springVelocity = 0;
                Settle(SelectedIndex);
            }
            if (e.Type != PointerEventType.LostCapture) ReleasePointer(id);
        }
    }

    private void SelectIndicator(NFloat pointerX)
    {
        NFloat width = 6 * (cards.Count - 1);
        for (var i = 0; i < cards.Count; i++) width += 2 * DotRadius(i);
        var scale = NFloat.Min(1, NFloat.Max(1, Frame.Width - 32) / width);
        var x = Frame.Center.X - width * scale / 2;
        if (pointerX < x - 12 || pointerX > x + width * scale + 12) return;
        for (var i = 0; i < cards.Count; i++)
        {
            x += (2 * DotRadius(i) + 6) * scale;
            if (pointerX <= x - 3 * scale || i == cards.Count - 1) { Select(i); return; }
        }
    }

    protected override void OnDeactivate()
    {
        var id = pointerId;
        pointerId = -1;
        dragging = settling = false;
        spring = null; springVelocity = 0;
        position = SelectedIndex;
        if (id != -1) ReleasePointer(id);
        UpdateTransforms();
        base.OnDeactivate();
    }

    private void Settle(int index)
    {
        springReturn = elastic && (position < 0 || position > cards.Count - 1);
        spring = null;
        animationFrom = position;
        animationTarget = index;
        animationStart = null;
        settling = true;
        RequestAnimationFrame();
        InvalidateRender();
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        if (!settling) return;
        animationStart ??= current;
        var progress = NFloat.Clamp((NFloat)((current - animationStart.Value).TotalMilliseconds / 300), 0, 1);
        var complete = progress >= 1;
        if (springReturn)
        {
            spring ??= new SpringMotionCurve(animationStart.Value, animationFrom * stride, springVelocity * stride, animationTarget * stride);
            position = spring.Value[current] / stride;
            complete = spring.Value.IsComplete(current);
        }
        else position = animationFrom + (animationTarget - animationFrom) * Easing.Evaluate(progress, 0.0005f);
        if (complete)
        {
            position = animationTarget;
            settling = false;
            spring = null; springVelocity = 0;
            if (SelectedIndex != animationTarget)
            {
                SelectedIndex = animationTarget;
                SelectionChanged?.Invoke();
            }
        }
        else RequestAnimationFrame();
        UpdateTransforms();
    }

    private sealed class CardContent : ViewCollection
    {
        private readonly Card card;
        private readonly Label title;
        private readonly Label counter;

        public CardContent(View example, string name, string count, ExampleCardDescription? description,
            NFloat? padding, bool clipContent)
        {
            Add(card = new Card
            {
                ShadowExtent = 12,
                Padding = padding,
                ClipContent = clipContent,
                ShadowColor = new Color(0f, 0f, 0f, 0.06f),
                Content = description is not null
                    ? new LayoutContent(new DrawingPresentation(new FittedDrawing(example), description))
                    : example is ICanvasDrawingExample ? new FittedDrawing(example) : new LayoutContent(example),
            });
            Add(title = new Label { Text = name, FontFamily = "Inter", FontSize = 20,
                FontWeight = FontWeight.SemiBold, TextColor = new Color(0x242529FF) });
            Add(counter = new Label { Text = count, FontFamily = "Inter", FontSize = 13,
                TextColor = new Color(0x757575FF) });
        }

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            card.Measure((available.Width, NFloat.Max(1, available.Height - 72)), context);
            title.Measure((available.Width, 32), context);
            counter.Measure((available.Width, 24), context);
            return available;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            card.Arrange((rect.X, rect.Y, rect.Width, NFloat.Max(1, rect.Height - 72)), context);
            title.Arrange((rect.X + 12, rect.Bottom - 56, NFloat.Max(0, rect.Width - 24), 32), context);
            counter.Arrange((rect.X + 12, rect.Bottom - 24, NFloat.Max(0, rect.Width - 24), 24), context);
        }
    }

    private sealed class DrawingPresentation : ViewCollection
    {
        private readonly View drawing;
        private readonly Label heading;
        private readonly Label title;
        private readonly Label description;
        private readonly VerticalStack code;

        public DrawingPresentation(View drawing, ExampleCardDescription copy)
        {
            Margin = 16;
            this.drawing = drawing;
            Add(heading = Layout.LayoutPreviewStyle.Text("CANVAS / " + copy.Category, 10,
                FontWeight.SemiBold, Layout.LayoutPreviewStyle.Teal));
            Add(title = Layout.LayoutPreviewStyle.Text(copy.Title, 24, FontWeight.SemiBold));
            Add(description = Layout.LayoutPreviewStyle.Text(copy.Description, 12,
                color: Layout.LayoutPreviewStyle.Muted));
            Add(drawing);
            Add(code = new VerticalStack());
            foreach (var line in ("C#: " + copy.Code).Split('\n'))
                code.Add(Layout.LayoutPreviewStyle.Text(line, 11, color: Layout.LayoutPreviewStyle.Blue));
        }

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            heading.Measure((available.Width, 16), context);
            title.Measure((available.Width, 32), context);
            description.Measure((available.Width, 20), context);
            drawing.Measure((available.Width, NFloat.Max(1, available.Height - 132)), context);
            code.Measure((available.Width, 40), context);
            return available;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            heading.Arrange((rect.X, rect.Y, rect.Width, 16), context);
            title.Arrange((rect.X, rect.Y + 24, rect.Width, 32), context);
            description.Arrange((rect.X, rect.Y + 60, rect.Width, 20), context);
            drawing.Arrange((rect.X, rect.Y + 90, rect.Width, NFloat.Max(1, rect.Height - 142)), context);
            code.Arrange((rect.X, rect.Bottom - 36, rect.Width, 36), context);
        }
    }

    private sealed class LayoutContent : ViewCollection
    {
        private readonly TransformView layout;

        public LayoutContent(View example) => Add(layout = new TransformView { Content = example });

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            var scale = NFloat.Max(0.001f, NFloat.Min(1, available.Width / 320));
            layout.Measure((available.Width / scale, available.Height / scale), context);
            return available;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            var scale = NFloat.Max(0.001f, NFloat.Min(1, rect.Width / 320));
            layout.Arrange((0, 0, rect.Width / scale, rect.Height / scale), context);
            layout.Transform = new AffineTransform(scale, 0, 0, scale, rect.X, rect.Y);
        }

        protected override void RenderCore(IContext context)
        {
            context.Save();
            try
            {
                context.BeginPath();
                context.Rect(Frame);
                context.Clip();
                base.RenderCore(context);
            }
            finally { context.Restore(); }
        }
    }

    private sealed class FittedDrawing : ViewCollection
    {
        private readonly View example;
        private readonly TransformView drawing;

        public FittedDrawing(View example)
        {
            this.example = example;
            Add(drawing = new TransformView { Content = example });
        }

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            drawing.Measure((600, 300), context);
            return available;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            var bounds = ((ICanvasDrawingExample)example).GetArtworkBounds(context);
            var scale = NFloat.Max(0.001f, NFloat.Min(rect.Width / bounds.Width, rect.Height / bounds.Height));
            drawing.Arrange((0, 0, 600, 300), context);
            drawing.Transform = new AffineTransform(scale, 0, 0, scale,
                rect.Center.X - bounds.Center.X * scale,
                rect.Center.Y - bounds.Center.Y * scale);
        }
    }
}
