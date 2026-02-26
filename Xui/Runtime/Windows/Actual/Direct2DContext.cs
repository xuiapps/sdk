using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Curves2D;
using Xui.Core.Math2D;
using static Xui.Runtime.Windows.D2D1;
using static Xui.Runtime.Windows.DWrite;

namespace Xui.Runtime.Windows.Actual;

public partial class Direct2DContext : IContext
{
    private readonly RenderTarget RenderTarget;

    private readonly D2D1.Factory3 D2D1Factory;

    private readonly DWrite.Factory DWriteFactory;

    private PaintStruct stroke;

    private PaintStruct fill;

    private StrokeStyleStruct StrokeStyle;

    private PathStr Path;

    private readonly Path2D path2d = new();

    private readonly PathReplaySink pathReplaySink;

    private float lineWidth = 1f;
    private float globalAlpha = 1f;

    private Stack<DrawingStateBlock.Ptr> drawingStateBlocks;
    private Stack<AffineTransform> transforms;
    private int layersCount = 0;
    private Stack<State> states;

    private TextFormat.Ptr textFormat;

    private float currentFontSizeDip;
    private Core.Canvas.FontMetrics currentFontMetrics;
    private string? currentFontFamilyName;
    private DWrite.FontWeight currentFontWeight;
    private DWrite.FontStyle currentFontStyle;
    private DWrite.FontStretch currentFontStretch;

    private struct State
    {
        public int Blocks;
        public int Layers;
        public int Transforms;

        // Brushes (AddRef'd on save, released via Dispose on restore)
        public Brush.Ptr FillBrush;
        public PaintStyle FillPaintStyle;
        public Brush.Ptr StrokeBrush;
        public PaintStyle StrokePaintStyle;

        // Pen
        public float GlobalAlpha;
        public float LineWidth;
        public StrokeStyleStruct.StrokeSnapshot StrokeSnapshot;

        // Text
        public TextAlign TextAlign;
        public TextBaseline TextBaseline;
        public TextFormat.Ptr TextFormat;
        public Core.Canvas.FontMetrics FontMetrics;
    }

    public Direct2DContext(RenderTarget renderTarget, D2D1.Factory3 d2d1Factory, DWrite.Factory dWriteFactory)
    {
        this.RenderTarget = renderTarget;
        this.RenderTarget.AddRef();

        this.D2D1Factory = d2d1Factory;
        this.D2D1Factory.AddRef();

        this.DWriteFactory = dWriteFactory;
        this.DWriteFactory.AddRef();

        this.stroke = new PaintStruct(this.RenderTarget, Colors.Black);
        this.fill = new PaintStruct(this.RenderTarget, Colors.White);

        this.StrokeStyle = new StrokeStyleStruct(this.D2D1Factory);
        this.Path = new PathStr(this.D2D1Factory);
        this.pathReplaySink = new PathReplaySink(this);

        this.drawingStateBlocks = new Stack<DrawingStateBlock.Ptr>();
        this.transforms = new Stack<AffineTransform>();
        this.states = new Stack<State>();
    }

    NFloat IPenContext.GlobalAlpha { set => this.globalAlpha = (float)value; }
    LineCap IPenContext.LineCap { set => this.StrokeStyle.LineCap = value; }
    Xui.Core.Canvas.LineJoin IPenContext.LineJoin { set => this.StrokeStyle.LineJoin = value; }
    NFloat IPenContext.LineWidth { set => this.lineWidth = (float)value; }
    NFloat IPenContext.MiterLimit { set => this.StrokeStyle.MiterLimit = value; }

    NFloat IPenContext.LineDashOffset { set => this.StrokeStyle.LineDashOffset = value; }

    public TextAlign TextAlign { get; set; }

    public TextBaseline TextBaseline { get; set; }

    void IStateContext.Save()
    {
        unsafe
        {
            this.states.Push(new State()
            {
                Blocks = this.drawingStateBlocks.Count,
                Transforms = this.transforms.Count,
                Layers = this.layersCount,

                FillBrush = this.fill.Brush,
                FillPaintStyle = this.fill.PaintStyle,
                StrokeBrush = this.stroke.Brush,
                StrokePaintStyle = this.stroke.PaintStyle,

                GlobalAlpha = this.globalAlpha,
                LineWidth = this.lineWidth,
                StrokeSnapshot = this.StrokeStyle.SaveSnapshot(),

                TextAlign = this.TextAlign,
                TextBaseline = this.TextBaseline,
                TextFormat = this.textFormat,
                FontMetrics = this.currentFontMetrics,
            });

            // AddRef COM objects so they survive Set* calls between Save/Restore.
            if (!this.fill.Brush.IsNull) COM.Unknown.AddRef(this.fill.Brush);
            if (!this.stroke.Brush.IsNull) COM.Unknown.AddRef(this.stroke.Brush);
            if (!this.textFormat.IsNull) COM.Unknown.AddRef(this.textFormat);
        }

        var block = this.D2D1Factory.CreateDrawingStateBlockPtr();
        this.RenderTarget.SaveDrawingState(block);
        this.drawingStateBlocks.Push(block);

        Matrix3X2F transform;
        this.RenderTarget.GetTransform(out transform);
        this.transforms.Push(transform);
    }

    void IStateContext.Restore()
    {
        if (this.states.Count > 0)
        {
            var state = this.states.Pop();

            while(this.drawingStateBlocks.Count > state.Blocks)
            {
                var block = this.drawingStateBlocks.Pop();
                this.RenderTarget.RestoreDrawingState(block);
                block.Dispose();
            }

            while(this.layersCount > state.Layers)
            {
                this.layersCount--;
                this.RenderTarget.PopLayer();
            }

            while(this.transforms.Count > state.Transforms)
            {
                var transform = this.transforms.Pop();
                this.RenderTarget.SetTransform(transform);
            }

            // Restore brushes (saved copies already have AddRef'd references)
            this.fill.Brush.Dispose();
            this.fill.Brush = state.FillBrush;
            this.fill.PaintStyle = state.FillPaintStyle;

            this.stroke.Brush.Dispose();
            this.stroke.Brush = state.StrokeBrush;
            this.stroke.PaintStyle = state.StrokePaintStyle;

            // Restore pen
            this.globalAlpha = state.GlobalAlpha;
            this.lineWidth = state.LineWidth;
            this.StrokeStyle.RestoreSnapshot(state.StrokeSnapshot);

            // Restore text
            this.TextAlign = state.TextAlign;
            this.TextBaseline = state.TextBaseline;
            this.textFormat.Dispose();
            this.textFormat = state.TextFormat;
            this.currentFontMetrics = state.FontMetrics;
        }
    }

    void IPenContext.SetLineDash(ReadOnlySpan<NFloat> segments) =>
        this.StrokeStyle.SetLineDash(segments);

    void IPenContext.SetStroke(Color color) =>
        this.stroke.SetSolidColor(color);

    void IPenContext.SetStroke(LinearGradient linearGradient) =>
        this.stroke.SetLinearGradient(linearGradient);

    void IPenContext.SetStroke(RadialGradient radialGradient) =>
        this.stroke.SetRadialGradient(radialGradient);

    void IPenContext.SetFill(Color color) =>
        this.fill.SetSolidColor(color);

    void IPenContext.SetFill(LinearGradient linearGradient) =>
        this.fill.SetLinearGradient(linearGradient);

    void IPenContext.SetFill(RadialGradient radialGradient) =>
        this.fill.SetRadialGradient(radialGradient);

    void IPathBuilder.BeginPath() =>
        this.path2d.BeginPath();

    void IGlyphPathBuilder.MoveTo(Point to) => this.path2d.MoveTo(to);

    void IGlyphPathBuilder.LineTo(Point to) => this.path2d.LineTo(to);

    void IGlyphPathBuilder.ClosePath() => this.path2d.ClosePath();

    void IGlyphPathBuilder.CurveTo(Point cp1, Point to) => this.path2d.CurveTo(cp1, to);

    void IPathBuilder.CurveTo(Point cp1, Point cp2, Point to) => this.path2d.CurveTo(cp1, cp2, to);

    void IPathBuilder.Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding) =>
        this.path2d.Arc(center, radius, startAngle, endAngle, winding);

    void IPathBuilder.ArcTo(Point cp1, Point cp2, NFloat radius) =>
        this.path2d.ArcTo(cp1, cp2, radius);

    void IPathBuilder.Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding) =>
        this.path2d.Ellipse(center, radiusX, radiusY, rotation, startAngle, endAngle, winding);

    void IPathBuilder.Rect(Rect rect) =>
        this.path2d.Rect(rect);

    void IPathBuilder.RoundRect(Rect rect, NFloat radius) =>
        this.path2d.RoundRect(rect, radius);

    void IPathBuilder.RoundRect(Rect rect, CornerRadius radius) =>
        this.path2d.RoundRect(rect, radius);

    private unsafe void ApplyBrushAlpha(Brush.Ptr brush)
    {
        if (!brush.IsNull && this.globalAlpha < 1f)
            D2D1.Brush.SetOpacity(brush, this.globalAlpha);
    }

    private unsafe void ResetBrushAlpha(Brush.Ptr brush)
    {
        if (!brush.IsNull && this.globalAlpha < 1f)
            D2D1.Brush.SetOpacity(brush, 1f);
    }

    void IPathDrawing.Fill(FillRule rule)
    {
        this.path2d.Visit(this.pathReplaySink);
        this.Path.SetFillMode(rule == FillRule.EvenOdd ? FillMode.Alternate : FillMode.Winding);
        var path = this.Path.PrepareToUse();
        if (!path.IsNull)
        {
            this.ApplyBrushAlpha(this.fill.Brush);
            this.RenderTarget.FillGeometry(path, this.fill.Brush);
            this.ResetBrushAlpha(this.fill.Brush);
        }
        this.Path.ClearAfterUse();
    }

    void IPathDrawing.Stroke()
    {
        this.path2d.Visit(this.pathReplaySink);
        var path = this.Path.PrepareToUse();
        if (!path.IsNull)
        {
            this.ApplyBrushAlpha(this.stroke.Brush);
            this.RenderTarget.DrawGeometry(path, this.stroke.Brush, this.lineWidth, this.StrokeStyle.GetStrokeStyle(this.lineWidth));
            this.ResetBrushAlpha(this.stroke.Brush);
        }
        this.Path.ClearAfterUse();
    }

    void IPathClipping.Clip()
    {
        unsafe
        {
            this.path2d.Visit(this.pathReplaySink);
            var geometry = this.Path.PrepareToUse();
            if (!geometry.IsNull)
            {
                LayerParameters layerParameters = new LayerParameters();
                layerParameters.GeometricMask = geometry;
                this.RenderTarget.PushLayer(layerParameters);
                this.layersCount++;
            }
            this.Path.ClearAfterUse();
        }
    }

    void IRectDrawingContext.StrokeRect(Rect rect)
    {
        this.ApplyBrushAlpha(this.stroke.Brush);
        this.RenderTarget.DrawRectangle(rect, this.stroke.Brush, this.lineWidth, this.StrokeStyle.GetStrokeStyle(this.lineWidth));
        this.ResetBrushAlpha(this.stroke.Brush);
    }

    void IRectDrawingContext.FillRect(Rect rect)
    {
        this.ApplyBrushAlpha(this.fill.Brush);
        this.RenderTarget.FillRectangle(rect, this.fill.Brush);
        this.ResetBrushAlpha(this.fill.Brush);
    }

    void ITextDrawingContext.FillText(string text, Point pos)
    {
        if (this.textFormat.IsNull)
        {
            return;
        }

        using var layout = this.DWriteFactory.CreateTextLayoutRef(
            text,
            this.textFormat,
            float.PositiveInfinity,
            float.PositiveInfinity);

        var tm = layout.GetTextMetrics();

        // Prefer line metrics for baseline + height
        float lineHeight = tm.Height;
        float alphabeticBaseline = 0f;

        if (layout.TryGetFirstLineMetrics(out var firstLine))
        {
            lineHeight = firstLine.Height;
            alphabeticBaseline = firstLine.Baseline;
        }

        float width = tm.Width;

        float dx = GetTextAlignOffsetX(this.TextAlign, width);
        float dy = GetTextBaselineOffsetY(this.TextBaseline, lineHeight, alphabeticBaseline);

        var x = (float)pos.X + dx;
        var y = (float)pos.Y + dy;

        this.ApplyBrushAlpha(this.fill.Brush);
        this.RenderTarget.DrawTextLayout(
            (x, y),
            layout,
            this.fill.Brush);
        this.ResetBrushAlpha(this.fill.Brush);

        static float GetTextAlignOffsetX(TextAlign align, float width)
        {
            switch (align)
            {
                case TextAlign.Center:
                    return -width * 0.5f;

                case TextAlign.Right:
                case TextAlign.End:
                    return -width;

                case TextAlign.Left:
                case TextAlign.Start:
                default:
                    return 0f;
            }
        }

        static float GetTextBaselineOffsetY(TextBaseline baseline, float lineHeight, float alphabeticBaseline)
        {
            switch (baseline)
            {
                case TextBaseline.Top:
                    return 0f;

                case TextBaseline.Middle:
                    // Placeholder until FontMetrics exist.
                    return -lineHeight * 0.5f;

                case TextBaseline.Bottom:
                    return -lineHeight;

                case TextBaseline.Alphabetic:
                    return -alphabeticBaseline;

                case TextBaseline.Hanging:
                    // Placeholder until hanging baseline exists.
                    return -alphabeticBaseline;

                case TextBaseline.Ideographic:
                    // Placeholder until ideographic baseline exists.
                    return -lineHeight;

                default:
                    return -alphabeticBaseline;
            }
        }
    }

    void ITextDrawingContext.FillText(ReadOnlySpan<char> text, Point pos)
    {
        if (this.textFormat.IsNull)
        {
            return;
        }

        using var layout = this.DWriteFactory.CreateTextLayoutRef(
            text,
            this.textFormat,
            float.PositiveInfinity,
            float.PositiveInfinity);

        var tm = layout.GetTextMetrics();

        float lineHeight = tm.Height;
        float alphabeticBaseline = 0f;

        if (layout.TryGetFirstLineMetrics(out var firstLine))
        {
            lineHeight = firstLine.Height;
            alphabeticBaseline = firstLine.Baseline;
        }

        float width = tm.Width;

        float dx = GetTextAlignOffsetX(this.TextAlign, width);
        float dy = GetTextBaselineOffsetY(this.TextBaseline, lineHeight, alphabeticBaseline);

        var x = (float)pos.X + dx;
        var y = (float)pos.Y + dy;

        this.ApplyBrushAlpha(this.fill.Brush);
        this.RenderTarget.DrawTextLayout(
            (x, y),
            layout,
            this.fill.Brush);
        this.ResetBrushAlpha(this.fill.Brush);

        static float GetTextAlignOffsetX(TextAlign align, float width)
        {
            switch (align)
            {
                case TextAlign.Center:
                    return -width * 0.5f;

                case TextAlign.Right:
                case TextAlign.End:
                    return -width;

                case TextAlign.Left:
                case TextAlign.Start:
                default:
                    return 0f;
            }
        }

        static float GetTextBaselineOffsetY(TextBaseline baseline, float lineHeight, float alphabeticBaseline)
        {
            switch (baseline)
            {
                case TextBaseline.Top:
                    return 0f;

                case TextBaseline.Middle:
                    return -lineHeight * 0.5f;

                case TextBaseline.Bottom:
                    return -lineHeight;

                case TextBaseline.Alphabetic:
                    return -alphabeticBaseline;

                case TextBaseline.Hanging:
                    return -alphabeticBaseline;

                case TextBaseline.Ideographic:
                    return -lineHeight;

                default:
                    return -alphabeticBaseline;
            }
        }
    }

    Core.Canvas.TextMetrics ITextMeasureContext.MeasureText(string text)
    {
        if (this.textFormat.IsNull)
        {
            return new Core.Canvas.TextMetrics(
                new Core.Canvas.LineMetrics(0, 0, 0, 0, 0),
                new Core.Canvas.FontMetrics(0, 0, 0, 0, 0, 0, 0));
        }

        using var layout = this.DWriteFactory.CreateTextLayoutRef(
            text,
            this.textFormat,
            float.PositiveInfinity,
            float.PositiveInfinity);

        var tm = layout.GetTextMetrics();

        float baseline = 0f;
        if (layout.TryGetFirstLineMetrics(out var firstLine))
        {
            baseline = firstLine.Baseline;
        }

        // Keep your current width behavior for now.
        float advanceWidth = tm.WidthIncludingTrailingWhitespace;

        // Alignment width must match FillText's behavior to keep visuals consistent
        float alignWidth = tm.Width; // same width you use in FillText dx computation
        float dx = GetTextAlignOffsetX(this.TextAlign, alignWidth);

        // tm.Left/tm.Width are in layout-box coordinates (origin at 0).
        // Shift into anchor-relative coordinates by applying the same dx as FillText.
        float left = tm.Left - dx;
        float right = (tm.Left + tm.Width) + dx;

        // Ink-ish vertical bounds via overhangs
        var overhang = layout.GetOverhangMetrics();

        float inkTop = -overhang.Top;
        float inkBottom = tm.Height + overhang.Bottom;

        float visualAscent = baseline - inkTop;
        float visualDescent = inkBottom - baseline;

        if (visualAscent < 0f) visualAscent = 0f;
        if (visualDescent < 0f) visualDescent = 0f;

        var line = new Core.Canvas.LineMetrics(
            width: advanceWidth,
            left: left,
            right: right,
            ascent: visualAscent,
            descent: visualDescent);

        return new Core.Canvas.TextMetrics(line, this.currentFontMetrics);

        static float GetTextAlignOffsetX(TextAlign align, float width)
        {
            switch (align)
            {
                case TextAlign.Center:
                    return -width * 0.5f;

                case TextAlign.Right:
                case TextAlign.End:
                    return -width;

                case TextAlign.Left:
                case TextAlign.Start:
                default:
                    return 0f;
            }
        }
    }

    Core.Canvas.TextMetrics ITextMeasureContext.MeasureText(ReadOnlySpan<char> text)
    {
        if (this.textFormat.IsNull)
        {
            return new Core.Canvas.TextMetrics(
                new Core.Canvas.LineMetrics(0, 0, 0, 0, 0),
                new Core.Canvas.FontMetrics(0, 0, 0, 0, 0, 0, 0));
        }

        using var layout = this.DWriteFactory.CreateTextLayoutRef(
            text,
            this.textFormat,
            float.PositiveInfinity,
            float.PositiveInfinity);

        var tm = layout.GetTextMetrics();

        float baseline = 0f;
        if (layout.TryGetFirstLineMetrics(out var firstLine))
        {
            baseline = firstLine.Baseline;
        }

        float advanceWidth = tm.WidthIncludingTrailingWhitespace;

        float alignWidth = tm.Width;
        float dx = GetTextAlignOffsetX(this.TextAlign, alignWidth);

        float left = tm.Left - dx;
        float right = (tm.Left + tm.Width) + dx;

        var overhang = layout.GetOverhangMetrics();

        float inkTop = -overhang.Top;
        float inkBottom = tm.Height + overhang.Bottom;

        float visualAscent = baseline - inkTop;
        float visualDescent = inkBottom - baseline;

        if (visualAscent < 0f) visualAscent = 0f;
        if (visualDescent < 0f) visualDescent = 0f;

        var line = new Core.Canvas.LineMetrics(
            width: advanceWidth,
            left: left,
            right: right,
            ascent: visualAscent,
            descent: visualDescent);

        return new Core.Canvas.TextMetrics(line, this.currentFontMetrics);

        static float GetTextAlignOffsetX(TextAlign align, float width)
        {
            switch (align)
            {
                case TextAlign.Center:
                    return -width * 0.5f;

                case TextAlign.Right:
                case TextAlign.End:
                    return -width;

                case TextAlign.Left:
                case TextAlign.Start:
                default:
                    return 0f;
            }
        }
    }

    void ITextMeasureContext.SetFont(Xui.Core.Canvas.Font font)
    {
        string fontFamilyName = font.FontFamily[0];

        // Use system default
        FontCollection? fontCollection = null;

        DWrite.FontWeight fontWeight = (DWrite.FontWeight)(uint)font.FontWeight;
        DWrite.FontStyle fontStyle =
            font.FontStyle.IsItalic ? DWrite.FontStyle.Italic :
            (font.FontStyle.IsOblique ? DWrite.FontStyle.Oblique : DWrite.FontStyle.Normal);

        float fontSize = (float)font.FontSize;
        DWrite.FontStretch fontStretch = DWrite.FontStretch.Normal;

        this.textFormat.Dispose();
        this.textFormat = this.DWriteFactory.CreateTextFormatPtr(
            fontFamilyName, fontCollection, fontWeight, fontStyle, fontStretch, fontSize, "en-US");

        this.currentFontMetrics = this.TryComputeFontMetrics(
            fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize);
    }

    private Core.Canvas.FontMetrics TryComputeFontMetrics(
        string familyName,
        DWrite.FontWeight weight,
        DWrite.FontStyle style,
        DWrite.FontStretch stretch,
        float fontSizeDip)
    {
        // Stable fallback (avoid zero height) + baselines consistent with our contract
        Core.Canvas.FontMetrics fallback = new Core.Canvas.FontMetrics(
            fontAscent: fontSizeDip * 0.8f,
            fontDescent: fontSizeDip * 0.2f,
            emAscent: fontSizeDip * 0.8f,
            emDescent: fontSizeDip * 0.2f,
            alphabeticBaseline: 0f,
            hangingBaseline: -(fontSizeDip * 0.8f),
            ideographicBaseline: fontSizeDip * 0.2f);

        try
        {
            using var collection = this.DWriteFactory.GetSystemFontCollectionRef();

            collection.FindFamilyName(familyName, out uint familyIndex, out bool exists);
            if (!exists)
            {
                return fallback;
            }

            using var family = collection.GetFontFamily(familyIndex);
            using var dwFont = family.GetFirstMatchingFont(weight, stretch, style);

            using var face0 = dwFont.CreateFontFace();
            using var face1 = DWrite.FontFace1.Ref.FromFontFace(face0);

            face1.GetMetrics(out DWrite.FontMetrics1 m);

            float unitsPerEm = Math.Max(1f, m.DesignUnitsPerEm);
            float scale = fontSizeDip / unitsPerEm;

            // Em metrics (typographic)
            float emAscent = m.Ascent * scale;
            float emDescent = m.Descent * scale;

            // Font bounding box (for your Orange box)
            // To match your macOS implementation (which uses CTFont.Ascent/Descent for the "font box"),
            // use the same typographic ascent/descent here.
            float fontAscent = emAscent;
            float fontDescent = emDescent;

            // Baselines (offsets relative to alphabetic baseline at 0)
            float alphabeticBaseline = 0f;

            float hangingBaseline = -emAscent;
            float ideographicBaseline = emDescent;

            return new Core.Canvas.FontMetrics(
                fontAscent: fontAscent,
                fontDescent: fontDescent,
                emAscent: emAscent,
                emDescent: emDescent,
                alphabeticBaseline: alphabeticBaseline,
                hangingBaseline: hangingBaseline,
                ideographicBaseline: ideographicBaseline);
        }
        catch
        {
            return fallback;
        }
    }

    void ITransformContext.Translate(Vector vector)
    {
        Matrix3X2F matrix = new();
        this.RenderTarget.GetTransform(out matrix);
        AffineTransform transform = matrix;
        transform = transform * AffineTransform.Translate(vector);
        matrix = transform;
        this.RenderTarget.SetTransform(matrix);
    }

    void ITransformContext.Rotate(NFloat angle)
    {
        Matrix3X2F matrix = new();
        this.RenderTarget.GetTransform(out matrix);
        AffineTransform transform = matrix;
        transform = transform * AffineTransform.Rotate(angle);
        matrix = transform;
        this.RenderTarget.SetTransform(matrix);
    }

    void ITransformContext.Scale(Vector vector)
    {
        Matrix3X2F matrix = new();
        this.RenderTarget.GetTransform(out matrix);
        AffineTransform transform = matrix;
        transform = transform * AffineTransform.Scale(vector);
        matrix = transform;
        this.RenderTarget.SetTransform(matrix);
    }

    void ITransformContext.SetTransform(AffineTransform transform)
    {
        Matrix3X2F matrix = transform;
        this.RenderTarget.SetTransform(matrix);
    }

    void ITransformContext.Transform(AffineTransform matrix)
    {
        Matrix3X2F m = new();
        this.RenderTarget.GetTransform(out m);
        AffineTransform transform = m;
        transform = transform * matrix;
        m = transform;
        this.RenderTarget.SetTransform(m);
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect dest)
    {
        if (image is not DirectXImage img) return;
        RectF d = dest;
        this.RenderTarget.DrawBitmap(img.D2D1Bitmap, d, 1f);
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect dest, NFloat opacity)
    {
        if (image is not DirectXImage img) return;
        RectF d = dest;
        this.RenderTarget.DrawBitmap(img.D2D1Bitmap, d, (float)opacity);
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect source, Rect dest, NFloat opacity)
    {
        if (image is not DirectXImage img) return;
        RectF s = source;
        RectF d = dest;
        this.RenderTarget.DrawBitmap(img.D2D1Bitmap, d, (float)opacity, s);
    }

    public void Dispose()
    {
        // If the RenderTarget dies, and we have to reacreate the Direct2DContext, we will really need to call these...
        // this.fill.Dispose();
        // this.stroke.Dispose();

        // TODO: The IContext is disposable, but it has different meaning from the Dispose here...
        // Make IContext IContext.Scope() that can dispose a scope, but doesn't wipe out the RenderTarget...
        // this.RenderTarget.Dispose();
        // GC.SuppressFinalize(this);
    }

    public void BeginDraw()
    {
    }

    public void EndDraw()
    {
        this.states.Clear();

        while(this.drawingStateBlocks.Count > 0)
        {
            var block = this.drawingStateBlocks.Pop();
            this.RenderTarget.RestoreDrawingState(block);
            block.Dispose();
        }

        while(this.layersCount > 0)
        {
            this.layersCount--;
            this.RenderTarget.PopLayer();
        }

        while(this.transforms.Count > 0)
        {
            Matrix3X2F transorm = this.transforms.Pop();
            this.RenderTarget.SetTransform(in transorm);
        }
        this.RenderTarget.SetTransform(in Matrix3X2F.Identity);
    }

    ~Direct2DContext()
    {
        // Debug.WriteLine($"Reached to finalizer for {this.GetType().FullName}. Treat as resource and call Dispose.");
        // this.RenderTarget.Dispose();
    }

    private struct PaintStruct : IDisposable
    {
        public readonly RenderTarget RenderTarget;

        public PaintStyle PaintStyle;

        public Brush.Ptr Brush;

        public PaintStruct(RenderTarget renderTarget, Color color)
        {
            this.PaintStyle = PaintStyle.SolidColor;
            this.RenderTarget = renderTarget;
            this.Brush = this.RenderTarget.CreateSolidColorBrushPtr(color);
        }

        public void Dispose()
        {
            this.Brush.Dispose();
        }

        public void SetSolidColor(Color color)
        {
            this.Brush.Dispose();
            this.Brush = this.RenderTarget.CreateSolidColorBrushPtr(color);
            this.PaintStyle = PaintStyle.SolidColor;
        }

        public void SetLinearGradient(LinearGradient linearGradient)
        {
            this.Brush.Dispose();

            LinearGradientBrush.Properties linearGradientBrushProperties = new()
            {
                StartPoint = linearGradient.StartPoint,
                EndPoint = linearGradient.EndPoint
            };
            BrushProperties brushProperties = new ()
            {
                Opacity = 1f,
                Transform = new() { _11 = 1f, _12 = 0f, _21 = 0f, _22 = 1f, _31 = 0f, _32 = 0f }
            };

            var abstractGradientStops = linearGradient.GradientStops;
            Span<D2D1.GradientStop> gradientStops = stackalloc D2D1.GradientStop[abstractGradientStops.Length];
            for(var i = 0; i < abstractGradientStops.Length; i++)
            {
                var abstr = abstractGradientStops[i];
                gradientStops[i] = new()
                {
                    Position = (float)abstr.Offset,
                    Color = abstr.Color
                };
            }

            this.Brush = this.RenderTarget.CreateLinearGradientBrushPtr(linearGradientBrushProperties, brushProperties, gradientStops, Gamma.Gamma_2_2, ExtendMode.Clamp);
            this.PaintStyle = PaintStyle.LinearGradient;
        }

        public void SetRadialGradient(RadialGradient radialGradient)
        {
            this.Brush.Dispose();

            RadialGradientBrush.Properties radialGradientBrushProperties = new()
            {
                GradientOriginOffset = radialGradient.StartCenter - radialGradient.EndCenter,
                Center = radialGradient.EndCenter,
                Radius = new Point2F() { X = (float)radialGradient.StartRadius, Y = (float)radialGradient.StartRadius },
                RadiusX = (float)radialGradient.EndRadius,
                RadiusY = (float)radialGradient.EndRadius,
            };

            BrushProperties brushProperties = new ()
            {
                Opacity = 1f,
                Transform = new() { _11 = 1f, _12 = 0f, _21 = 0f, _22 = 1f, _31 = 0f, _32 = 0f }
            };

            var abstractGradientStops = radialGradient.GradientStops;
            Span<D2D1.GradientStop> gradientStops = stackalloc D2D1.GradientStop[abstractGradientStops.Length];
            for(var i = 0; i < abstractGradientStops.Length; i++)
            {
                var abstr = abstractGradientStops[i];
                gradientStops[i] = new()
                {
                    Position = (float)abstr.Offset,
                    Color = abstr.Color
                };
            }

            this.Brush = this.RenderTarget.CreateRadialGradientBrushPtr(radialGradientBrushProperties, brushProperties, gradientStops, Gamma.Gamma_2_2, ExtendMode.Clamp);
            this.PaintStyle = PaintStyle.LinearGradient;
        }

    }

    private struct StrokeStyleStruct
    {
        public readonly Factory3 Factory;

        private StrokeStyle.Ptr strokeStyle;

        private bool strokeStyleValid = false;

        private float lastLineWidth;

        private List<float> dashList;

        private LineCap lineCap;

        private Xui.Core.Canvas.LineJoin lineJoin;

        private float miterLimit;

        private float lineDashOffset;

        public StrokeStyleStruct(Factory3 factory)
        {
            this.Factory = factory;
            this.dashList = new List<float>();
            this.miterLimit = 10f;
            this.lineDashOffset = 0f;
        }

        public StrokeStyle.Ptr GetStrokeStyle(float lineWidth) => this.UpdateStrokeStyle(lineWidth);

        public LineCap LineCap
        {
            set
            {
                if (this.lineCap != value)
                {
                    this.lineCap = value;
                    this.InvalidateStroke();
                }
            }
        }

        public Xui.Core.Canvas.LineJoin LineJoin
        {
            set
            {
                if (this.lineJoin != value)
                {
                    this.lineJoin = value;
                    this.InvalidateStroke();
                }
            }
        }

        public NFloat MiterLimit
        {
            set
            {
                if (this.miterLimit != value)
                {
                    this.miterLimit = (float)value;
                    this.InvalidateStroke();
                }
            }
        }

        public NFloat LineDashOffset
        {
            set
            {
                if (this.lineDashOffset != value)
                {
                    this.lineDashOffset = (float)value;
                    this.InvalidateStroke();
                }
            }
        }

        public void SetLineDash(ReadOnlySpan<NFloat> segments)
        {
            this.dashList.Clear();
            foreach(var f in segments)
            {
                this.dashList.Add((float)f);
            }
            this.InvalidateStroke();
        }

        public struct StrokeSnapshot
        {
            public LineCap LineCap;
            public Xui.Core.Canvas.LineJoin LineJoin;
            public float MiterLimit;
            public float LineDashOffset;
            public float[]? DashPattern;
        }

        public StrokeSnapshot SaveSnapshot()
        {
            return new StrokeSnapshot
            {
                LineCap = this.lineCap,
                LineJoin = this.lineJoin,
                MiterLimit = this.miterLimit,
                LineDashOffset = this.lineDashOffset,
                DashPattern = this.dashList.Count > 0 ? this.dashList.ToArray() : null
            };
        }

        public void RestoreSnapshot(StrokeSnapshot snapshot)
        {
            this.lineCap = snapshot.LineCap;
            this.lineJoin = snapshot.LineJoin;
            this.miterLimit = snapshot.MiterLimit;
            this.lineDashOffset = snapshot.LineDashOffset;
            this.dashList.Clear();
            if (snapshot.DashPattern != null)
                this.dashList.AddRange(snapshot.DashPattern);
            this.InvalidateStroke();
        }

        private void InvalidateStroke()
        {
            this.strokeStyle.Dispose();
            this.strokeStyleValid = false;
        }

        private StrokeStyle.Ptr UpdateStrokeStyle(float lineWidth)
        {
            // D2D custom dash values are multiples of stroke width,
            // but Canvas uses absolute pixel values. Rebuild when lineWidth changes.
            if (this.strokeStyleValid && this.dashList.Count > 0 && this.lastLineWidth != lineWidth)
            {
                this.InvalidateStroke();
            }

            if (!this.strokeStyleValid)
            {
                this.lastLineWidth = lineWidth;

                if (this.dashList.Count > 0 ||
                    this.lineCap != LineCap.Butt ||
                    this.lineJoin != Xui.Core.Canvas.LineJoin.Miter ||
                    this.miterLimit != 10 ||
                    this.lineDashOffset != 0)
                {
                    // There are non-default values, build StrokeStyle
                    CapStyle capStyle = Map(this.lineCap);

                    // D2D dash values are in multiples of stroke width.
                    // Canvas dash values are in absolute pixels — normalize by dividing.
                    float dashScale = lineWidth > 0 ? lineWidth : 1f;

                    StrokeStyleProperties strokeStyleProperties = new ()
                    {
                        StartCap = capStyle,
                        EndCap = capStyle,
                        DashCap = capStyle,
                        LineJoin = Map(this.lineJoin),
                        MiterLimit = this.miterLimit,
                        DashStyle = this.dashList.Count == 0 ? DashStyle.Solid : DashStyle.Custom,
                        DashOffset = this.lineDashOffset / dashScale
                    };

                    // Normalize dash pattern from absolute pixels to stroke-width multiples
                    Span<float> normalizedDashes = stackalloc float[this.dashList.Count];
                    for (int i = 0; i < this.dashList.Count; i++)
                    {
                        normalizedDashes[i] = this.dashList[i] / dashScale;
                    }

                    this.strokeStyle = this.Factory.CreateStrokeStylePtr(strokeStyleProperties, normalizedDashes);
                }

                this.strokeStyleValid = true;
            }

            return this.strokeStyle;
        }

        private static D2D1.LineJoin Map(Xui.Core.Canvas.LineJoin lineJoin)
        {
            switch (lineJoin)
            {
                case Xui.Core.Canvas.LineJoin.Bevel: return D2D1.LineJoin.Bevel;
                case Xui.Core.Canvas.LineJoin.Round: return D2D1.LineJoin.Round;
                default: return D2D1.LineJoin.Miter;
            }
        }

        private static CapStyle Map(LineCap lineCap)
        {
            switch(lineCap)
            {
                case LineCap.Round: return CapStyle.Round;
                case LineCap.Square: return CapStyle.Square;
                default: return CapStyle.Flat;
            }
        }
    }

    /// <summary>
    /// Adapter that replays Path2D commands into the Direct2D PathStruct geometry sink.
    /// Allocated once and reused — forwards all IPathBuilder calls to the owner's PathStruct field.
    /// </summary>
    private class PathReplaySink : IPathBuilder
    {
        private readonly Direct2DContext owner;

        public PathReplaySink(Direct2DContext owner) => this.owner = owner;

        public void BeginPath() { /* no-op during replay */ }
        public void MoveTo(Point to) => owner.Path.MoveTo(to);
        public void LineTo(Point to) => owner.Path.LineTo(to);
        public void ClosePath() => owner.Path.ClosePath();
        public void CurveTo(Point cp1, Point to) => owner.Path.CurveTo(cp1, to);
        public void CurveTo(Point cp1, Point cp2, Point to) => owner.Path.CurveTo(cp1, cp2, to);

        public void Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding) =>
            owner.Path.Arc(center, radius, startAngle, endAngle, winding);

        public void ArcTo(Point cp1, Point cp2, NFloat radius) =>
            owner.Path.ArcTo(cp1, cp2, radius);

        public void Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding) =>
            owner.Path.Ellipse(center, radiusX, radiusY, rotation, startAngle, endAngle, winding);

        public void Rect(Rect rect) => owner.Path.Rect(rect);
        public void RoundRect(Rect rect, NFloat radius) => owner.Path.RoundRect(rect, radius);
        public void RoundRect(Rect rect, CornerRadius radius) => owner.Path.RoundRect(rect, radius);
    }
}
