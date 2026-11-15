namespace Xui.Core.Canvas.Composition.Commands;

/// <summary>Discriminator for a packed canvas command record.</summary>
public enum CanvasOp
{
    /// <summary>The Save operation.</summary>
    Save,
    /// <summary>The Restore operation.</summary>
    Restore,
    /// <summary>The BeginPath operation.</summary>
    BeginPath,
    /// <summary>The ClosePath operation.</summary>
    ClosePath,
    /// <summary>The Stroke operation.</summary>
    Stroke,
    /// <summary>The Clip operation.</summary>
    Clip,
    /// <summary>The GlobalAlpha operation.</summary>
    GlobalAlpha,
    /// <summary>The LineWidth operation.</summary>
    LineWidth,
    /// <summary>The MiterLimit operation.</summary>
    MiterLimit,
    /// <summary>The LineDashOffset operation.</summary>
    LineDashOffset,
    /// <summary>The LineCap operation.</summary>
    LineCap,
    /// <summary>The LineJoin operation.</summary>
    LineJoin,
    /// <summary>The TextAlign operation.</summary>
    TextAlign,
    /// <summary>The TextBaseline operation.</summary>
    TextBaseline,
    /// <summary>The Translate operation.</summary>
    Translate,
    /// <summary>The Scale operation.</summary>
    Scale,
    /// <summary>The Rotate operation.</summary>
    Rotate,
    /// <summary>The Transform operation.</summary>
    Transform,
    /// <summary>The SetTransform operation.</summary>
    SetTransform,
    /// <summary>The FillRect operation.</summary>
    FillRect,
    /// <summary>The StrokeRect operation.</summary>
    StrokeRect,
    /// <summary>The Rect operation.</summary>
    Rect,
    /// <summary>The MoveTo operation.</summary>
    MoveTo,
    /// <summary>The LineTo operation.</summary>
    LineTo,
    /// <summary>The Quadratic operation.</summary>
    Quadratic,
    /// <summary>The Cubic operation.</summary>
    Cubic,
    /// <summary>The ArcTo operation.</summary>
    ArcTo,
    /// <summary>The Arc operation.</summary>
    Arc,
    /// <summary>The Ellipse operation.</summary>
    Ellipse,
    /// <summary>The RoundRect operation.</summary>
    RoundRect,
    /// <summary>The Fill operation.</summary>
    Fill,
    /// <summary>The FilterNone operation.</summary>
    FilterNone,
    /// <summary>The Brightness operation.</summary>
    Brightness,
    /// <summary>The Font operation.</summary>
    Font,
    /// <summary>The FillColor operation.</summary>
    FillColor,
    /// <summary>The FillLinear operation.</summary>
    FillLinear,
    /// <summary>The FillRadial operation.</summary>
    FillRadial,
    /// <summary>The FillPattern operation.</summary>
    FillPattern,
    /// <summary>The StrokeColor operation.</summary>
    StrokeColor,
    /// <summary>The StrokeLinear operation.</summary>
    StrokeLinear,
    /// <summary>The StrokeRadial operation.</summary>
    StrokeRadial,
    /// <summary>The StrokePattern operation.</summary>
    StrokePattern,
    /// <summary>The Text operation.</summary>
    Text,
    /// <summary>The Dashes operation.</summary>
    Dashes,
    /// <summary>The Image operation.</summary>
    Image,
    /// <summary>The ImageCrop operation.</summary>
    ImageCrop,
    /// <summary>Text drawn from a retained string resource.</summary>
    TextResource,
}
