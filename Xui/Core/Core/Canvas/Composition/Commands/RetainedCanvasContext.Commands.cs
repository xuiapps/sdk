using Xui.Core.Canvas.Composition.Commands.Records;
#pragma warning disable CS1591 // Backend recording implementation, not application-facing canvas API.
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands;

public sealed partial class RetainedCanvasContext
{
    public void Save() => Emit(new SaveCommand());
    public void Restore() => Emit(new RestoreCommand());
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
    public TextAlign TextAlign { set => Emit(new TextAlignCommand { Value=value }); }
    public TextBaseline TextBaseline { set => Emit(new TextBaselineCommand { Value=value }); }
    public void Translate(Vector value) => Emit(new TranslateCommand { Value=value });
    public void Scale(Vector value) => Emit(new ScaleCommand { Value=value });
    public void Rotate(nfloat angle) => Emit(new RotateCommand { Angle=angle });
    public void Transform(AffineTransform value) => Emit(new TransformCommand { Value=value });
    public void SetTransform(AffineTransform value) => Emit(new SetTransformCommand { Value=value });
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
    public void SetFont(Font font) => Emit(new FontCommand(), font: font);
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
    public TextMetrics MeasureText(string text) => measure.MeasureText(text);
    public TextMetrics MeasureText(ReadOnlySpan<char> text) => measure.MeasureText(text);
}
