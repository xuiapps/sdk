using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;

namespace Xui.Runtime.Browser.Actual;

/// <summary>Records Canvas operations for one synchronous bulk replay in JavaScript.</summary>
public partial class BrowserDrawingContext : IContext
{
    // Wire order is shared with canvas-player.js. Coordinates become doubles only at this boundary.
    private enum Op
    {
        Save, Restore, BeginPath, ClosePath, MoveTo, LineTo, Quadratic, Bezier, Arc, ArcTo, Ellipse,
        Rect, RoundRect, Clip, Fill, Stroke, FillRect, StrokeRect, Transform, SetTransform,
        FillColor, StrokeColor, GlobalAlpha, LineWidth, LineCap, LineJoin, MiterLimit, DashOffset,
        LineDash, Font, TextAlign, TextBaseline, FillText, LinearGradient, RadialGradient,
        DrawImage, Pattern, GpuSurface
    }

    private double[] commands = new double[4096];
    private int count;
    private readonly List<string> strings = new();
    private readonly Dictionary<string, int> stringIds = new(StringComparer.Ordinal);
    private readonly Dictionary<Font, string> fontNames = new();
    private readonly Dictionary<(string Font, string Text), TextMetrics> measurements = new();
    private readonly double[] metrics = new double[12];
    private readonly Stack<(FilterState Filter, string Font, AffineTransform Transform)> states = new();
    private FilterState filter;
    private AffineTransform transform = AffineTransform.Identity;
    private string font = "15px Inter";
    private ImmediateCompositionContext? composition;
    private double pixelRatio = 1;

    [JSImport("submit", "xui-browser")]
    private static partial void Submit([JSMarshalAs<JSType.MemoryView>] Span<double> commands, string[] strings);
    [JSImport("beginFrame", "xui-browser")]
    private static partial void BeginFrame(double pixelRatio);
    [JSImport("measureText", "xui-browser")]
    private static partial void Measure(string font, string text, [JSMarshalAs<JSType.MemoryView>] Span<double> metrics);

    internal void Reset(double scale)
    {
        count = 0;
        strings.Clear();
        stringIds.Clear();
        states.Clear();
        filter = default;
        font = "15px Inter";
        pixelRatio = scale;
        transform = AffineTransform.Identity;
        gpuFrame++;
        BeginFrame(scale);
    }

    internal void Flush()
    {
        if (count == 0) return;
        Submit(commands.AsSpan(0, count), strings.ToArray());
        count = 0;
        strings.Clear();
        stringIds.Clear();
    }

    private void Write(Op op, params ReadOnlySpan<double> args)
    {
        Ensure(args.Length + 1);
        commands[count++] = (int)op;
        args.CopyTo(commands.AsSpan(count));
        count += args.Length;
    }
    private void Ensure(int length)
    {
        if (count + length > commands.Length)
            Array.Resize(ref commands, Math.Max(count + length, commands.Length * 2));
    }
    private int Text(string value)
    {
        if (stringIds.TryGetValue(value, out var id)) return id;
        id = strings.Count;
        strings.Add(value);
        stringIds.Add(value, id);
        return id;
    }
    private static uint Pack(Color color) =>
        (uint)(Math.Clamp((double)color.Red, 0, 1) * 255 + .5) << 24 |
        (uint)(Math.Clamp((double)color.Green, 0, 1) * 255 + .5) << 16 |
        (uint)(Math.Clamp((double)color.Blue, 0, 1) * 255 + .5) << 8 |
        (uint)(Math.Clamp((double)color.Alpha, 0, 1) * 255 + .5);

    public CompositionGroup Group(scoped in GroupOptions options) =>
        (composition ??= new ImmediateCompositionContext(this)).Group(in options);
    public void EndGroup() => (composition ?? throw new InvalidOperationException("No composition group to close.")).EndGroup();
    public void SetFilter() => filter = default;
    public void SetFilter(Brightness brightness) => filter = new(brightness);
    public NFloat GlobalAlpha { set => Write(Op.GlobalAlpha, value); }
    public NFloat LineWidth { set => Write(Op.LineWidth, value); }
    public NFloat MiterLimit { set => Write(Op.MiterLimit, value); }
    public NFloat LineDashOffset { set => Write(Op.DashOffset, value); }
    public LineCap LineCap { set => Write(Op.LineCap, value switch { LineCap.Round => 1, LineCap.Square => 2, _ => 0 }); }
    public LineJoin LineJoin { set => Write(Op.LineJoin, value switch { LineJoin.Round => 1, LineJoin.Bevel => 2, _ => 0 }); }
    public TextAlign TextAlign { set => Write(Op.TextAlign, value switch { TextAlign.End => 1, TextAlign.Left => 2, TextAlign.Right => 3, TextAlign.Center => 4, _ => 0 }); }
    public TextBaseline TextBaseline { set => Write(Op.TextBaseline, value switch { TextBaseline.Top => 1, TextBaseline.Hanging => 2, TextBaseline.Middle => 3, TextBaseline.Ideographic => 4, TextBaseline.Bottom => 5, _ => 0 }); }

    public void Save() { states.Push((filter, font, transform)); Write(Op.Save); }
    public void Restore()
    {
        if (!states.TryPop(out var state)) return;
        (filter, font, transform) = state;
        Write(Op.Restore);
    }
    public void BeginPath() => Write(Op.BeginPath);
    public void ClosePath() => Write(Op.ClosePath);
    public void MoveTo(Point p) => Write(Op.MoveTo, p.X, p.Y);
    public void LineTo(Point p) => Write(Op.LineTo, p.X, p.Y);
    public void CurveTo(Point cp, Point to) => Write(Op.Quadratic, cp.X, cp.Y, to.X, to.Y);
    public void CurveTo(Point cp1, Point cp2, Point to) => Write(Op.Bezier, cp1.X, cp1.Y, cp2.X, cp2.Y, to.X, to.Y);
    public void Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise) =>
        Write(Op.Arc, center.X, center.Y, radius, startAngle, endAngle, winding == Winding.CounterClockWise ? 1 : 0);
    public void ArcTo(Point cp1, Point cp2, NFloat radius) => Write(Op.ArcTo, cp1.X, cp1.Y, cp2.X, cp2.Y, radius);
    public void Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise) =>
        Write(Op.Ellipse, center.X, center.Y, radiusX, radiusY, rotation, startAngle, endAngle, winding == Winding.CounterClockWise ? 1 : 0);
    public void Rect(Rect r) => Write(Op.Rect, r.X, r.Y, r.Width, r.Height);
    public void RoundRect(Rect r, NFloat radius) => RoundRect(r, new CornerRadius(radius));
    public void RoundRect(Rect r, CornerRadius radius) =>
        Write(Op.RoundRect, r.X, r.Y, r.Width, r.Height, radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);
    public void Clip() => Write(Op.Clip);
    public void Fill(FillRule rule = FillRule.NonZero) => Write(Op.Fill, rule == FillRule.EvenOdd ? 1 : 0);
    public void Stroke() => Write(Op.Stroke);
    public void FillRect(Rect r) => Write(Op.FillRect, r.X, r.Y, r.Width, r.Height);
    public void StrokeRect(Rect r) => Write(Op.StrokeRect, r.X, r.Y, r.Width, r.Height);
    public void Translate(Vector v) => Transform(AffineTransform.Translate(v));
    public void Scale(Vector v) => Transform(AffineTransform.Scale(v));
    public void Rotate(NFloat angle) => Transform(AffineTransform.Rotate(angle));
    public void Transform(AffineTransform m)
    {
        transform *= m;
        Write(Op.Transform, m.A, m.B, m.C, m.D, m.Tx, m.Ty);
    }
    public void SetTransform(AffineTransform m)
    {
        transform = m;
        Write(Op.SetTransform, m.A * pixelRatio, m.B * pixelRatio, m.C * pixelRatio, m.D * pixelRatio, m.Tx * pixelRatio, m.Ty * pixelRatio);
    }
    public void SetFill(Color color) => Write(Op.FillColor, Pack(filter.Apply(color)));
    public void SetStroke(Color color) => Write(Op.StrokeColor, Pack(filter.Apply(color)));
    public void SetFill(LinearGradient g) => Linear(g, false);
    public void SetStroke(LinearGradient g) => Linear(g, true);
    private void Linear(LinearGradient g, bool stroke)
    {
        Write(Op.LinearGradient, stroke ? 1 : 0, g.StartPoint.X, g.StartPoint.Y, g.EndPoint.X, g.EndPoint.Y, g.GradientStops.Length);
        Stops(g.GradientStops);
    }
    public void SetFill(RadialGradient g) => Radial(g, false);
    public void SetStroke(RadialGradient g) => Radial(g, true);
    private void Radial(RadialGradient g, bool stroke)
    {
        Write(Op.RadialGradient, stroke ? 1 : 0, g.StartCenter.X, g.StartCenter.Y, g.StartRadius, g.EndCenter.X, g.EndCenter.Y, g.EndRadius, g.GradientStops.Length);
        Stops(g.GradientStops);
    }
    private void Stops(ReadOnlySpan<GradientStop> stops)
    {
        Ensure(stops.Length * 2);
        foreach (var stop in stops) { commands[count++] = stop.Offset; commands[count++] = Pack(filter.Apply(stop.Color)); }
    }
    public void SetLineDash(ReadOnlySpan<NFloat> segments)
    {
        Write(Op.LineDash, segments.Length);
        Ensure(segments.Length);
        foreach (var segment in segments) commands[count++] = segment;
    }
    public void SetFont(Font value)
    {
        if (!fontNames.TryGetValue(value, out var css))
        {
            var style = value.FontStyle.IsItalic ? "italic " : value.FontStyle.IsOblique ? FormattableString.Invariant($"oblique {value.FontStyle.ObliqueAngle}deg ") : "";
            var family = value.FontFamily.Replace("\\", "\\\\").Replace("\"", "\\\"");
            css = FormattableString.Invariant($"{style}{(double)value.FontWeight} {value.FontSize}px \"{family}\"");
            if (fontNames.Count >= 256) fontNames.Clear();
            fontNames.Add(value, css);
        }
        font = css;
        Write(Op.Font, Text(css));
    }
    public void FillText(string text, Point pos) => Write(Op.FillText, Text(text), pos.X, pos.Y);
    public TextMetrics MeasureText(string text)
    {
        var key = (font, text);
        if (measurements.TryGetValue(key, out var result)) return result;
        Measure(font, text, metrics);
        result = new TextMetrics(
            new LineMetrics((NFloat)metrics[0], (NFloat)metrics[1], (NFloat)metrics[2], (NFloat)metrics[3], (NFloat)metrics[4]),
            new FontMetrics((NFloat)metrics[5], (NFloat)metrics[6], (NFloat)metrics[7], (NFloat)metrics[8],
                -(NFloat)metrics[9], -(NFloat)metrics[10], -(NFloat)metrics[11]));
        if (measurements.Count >= 4096) measurements.Clear();
        measurements.Add(key, result);
        return result;
    }
    public void SetFill(ImagePattern pattern) => Pattern(pattern, false);
    public void SetStroke(ImagePattern pattern) => Pattern(pattern, true);
    private void Pattern(ImagePattern p, bool stroke) => Write(Op.Pattern, ImageId(p.Image), (int)p.Repetition, stroke ? 1 : 0);
    private static int ImageId(IImage image) => image is BrowserImage browser ? browser.Id : throw new ArgumentException("Image belongs to another runtime.", nameof(image));
    public void DrawImage(IImage image, Rect dest) => DrawImage(image, dest, 1);
    public void DrawImage(IImage image, Rect dest, NFloat opacity) => DrawImage(image, new Rect(Point.Zero, image.Size), dest, opacity);
    public void DrawImage(IImage image, Rect source, Rect dest, NFloat opacity) =>
        Write(Op.DrawImage, ImageId(image), source.X, source.Y, source.Width, source.Height, dest.X, dest.Y, dest.Width, dest.Height, opacity);
    public void Dispose() { DisposeGpuSurfaces(); strings.Clear(); stringIds.Clear(); fontNames.Clear(); measurements.Clear(); states.Clear(); }
}
