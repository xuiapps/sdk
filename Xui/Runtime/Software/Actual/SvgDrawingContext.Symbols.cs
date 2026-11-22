using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Symbols;
using Xui.Core.Math2D;

namespace Xui.Runtime.Software.Actual;

public sealed partial class SvgDrawingContext
{
    private readonly Dictionary<SymbolContent, string> symbolIds = new();

    /// <summary>Writes one vector definition and a use element for each placement.</summary>
    public void DrawSymbol(SymbolContent content)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        CheckCompositionDrawing();
        var bounds = content.Bounds;
        // Brightness is applied when paints are selected. Preserve that behavior instead
        // of silently reusing a definition compiled with a different color adjustment.
        if (filter.Amount != 1) { content.Replay(this); return; }
        if (!symbolIds.TryGetValue(content, out var id))
        {
            id = $"symbol{symbolIds.Count + 1}";
            var previousWriter = bodyWriter;
            var previousTransform = transform;
            var previousFont = font;
            var previousAlign = TextAlign;
            var previousBaseline = TextBaseline;
            using var buffer = new MemoryStream();
            using var writer = new StreamWriter(buffer, new UTF8Encoding(false), leaveOpen: true);
            try
            {
                bodyWriter = writer;
                transform = AffineTransform.Identity;
                content.Replay(this);
                writer.Flush();
                // Gradient/image/clip definitions were emitted into the shared defs writer;
                // append the completed drawing afterwards so definitions aren't nested.
                // The command tape already clips to Bounds. Avoid adding a second
                // viewport clip, while making the reusable drawing's coordinates explicit.
                defsWriter.WriteLine($"<symbol id=\"{id}\" viewBox=\"{F(bounds.X)} {F(bounds.Y)} {F(bounds.Width)} {F(bounds.Height)}\" preserveAspectRatio=\"none\" overflow=\"visible\">");
                defsWriter.Write(Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length));
                defsWriter.WriteLine("</symbol>");
                symbolIds.Add(content, id);
            }
            finally
            {
                bodyWriter = previousWriter; transform = previousTransform;
                font = previousFont; TextAlign = previousAlign; TextBaseline = previousBaseline;
            }
        }
        Ident();
        // xlink works in older SVG viewers, including platform thumbnail renderers.
        // Matching x/y and viewport size cancel viewBox's origin/scale mapping,
        // preserving the caller's coordinates even for negative symbol bounds.
        bodyWriter.WriteLine($"<use href=\"#{id}\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xlink:href=\"#{id}\" x=\"{F(bounds.X)}\" y=\"{F(bounds.Y)}\" width=\"{F(bounds.Width)}\" height=\"{F(bounds.Height)}\"/>");
    }
}
