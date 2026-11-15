using Xui.Core.Canvas.Composition.Commands.Records;
using System.Runtime.InteropServices;
using Xui.Core.Canvas.Filter;

namespace Xui.Core.Canvas.Composition.Commands;

public sealed partial class CanvasCommandBuffer
{
    private void Append<T>(in T command, ReadOnlySpan<GradientStop> stops, ReadOnlySpan<char> text,
        ReadOnlySpan<nfloat> dashes, Font font, object? resource) where T : unmanaged
    {
        switch (CommandLayout.Op(in command))
        {
            case CanvasOp.Font:
            {
                var c = CommandLayout.Read<FontCommand, T>(in command);
                c.Family=RetainResource(font.FontFamily);
                c.Size=font.FontSize; c.Weight=font.FontWeight; c.Style=font.FontStyle;
                c.Stretch=font.FontStretch; c.LineHeight=font.LineHeight;
                Write(in c); break;
            }
            case CanvasOp.FillLinear:
            {
                var c = CommandLayout.Read<FillLinearCommand, T>(in command);
                c.Length=stops.Length;
                Write(in c, MemoryMarshal.AsBytes(stops)); break;
            }
            case CanvasOp.FillRadial:
            {
                var c = CommandLayout.Read<FillRadialCommand, T>(in command);
                c.Length=stops.Length;
                Write(in c, MemoryMarshal.AsBytes(stops)); break;
            }
            case CanvasOp.FillPattern:
            {
                var c = CommandLayout.Read<FillPatternCommand, T>(in command);
                ArgumentNullException.ThrowIfNull(resource);
                c.Image=RetainResource(resource);
                Write(in c); break;
            }
            case CanvasOp.StrokeLinear:
            {
                var c = CommandLayout.Read<StrokeLinearCommand, T>(in command);
                c.Length=stops.Length;
                Write(in c, MemoryMarshal.AsBytes(stops)); break;
            }
            case CanvasOp.StrokeRadial:
            {
                var c = CommandLayout.Read<StrokeRadialCommand, T>(in command);
                c.Length=stops.Length;
                Write(in c, MemoryMarshal.AsBytes(stops)); break;
            }
            case CanvasOp.StrokePattern:
            {
                var c = CommandLayout.Read<StrokePatternCommand, T>(in command);
                ArgumentNullException.ThrowIfNull(resource);
                c.Image=RetainResource(resource);
                Write(in c); break;
            }
            case CanvasOp.TextResource:
            {
                var c = CommandLayout.Read<TextResourceCommand, T>(in command);
                c.Text=RetainResource((string)resource!);
                Write(in c); break;
            }
            case CanvasOp.Text:
            {
                var c = CommandLayout.Read<TextCommand, T>(in command);
                c.Length=text.Length;
                Write(in c, MemoryMarshal.AsBytes(text)); break;
            }
            case CanvasOp.Dashes:
            {
                var c = CommandLayout.Read<DashesCommand, T>(in command);
                c.Length=dashes.Length;
                Write(in c, MemoryMarshal.AsBytes(dashes)); break;
            }
            case CanvasOp.Image:
            {
                var c = CommandLayout.Read<ImageCommand, T>(in command);
                ArgumentNullException.ThrowIfNull(resource);
                c.Image=RetainResource(resource);
                Write(in c); break;
            }
            case CanvasOp.ImageCrop:
            {
                var c = CommandLayout.Read<ImageCropCommand, T>(in command);
                ArgumentNullException.ThrowIfNull(resource);
                c.Image=RetainResource(resource);
                Write(in c); break;
            }
            default: Write(in command); break;
        }
    }

    private static void CountResources(ReadOnlySpan<byte> record, ref int resourceCount)
    {
        switch (MemoryMarshal.Read<Command>(record).Op)
        {
            case CanvasOp.TextResource: { var c=MemoryMarshal.Read<TextResourceCommand>(record); resourceCount=c.Text.Id+1; break; }
            case CanvasOp.Font: { var c=MemoryMarshal.Read<FontCommand>(record); resourceCount=c.Family.Id+1; break; }
            case CanvasOp.FillPattern: { var c=MemoryMarshal.Read<FillPatternCommand>(record); resourceCount=c.Image.Id+1; break; }
            case CanvasOp.StrokePattern: { var c=MemoryMarshal.Read<StrokePatternCommand>(record); resourceCount=c.Image.Id+1; break; }
            case CanvasOp.Image: { var c=MemoryMarshal.Read<ImageCommand>(record); resourceCount=c.Image.Id+1; break; }
            case CanvasOp.ImageCrop: { var c=MemoryMarshal.Read<ImageCropCommand>(record); resourceCount=c.Image.Id+1; break; }
        }
    }

    private static bool SameRecord(CanvasOp op, ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        switch (op)
        {
            case CanvasOp.Save: return true;
            case CanvasOp.Restore: return true;
            case CanvasOp.BeginPath: return true;
            case CanvasOp.ClosePath: return true;
            case CanvasOp.Stroke: return true;
            case CanvasOp.Clip: return true;
            case CanvasOp.GlobalAlpha:
            {
                var a=MemoryMarshal.Read<GlobalAlphaCommand>(left);
                var b=MemoryMarshal.Read<GlobalAlphaCommand>(right);
                return a.Value.Equals(b.Value);
            }
            case CanvasOp.LineWidth:
            {
                var a=MemoryMarshal.Read<LineWidthCommand>(left);
                var b=MemoryMarshal.Read<LineWidthCommand>(right);
                return a.Value.Equals(b.Value);
            }
            case CanvasOp.MiterLimit:
            {
                var a=MemoryMarshal.Read<MiterLimitCommand>(left);
                var b=MemoryMarshal.Read<MiterLimitCommand>(right);
                return a.Value.Equals(b.Value);
            }
            case CanvasOp.LineDashOffset:
            {
                var a=MemoryMarshal.Read<LineDashOffsetCommand>(left);
                var b=MemoryMarshal.Read<LineDashOffsetCommand>(right);
                return a.Value.Equals(b.Value);
            }
            case CanvasOp.LineCap:
            {
                var a=MemoryMarshal.Read<LineCapCommand>(left);
                var b=MemoryMarshal.Read<LineCapCommand>(right);
                return a.Value == b.Value;
            }
            case CanvasOp.LineJoin:
            {
                var a=MemoryMarshal.Read<LineJoinCommand>(left);
                var b=MemoryMarshal.Read<LineJoinCommand>(right);
                return a.Value == b.Value;
            }
            case CanvasOp.TextAlign:
            {
                var a=MemoryMarshal.Read<TextAlignCommand>(left);
                var b=MemoryMarshal.Read<TextAlignCommand>(right);
                return a.Value == b.Value;
            }
            case CanvasOp.TextBaseline:
            {
                var a=MemoryMarshal.Read<TextBaselineCommand>(left);
                var b=MemoryMarshal.Read<TextBaselineCommand>(right);
                return a.Value == b.Value;
            }
            case CanvasOp.Translate: return true;
            case CanvasOp.Scale: return true;
            case CanvasOp.Rotate: return true;
            case CanvasOp.Transform: return true;
            case CanvasOp.SetTransform: return true;
            case CanvasOp.FillRect:
            {
                var a=MemoryMarshal.Read<FillRectCommand>(left);
                var b=MemoryMarshal.Read<FillRectCommand>(right);
                return a.Bounds.X.Equals(b.Bounds.X) && a.Bounds.Y.Equals(b.Bounds.Y) && a.Bounds.Width.Equals(b.Bounds.Width) && a.Bounds.Height.Equals(b.Bounds.Height);
            }
            case CanvasOp.StrokeRect:
            {
                var a=MemoryMarshal.Read<StrokeRectCommand>(left);
                var b=MemoryMarshal.Read<StrokeRectCommand>(right);
                return a.Bounds.X.Equals(b.Bounds.X) && a.Bounds.Y.Equals(b.Bounds.Y) && a.Bounds.Width.Equals(b.Bounds.Width) && a.Bounds.Height.Equals(b.Bounds.Height);
            }
            case CanvasOp.Rect:
            {
                var a=MemoryMarshal.Read<RectCommand>(left);
                var b=MemoryMarshal.Read<RectCommand>(right);
                return a.Bounds.X.Equals(b.Bounds.X) && a.Bounds.Y.Equals(b.Bounds.Y) && a.Bounds.Width.Equals(b.Bounds.Width) && a.Bounds.Height.Equals(b.Bounds.Height);
            }
            case CanvasOp.MoveTo:
            {
                var a=MemoryMarshal.Read<MoveToCommand>(left);
                var b=MemoryMarshal.Read<MoveToCommand>(right);
                return a.Position.X.Equals(b.Position.X) && a.Position.Y.Equals(b.Position.Y);
            }
            case CanvasOp.LineTo:
            {
                var a=MemoryMarshal.Read<LineToCommand>(left);
                var b=MemoryMarshal.Read<LineToCommand>(right);
                return a.Position.X.Equals(b.Position.X) && a.Position.Y.Equals(b.Position.Y);
            }
            case CanvasOp.Quadratic:
            {
                var a=MemoryMarshal.Read<QuadraticCommand>(left);
                var b=MemoryMarshal.Read<QuadraticCommand>(right);
                return a.Control.X.Equals(b.Control.X) && a.Control.Y.Equals(b.Control.Y) && a.End.X.Equals(b.End.X) && a.End.Y.Equals(b.End.Y);
            }
            case CanvasOp.Cubic:
            {
                var a=MemoryMarshal.Read<CubicCommand>(left);
                var b=MemoryMarshal.Read<CubicCommand>(right);
                return a.Control1.X.Equals(b.Control1.X) && a.Control1.Y.Equals(b.Control1.Y) && a.Control2.X.Equals(b.Control2.X) && a.Control2.Y.Equals(b.Control2.Y) && a.End.X.Equals(b.End.X) && a.End.Y.Equals(b.End.Y);
            }
            case CanvasOp.ArcTo:
            {
                var a=MemoryMarshal.Read<ArcToCommand>(left);
                var b=MemoryMarshal.Read<ArcToCommand>(right);
                return a.Control.X.Equals(b.Control.X) && a.Control.Y.Equals(b.Control.Y) && a.End.X.Equals(b.End.X) && a.End.Y.Equals(b.End.Y) && a.Radius.Equals(b.Radius);
            }
            case CanvasOp.Arc:
            {
                var a=MemoryMarshal.Read<ArcCommand>(left);
                var b=MemoryMarshal.Read<ArcCommand>(right);
                return a.Center.X.Equals(b.Center.X) && a.Center.Y.Equals(b.Center.Y) && a.Radius.Equals(b.Radius) && a.StartAngle.Equals(b.StartAngle) && a.EndAngle.Equals(b.EndAngle) && a.Winding == b.Winding;
            }
            case CanvasOp.Ellipse:
            {
                var a=MemoryMarshal.Read<EllipseCommand>(left);
                var b=MemoryMarshal.Read<EllipseCommand>(right);
                return a.Center.X.Equals(b.Center.X) && a.Center.Y.Equals(b.Center.Y) && a.RadiusX.Equals(b.RadiusX) && a.RadiusY.Equals(b.RadiusY) && a.Rotation.Equals(b.Rotation) && a.StartAngle.Equals(b.StartAngle) && a.EndAngle.Equals(b.EndAngle) && a.Winding == b.Winding;
            }
            case CanvasOp.RoundRect:
            {
                var a=MemoryMarshal.Read<RoundRectCommand>(left);
                var b=MemoryMarshal.Read<RoundRectCommand>(right);
                return a.Bounds.X.Equals(b.Bounds.X) && a.Bounds.Y.Equals(b.Bounds.Y) && a.Bounds.Width.Equals(b.Bounds.Width) && a.Bounds.Height.Equals(b.Bounds.Height) && a.Corners.TopLeft.Equals(b.Corners.TopLeft) && a.Corners.TopRight.Equals(b.Corners.TopRight) && a.Corners.BottomLeft.Equals(b.Corners.BottomLeft) && a.Corners.BottomRight.Equals(b.Corners.BottomRight);
            }
            case CanvasOp.Fill:
            {
                var a=MemoryMarshal.Read<FillCommand>(left);
                var b=MemoryMarshal.Read<FillCommand>(right);
                return a.Rule == b.Rule;
            }
            case CanvasOp.FilterNone: return true;
            case CanvasOp.Brightness:
            {
                var a=MemoryMarshal.Read<BrightnessCommand>(left);
                var b=MemoryMarshal.Read<BrightnessCommand>(right);
                return a.Amount.Equals(b.Amount);
            }
            case CanvasOp.Font:
            {
                var a=MemoryMarshal.Read<FontCommand>(left);
                var b=MemoryMarshal.Read<FontCommand>(right);
                return a.Family.Id == b.Family.Id && a.Size.Equals(b.Size) && a.Weight == b.Weight &&
                    a.Style.IsItalic == b.Style.IsItalic && a.Style.IsOblique == b.Style.IsOblique &&
                    a.Style.ObliqueAngle.Equals(b.Style.ObliqueAngle) && a.Stretch.Equals(b.Stretch) &&
                    a.LineHeight.Equals(b.LineHeight);
            }
            case CanvasOp.FillColor:
            {
                var a=MemoryMarshal.Read<FillColorCommand>(left);
                var b=MemoryMarshal.Read<FillColorCommand>(right);
                return a.Color.Red.Equals(b.Color.Red) && a.Color.Green.Equals(b.Color.Green) && a.Color.Blue.Equals(b.Color.Blue) && a.Color.Alpha.Equals(b.Color.Alpha);
            }
            case CanvasOp.FillLinear:
            {
                var a=MemoryMarshal.Read<FillLinearCommand>(left);
                var b=MemoryMarshal.Read<FillLinearCommand>(right);
                return a.StartPoint.X.Equals(b.StartPoint.X) && a.StartPoint.Y.Equals(b.StartPoint.Y) && a.EndPoint.X.Equals(b.EndPoint.X) && a.EndPoint.Y.Equals(b.EndPoint.Y) && a.Length == b.Length;
            }
            case CanvasOp.FillRadial:
            {
                var a=MemoryMarshal.Read<FillRadialCommand>(left);
                var b=MemoryMarshal.Read<FillRadialCommand>(right);
                return a.StartCenter.X.Equals(b.StartCenter.X) && a.StartCenter.Y.Equals(b.StartCenter.Y) && a.StartRadius.Equals(b.StartRadius) && a.EndCenter.X.Equals(b.EndCenter.X) && a.EndCenter.Y.Equals(b.EndCenter.Y) && a.EndRadius.Equals(b.EndRadius) && a.Length == b.Length;
            }
            case CanvasOp.FillPattern:
            {
                var a=MemoryMarshal.Read<FillPatternCommand>(left);
                var b=MemoryMarshal.Read<FillPatternCommand>(right);
                return a.Image.Id == b.Image.Id && a.Repeat == b.Repeat;
            }
            case CanvasOp.StrokeColor:
            {
                var a=MemoryMarshal.Read<StrokeColorCommand>(left);
                var b=MemoryMarshal.Read<StrokeColorCommand>(right);
                return a.Color.Red.Equals(b.Color.Red) && a.Color.Green.Equals(b.Color.Green) && a.Color.Blue.Equals(b.Color.Blue) && a.Color.Alpha.Equals(b.Color.Alpha);
            }
            case CanvasOp.StrokeLinear:
            {
                var a=MemoryMarshal.Read<StrokeLinearCommand>(left);
                var b=MemoryMarshal.Read<StrokeLinearCommand>(right);
                return a.StartPoint.X.Equals(b.StartPoint.X) && a.StartPoint.Y.Equals(b.StartPoint.Y) && a.EndPoint.X.Equals(b.EndPoint.X) && a.EndPoint.Y.Equals(b.EndPoint.Y) && a.Length == b.Length;
            }
            case CanvasOp.StrokeRadial:
            {
                var a=MemoryMarshal.Read<StrokeRadialCommand>(left);
                var b=MemoryMarshal.Read<StrokeRadialCommand>(right);
                return a.StartCenter.X.Equals(b.StartCenter.X) && a.StartCenter.Y.Equals(b.StartCenter.Y) && a.StartRadius.Equals(b.StartRadius) && a.EndCenter.X.Equals(b.EndCenter.X) && a.EndCenter.Y.Equals(b.EndCenter.Y) && a.EndRadius.Equals(b.EndRadius) && a.Length == b.Length;
            }
            case CanvasOp.StrokePattern:
            {
                var a=MemoryMarshal.Read<StrokePatternCommand>(left);
                var b=MemoryMarshal.Read<StrokePatternCommand>(right);
                return a.Image.Id == b.Image.Id && a.Repeat == b.Repeat;
            }
            case CanvasOp.TextResource:
            {
                var a=MemoryMarshal.Read<TextResourceCommand>(left);
                var b=MemoryMarshal.Read<TextResourceCommand>(right);
                return a.Text.Id == b.Text.Id && a.Position.X.Equals(b.Position.X) && a.Position.Y.Equals(b.Position.Y);
            }
            case CanvasOp.Text:
            {
                var a=MemoryMarshal.Read<TextCommand>(left);
                var b=MemoryMarshal.Read<TextCommand>(right);
                return a.Position.X.Equals(b.Position.X) && a.Position.Y.Equals(b.Position.Y) && a.Length == b.Length;
            }
            case CanvasOp.Dashes:
            {
                var a=MemoryMarshal.Read<DashesCommand>(left);
                var b=MemoryMarshal.Read<DashesCommand>(right);
                return a.Length == b.Length;
            }
            case CanvasOp.Image:
            {
                var a=MemoryMarshal.Read<ImageCommand>(left);
                var b=MemoryMarshal.Read<ImageCommand>(right);
                return a.Image.Id == b.Image.Id && a.Destination.X.Equals(b.Destination.X) && a.Destination.Y.Equals(b.Destination.Y) && a.Destination.Width.Equals(b.Destination.Width) && a.Destination.Height.Equals(b.Destination.Height) && a.Opacity.Equals(b.Opacity);
            }
            case CanvasOp.ImageCrop:
            {
                var a=MemoryMarshal.Read<ImageCropCommand>(left);
                var b=MemoryMarshal.Read<ImageCropCommand>(right);
                return a.Image.Id == b.Image.Id && a.Source.X.Equals(b.Source.X) && a.Source.Y.Equals(b.Source.Y) && a.Source.Width.Equals(b.Source.Width) && a.Source.Height.Equals(b.Source.Height) && a.Destination.X.Equals(b.Destination.X) && a.Destination.Y.Equals(b.Destination.Y) && a.Destination.Width.Equals(b.Destination.Width) && a.Destination.Height.Equals(b.Destination.Height) && a.Opacity.Equals(b.Opacity);
            }
            default: throw new InvalidOperationException("Unknown canvas command.");
        }
    }

    private static bool SamePayload(CanvasOp op, ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        var headerSize = CommandLayout.Size(op);
        var a = left[headerSize..]; var b = right[headerSize..];
        switch (op)
        {
            case CanvasOp.FillLinear:
            case CanvasOp.FillRadial:
            case CanvasOp.StrokeLinear:
            case CanvasOp.StrokeRadial:
                var stopsA = MemoryMarshal.Cast<byte, GradientStop>(a);
                var stopsB = MemoryMarshal.Cast<byte, GradientStop>(b);
                if (stopsA.Length != stopsB.Length) return false;
                for (var i = 0; i < stopsA.Length; i++)
                    if (stopsA[i].Offset != stopsB[i].Offset || !stopsA[i].Color.Equals(stopsB[i].Color)) return false;
                return true;
            case CanvasOp.Dashes:
                return MemoryMarshal.Cast<byte, nfloat>(a).SequenceEqual(MemoryMarshal.Cast<byte, nfloat>(b));
            default:
                return a.SequenceEqual(b);
        }
    }

    /// <summary>Replays the owned stream into a canvas, reading each typed record without alignment assumptions.</summary>
    public void Replay(IContext target)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        replayDepth++;
        try
        {
            var remaining = Data;
            while (!remaining.IsEmpty)
            {
                var op = MemoryMarshal.Read<Command>(remaining).Op;
                switch (op)
                {
                    case CanvasOp.Save:
                    {
                        target.Save(); break;
                    }
                    case CanvasOp.Restore:
                    {
                        target.Restore(); break;
                    }
                    case CanvasOp.BeginPath:
                    {
                        target.BeginPath(); break;
                    }
                    case CanvasOp.ClosePath:
                    {
                        target.ClosePath(); break;
                    }
                    case CanvasOp.Stroke:
                    {
                        target.Stroke(); break;
                    }
                    case CanvasOp.Clip:
                    {
                        target.Clip(); break;
                    }
                    case CanvasOp.GlobalAlpha:
                    {
                        var c=MemoryMarshal.Read<GlobalAlphaCommand>(remaining);
                        target.GlobalAlpha = c.Value; break;
                    }
                    case CanvasOp.LineWidth:
                    {
                        var c=MemoryMarshal.Read<LineWidthCommand>(remaining);
                        target.LineWidth = c.Value; break;
                    }
                    case CanvasOp.MiterLimit:
                    {
                        var c=MemoryMarshal.Read<MiterLimitCommand>(remaining);
                        target.MiterLimit = c.Value; break;
                    }
                    case CanvasOp.LineDashOffset:
                    {
                        var c=MemoryMarshal.Read<LineDashOffsetCommand>(remaining);
                        target.LineDashOffset = c.Value; break;
                    }
                    case CanvasOp.LineCap:
                    {
                        var c=MemoryMarshal.Read<LineCapCommand>(remaining);
                        target.LineCap = c.Value; break;
                    }
                    case CanvasOp.LineJoin:
                    {
                        var c=MemoryMarshal.Read<LineJoinCommand>(remaining);
                        target.LineJoin = c.Value; break;
                    }
                    case CanvasOp.TextAlign:
                    {
                        var c=MemoryMarshal.Read<TextAlignCommand>(remaining);
                        target.TextAlign = c.Value; break;
                    }
                    case CanvasOp.TextBaseline:
                    {
                        var c=MemoryMarshal.Read<TextBaselineCommand>(remaining);
                        target.TextBaseline = c.Value; break;
                    }
                    case CanvasOp.Translate:
                    {
                        var c=MemoryMarshal.Read<TranslateCommand>(remaining);
                        target.Translate(c.Value); break;
                    }
                    case CanvasOp.Scale:
                    {
                        var c=MemoryMarshal.Read<ScaleCommand>(remaining);
                        target.Scale(c.Value); break;
                    }
                    case CanvasOp.Rotate:
                    {
                        var c=MemoryMarshal.Read<RotateCommand>(remaining);
                        target.Rotate(c.Angle); break;
                    }
                    case CanvasOp.Transform:
                    {
                        var c=MemoryMarshal.Read<TransformCommand>(remaining);
                        target.Transform(c.Value); break;
                    }
                    case CanvasOp.SetTransform:
                    {
                        var c=MemoryMarshal.Read<SetTransformCommand>(remaining);
                        target.SetTransform(c.Value); break;
                    }
                    case CanvasOp.FillRect:
                    {
                        var c=MemoryMarshal.Read<FillRectCommand>(remaining);
                        target.FillRect(c.Bounds); break;
                    }
                    case CanvasOp.StrokeRect:
                    {
                        var c=MemoryMarshal.Read<StrokeRectCommand>(remaining);
                        target.StrokeRect(c.Bounds); break;
                    }
                    case CanvasOp.Rect:
                    {
                        var c=MemoryMarshal.Read<RectCommand>(remaining);
                        target.Rect(c.Bounds); break;
                    }
                    case CanvasOp.MoveTo:
                    {
                        var c=MemoryMarshal.Read<MoveToCommand>(remaining);
                        target.MoveTo(c.Position); break;
                    }
                    case CanvasOp.LineTo:
                    {
                        var c=MemoryMarshal.Read<LineToCommand>(remaining);
                        target.LineTo(c.Position); break;
                    }
                    case CanvasOp.Quadratic:
                    {
                        var c=MemoryMarshal.Read<QuadraticCommand>(remaining);
                        target.CurveTo(c.Control, c.End); break;
                    }
                    case CanvasOp.Cubic:
                    {
                        var c=MemoryMarshal.Read<CubicCommand>(remaining);
                        target.CurveTo(c.Control1, c.Control2, c.End); break;
                    }
                    case CanvasOp.ArcTo:
                    {
                        var c=MemoryMarshal.Read<ArcToCommand>(remaining);
                        target.ArcTo(c.Control, c.End, c.Radius); break;
                    }
                    case CanvasOp.Arc:
                    {
                        var c=MemoryMarshal.Read<ArcCommand>(remaining);
                        target.Arc(c.Center, c.Radius, c.StartAngle, c.EndAngle, c.Winding); break;
                    }
                    case CanvasOp.Ellipse:
                    {
                        var c=MemoryMarshal.Read<EllipseCommand>(remaining);
                        target.Ellipse(c.Center, c.RadiusX, c.RadiusY, c.Rotation, c.StartAngle, c.EndAngle, c.Winding); break;
                    }
                    case CanvasOp.RoundRect:
                    {
                        var c=MemoryMarshal.Read<RoundRectCommand>(remaining);
                        target.RoundRect(c.Bounds, c.Corners); break;
                    }
                    case CanvasOp.Fill:
                    {
                        var c=MemoryMarshal.Read<FillCommand>(remaining);
                        target.Fill(c.Rule); break;
                    }
                    case CanvasOp.FilterNone:
                    {
                        target.SetFilter(); break;
                    }
                    case CanvasOp.Brightness:
                    {
                        var c=MemoryMarshal.Read<BrightnessCommand>(remaining);
                        target.SetFilter(new Brightness(c.Amount)); break;
                    }
                    case CanvasOp.Font:
                    {
                        var c=MemoryMarshal.Read<FontCommand>(remaining);
                        target.SetFont(new Font(c.Size, (string)GetResource(c.Family)!, c.Weight, c.Style, c.Stretch, c.LineHeight)); break;
                    }
                    case CanvasOp.FillColor:
                    {
                        var c=MemoryMarshal.Read<FillColorCommand>(remaining);
                        target.SetFill(c.Color); break;
                    }
                    case CanvasOp.FillLinear:
                    {
                        var c=MemoryMarshal.Read<FillLinearCommand>(remaining);
                        target.SetFill(new LinearGradient(c.StartPoint, c.EndPoint, CommandLayout.Payload<FillLinearCommand, GradientStop>(remaining, c.Length))); break;
                    }
                    case CanvasOp.FillRadial:
                    {
                        var c=MemoryMarshal.Read<FillRadialCommand>(remaining);
                        target.SetFill(new RadialGradient(c.StartCenter, c.StartRadius, c.EndCenter, c.EndRadius, CommandLayout.Payload<FillRadialCommand, GradientStop>(remaining, c.Length))); break;
                    }
                    case CanvasOp.FillPattern:
                    {
                        var c=MemoryMarshal.Read<FillPatternCommand>(remaining);
                        target.SetFill(new ImagePattern((IImage)GetResource(c.Image)!, c.Repeat)); break;
                    }
                    case CanvasOp.StrokeColor:
                    {
                        var c=MemoryMarshal.Read<StrokeColorCommand>(remaining);
                        target.SetStroke(c.Color); break;
                    }
                    case CanvasOp.StrokeLinear:
                    {
                        var c=MemoryMarshal.Read<StrokeLinearCommand>(remaining);
                        target.SetStroke(new LinearGradient(c.StartPoint, c.EndPoint, CommandLayout.Payload<StrokeLinearCommand, GradientStop>(remaining, c.Length))); break;
                    }
                    case CanvasOp.StrokeRadial:
                    {
                        var c=MemoryMarshal.Read<StrokeRadialCommand>(remaining);
                        target.SetStroke(new RadialGradient(c.StartCenter, c.StartRadius, c.EndCenter, c.EndRadius, CommandLayout.Payload<StrokeRadialCommand, GradientStop>(remaining, c.Length))); break;
                    }
                    case CanvasOp.StrokePattern:
                    {
                        var c=MemoryMarshal.Read<StrokePatternCommand>(remaining);
                        target.SetStroke(new ImagePattern((IImage)GetResource(c.Image)!, c.Repeat)); break;
                    }
                    case CanvasOp.TextResource:
                    {
                        var c=MemoryMarshal.Read<TextResourceCommand>(remaining);
                        target.FillText((string)GetResource(c.Text)!, c.Position); break;
                    }
                    case CanvasOp.Text:
                    {
                        var c=MemoryMarshal.Read<TextCommand>(remaining);
                        target.FillText(CommandLayout.Payload<TextCommand, char>(remaining, c.Length), c.Position); break;
                    }
                    case CanvasOp.Dashes:
                    {
                        var c=MemoryMarshal.Read<DashesCommand>(remaining);
                        target.SetLineDash(CommandLayout.Payload<DashesCommand, nfloat>(remaining, c.Length)); break;
                    }
                    case CanvasOp.Image:
                    {
                        var c=MemoryMarshal.Read<ImageCommand>(remaining);
                        target.DrawImage((IImage)GetResource(c.Image)!, c.Destination, c.Opacity); break;
                    }
                    case CanvasOp.ImageCrop:
                    {
                        var c=MemoryMarshal.Read<ImageCropCommand>(remaining);
                        target.DrawImage((IImage)GetResource(c.Image)!, c.Source, c.Destination, c.Opacity); break;
                    }
                    default: throw new InvalidOperationException("Unknown canvas command.");
                }
                remaining = remaining[CommandLayout.Size(remaining)..];
            }
        }
        finally { replayDepth--; }
    }
}
