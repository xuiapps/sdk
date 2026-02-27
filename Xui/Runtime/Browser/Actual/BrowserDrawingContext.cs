using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Browser.Actual;

public partial class BrowserDrawingContext : IContext
{
    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.reset", "main.js")]
    internal static partial void CanvasReset();

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setFillStyle", "main.js")]
    internal static partial void CanvasSetFillStyle(string fillStyle);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setLinearGradientFillStyle", "main.js")]
    internal static partial void CanvasSetLinearGradientFillStyle(double startX, double startY, double endX, double endY, double[] offsets, string[] colors);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setRadialGradientFillStyle", "main.js")]
    internal static partial void CanvasSetRadialGradientFillStyle(double startX, double startY, double startR, double endX, double endY, double endR, double[] offsets, string[] colors);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setStrokeStyle", "main.js")]
    internal static partial void CanvasSetStrokeStyle(string strokeStyle);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setLinearGradientStrokeStyle", "main.js")]
    internal static partial void CanvasSetLinearGradientStrokeStyle(double startX, double startY, double endX, double endY, double[] offsets, string[] colors);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setRadialGradientStrokeStyle", "main.js")]
    internal static partial void CanvasSetRadialGradientStrokeStyle(double startX, double startY, double startR, double endX, double endY, double endR, double[] offsets, string[] colors);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.fillRect", "main.js")]
    internal static partial void CanvasFillRect(double x, double y, double width, double height);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.strokeRect", "main.js")]
    internal static partial void CanvasStrokeRect(double x, double y, double width, double height);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setFont", "main.js")]
    internal static partial void CanvasSetFont(string font);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setGlobalAlpha", "main.js")]
    internal static partial void CanvasSetGlobalAlpha(double alpha);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setGlobalAlpha", "main.js")]
    internal static partial void CanvasSetLineCap(string lineCap);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setLineJoin", "main.js")]
    internal static partial void CanvasSetLineJoin(string lineCap);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setLineWidth", "main.js")]
    internal static partial void CanvasSetLineWidth(double lineWidth);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setLineMiterLimit", "main.js")]
    internal static partial void CanvasSetLineMiterLimit(double miterLimit);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setLineDashOffset", "main.js")]
    internal static partial void CanvasSetLineDashOffset(double lineDashOffset);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setTextAlign", "main.js")]
    internal static partial void CanvasSetTextAlign(string textAlign);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setTextBaseline", "main.js")]
    internal static partial void CanvasSetTextBaseline(string textBaseline);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.arc", "main.js")]
    internal static partial void CanvasArc(double x, double y, double radius, double startAngle, double endAngle, bool counterClockWise);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.arcTo", "main.js")]
    internal static partial void CanvasArcTo(double x1, double y1, double x2, double y2, double radius);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.beginPath", "main.js")]
    internal static partial void CanvasBeginPath();

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.clip", "main.js")]
    internal static partial void CanvasClip();

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.closePath", "main.js")]
    internal static partial void CanvasClosePath();

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.quadraticCurveTo", "main.js")]
    internal static partial void CanvasQuadraticCurveTo(double cpx, double cpy, double x, double y);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.bezierCurveTo", "main.js")]
    internal static partial void CanvasBezierCurveTo(double cp1x, double cp1y, double xp2x, double xp2y, double x, double y);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.ellipse", "main.js")]
    internal static partial void CanvasEllipse(double x, double y, double radiusX, double radiusY, double rotation, double startAngle, double endAngle, bool counterclockwise);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.fill", "main.js")]
    internal static partial void CanvasFill(string fillRule);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.fillText", "main.js")]
    internal static partial void CanvasFillText(string text, double x, double y);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.lineTo", "main.js")]
    internal static partial void CanvasLineTo(double x, double y);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.measureText", "main.js")]
    internal static partial JSObject CanvasMeasureText(string text);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.moveTo", "main.js")]
    internal static partial void CanvasMoveTo(double x, double y);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.rect", "main.js")]
    internal static partial void CanvasRect(double x, double y, double width, double height);
    
    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.restore", "main.js")]
    internal static partial void CanvasRestore();

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.rotate", "main.js")]
    internal static partial void CanvasRotate(double angle);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.roundRect", "main.js")]
    internal static partial void CanvasRoundRect(double x, double y, double width, double height, double radii);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.roundRect4", "main.js")]
    internal static partial void CanvasRoundRect4(double x, double y, double width, double height, double topLeft, double topRight, double bottomRight, double bottomLeft);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.save", "main.js")]
    internal static partial void CanvasSave();

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.scale", "main.js")]
    internal static partial void CanvasScale(double x, double y);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.translate", "main.js")]
    internal static partial void CanvasTranslate(double x, double y);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.transform", "main.js")]
    internal static partial void CanvasTransform(double a, double b, double c, double d, double e, double f);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setTransform", "main.js")]
    internal static partial void CanvasSetTransform(double a, double b, double c, double d, double e, double f);

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.stroke", "main.js")]
    internal static partial void CanvasStroke();

    [JSImport("Xui.Runtime.Browser.Actual.BrowserDrawingContext.setLineDash", "main.js")]
    internal static partial void CanvasSetLineDash(double[] segments);

    public static readonly BrowserDrawingContext Instance = new BrowserDrawingContext();

    public NFloat GlobalAlpha { set => CanvasSetGlobalAlpha(value); }

    public LineCap LineCap
    {
        set
        {
            switch(value)
            {
                case LineCap.Butt:
                    CanvasSetLineCap("butt");
                    return;
                case LineCap.Round:
                    CanvasSetLineCap("butt");
                    return;
                case LineCap.Square:
                    CanvasSetLineCap("square");
                    return;
            }
        }
    }

    public LineJoin LineJoin
    {
        set
        {
            switch(value)
            {
                case LineJoin.Round:
                    CanvasSetLineJoin("round");
                    return;
                case LineJoin.Bevel:
                    CanvasSetLineJoin("bevel");
                    return;
                case LineJoin.Miter:
                    CanvasSetLineJoin("miter");
                    return;
            }
            // "round", "bevel", and "miter"
        }
    }

    public NFloat LineWidth { set => CanvasSetLineWidth((double)value); }
    public NFloat MiterLimit { set => CanvasSetLineMiterLimit((double)value); }
    public NFloat LineDashOffset { set => CanvasSetLineDashOffset((double)value); }

    public TextAlign TextAlign
    {
        set
        {
            switch(value)
            {
                case TextAlign.Start:
                    CanvasSetTextAlign("start");
                    break;
                case TextAlign.End:
                    CanvasSetTextAlign("end");
                    break;
                case TextAlign.Left:
                    CanvasSetTextAlign("left");
                    break;
                case TextAlign.Right:
                    CanvasSetTextAlign("right");
                    break;
                case TextAlign.Center:
                    CanvasSetTextAlign("center");
                    break;
            }
        }
    }

    public TextBaseline TextBaseline
    {
        set
        {
            switch(value)
            {
                case TextBaseline.Top:
                    CanvasSetTextBaseline("top");
                    break;
                case TextBaseline.Hanging:
                    CanvasSetTextBaseline("hanging");
                    break;
                case TextBaseline.Middle:
                    CanvasSetTextBaseline("middle");
                    break;
                case TextBaseline.Alphabetic:
                    CanvasSetTextBaseline("alphabetic");
                    break;
                case TextBaseline.Ideographic:
                    CanvasSetTextBaseline("ideographic");
                    break;
                case TextBaseline.Bottom:
                    CanvasSetTextBaseline("bottom");
                    break;
            }
        }
    }

    public void Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise) =>
        CanvasArc(center.X, center.Y, radius, startAngle, endAngle, winding == Winding.CounterClockWise);

    public void ArcTo(Point cp1, Point cp2, NFloat radius) =>
        CanvasArcTo(cp1.X, cp1.Y, cp2.X, cp2.Y, radius);

    public void BeginPath() => CanvasBeginPath();

    public void Clip() => CanvasClip();

    public void ClosePath() => CanvasClosePath();

    public void CurveTo(Point cp1, Point to) => CanvasQuadraticCurveTo(cp1.X, cp1.Y, to.X, to.Y);

    public void CurveTo(Point cp1, Point cp2, Point to) => CanvasBezierCurveTo(cp1.X, cp1.Y, cp2.X, cp2.Y, to.X, to.Y);

    public void Dispose()
    {
        // throw new NotImplementedException();
    }

    public void Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise) =>
        CanvasEllipse(center.X, center.Y, radiusX, radiusY, rotation, startAngle, endAngle, winding == Winding.CounterClockWise);

    public void Fill(FillRule rule = FillRule.NonZero)
    {
        switch(rule)
        {
            case FillRule.EvenOdd:
                CanvasFill("evenodd");
                return;
            case FillRule.NonZero:
                CanvasFill("nonzero");
                return;
        }
    }

    public void FillRect(Rect rect) =>
        CanvasFillRect(rect.X, rect.Y, rect.Width, rect.Height);

    public void FillText(string text, Point pos) =>
        CanvasFillText(text, pos.X, pos.Y);

    public void LineTo(Point to) => CanvasLineTo(to.X, to.Y);

    public TextMetrics MeasureText(string text)
    {
        using var jObj = CanvasMeasureText(text);
        var width = (NFloat)jObj.GetPropertyAsDouble("width");
        var height = (NFloat)jObj.GetPropertyAsDouble("height");
        var line = new LineMetrics(width, left: 0, right: width, ascent: height, descent: 0);
        return new TextMetrics(line, default);
    }

    public void MoveTo(Point to) => CanvasMoveTo(to.X, to.Y);

    public void Rect(Rect rect) => CanvasRect(rect.X, rect.Y, rect.Width, rect.Height);

    public void Restore() => CanvasRestore();

    public void Rotate(NFloat angle) => CanvasRotate(angle);

    public void RoundRect(Rect rect, NFloat radius) =>
        CanvasRoundRect(rect.X, rect.Y, rect.Width, rect.Height, radius);

    public void RoundRect(Rect rect, CornerRadius radius) =>
        CanvasRoundRect4(rect.X, rect.Y, rect.Width, rect.Height, radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);

    public void Save() => CanvasSave();

    public void Scale(Vector vector) => CanvasScale(vector.X, vector.Y);

    public void SetFill(Color color) =>
        CanvasSetFillStyle($"rgba({color.Red * 255}, {color.Green * 255}, {color.Blue * 255}, {color.Alpha})");

    public void SetFill(LinearGradient linearGradient)
    {
        var start = linearGradient.StartPoint;
        var end = linearGradient.EndPoint;
        
        double[] offsets = new double[linearGradient.GradientStops.Length];
        string[] colors = new string[linearGradient.GradientStops.Length];
        
        for (var i = 0; i < linearGradient.GradientStops.Length; i++)
        {
            var gradientStop = linearGradient.GradientStops[i];
            var offset = gradientStop.Offset;
            var color = gradientStop.Color;
            offsets[i] = offset;
            colors[i] = $"rgba({color.Red * 255}, {color.Green * 255}, {color.Blue * 255}, {color.Alpha})";
        }

        CanvasSetLinearGradientFillStyle(start.X, start.Y, end.X, end.Y, offsets, colors);
    }

    public void SetFill(RadialGradient radialGradient)
    {
        double[] offsets = new double[radialGradient.GradientStops.Length];
        string[] colors = new string[radialGradient.GradientStops.Length];
        
        for (var i = 0; i < radialGradient.GradientStops.Length; i++)
        {
            var gradientStop = radialGradient.GradientStops[i];
            var offset = gradientStop.Offset;
            var color = gradientStop.Color;
            offsets[i] = offset;
            colors[i] = $"rgba({color.Red * 255}, {color.Green * 255}, {color.Blue * 255}, {color.Alpha})";
        }

        CanvasSetRadialGradientFillStyle(
            radialGradient.StartCenter.X, radialGradient.StartCenter.Y, radialGradient.StartRadius,
            radialGradient.EndCenter.X, radialGradient.EndCenter.Y, radialGradient.EndRadius,
            offsets,
            colors
        );
    }

    public void SetFont(Font font)
    {
        string f = "";
        if (font.FontStyle.IsItalic)
        {
            f += "italic ";
        }
        else if (font.FontStyle.IsOblique)
        {
            if (font.FontStyle.ObliqueAngle == 14f)
            {
                f += "oblique " + font.FontStyle.ObliqueAngle + "deg ";
            }
            else
            {
                f += "oblique ";
            }
        }

        f += $"{Math.Round(font.FontWeight)} {font.FontSize}px/{font.LineHeight}px ";
        foreach(var fName in font.FontFamily)
        {
            // TODO: Escape " in font name...
            f += $"\"{fName}\"";
        }

        // TODO: <font-stretch>
        
        CanvasSetFont(f);
    }

    public void SetLineDash(ReadOnlySpan<NFloat> segments)
    {
        // TODO: Try to do these things without garbage...
        double[] s = new double[segments.Length];
        for(var i = 0; i < segments.Length; i++)
        {
            s[i] = segments[i];
        }
        CanvasSetLineDash(s);
    }

    public void SetStroke(Color color) =>
        CanvasSetStrokeStyle($"rgba({color.Red * 255}, {color.Green * 255}, {color.Blue * 255}, {color.Alpha})");

    public void SetStroke(LinearGradient linearGradient)
    {
        var start = linearGradient.StartPoint;
        var end = linearGradient.EndPoint;
        
        double[] offsets = new double[linearGradient.GradientStops.Length];
        string[] colors = new string[linearGradient.GradientStops.Length];
        
        for (var i = 0; i < linearGradient.GradientStops.Length; i++)
        {
            var gradientStop = linearGradient.GradientStops[i];
            var offset = gradientStop.Offset;
            var color = gradientStop.Color;
            offsets[i] = offset;
            colors[i] = $"rgba({color.Red * 255}, {color.Green * 255}, {color.Blue * 255}, {color.Alpha})";
        }

        CanvasSetLinearGradientStrokeStyle(start.X, start.Y, end.X, end.Y, offsets, colors);
    }

    public void SetStroke(RadialGradient radialGradient)
    {
        double[] offsets = new double[radialGradient.GradientStops.Length];
        string[] colors = new string[radialGradient.GradientStops.Length];
        
        for (var i = 0; i < radialGradient.GradientStops.Length; i++)
        {
            var gradientStop = radialGradient.GradientStops[i];
            var offset = gradientStop.Offset;
            var color = gradientStop.Color;
            offsets[i] = offset;
            colors[i] = $"rgba({color.Red * 255}, {color.Green * 255}, {color.Blue * 255}, {color.Alpha})";
        }

        CanvasSetRadialGradientStrokeStyle(
            radialGradient.StartCenter.X, radialGradient.StartCenter.Y, radialGradient.StartRadius,
            radialGradient.EndCenter.X, radialGradient.EndCenter.Y, radialGradient.EndRadius,
            offsets,
            colors
        );
    }


    public void SetTransform(AffineTransform transform) =>
        CanvasSetTransform(transform.A, transform.B, transform.C, transform.D, transform.Tx, transform.Ty);

    public void Stroke() => CanvasStroke();

    public void StrokeRect(Rect rect) =>
        CanvasStrokeRect(rect.X, rect.Y, rect.Width, rect.Height);

    public void Transform(AffineTransform matrix) =>
        CanvasTransform(matrix.A, matrix.B, matrix.C, matrix.D, matrix.Tx, matrix.Ty);

    public void Translate(Vector vector) =>
        CanvasTranslate(vector.X, vector.Y);

    public void SetFill(ImagePattern pattern) => throw new NotImplementedException();
    public void SetStroke(ImagePattern pattern) => throw new NotImplementedException();

    public void DrawImage(IImage image, Rect dest) => throw new NotImplementedException();
    public void DrawImage(IImage image, Rect dest, NFloat opacity) => throw new NotImplementedException();
    public void DrawImage(IImage image, Rect source, Rect dest, NFloat opacity) => throw new NotImplementedException();
}
