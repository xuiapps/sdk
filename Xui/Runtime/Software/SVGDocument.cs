using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;

namespace Xui.Runtime.Software;

/// <summary>
/// Renders a view tree to a standalone SVG document without creating a window or accepting input.
/// </summary>
/// <remarks>
/// <para>
/// This is intended for documentation, static web graphics, and deterministic component rendering.
/// It performs the same measure, arrange, and render passes that a root view receives during a
/// window render. <see cref="Time"/> is supplied to the layout pass, but no animation or input
/// events are generated.
/// </para>
/// <para>
/// Supply <see cref="FontUris"/> whenever layout depends on a bundled font. The optional
/// <see cref="FontResolver"/> controls whether those fonts are treated as installed, linked from
/// the web, or embedded in the resulting SVG.
/// </para>
/// </remarks>
public sealed class SVGDocument
{
    /// <summary>The view tree to lay out and render.</summary>
    public View? Content { get; set; }

    /// <summary>The SVG viewport size. Defaults to 800 by 600 logical pixels.</summary>
    public Size Size { get; set; } = (800, 600);

    /// <summary>The deterministic time supplied as the current layout time. Defaults to zero.</summary>
    public TimeSpan Time { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Font sources used for text measurement and optional SVG font emission. Defaults to no fonts.
    /// </summary>
    public IEnumerable<Uri> FontUris { get; set; } = [];

    /// <summary>Optional descriptor-first font source used in preference to <see cref="FontUris"/>.</summary>
    public IFontSource? FontSource { get; set; }

    /// <summary>Controls how resolved font faces are represented in the SVG.</summary>
    public SvgDrawingContext.SvgFontResolver FontResolver { get; set; } = SvgDrawingContext.SvgFontResolver.Default;

    /// <summary>Controls deterministic numeric formatting in the generated SVG.</summary>
    public SvgDrawingContext.NumericFormat NumericFormat { get; set; } = SvgDrawingContext.NumericFormat.Default;

    /// <summary>
    /// Lays out <see cref="Content"/> and returns a complete SVG document.
    /// </summary>
    public string ToSVG()
    {
        using var stream = new MemoryStream();
        using (var context = new SvgDrawingContext(
            this.Size,
            stream,
            catalog: this.FontSource is null ? new Catalog(this.FontUris) : new Catalog(this.FontSource),
            resolver: this.FontResolver,
            numericFormat: this.NumericFormat,
            keepOpen: true))
        {
            var frame = new LayoutFrameContext(this.Time, this.Time, context, context, default);
            var update = new LayoutUpdate(
                LayoutPass.Measure | LayoutPass.Arrange | LayoutPass.Render,
                new MeasureConstraints(this.Size, LayoutSizeMode.Exact, LayoutSizeMode.Exact),
                new ArrangeConstraints(this.Size, default, (0, 0)));
            LayoutMeasurements measurements = default;
            this.Content?.Update(in frame, in update, ref measurements);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
