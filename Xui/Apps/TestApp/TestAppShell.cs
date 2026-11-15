using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

/// <summary>Hosts TestApp content and its window-level diagnostics footer.</summary>
public sealed class TestAppShell : View
{
    public static readonly NFloat FooterHeight = 18;

    private readonly View content;
    private readonly TestAppFooter? footer;

    public TestAppShell(View content, LayoutFrameInstruments instruments, bool isFooterVisible)
    {
        this.content = content;
        this.AddProtectedChild(content);
        if (isFooterVisible)
        {
            this.footer = new TestAppFooter(instruments);
            this.AddProtectedChild(this.footer);
        }
    }

    public override int Count => footer is null ? 1 : 2;
    public override View this[int index] => index switch
    {
        0 => content,
        1 when footer is not null => footer,
        _ => throw new IndexOutOfRangeException(),
    };

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var footerHeight = footer is null ? 0 : FooterHeight;
        var contentSize = content.Measure(new Size(available.Width, NFloat.Max(0, available.Height - footerHeight)), context);
        var footerSize = footer?.Measure(new Size(available.Width, footerHeight), context) ?? Size.Empty;
        return new Size(NFloat.Max(contentSize.Width, footerSize.Width), contentSize.Height + footerHeight);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var footerHeight = footer is null ? 0 : FooterHeight;
        var contentHeight = NFloat.Max(0, rect.Height - footerHeight);
        content.Arrange(new Rect(rect.X, rect.Y, rect.Width, contentHeight), context);
        footer?.Arrange(new Rect(rect.X, rect.Y + contentHeight, rect.Width, footerHeight), context);
    }

    protected override void RenderCore(IContext context)
    {
        this.RenderChild(context, content);
        if (footer is not null)
            this.RenderChild(context, footer);
    }
}

/// <summary>Draws TestApp's window-level layout counters at the bottom-right edge.</summary>
internal sealed class TestAppFooter : View
{
    private readonly LayoutFrameInstruments instruments;

    public TestAppFooter(LayoutFrameInstruments instruments)
    {
        this.instruments = instruments;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context) => available;

    protected override void RenderCore(IContext context)
    {
        var elapsedMilliseconds = instruments.ElapsedMilliseconds;
        var estimatedFramesPerSecond = elapsedMilliseconds > 0
            ? 1000d / elapsedMilliseconds
            : 0;

        context.BeginPath();
        context.Rect(this.Frame);
        context.SetFill(new Color(0x202124EE));
        context.Fill();

        context.SetFont(new Font(10, "Inter"));
        context.TextAlign = TextAlign.Right;
        context.TextBaseline = TextBaseline.Middle;
        context.SetFill(Colors.White);
        context.FillText($"Anim {instruments.AnimationCount}  Measure {instruments.MeasureCount}  Arrange {instruments.ArrangeCount}  Render {instruments.RenderCount}  Views {instruments.UniqueViewCount}  TextMeasure {instruments.TextMeasureMilliseconds:F2} ms  {elapsedMilliseconds:F2} ms  ~{estimatedFramesPerSecond:F0} FPS", new Point(this.Frame.Right - 4, this.Frame.Center.Y));
    }
}
