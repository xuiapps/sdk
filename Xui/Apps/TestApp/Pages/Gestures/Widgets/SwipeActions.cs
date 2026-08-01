using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Gestures.Widgets;

/// <summary>Horizontal message actions negotiate with a vertical inbox.</summary>
public sealed class SwipeActions : ViewCollection
{
    private readonly ScrollView feed;
    private readonly Label status;
    public int ArchivedCount { get; private set; }
    public SwipeActions()
    {
        Id = "SwipeActions";
        status = Text("4 conversations · A little room to breathe", 11, color: Muted);
        status.Margin = (0, 0, 16, 0);
        var stack = new VerticalStack { Margin = InputDemoCard.ContentInset };
        stack.Add(status);
        string[] names = ["Maya Chen", "The Studio", "Weekend Club", "Alex Morgan"];
        string[] previews = ["Coffee and a new idea?", "Your moodboard is ready.", "Let's take the scenic route.", "A small note to say hello."];
        string[] initials = ["MC", "ST", "WC", "AM"];
        for (var i = 0; i < names.Length; i++)
        {
            stack.Add(new Message(names[i], previews[i], initials[i], archived =>
            {
                ArchivedCount += archived ? 1 : -1;
                status.Text = ArchivedCount == 0 ? "4 conversations · A little room to breathe" : $"{ArchivedCount} archived · Tap an archived row to undo";
            }) { Id = $"SwipeMessage{i}", MinimumHeight = 88, MaximumHeight = 88, Margin = (0, 0, 12, 0) });
        }
        Add(feed = new ScrollView { Id = "InboxFeed", Direction = ScrollDirection.Vertical, ScrollbarEndInset = 0, Content = stack });
    }
    protected override Size MeasureCore(Size available, IMeasureContext context)
    { feed.Measure(available, context); return available; }
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    { feed.Arrange(rect, context); }

    private sealed class Message(string name, string preview, string initials, Action<bool> changed) : View
    {
        private int pointer = -1;
        private NFloat offset, startX, startOffset, from, target;
        private bool settling, moved, archived, read;
        private TimeSpan? started;
        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (phase != EventPhase.Bubble) return;
            if (e.Type == PointerEventType.Down && pointer == -1)
            {
                pointer = e.PointerId; startX = e.State.Position.X; startOffset = offset; moved = false; settling = false;
                CapturePointer(pointer, PointerGestures.DragHorizontalTentative);
            }
            else if (e.PointerId == pointer)
            {
                if (e.Type == PointerEventType.Move)
                {
                    var dx = e.State.Position.X - startX;
                    moved |= NFloat.Abs(dx) > 8;
                    if (NFloat.Abs(dx) > 20) CapturePointer(pointer, PointerGestures.Drag);
                    if (!archived) offset = NFloat.Clamp(startOffset + dx, -84, 0);
                    InvalidateRender();
                }
                else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
                {
                    var id = pointer; pointer = -1;
                    if (e.Type == PointerEventType.Up && !moved)
                    {
                        if (archived) { archived = false; changed(false); }
                        else if (offset < -40 && e.State.Position.X >= Frame.Right - 84)
                        { archived = true; changed(true); }
                        else read = !read;
                    }
                    target = archived ? 0 : e.Type != PointerEventType.Up ? (startOffset < -42 ? -84 : 0) : offset < -36 ? -84 : 0;
                    from = offset; started = null; settling = true; RequestAnimationFrame(); InvalidateRender();
                    if (e.Type != PointerEventType.LostCapture) ReleasePointer(id);
                }
            }
        }
        protected override void AnimateCore(TimeSpan previous, TimeSpan current)
        {
            if (!settling) return;
            started ??= current;
            var t = NFloat.Clamp((NFloat)(current - started.Value).TotalSeconds / 0.22f, 0, 1);
            offset = from + (target - from) * Easing.EaseInOutCubic(t); InvalidateRender();
            if (t == 1) settling = false; else RequestAnimationFrame();
        }
        protected override void OnDeactivate()
        {
            var id = pointer; pointer = -1; settling = false;
            if (id != -1) ReleasePointer(id);
            base.OnDeactivate();
        }
        protected override void RenderCore(IContext ctx)
        {
            InputDrawing.Clip(ctx, Frame);
            if (archived)
            {
                InputDrawing.Panel(ctx, Frame, PaleTeal);
                InputDrawing.Text(ctx, "Archived. A clearer day.", (Frame.X + 16, Frame.Center.Y - 10), 13, Teal, true);
                InputDrawing.Text(ctx, "Tap to undo", (Frame.X + 16, Frame.Center.Y + 13), 11, Teal); return;
            }
            InputDrawing.Panel(ctx, Frame, Teal);
            InputDrawing.Text(ctx, "Archive", (Frame.Right - 42, Frame.Center.Y), 12, Colors.White, true, TextAlign.Center);
            var rect = new Rect(Frame.X + offset, Frame.Y, Frame.Width, Frame.Height);
            InputDrawing.Panel(ctx, rect, Colors.White);
            InputDrawing.Panel(ctx, new Rect(rect.X + 12, rect.Y + 22, 42, 42), PaleBlue, 21);
            InputDrawing.Text(ctx, initials, (rect.X + 33, rect.Center.Y), 12, Blue, true, TextAlign.Center);
            InputDrawing.Text(ctx, name, (rect.X + 65, rect.Y + 28), 13, Ink, !read);
            InputDrawing.Text(ctx, read ? "Read · " + preview : preview, (rect.X + 65, rect.Y + 52), 10, Muted);
            if (!read) { ctx.SetFill(Blue); ctx.BeginPath(); ctx.Arc((rect.Right - 16, rect.Y + 28), 3, 0, NFloat.Pi * 2); ctx.Fill(); }
        }
    }
}
