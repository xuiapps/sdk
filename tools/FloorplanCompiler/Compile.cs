using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.IOS.Actual;
using Xui.Runtime.Software;
using static Xui.Runtime.IOS.CoreGraphics;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

public sealed partial class FloorplanDrawing
{
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern nint CGBitmapContextCreate(nint data, nuint width, nuint height, nuint bitsPerComponent, nuint bytesPerRow, nint space, CGBitmapInfo info);
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern nint CGBitmapContextGetData(nint context);

    private sealed record Sprite(string Id, int Level, Rect Local, int Width, int Height, byte[] Pixels)
    { public int X, Y; }
    private sealed record Placement(Sprite Sprite, AffineTransform Transform);

    public static void CompileGpuAssets(string source, string destination)
    {
        const int padding = 16, mipCount = 5, atlasWidth = 2048;
        Directory.CreateDirectory(destination);
        var sprites = new List<Sprite>();
        var layers = new List<Placement>[4];
        var labels = new List<object>[4];
        var structure = new List<GeometryQuad>[4];
        var colors = new Dictionary<uint, Sprite>();
        for (int level = 0; level < 4; level++)
        {
            var root = XDocument.Load(source).Root!;
            foreach (var e in root.DescendantsAndSelf().Where(e => e.Attribute("data-state-attrs") != null).ToArray())
            {
                foreach (var name in e.Attribute("data-state-attrs")!.Value.Split(' '))
                    e.SetAttributeValue(name, e.Attribute($"data-l{level}w{(level == 0 ? 0 : 1)}-{name}")!.Value);
                e.Attribute("data-state-attrs")!.Remove();
            }
            structure[level] = CompileStructure(root, level, colors, sprites);
            SplitSimplifiedWorkstations(root);
            var ids = root.DescendantsAndSelf().Where(e => e.Attribute("id") != null).ToDictionary(e => e.Attribute("id")!.Value);
            // Expand composite workstations before importing, leaving each furniture symbol independent.
            foreach (var use in root.Descendants().Where(e => e.Name.LocalName == "use").ToArray())
            {
                var target = ids[use.Attribute("href")!.Value[1..]];
                if (target.Attribute("data-source-id")?.Value != "workstation") continue;
                var vb = RectFrom(target.Attribute("viewBox")!.Value);
                var w = Attr(use, "width", vb.Width); var h = Attr(use, "height", vb.Height);
                var scale = NFloat.Min(w / vb.Width, h / vb.Height);
                var m = Transform(use.Attribute("transform")?.Value) * AffineTransform.Translate((Attr(use, "x"), Attr(use, "y")))
                    * AffineTransform.Translate(((w - vb.Width * scale) / 2, (h - vb.Height * scale) / 2))
                    * AffineTransform.Scale((scale, scale)) * AffineTransform.Translate((-vb.X, -vb.Y));
                var group = new XElement(root.Name.Namespace + "g", use.Attributes().Where(a => a.Name.LocalName is not ("href" or "x" or "y" or "width" or "height" or "transform")));
                group.SetAttributeValue("transform", FormattableString.Invariant($"matrix({m.A} {m.B} {m.C} {m.D} {m.Tx} {m.Ty})"));
                group.Add(target.Elements().Select(e => new XElement(e)));
                use.ReplaceWith(group);
            }
            var drawing = new Loader(root).Build();
            var symbols = new Dictionary<SymbolDefinition, Sprite>(ReferenceEqualityComparer.Instance);
            layers[level] = new(); labels[level] = new();
            foreach (var item in drawing.levels[level])
            {
                if (item.Symbol is { } symbol && !symbol.SourceId.Contains("door", StringComparison.Ordinal))
                {
                    if (!symbols.TryGetValue(symbol, out var sprite))
                    {
                        sprite = Rasterize(symbol, level);
                        symbols.Add(symbol, sprite); sprites.Add(sprite);
                    }
                    layers[level].Add(new(sprite, item.Transform));
                }
                else if (item.Text != null && item.Fill is { } fill)
                {
                    var m = item.Transform; var c = fill.Color;
                    labels[level].Add(new { Text = item.Text, X = (double)item.Position.X, Y = (double)item.Position.Y,
                        Size = (double)item.Font.FontSize, Weight = (double)item.Font.FontWeight.Value, Align = (int)item.Align,
                        Transform = new double[] { (double)m.A, (double)m.B, (double)m.C, (double)m.D, (double)m.Tx, (double)m.Ty },
                        Bounds = new double[] { (double)item.Bounds.X, (double)item.Bounds.Y, (double)item.Bounds.Width, (double)item.Bounds.Height },
                        Color = new double[] { (double)c.Red, (double)c.Green, (double)c.Blue, (double)(c.Alpha * item.Alpha) } });
                }
            }
        }
        int x = 0, y = 0, rowHeight = 0;
        foreach (var sprite in sprites.OrderByDescending(s => s.Height).ThenByDescending(s => s.Width))
        {
            int w = Align(sprite.Width + padding * 2), h = Align(sprite.Height + padding * 2);
            if (w > atlasWidth) throw new InvalidDataException("Sprite exceeds atlas width.");
            if (x + w > atlasWidth) { x = 0; y += rowHeight; rowHeight = 0; }
            sprite.X = x + padding; sprite.Y = y + padding;
            x += w; rowHeight = Math.Max(rowHeight, h);
        }
        int atlasHeight = 1;
        while (atlasHeight < y + rowHeight) atlasHeight *= 2;
        var pixels = new byte[atlasWidth * atlasHeight * 4];
        foreach (var sprite in sprites)
            for (int row = 0; row < sprite.Height; row++)
                sprite.Pixels.AsSpan(row * sprite.Width * 4, sprite.Width * 4)
                    .CopyTo(pixels.AsSpan(((sprite.Y + row) * atlasWidth + sprite.X) * 4));
        using (var writer = new BinaryWriter(new GZipStream(File.Create(Path.Combine(destination, "atlas.bin.gz")), CompressionLevel.SmallestSize)))
        {
            writer.Write("XFPA"u8); writer.Write(1); writer.Write(atlasWidth); writer.Write(atlasHeight); writer.Write(mipCount);
            int w = atlasWidth, h = atlasHeight;
            for (int mip = 0; mip < mipCount; mip++)
            {
                writer.Write(pixels.Length); writer.Write(pixels);
                // PNG previews use straight alpha; native uploads and downsampling use premultiplied data.
                var preview = pixels.ToArray();
                for (int i = 0; i < preview.Length; i += 4)
                    if (preview[i + 3] > 0)
                        for (int c = 0; c < 3; c++) preview[i + c] = (byte)Math.Min(255, preview[i + c] * 255 / preview[i + 3]);
                PngEncoder.SaveRGBA(Path.Combine(destination, $"atlas-mip{mip}.png"), preview, w, h);
                if (mip + 1 == mipCount) break;
                var next = new byte[w / 2 * (h / 2) * 4];
                for (int py = 0; py < h / 2; py++)
                    for (int px = 0; px < w / 2; px++)
                        for (int c = 0; c < 4; c++)
                            next[(py * (w / 2) + px) * 4 + c] = (byte)((pixels[((py * 2) * w + px * 2) * 4 + c]
                                + pixels[((py * 2) * w + px * 2 + 1) * 4 + c] + pixels[((py * 2 + 1) * w + px * 2) * 4 + c]
                                + pixels[((py * 2 + 1) * w + px * 2 + 1) * 4 + c] + 2) / 4);
                pixels = next; w /= 2; h /= 2;
            }
        }
        for (int level = 0; level < layers.Length; level++)
        {
            using var writer = new BinaryWriter(File.Create(Path.Combine(destination, $"quads-l{level}.bin")));
            writer.Write("XFPQ"u8); writer.Write(1); writer.Write(level);
            var layer = layers[level];
            {
                writer.Write(layer.Count);
                foreach (var placement in layer)
                {
                    var s = placement.Sprite; var b = s.Local; var m = placement.Transform;
                    Point[] corners = [(b.X, b.Y), (b.Right, b.Y), (b.Right, b.Bottom), (b.X, b.Bottom)];
                    foreach (var corner in corners) { var p = m * corner; writer.Write((float)p.X); writer.Write((float)p.Y); }
                    writer.Write((float)s.X / atlasWidth); writer.Write((float)s.Y / atlasHeight);
                    writer.Write((float)(s.X + s.Width) / atlasWidth); writer.Write((float)(s.Y + s.Height) / atlasHeight);
                    var scale = NFloat.Max(NFloat.Sqrt(m.A * m.A + m.B * m.B), NFloat.Sqrt(m.C * m.C + m.D * m.D));
                    writer.Write((float)NFloat.Log2(2 / scale));
                }
            }
        }
        for (int level = 0; level < structure.Length; level++)
        {
            using var writer = new BinaryWriter(File.Create(Path.Combine(destination, $"structure-l{level}.bin")));
            writer.Write("XFPG"u8); writer.Write(1); writer.Write(level); writer.Write(structure[level].Count);
            foreach (var quad in structure[level])
            {
                foreach (var vertex in quad.Vertices)
                {
                    writer.Write((float)vertex.Point.X); writer.Write((float)vertex.Point.Y);
                    writer.Write((float)vertex.Normal.X); writer.Write((float)vertex.Normal.Y); writer.Write((float)vertex.Coverage);
                }
                writer.Write((quad.Color.X + quad.Color.Width / 2f) / atlasWidth);
                writer.Write((quad.Color.Y + quad.Color.Height / 2f) / atlasHeight);
            }
        }
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(Path.Combine(destination, "atlas.json"), JsonSerializer.Serialize(new {
            Version = 1, SourceSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source))),
            Width = atlasWidth, Height = atlasHeight, MipCount = mipCount, Padding = padding,
            Sprites = sprites.Select(s => new { s.Id, s.Level, s.X, s.Y, s.Width, s.Height,
                LocalBounds = new double[] { (double)s.Local.X, (double)s.Local.Y, (double)s.Local.Width, (double)s.Local.Height } }),
            QuadCounts = layers.Select(l => l.Count), StructureQuadCounts = structure.Select(l => l.Count), Labels = labels
        }, options));
        Console.WriteLine($"Atlas {atlasWidth}x{atlasHeight}, {sprites.Count} sprites, {mipCount} mip levels; quads: {string.Join(", ", layers.Select(l => l.Count))}");
        Console.WriteLine($"Structure quads (including edge coverage): {string.Join(", ", structure.Select(l => l.Count))}");
        static int Align(int value) => (value + 15) & ~15;
    }

    private static Sprite Rasterize(SymbolDefinition symbol, int level)
    {
        var bounds = symbol.Bounds;
        int w = (int)NFloat.Ceiling(bounds.Width * 2), h = (int)NFloat.Ceiling(bounds.Height * 2);
        using var space = CGColorSpaceRef.CreateDeviceRGB();
        var bitmap = CGBitmapContextCreate(0, (nuint)w, (nuint)h, 8, (nuint)(w * 4), space,
            CGBitmapInfo.ByteOrder32Little | CGBitmapInfo.AlphaPremultipliedFirst);
        if (bitmap == 0) throw new InvalidOperationException("Unable to allocate sprite bitmap.");
        try
        {
            // Bitmap storage is top-to-bottom, but Quartz starts with a bottom-left origin.
            // Flip the drawing CTM before cropping so both pixels and local bounds use SVG's Y-down space.
            using var native = new IOSDrawingContext(); native.Bind(bitmap);
            IContext context = native;
            context.Translate((0, h)); context.Scale((2, -2)); context.Translate((-bounds.X, -bounds.Y));
            Draw(context, symbol.Items, bounds, 1, false, false, cull: false);
            native.Unbind();
            var pixels = new byte[w * h * 4]; Marshal.Copy(CGBitmapContextGetData(bitmap), pixels, 0, pixels.Length);
            for (int i = 0; i < pixels.Length; i += 4) (pixels[i], pixels[i + 2]) = (pixels[i + 2], pixels[i]);
            int left = w, top = h, right = 0, bottom = 0;
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                if (pixels[(y * w + x) * 4 + 3] != 0) { left = Math.Min(left, x); top = Math.Min(top, y); right = Math.Max(right, x + 1); bottom = Math.Max(bottom, y + 1); }
            if (left >= right) { left = top = 0; right = bottom = 1; }
            left = Math.Max(0, left - 2); top = Math.Max(0, top - 2); right = Math.Min(w, right + 2); bottom = Math.Min(h, bottom + 2);
            int width = right - left, height = bottom - top;
            var cropped = new byte[width * height * 4];
            for (int y = 0; y < height; y++) pixels.AsSpan(((top + y) * w + left) * 4, width * 4).CopyTo(cropped.AsSpan(y * width * 4));
            return new(symbol.SourceId, level, new(bounds.X + (NFloat)left / 2, bounds.Y + (NFloat)top / 2, (NFloat)width / 2, (NFloat)height / 2), width, height, cropped);
        }
        finally { CGContextRef.CGContextRelease(bitmap); }
    }

    private static void SplitSimplifiedWorkstations(XElement root)
    {
        // L0/L1 author their desk and chair as rectangles rather than nested symbol uses.
        // Promote those shapes before expanding the workstation, otherwise the symbol-only
        // atlas export silently drops both. Keep the authored geometry and colors exactly.
        var defs = root.Elements().Single(e => e.Name.LocalName == "defs");
        foreach (var workstation in defs.Elements().Where(e => e.Attribute("data-source-id")?.Value == "workstation").ToArray())
        {
            if (workstation.Descendants().Any(e => e.Name.LocalName == "use")) continue;
            var shapes = workstation.Descendants().Where(e => e.Name.LocalName == "rect").ToArray();
            if (shapes.Length != 2) throw new InvalidDataException("Expected a desk and chair in the simplified workstation.");
            var bounds = RectFrom(workstation.Attribute("viewBox")!.Value);
            for (int index = 0; index < shapes.Length; index++)
            {
                string part = index == 0 ? "desk" : "chair";
                string id = workstation.Attribute("id")!.Value + "-" + part;
                var symbol = new XElement(workstation.Name, workstation.Attributes());
                symbol.SetAttributeValue("id", id);
                symbol.SetAttributeValue("data-source-id", "workstation-" + part);
                symbol.Add(new XElement(shapes[index]));
                defs.Add(symbol);
                shapes[index].ReplaceWith(new XElement(root.Name.Namespace + "use",
                    new XAttribute("href", "#" + id), new XAttribute("x", (double)bounds.X), new XAttribute("y", (double)bounds.Y),
                    new XAttribute("width", (double)bounds.Width), new XAttribute("height", (double)bounds.Height)));
            }
        }
    }
}
