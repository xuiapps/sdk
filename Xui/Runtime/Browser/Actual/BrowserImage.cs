using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Browser.Actual;

/// <summary>A browser-decoded image; loading never blocks the browser UI thread.</summary>
public sealed partial class BrowserImage : IImage, IDisposable
{
    private static int nextId;
    internal int Id { get; } = ++nextId;
    private readonly Action invalidate;
    private int generation;
    private bool disposed;
    public Size Size { get; private set; }
    public BrowserImage(Action invalidate) => this.invalidate = invalidate;

    [JSImport("loadImage", "xui-browser")]
    private static partial Task<JSObject> LoadImage(int id, string uri);
    [JSImport("releaseImage", "xui-browser")]
    private static partial void ReleaseImage(int id);
    [JSImport("uploadImage", "xui-browser")]
    private static partial void UploadImage(int id, int width, int height, [JSMarshalAs<JSType.MemoryView>] Span<byte> pixels);

    public void Load(string uri) => _ = LoadAndReportAsync(uri);
    private async Task LoadAndReportAsync(string uri)
    {
        try { await LoadAsync(uri); }
        catch (Exception error) { Console.Error.WriteLine($"Image '{uri}': {error.Message}"); }
    }
    public async Task LoadAsync(string uri)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var request = ++generation;
        Size = Size.Empty;
        using var size = await LoadImage(Id, uri);
        if (disposed || request != generation) return;
        Size = new Size((NFloat)size.GetPropertyAsDouble("width"), (NFloat)size.GetPropertyAsDouble("height"));
        invalidate();
    }
    public void LoadPixels(int width, int height, ReadOnlySpan<byte> bgra32Data)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (width <= 0 || height <= 0 || bgra32Data.Length != checked(width * height * 4))
            throw new ArgumentException("Expected width * height * 4 BGRA bytes.");
        generation++;
        // The interop view is read-only by convention; the player copies before converting BGRA to RGBA.
        UploadImage(Id, width, height, MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(bgra32Data), bgra32Data.Length));
        Size = new Size(width, height);
        invalidate();
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        generation++;
        ReleaseImage(Id);
    }
}
