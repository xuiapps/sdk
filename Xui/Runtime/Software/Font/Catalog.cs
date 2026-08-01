using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xui.Core.Canvas;

namespace Xui.Runtime.Software.Font;

public class Catalog
{
    private readonly IReadOnlyList<Uri> fontUris;
    private readonly IFontSource? fontSource;

    private readonly Dictionary<FontFace, TrueTypeFont?> faceToFont = new (32, FontFace.Comparer);

    private readonly Dictionary<Uri, FontFace?> uriToFace = new();

    private readonly FontMetricsCache fontMetrics = new();

    public Catalog(params IEnumerable<Uri>[] sources)
    {
        var list = new List<Uri>();
        foreach (var source in sources)
        {
            list.AddRange(source);
        }

        fontUris = list.AsReadOnly();
    }

    /// <summary>Creates a catalog backed by descriptor-first, lazily loaded font resources.</summary>
    public Catalog(IFontSource fontSource)
    {
        this.fontSource = fontSource;
        fontUris = fontSource.Resources.Select(resource => resource.Uri).ToArray();
    }

    public virtual IEnumerable<Uri> Fonts() => this.fontUris;

    public virtual ReadOnlyMemory<byte> LoadFromUri(Uri uri)
    {
        return uri.Scheme switch
        {
            "embedded" => LoadEmbedded(uri),
            "file"     => File.ReadAllBytes(uri.LocalPath),
            _          => throw new NotSupportedException($"Unsupported font URI scheme: {uri.Scheme}")
        };
    }

    /// <summary>
    /// Loads embedded font data from a URI in the format: embedded://Assembly.Name/Folder/File.ttf
    /// </summary>
    /// <param name="uri">The embedded URI, e.g. embedded://Xui.Core.Fonts/Inter/Inter-Bold.ttf</param>
    /// <returns>A byte array containing the font file contents.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resource or assembly cannot be found.</exception>
    public static ReadOnlyMemory<byte> LoadEmbedded(Uri uri)
    {
        return Xui.Core.Canvas.FontSource.LoadEmbedded(uri);
    }

    public IEnumerable<Uri> AStar(FontFace font)
    {
        var queue = new List<(Uri uri, int score)>();

        foreach (var uri in Fonts())
        {
            var score = Score(uri, font);
            queue.Add((uri, score));
        }

        queue.Sort((a, b) => a.score.CompareTo(b.score));

        foreach (var (uri, _) in queue)
            yield return uri;
    }

    private static int Score(Uri uri, FontFace font)
    {
        string file = System.IO.Path.GetFileNameWithoutExtension(uri.AbsoluteUri).ToLowerInvariant();
        string family = font.Family.Length > 0 ? font.Family.ToLowerInvariant() : "";

        int score = 0;

        // Family match is most important
        if (file.Contains(family))
            score -= 100;

        // Prefer regular for normal weights
        if (font.Weight == FontWeight.Normal)
        {
            if (file.Contains("regular"))
                score -= 20;
            else if (file.Contains("bold"))
                score += 10;
        }
        else if (font.Weight >= 600)
        {
            if (file.Contains("bold"))
                score -= 10;
            else if (file.Contains("regular"))
                score += 5;
        }

        // Style
        if (font.Style.IsItalic && file.Contains("italic"))
            score -= 10;
        else if (font.Style.IsOblique && file.Contains("oblique"))
            score -= 8;

        // Stretch
        if (font.Stretch < FontStretch.Normal && file.Contains("condensed"))
            score -= 5;
        else if (font.Stretch > FontStretch.Normal && file.Contains("expanded"))
            score -= 5;

        return score;
    }

    public TextMetrics MeasureText(in Core.Canvas.Font font, string text, TextAlign textAlign = TextAlign.Left, TextBaseline textBaseline = TextBaseline.Alphabetic)
    {
        var ttf = FontForFace(ToFace(in font));
        if (ttf is not null)
        {
            var line = ttf.MeasureLine(text, font, textAlign, textBaseline);
            if (!fontMetrics.TryGet(font, out var metrics))
            {
                metrics = ttf.Metrics(font);
                fontMetrics.Set(font, metrics);
            }
            return new TextMetrics(line, metrics);
        }

        return default;
    }

    public TrueTypeFont? FontForFace(in FontFace face)
    {
        if (faceToFont.TryGetValue(face, out var cachedTtf))
        {
            return cachedTtf;
        }
        
        if (fontSource is not null)
        {
            var request = new Xui.Core.Canvas.Font(1, face.Family, face.Weight, face.Style, face.Stretch);
            if (fontSource.TryGet(in request, out var resource))
            {
                var data = fontSource.Load(in resource);
                var ttf = new TrueTypeFont(data, resource.Uri);
                faceToFont[face] = ttf;
                return ttf;
            }
        }

        // Go through font URIs sorted by how likely they are to represent this font.
        foreach(var uri in AStar(face))
        {
            FontFace? uriFace;
            if (!uriToFace.TryGetValue(uri, out uriFace))
            {
                var data = this.LoadFromUri(uri);
                uriFace = TrueTypeFont.FontFace(data.Span);
                uriToFace[uri] = uriFace;
            }

            if (!uriFace.HasValue)
            {
                continue;
            }

            if (FontFace.Comparer.Equals(uriFace.Value, face))
            {
                var data = this.LoadFromUri(uri);
                var ttf = new TrueTypeFont(data, uri);
                faceToFont[uriFace.Value] = ttf;
                return ttf;
            }
        }

        return null;
    }

    private static FontFace ToFace(in Xui.Core.Canvas.Font font)
    {
        // TODO: Add mechanism to specify the default font for the system.

        var family = string.IsNullOrEmpty(font.FontFamily) ? "Default" : font.FontFamily;

        return new FontFace(
            family,
            font.FontWeight,
            font.FontStyle,
            font.FontStretch
        );
    }
}
