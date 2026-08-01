using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Middleware.Emulator.UI;

/// <summary>Renders a device name, truncating it before it can overlap the disclosure button.</summary>
internal sealed class DeviceNameView(string text) : View
{
    private readonly Font font = new(13, "Inter", fontWeight: 600);

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        context.SetFont(font);
        return context.MeasureText(text).Size;
    }

    protected override void RenderCore(IContext context)
    {
        context.Save();
        try
        {
            context.BeginPath();
            context.Rect(Frame);
            ((IPathClipping)context).Clip();

            context.SetFont(font);
            context.TextBaseline = TextBaseline.Top;
            context.TextAlign = TextAlign.Left;
            context.SetFill(Colors.Black);
            context.FillText(FitText(context), Frame.TopLeft);
        }
        finally
        {
            context.Restore();
        }
    }

    private string FitText(IContext context)
    {
        if (context.MeasureText(text).Size.Width <= Frame.Width)
            return text;

        const string ellipsis = "…";
        for (var length = text.Length; length > 0; length--)
        {
            var candidate = text[..length] + ellipsis;
            if (context.MeasureText(candidate).Size.Width <= Frame.Width)
                return candidate;
        }

        return string.Empty;
    }
}
