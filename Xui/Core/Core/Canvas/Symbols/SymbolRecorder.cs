using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Commands.Records;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Symbols;

// A definition owns a separate canvas so the scene's native target and its state are never
// redirected while recording. Only creation/update allocates; cached scopes are stack-only.
internal sealed class SymbolRecorder : IContext
{
    private CanvasCommandBuffer? commands = new();
    private readonly IContext measure;
    private readonly Rect bounds;
    private readonly Stack<(Font, TextAlign, TextBaseline)> states = new();
    private Font font = new();
    private TextAlign align;
    private TextBaseline baseline;
    internal SymbolRecorder(IContext measure, Rect bounds)
    {
        this.measure=measure; this.bounds=bounds;
        GlobalAlpha=1; SetFill(Colors.Black); SetStroke(Colors.Black);
        LineWidth=1; LineCap=LineCap.Butt; LineJoin=LineJoin.Miter; MiterLimit=10;
        LineDashOffset=0; SetLineDash([]); SetFont(font);
        TextAlign=TextAlign.Start; TextBaseline=TextBaseline.Alphabetic;
    }
    private void CheckOpen() => ObjectDisposedException.ThrowIf(commands is null, this);
    private void Emit<T>(in T command, ReadOnlySpan<GradientStop> gradients=default,
        ReadOnlySpan<char> text=default, ReadOnlySpan<nfloat> numbers=default,
        Font font=default, object? resource=null) where T : unmanaged
    { CheckOpen(); commands!.Add(in command, gradients, text, numbers, font, resource); }
    internal SymbolContent Complete()
    {
        CheckOpen();
        if (states.Count!=0) throw new InvalidOperationException("Balance Save/Restore before completing a symbol.");
        var content=new SymbolContent(commands!, bounds); commands=null; return content;
    }
    public void Dispose() { commands?.Dispose(); commands=null; }
    public SymbolScope Symbol(scoped in SymbolOptions options) => throw new NotSupportedException("Define leaf symbols; nested symbol definitions are not supported.");
    public CompositionGroup Group(scoped in GroupOptions options) => throw new NotSupportedException("A symbol is drawing content, not a native view hierarchy.");
    public void EndGroup() => throw new NotSupportedException("A symbol cannot contain composition groups.");
    public void DrawSymbol(SymbolContent content) => throw new NotSupportedException("Define leaf symbols; nested symbol definitions are not supported.");
    public void Save() { CheckOpen(); states.Push((font, align, baseline)); Emit(new SaveCommand()); }
    public void Restore() { CheckOpen(); if (states.Count == 0) throw new InvalidOperationException("Unbalanced symbol Restore."); (font, align, baseline) = states.Pop(); Emit(new RestoreCommand()); }
    public void BeginPath() => Emit(new BeginPathCommand());
    public void ClosePath() => Emit(new ClosePathCommand());
    public void Stroke() => Emit(new StrokeCommand());
    public void Clip() => Emit(new ClipCommand());
    public nfloat GlobalAlpha { set => Emit(new GlobalAlphaCommand { Value=value }); }
    public nfloat LineWidth { set => Emit(new LineWidthCommand { Value=value }); }
    public nfloat MiterLimit { set => Emit(new MiterLimitCommand { Value=value }); }
    public nfloat LineDashOffset { set => Emit(new LineDashOffsetCommand { Value=value }); }
    public LineCap LineCap { set => Emit(new LineCapCommand { Value=value }); }
    public LineJoin LineJoin { set => Emit(new LineJoinCommand { Value=value }); }
    public TextAlign TextAlign { set { align=value; Emit(new TextAlignCommand { Value=value }); } }
    public TextBaseline TextBaseline { set { baseline=value; Emit(new TextBaselineCommand { Value=value }); } }
    public void Translate(Vector value) => Emit(new TranslateCommand { Value=value });
    public void Scale(Vector value) => Emit(new ScaleCommand { Value=value });
    public void Rotate(nfloat angle) => Emit(new RotateCommand { Angle=angle });
    public void Transform(AffineTransform value) => Emit(new TransformCommand { Value=value });
    public void SetTransform(AffineTransform value) => throw new NotSupportedException("Use relative transforms inside a symbol.");
    public void FillRect(Rect bounds) => Emit(new FillRectCommand { Bounds=bounds });
    public void StrokeRect(Rect bounds) => Emit(new StrokeRectCommand { Bounds=bounds });
    public void Rect(Rect bounds) => Emit(new RectCommand { Bounds=bounds });
    public void MoveTo(Point position) => Emit(new MoveToCommand { Position=position });
    public void LineTo(Point position) => Emit(new LineToCommand { Position=position });
    public void CurveTo(Point control, Point end) => Emit(new QuadraticCommand { Control=control, End=end });
    public void CurveTo(Point control1, Point control2, Point end) => Emit(new CubicCommand { Control1=control1, Control2=control2, End=end });
    public void ArcTo(Point control, Point end, nfloat radius) => Emit(new ArcToCommand { Control=control, End=end, Radius=radius });
    public void Arc(Point center, nfloat radius, nfloat start, nfloat end, Winding winding = Winding.ClockWise) => Emit(new ArcCommand { Center=center, Radius=radius, StartAngle=start, EndAngle=end, Winding=winding });
    public void Ellipse(Point center, nfloat rx, nfloat ry, nfloat rotation, nfloat start, nfloat end, Winding winding = Winding.ClockWise) => Emit(new EllipseCommand { Center=center, RadiusX=rx, RadiusY=ry, Rotation=rotation, StartAngle=start, EndAngle=end, Winding=winding });
    public void RoundRect(Rect bounds, CornerRadius corners) => Emit(new RoundRectCommand { Bounds=bounds, Corners=corners });
    public void Fill(FillRule rule = FillRule.NonZero) => Emit(new FillCommand { Rule=rule });
    public void SetFilter() => Emit(new FilterNoneCommand());
    public void SetFilter(Brightness brightness) => Emit(new BrightnessCommand { Amount=brightness.Amount });
    public void SetFont(Font font) { this.font=font; Emit(new FontCommand(), font: font); }
    public void SetFill(Color color) => Emit(new FillColorCommand { Color=color });
    public void SetFill(LinearGradient gradient) => Emit(new FillLinearCommand { StartPoint=gradient.StartPoint, EndPoint=gradient.EndPoint }, gradients: gradient.GradientStops);
    public void SetFill(RadialGradient gradient) => Emit(new FillRadialCommand { StartCenter=gradient.StartCenter, StartRadius=gradient.StartRadius, EndCenter=gradient.EndCenter, EndRadius=gradient.EndRadius }, gradients: gradient.GradientStops);
    public void SetFill(ImagePattern pattern) => Emit(new FillPatternCommand { Repeat=pattern.Repetition }, resource: pattern.Image);
    public void SetStroke(Color color) => Emit(new StrokeColorCommand { Color=color });
    public void SetStroke(LinearGradient gradient) => Emit(new StrokeLinearCommand { StartPoint=gradient.StartPoint, EndPoint=gradient.EndPoint }, gradients: gradient.GradientStops);
    public void SetStroke(RadialGradient gradient) => Emit(new StrokeRadialCommand { StartCenter=gradient.StartCenter, StartRadius=gradient.StartRadius, EndCenter=gradient.EndCenter, EndRadius=gradient.EndRadius }, gradients: gradient.GradientStops);
    public void SetStroke(ImagePattern pattern) => Emit(new StrokePatternCommand { Repeat=pattern.Repetition }, resource: pattern.Image);
    public void FillText(ReadOnlySpan<char> text, Point position) => Emit(new TextCommand { Position=position }, text: text);
    public void SetLineDash(ReadOnlySpan<nfloat> segments) => Emit(new DashesCommand(), numbers: segments);
    public void DrawImage(IImage image, Rect destination, nfloat opacity) => Emit(new ImageCommand { Destination=destination, Opacity=opacity }, resource: image);
    public void DrawImage(IImage image, Rect source, Rect destination, nfloat opacity) => Emit(new ImageCropCommand { Source=source, Destination=destination, Opacity=opacity }, resource: image);
    public void RoundRect(Rect rect, nfloat radius) => RoundRect(rect, new CornerRadius(radius));
    public void FillText(string text, Point pos) => Emit(new TextResourceCommand { Position=pos }, resource: text ?? string.Empty);
    public void DrawImage(IImage image, Rect dest) => DrawImage(image, dest, 1);
    public TextMetrics MeasureText(string text) => MeasureText(text.AsSpan());
    public TextMetrics MeasureText(ReadOnlySpan<char> text)
    {
        CheckOpen();
        measure.Save();
        try { measure.SetFont(font); measure.TextAlign=align; measure.TextBaseline=baseline; return measure.MeasureText(text); }
        finally { measure.Restore(); }
    }
}
