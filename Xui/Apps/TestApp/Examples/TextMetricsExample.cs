using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples;

public class TextMetricsExample : Example
{
    public TextMetricsExample()
    {
        Title = "Text Metrics";
        Content = new TextMetricsView();
    }

    /// <summary>One text sample per metric, with a single box or baseline for each.</summary>
    public class TextMetricsView : View
    {
        protected override void RenderCore(IContext context)
        {
            if (Frame.Width <= 0 || Frame.Height <= 0) return;
            context.Save();
            try { DrawSamples(context); }
            finally { context.Restore(); }
        }

        private void DrawSamples(IContext context)
        {
            const string text = "Hello World!";
            var rowHeight = NFloat.Min(132, Frame.Height * 0.94f / 5);
            var labelSize = NFloat.Min(14, rowHeight * 0.19f);
            var labelHeight = labelSize * 1.3f;
            var gap = rowHeight * 0.06f;
            var sampleHeight = rowHeight - labelHeight - gap - rowHeight * 0.18f;
            context.TextAlign = TextAlign.Center;
            context.TextBaseline = TextBaseline.Alphabetic;
            context.SetFont(new Font(64, "Inter", FontWeight.Normal));
            var reference = context.MeasureText(text);
            var fontSize = NFloat.Min(64, NFloat.Min(
                64 * Frame.Width * 0.92f / reference.Line.Width,
                64 * sampleHeight / (reference.Font.FontBoundingBoxAscent + reference.Font.FontBoundingBoxDescent)));
            context.SetFont(new Font(fontSize, "Inter", FontWeight.Normal));
            var metrics = context.MeasureText(text);
            var top = Frame.Center.Y - rowHeight * 5 / 2;

            for (var i = 0; i < 5; i++)
            {
                var (name, color) = i switch
                {
                    0 => ("Actual Glyph Box", new Color(0xD23D48FF)),
                    1 => ("Font Bounding Box", new Color(0xC57912FF)),
                    2 => ("Alphabetic Baseline", new Color(0x424851FF)),
                    3 => ("Ideographic Baseline", new Color(0x218354FF)),
                    _ => ("Hanging Baseline", new Color(0x3265CCFF)),
                };
                var rowTop = top + i * rowHeight;
                context.SetFont(new Font(labelSize, "Inter", FontWeight.Normal));
                context.TextBaseline = TextBaseline.Top;
                context.SetFill(color);
                context.FillText(name, (Frame.Center.X, rowTop));

                var origin = new Point(Frame.Center.X,
                    rowTop + labelHeight + gap + metrics.Font.FontBoundingBoxAscent);
                context.SetFont(new Font(fontSize, "Inter", FontWeight.Normal));
                context.TextBaseline = TextBaseline.Alphabetic;
                context.SetFill(Colors.Black);
                context.FillText(text, origin);
                context.SetStroke(color);
                context.LineWidth = 1;

                if (i == 0)
                {
                    context.StrokeRect(new Rect(
                        origin.X - metrics.Line.ActualBoundingBoxLeft,
                        origin.Y - metrics.Line.ActualBoundingBoxAscent,
                        metrics.Line.ActualBoundingBoxLeft + metrics.Line.ActualBoundingBoxRight,
                        metrics.Line.ActualBoundingBoxAscent + metrics.Line.ActualBoundingBoxDescent));
                }
                else if (i == 1)
                {
                    context.StrokeRect(new Rect(
                        origin.X - metrics.Line.Width / 2,
                        origin.Y - metrics.Font.FontBoundingBoxAscent,
                        metrics.Line.Width,
                        metrics.Font.FontBoundingBoxAscent + metrics.Font.FontBoundingBoxDescent));
                }
                else
                {
                    var baseline = i switch
                    {
                        2 => metrics.Font.AlphabeticBaseline,
                        3 => metrics.Font.IdeographicBaseline,
                        _ => metrics.Font.HangingBaseline,
                    };
                    var y = origin.Y + baseline;
                    context.BeginPath();
                    context.MoveTo((origin.X - metrics.Line.Width / 2 - 4, y));
                    context.LineTo((origin.X + metrics.Line.Width / 2 + 4, y));
                    context.Stroke();
                }
            }
        }
    }
}
