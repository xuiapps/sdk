using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Android.Actual;

internal sealed class AndroidImage(AndroidResources owner) : IImage
{
    internal AndroidResources Owner => owner;
    internal int Id { get; private set; }
    private int pixelId;
    public Size Size { get; private set; }
    public void Load(string uri) => (Id, Size) = owner.Load(uri);
    // Asset decode stays on the UI thread; asynchronous remote loading is not yet supported.
    public Task LoadAsync(string uri) { Load(uri); return Task.CompletedTask; }
    public void LoadPixels(int width, int height, ReadOnlySpan<byte> pixels)
    { Id = pixelId = owner.Pixels(pixelId, width, height, pixels); Size = new(width, height); }
}
