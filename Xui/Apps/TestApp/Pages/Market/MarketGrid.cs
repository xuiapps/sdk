using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Core.UI.Layer;
using Xui.DevKit.UI.Layer;
using NFloat = System.Runtime.InteropServices.NFloat;
using static Xui.Apps.TestApp.Pages.Market.MarketDrawing;

namespace Xui.Apps.TestApp.Pages.Market;

/// <summary>
/// One leaf view for the entire dataset. A reusable value-type layer composition is fed each
/// visible item; there are no row views, cell objects, bindings, or recycling queues.
/// A row's data is generated on first visibility and cached for subsequent frames.
/// ScrollView supplies the culling rectangle in content coordinates and owns inertia/gesture negotiation.
/// </summary>
public sealed class MarketGrid : View
{
    public static readonly NFloat RowHeight = 100;
    public static readonly NFloat IntroHeight = 168;
    public MarketData Data { get; }
    public Action<MarketStock>? Selected { get; set; }
    public int FirstRenderedRow { get; private set; }
    public int RenderedRowCount { get; private set; }
    internal MarketStock CurrentItem = null!;
    private int pressedRow = -1, pointer = -1;
    private PressFeedbackLayer<MarketGrid> feedback = new() { Background = Paper, RippleColor = Ink };
    private DockLayer.Dock4<MarketGrid, MarketCell<LogoPainter>, MarketCell<IdentityPainter>,
        MarketCell<QuotePainter>, MarketCell<ChevronPainter>> row;

    public MarketGrid(MarketData data)
    {
        Id = "Market.Grid"; Data = data;
        row.Child1 = new() { Align = DockLayer.Align.Left, Child = new() { Width = 62 } };
        row.Child2 = new() { Align = DockLayer.Align.Left };
        row.Child3 = new() { Align = DockLayer.Align.Stretch };
        row.Child4 = new() { Align = DockLayer.Align.Right, Child = new() { Width = 34 } };
    }

    protected override Size MeasureCore(Size available, IMeasureContext context) =>
        (NFloat.IsFinite(available.Width) ? available.Width : 390, IntroHeight + RowHeight * Data.Count);

    protected override void RenderCore(IContext context)
    {
        var visible = RenderCullingFrame ?? Frame;
        var first = Math.Clamp((int)NFloat.Floor((visible.Y - Frame.Y - IntroHeight) / RowHeight), 0, Data.Count);
        var end = Math.Clamp((int)NFloat.Ceiling((visible.Bottom - Frame.Y - IntroHeight) / RowHeight), first, Data.Count);
        FirstRenderedRow = first; RenderedRowCount = end - first;
        context.SetFill(Colors.White); context.FillRect(visible);
        if (visible.Y < Frame.Y + IntroHeight && visible.Bottom > Frame.Y)
        {
            var x = Frame.X + 16; var y = Frame.Y;
            Text(context, "THE MARKET / SIMULATED", (x, y + 20), Caption, Purple);
            Text(context, "Market Pulse", (x, y + 43), Price, Ink);
            Text(context, "Explore 5,000 companies, their stories", (x, y + 91), Body, Muted);
            Text(context, "and the trends behind every price.", (x, y + 110), Body, Muted);
            Text(context, "60 SESSIONS  /  5,000 COMPANIES", (x, y + 140), Caption, Purple);
            Rule(context, (Frame.X, y + IntroHeight), (Frame.Right, y + IntroHeight), Line);
        }
        row.Child2.Child.Width = NFloat.Min(146, NFloat.Max(104, (Frame.Width - 96) * .38f));
        // Layout is arithmetic only. Measure once for the row width, never per item/text run.
        row.Measure(this, (Frame.Width, RowHeight), context);
        for (var i = first; i < end; i++)
        {
            CurrentItem = Data[i];
            var rect = RowBounds(i);
            feedback.Target = i;
            feedback.Arrange(this, rect, context);
            feedback.Render(this, context);
            row.Arrange(this, rect, context);
            row.Render(this, context);
            Rule(context, (rect.X + 62, rect.Bottom), rect.BottomRight, Line);
        }
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.Type is PointerEventType.Cancel or PointerEventType.LostCapture && e.PointerId == pointer)
        { pointer = -1; pressedRow = -1; feedback.End(this, false); }
        if (phase != EventPhase.Tunnel) return;
        if (e.Type == PointerEventType.Down && (pointer == -1 || pointer == e.PointerId))
        {
            // A new sequence for this pointer replaces any unfinished press. Other live
            // pointers must not steal the active row or trigger its action.
            var previous = pointer;
            pointer = pressedRow = -1;
            feedback.End(this, false);
            if (previous != -1) ReleasePointer(previous);
            pressedRow = RowAt(e.State.Position);
            if (pressedRow < 0) return;
            pointer = e.PointerId;
            feedback.Target = pressedRow;
            feedback.SetBounds(RowBounds(pressedRow));
            feedback.Begin(this, e.State.Position, PressFeedbackLayer<MarketGrid>.ShouldDelay(this, ref e));
            CapturePointer(e.PointerId, PointerGestures.Tap);
        }
        // Moving inside the row keeps the press alive. An actual scroll/back gesture takes
        // capture and sends Cancel/LostCapture; displacement on an unrelated axis must not cancel it.
        else if (e.Type == PointerEventType.Up && e.PointerId == pointer)
        {
            var index = pressedRow;
            var tapped = index >= 0 && RowAt(e.State.Position) == index;
            pointer = -1; pressedRow = -1;
            feedback.End(this, tapped);
            ReleasePointer(e.PointerId);
            if (tapped) Selected?.Invoke(Data[index]);
        }
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        feedback.Animate(this, previous, current);
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        feedback.Activate(this);
    }

    protected override void OnDeactivate()
    {
        var captured = pointer;
        pointer = pressedRow = -1;
        feedback.End(this, false);
        if (captured != -1) ReleasePointer(captured);
        base.OnDeactivate();
    }

    private Rect RowBounds(int index) =>
        new(Frame.X, Frame.Y + IntroHeight + index * RowHeight, Frame.Width, RowHeight);

    private int RowAt(Point point)
    {
        if (!Frame.Contains(point) || point.Y < Frame.Y + IntroHeight) return -1;
        var index = (int)((point.Y - Frame.Y - IntroHeight) / RowHeight);
        return index < Data.Count ? index : -1;
    }
}

// Static painter dispatch keeps the layer composition unboxed; Dock4 lays out its four slots.
internal interface IMarketPainter
{
    static abstract void Draw(MarketGrid host, IContext context, Rect bounds);
}

internal struct MarketCell<TPainter> : ILayer<MarketGrid> where TPainter : struct, IMarketPainter
{
    public NFloat Width;
    private Rect bounds;
    public Size Measure(MarketGrid view, Size available, IMeasureContext context) => (Width, MarketGrid.RowHeight);
    public void Arrange(MarketGrid view, Rect rect, IMeasureContext context) => bounds = rect;
    public void Render(MarketGrid view, IContext context) => TPainter.Draw(view, context, bounds);
    public void Animate(MarketGrid view, TimeSpan previousTime, TimeSpan currentTime) { }
    public void OnPointerEvent(MarketGrid view, ref PointerEventRef e, EventPhase phase) { }
    public void OnKeyDown(MarketGrid view, ref KeyEventRef e) { }
    public void OnChar(MarketGrid view, ref KeyEventRef e) { }
    public void OnFocus(MarketGrid view) { }
    public void OnBlur(MarketGrid view) { }
}

internal readonly struct LogoPainter : IMarketPainter
{
    public static void Draw(MarketGrid host, IContext ctx, Rect r) => Logo(ctx, host.CurrentItem, (r.X + 12, r.Y + 26, 40, 40));
}

internal readonly struct IdentityPainter : IMarketPainter
{
    public static void Draw(MarketGrid host, IContext ctx, Rect r)
    {
        var item = host.CurrentItem;
        // Clip one column, so narrow screens never let long names overlap prices.
        ctx.Save(); ctx.BeginPath(); ctx.Rect((r.X, r.Y, NFloat.Max(0, r.Width - 8), r.Height)); ctx.Clip();
        Text(ctx, item.Symbol, (r.X, r.Y + 15), Strong, Ink);
        Text(ctx, item.Name, (r.X, r.Y + 37), Body, Ink);
        Text(ctx, item.Sector, (r.X, r.Y + 56), Small, Muted);
        Text(ctx, item.Volume, (r.X, r.Y + 72), Small, Muted);
        ctx.Restore();
    }
}

internal readonly struct QuotePainter : IMarketPainter
{
    public static void Draw(MarketGrid host, IContext ctx, Rect r)
    {
        var item = host.CurrentItem;
        Text(ctx, item.Price, (r.Right, r.Y + 15), Strong, Ink, TextAlign.Right);
        Text(ctx, item.Change, (r.X, r.Y + 16), Body, item.IsPositive ? Up : Down);
        Sparkline(ctx, item, (r.X, r.Y + 39, r.Width, 47));
    }
}

internal readonly struct ChevronPainter : IMarketPainter
{
    public static void Draw(MarketGrid host, IContext ctx, Rect r)
    {
        ctx.SetStroke(Muted); ctx.LineWidth = 1.5f; ctx.LineCap = LineCap.Round;
        ctx.BeginPath(); ctx.MoveTo((r.Center.X - 2, r.Center.Y - 4)); ctx.LineTo((r.Center.X + 2, r.Center.Y));
        ctx.LineTo((r.Center.X - 2, r.Center.Y + 4)); ctx.Stroke();
    }
}
