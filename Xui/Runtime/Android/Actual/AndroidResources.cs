using System.Runtime.CompilerServices;
using Xui.Core.Canvas.Symbols;
using System.Buffers.Binary;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.Android.Composition;
using Xui.Runtime.Android.Interop;

namespace Xui.Runtime.Android.Actual;

internal sealed class AndroidResources(nint host, IFontSource? fonts) : IImagePipeline, IAndroidResources, IDisposable
{
    private readonly Dictionary<Font, int> faces = new();
    private readonly Dictionary<string, (int Id, Size Size)> images = new();
    private readonly WireBuffer request = new();
    private int nextFont, nextImage, nextSymbol;
    private readonly ConditionalWeakTable<SymbolContent, SymbolId> symbols = new();
    private readonly List<(WeakReference<SymbolContent> Content, int Id)> symbolLifetimes = new();
    private readonly WireBuffer symbolRequest = new();
    private sealed record SymbolId(int Value);
    public int Symbol(SymbolContent content)
    {
        if (symbols.TryGetValue(content, out var found)) return found.Value;
        // Recorded frames hold strong references to definitions. Only collect native records
        // once neither callers nor any display list can still use them.
        for (int i=symbolLifetimes.Count-1; i>=0; i--)
        {
            if (symbolLifetimes[i].Content.TryGetTarget(out _)) continue;
            request.Clear(); request.Int(symbolLifetimes[i].Id); AndroidBridge.Invoke(host, 11, request);
            symbolLifetimes.RemoveAt(i);
        }
        int id=++nextSymbol;
        symbolRequest.Clear(); symbolRequest.Int(id); symbolRequest.Rect(content.Bounds);
        // Font registration uses request; it must not overwrite this definition packet.
        content.Replay(new AndroidCommandWriter(symbolRequest, this));
        AndroidBridge.Invoke(host, 10, symbolRequest);
        symbols.Add(content, new(id)); symbolLifetimes.Add((new(content), id));
        return id;
    }
    public int Font(Font font)
    {
        var face = font with { FontSize = 1, LineHeight = 1 };
        if (faces.TryGetValue(face, out var id)) return id;
        id = ++nextFont;
        request.Clear(); request.Int(id); request.Text(font.FontFamily ?? "sans-serif");
        request.Int((int)font.FontWeight.Value); request.Int(font.FontStyle.IsItalic ? 1 : 0);
        ReadOnlySpan<byte> bytes = fonts?.TryGet(font, out var resource) == true ? fonts.Load(resource).Span : default;
        request.Int(bytes.Length); request.Bytes(bytes);
        AndroidBridge.Invoke(host, 2, request);
        faces.Add(face, id); return id;
    }
    public int Image(IImage image) => image is AndroidImage own && ReferenceEquals(own.Owner, this)
        ? own.Id : throw new ArgumentException("Image belongs to another Android window/backend.", nameof(image));
    public IImage CreateImage() => new AndroidImage(this);
    internal (int, Size) Load(string uri)
    {
        if (images.TryGetValue(uri, out var value)) return value;
        var path = ResolveAsset(uri);
        var id = ++nextImage;
        request.Clear(); request.Int(id); request.Text(path);
        var result = AndroidBridge.Invoke(host, 3, request, 8);
        value = (id, new Size(BinaryPrimitives.ReadInt32LittleEndian(result), BinaryPrimitives.ReadInt32LittleEndian(result[4..])));
        images.Add(uri, value); return value;
    }
    internal static string ResolveAsset(string uri)
    {
        if (!AssetUri.TryGetRelativePath(uri, out var path))
            throw new NotSupportedException("Android images currently support packaged asset:// URIs.");
        return "Assets/" + path;
    }
    internal int Pixels(int id, int width, int height, ReadOnlySpan<byte> pixels)
    {
        if (width <= 0 || height <= 0 || pixels.Length != checked(width * height * 4)) throw new ArgumentException("Invalid BGRA image dimensions.");
        if (id == 0) id = ++nextImage;
        request.Clear(); request.Int(id); request.Int(width); request.Int(height); request.Bytes(pixels);
        AndroidBridge.Invoke(host, 4, request); return id;
    }
    public void Dispose() { request.Dispose(); symbolRequest.Dispose(); symbols.Clear(); symbolLifetimes.Clear(); faces.Clear(); images.Clear(); }
}
