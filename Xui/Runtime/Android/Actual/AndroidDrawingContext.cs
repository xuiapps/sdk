using System;
using System.Runtime.InteropServices;
using Android.Graphics;
using Android.Net;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;

using ARect = global::Android.Graphics.Rect;

namespace Xui.Runtime.Android.Actual;

public class AndroidDrawingContext : IContext
{
    private FilterState filter;
    public void SetFilter() => filter = default;
    public void SetFilter(Brightness brightness) => filter = new(brightness);

    private Paint aFillPaint;
    private Paint aStrokePaint;

    private ARect aRect;

    public AndroidDrawingContext()
    {
        this.aFillPaint = new Paint();
        this.aFillPaint.SetStyle(Paint.Style.Fill);

        this.aStrokePaint = new Paint();
        this.aStrokePaint.SetStyle(Paint.Style.Stroke);

        this.aRect = new ARect();
    }

    public Canvas? Canvas { get; set; }

    public NFloat GlobalAlpha { set => throw new NotImplementedException(); }
    
    public LineCap LineCap
    {
        set
        {
            switch(value)
            {
                case LineCap.Butt:
                    this.aStrokePaint.StrokeCap = Paint.Cap.Butt;
                    break;
                case LineCap.Round:
                    this.aStrokePaint.StrokeCap = Paint.Cap.Round;
                    break;
                case LineCap.Square:
                    this.aStrokePaint.StrokeCap = Paint.Cap.Round;
                    break;
            }
        }
    }
    public LineJoin LineJoin
    {
        set
        {
            switch(value)
            {
                case LineJoin.Miter:
                    this.aStrokePaint.StrokeJoin = Paint.Join.Miter;
                    break;
                case LineJoin.Round:
                    this.aStrokePaint.StrokeJoin = Paint.Join.Round;
                    break;
                case LineJoin.Bevel:
                    this.aStrokePaint.StrokeJoin = Paint.Join.Bevel;
                    break;
            }
        }
    }

    public NFloat LineWidth
    {
        set
        {
            this.aStrokePaint.StrokeWidth = (float)value;
        }
    }

    public NFloat MiterLimit
    {
        set
        {
            this.aStrokePaint.StrokeMiter = (float)value;
        }
    }
    
    // TODO: Setting this should invalidate the dash path effect...
    public NFloat LineDashOffset { get; set; }
    public TextAlign TextAlign { set => throw new NotImplementedException(); }
    public TextBaseline TextBaseline { set => throw new NotImplementedException(); }

    public void Arc(Core.Math2D.Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise)
    {
        throw new NotImplementedException();
    }

    public void ArcTo(Core.Math2D.Point cp1, Core.Math2D.Point cp2, NFloat radius)
    {
        throw new NotImplementedException();
    }

    public void BeginPath()
    {
        throw new NotImplementedException();
    }

    public void Clip()
    {
        throw new NotImplementedException();
    }

    public void ClosePath()
    {
        throw new NotImplementedException();
    }

    public void CurveTo(Core.Math2D.Point cp1, Core.Math2D.Point to)
    {
        throw new NotImplementedException();
    }

    public void CurveTo(Core.Math2D.Point cp1, Core.Math2D.Point cp2, Core.Math2D.Point to)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // throw new NotImplementedException();
    }

    public void Ellipse(Core.Math2D.Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise)
    {
        throw new NotImplementedException();
    }

    public void Fill(FillRule rule = FillRule.NonZero)
    {
        throw new NotImplementedException();
    }

    public void FillRect(Core.Math2D.Rect rect)
    {
        // TODO: Maybe use a path and draw a rect to avoid int convertion.
        this.aRect.Left = (int)rect.Left;
        this.aRect.Top = (int)rect.Top;
        this.aRect.Right = (int)rect.Right;
        this.aRect.Bottom = (int)rect.Bottom;

        this.Canvas!.DrawRect(this.aRect, this.aFillPaint);
    }

    public void FillText(string text, Core.Math2D.Point pos)
    {
        this.Canvas!.DrawText(text, (float)pos.X, (float)pos.Y, this.aFillPaint);
    }

    public void LineTo(Core.Math2D.Point to)
    {
        throw new NotImplementedException();
    }

    public Vector MeasureText(string text)
    {
        throw new NotImplementedException();
    }

    public void MoveTo(Core.Math2D.Point to)
    {
        throw new NotImplementedException();
    }

    public void Rect(Core.Math2D.Rect rect)
    {
        throw new NotImplementedException();
    }

    public void Restore()
    {
        throw new NotImplementedException();
    }

    public void Rotate(NFloat angle)
    {
        throw new NotImplementedException();
    }

    public void RoundRect(Core.Math2D.Rect rect, NFloat radius)
    {
        throw new NotImplementedException();
    }

    public void RoundRect(Core.Math2D.Rect rect, CornerRadius radius)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Scale(Vector vector)
    {
        throw new NotImplementedException();
    }

    public void SetFill(Core.Canvas.Color color)
    {
        color = filter.Apply(color);
        this.aFillPaint.SetARGB((int)(color.Alpha * 255), (int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255));
    }

    public void SetFill(Core.Canvas.LinearGradient linearGradient)
    {
        throw new NotImplementedException();
    }

    public void SetFill(Core.Canvas.RadialGradient radialGradient)
    {
        throw new NotImplementedException();
    }

    public void SetFont(Font font)
    {
        TypefaceStyle typefaceStyle;
        if (font.FontWeight == 700 && font.FontStyle.IsItalic)
        {
            this.aFillPaint.TextSkewX = 0;
            typefaceStyle = TypefaceStyle.BoldItalic;
        }
        else if (font.FontWeight == 700)
        {
            typefaceStyle = TypefaceStyle.Bold;
        }
        else if (font.FontStyle.IsItalic)
        {
            this.aFillPaint.TextSkewX = 0;
            typefaceStyle = TypefaceStyle.Italic;
        }
        else
        {
            typefaceStyle = TypefaceStyle.Normal;
        }

        if (font.FontStyle.IsOblique)
        {
            // Affects transform within the paint
            // this.aFillPaint.TextSkewX = (float) -font.FontStyle.ObliqueAngle;
        }
        else
        {
            this.aFillPaint.TextSkewX = 0;
        }

        this.aFillPaint.TextSize = (float)font.FontSize;

        // TODO: Custom font weight,
        
        Typeface? tf = Typeface.Create(font.FontFamily, typefaceStyle);
        this.aFillPaint.SetTypeface(tf);
    }

    public void SetLineDash(ReadOnlySpan<NFloat> segments)
    {
        throw new NotImplementedException();
    }

    public void SetStroke(Core.Canvas.Color color)
    {
        color = filter.Apply(color);
        this.aStrokePaint.SetARGB((int)(color.Alpha * 255), (int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255));
    }

    public void SetStroke(Core.Canvas.LinearGradient linearGradient)
    {
        throw new NotImplementedException();
    }

    public void SetStroke(Core.Canvas.RadialGradient radialGradient)
    {
        throw new NotImplementedException();
    }

    public void SetTransform(AffineTransform transform)
    {
        throw new NotImplementedException();
    }

    public void Stroke()
    {
        throw new NotImplementedException();
    }

    public void StrokeRect(Core.Math2D.Rect rect)
    {
        // TODO: Maybe use a path and draw a rect to avoid int convertion.
        this.aRect.Left = (int)rect.Left;
        this.aRect.Top = (int)rect.Top;
        this.aRect.Right = (int)rect.Right;
        this.aRect.Bottom = (int)rect.Bottom;

        this.Canvas!.DrawRect(this.aRect, this.aStrokePaint);
    }

    public void Transform(AffineTransform matrix)
    {
        throw new NotImplementedException();
    }

    public void Translate(Vector vector)
    {
        throw new NotImplementedException();
    }
}
