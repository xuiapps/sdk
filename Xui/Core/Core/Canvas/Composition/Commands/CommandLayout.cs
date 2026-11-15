using Xui.Core.Canvas.Composition.Commands.Records;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands;

internal static class CommandLayout
{
    internal static TRecord Read<TRecord, T>(in T command) where TRecord : unmanaged where T : unmanaged =>
        MemoryMarshal.Read<TRecord>(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in command, 1)));

    internal static CanvasOp Op<T>(in T command) where T : unmanaged => Read<Command, T>(in command).Op;

    internal static bool Draws(CanvasOp op) => op is CanvasOp.Stroke or CanvasOp.FillRect or
        CanvasOp.StrokeRect or CanvasOp.Fill or CanvasOp.Text or CanvasOp.TextResource or CanvasOp.Image or CanvasOp.ImageCrop;

    // The fixed header is followed by Length unmanaged elements for variable-size records.
    internal static int Size(ReadOnlySpan<byte> record)
    {
        var op = MemoryMarshal.Read<Command>(record).Op;
        var payloadBytes = op switch
        {
            CanvasOp.FillLinear => checked(MemoryMarshal.Read<FillLinearCommand>(record).Length * Unsafe.SizeOf<GradientStop>()),
            CanvasOp.FillRadial => checked(MemoryMarshal.Read<FillRadialCommand>(record).Length * Unsafe.SizeOf<GradientStop>()),
            CanvasOp.StrokeLinear => checked(MemoryMarshal.Read<StrokeLinearCommand>(record).Length * Unsafe.SizeOf<GradientStop>()),
            CanvasOp.StrokeRadial => checked(MemoryMarshal.Read<StrokeRadialCommand>(record).Length * Unsafe.SizeOf<GradientStop>()),
            CanvasOp.Text => checked(MemoryMarshal.Read<TextCommand>(record).Length * Unsafe.SizeOf<char>()),
            CanvasOp.Dashes => checked(MemoryMarshal.Read<DashesCommand>(record).Length * Unsafe.SizeOf<nfloat>()),
            _ => 0,
        };
        return checked(Size(op) + payloadBytes);
    }

    internal static ReadOnlySpan<TElement> Payload<THeader, TElement>(ReadOnlySpan<byte> record, int length)
        where THeader : unmanaged where TElement : unmanaged =>
        MemoryMarshal.Cast<byte, TElement>(record.Slice(Unsafe.SizeOf<THeader>(), checked(length * Unsafe.SizeOf<TElement>())));

    internal static int Size(CanvasOp op) => op switch
    {
        CanvasOp.Save => Unsafe.SizeOf<SaveCommand>(),
        CanvasOp.Restore => Unsafe.SizeOf<RestoreCommand>(),
        CanvasOp.BeginPath => Unsafe.SizeOf<BeginPathCommand>(),
        CanvasOp.ClosePath => Unsafe.SizeOf<ClosePathCommand>(),
        CanvasOp.Stroke => Unsafe.SizeOf<StrokeCommand>(),
        CanvasOp.Clip => Unsafe.SizeOf<ClipCommand>(),
        CanvasOp.GlobalAlpha => Unsafe.SizeOf<GlobalAlphaCommand>(),
        CanvasOp.LineWidth => Unsafe.SizeOf<LineWidthCommand>(),
        CanvasOp.MiterLimit => Unsafe.SizeOf<MiterLimitCommand>(),
        CanvasOp.LineDashOffset => Unsafe.SizeOf<LineDashOffsetCommand>(),
        CanvasOp.LineCap => Unsafe.SizeOf<LineCapCommand>(),
        CanvasOp.LineJoin => Unsafe.SizeOf<LineJoinCommand>(),
        CanvasOp.TextAlign => Unsafe.SizeOf<TextAlignCommand>(),
        CanvasOp.TextBaseline => Unsafe.SizeOf<TextBaselineCommand>(),
        CanvasOp.Translate => Unsafe.SizeOf<TranslateCommand>(),
        CanvasOp.Scale => Unsafe.SizeOf<ScaleCommand>(),
        CanvasOp.Rotate => Unsafe.SizeOf<RotateCommand>(),
        CanvasOp.Transform => Unsafe.SizeOf<TransformCommand>(),
        CanvasOp.SetTransform => Unsafe.SizeOf<SetTransformCommand>(),
        CanvasOp.FillRect => Unsafe.SizeOf<FillRectCommand>(),
        CanvasOp.StrokeRect => Unsafe.SizeOf<StrokeRectCommand>(),
        CanvasOp.Rect => Unsafe.SizeOf<RectCommand>(),
        CanvasOp.MoveTo => Unsafe.SizeOf<MoveToCommand>(),
        CanvasOp.LineTo => Unsafe.SizeOf<LineToCommand>(),
        CanvasOp.Quadratic => Unsafe.SizeOf<QuadraticCommand>(),
        CanvasOp.Cubic => Unsafe.SizeOf<CubicCommand>(),
        CanvasOp.ArcTo => Unsafe.SizeOf<ArcToCommand>(),
        CanvasOp.Arc => Unsafe.SizeOf<ArcCommand>(),
        CanvasOp.Ellipse => Unsafe.SizeOf<EllipseCommand>(),
        CanvasOp.RoundRect => Unsafe.SizeOf<RoundRectCommand>(),
        CanvasOp.Fill => Unsafe.SizeOf<FillCommand>(),
        CanvasOp.FilterNone => Unsafe.SizeOf<FilterNoneCommand>(),
        CanvasOp.Brightness => Unsafe.SizeOf<BrightnessCommand>(),
        CanvasOp.Font => Unsafe.SizeOf<FontCommand>(),
        CanvasOp.FillColor => Unsafe.SizeOf<FillColorCommand>(),
        CanvasOp.FillLinear => Unsafe.SizeOf<FillLinearCommand>(),
        CanvasOp.FillRadial => Unsafe.SizeOf<FillRadialCommand>(),
        CanvasOp.FillPattern => Unsafe.SizeOf<FillPatternCommand>(),
        CanvasOp.StrokeColor => Unsafe.SizeOf<StrokeColorCommand>(),
        CanvasOp.StrokeLinear => Unsafe.SizeOf<StrokeLinearCommand>(),
        CanvasOp.StrokeRadial => Unsafe.SizeOf<StrokeRadialCommand>(),
        CanvasOp.StrokePattern => Unsafe.SizeOf<StrokePatternCommand>(),
        CanvasOp.TextResource => Unsafe.SizeOf<TextResourceCommand>(),
        CanvasOp.Text => Unsafe.SizeOf<TextCommand>(),
        CanvasOp.Dashes => Unsafe.SizeOf<DashesCommand>(),
        CanvasOp.Image => Unsafe.SizeOf<ImageCommand>(),
        CanvasOp.ImageCrop => Unsafe.SizeOf<ImageCropCommand>(),
        _ => throw new InvalidOperationException("Unknown canvas command."),
    };
}
